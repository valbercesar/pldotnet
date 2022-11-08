/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *                      procedural languages (PL)
 *
 *
 * Copyright 2019-2020 Brick Abode
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * pldotnet_csharp.c - Postgres PL handlers for C# and functions
 *
 */

#include "pldotnet_csharp.h"
#include <access/htup.h>
#include <catalog/pg_proc.h>
#include <postgres.h>
#include <utils/syscache.h>
#include "pldotnet_hostfxr.h"

/*
 * Exported functions
 */
PG_FUNCTION_INFO_V1(plcsharp_call_handler);
PG_FUNCTION_INFO_V1(plcsharp_inline_handler);
PG_FUNCTION_INFO_V1(plcsharp_validator);

/*
 * START: declaring functions
 */

/**
 * @brief The call_handler will be called to execute the procedural
 * language's functions.  The call handler receives a pointer to a
 * FunctionCallInfoData struct containing argument values and information
 * about the called function, and it is expected to return a Datum result.
 *
 * @param PG_FUNCTION_ARGS The standard parameter list for fmgr-compatible
 * functions.
 *
 * @return The datum that can be stored in a PostgreSQL table.
 */
Datum plcsharp_call_handler(PG_FUNCTION_ARGS);

/**
 * @brief The inline_handler will be called to execute an anonymous code
 * block (DO command) in this language.
 *
 * @param PG_FUNCTION_ARGS The standard parameter list for
 * fmgr-compatible functions.
 *
 * @return The datum that can be stored in a PostgreSQL table.
 */
Datum plcsharp_inline_handler(PG_FUNCTION_ARGS);

/**
 * @brief The validator function will inspect the function body for syntactical
 * correctness, but it can also look at other properties of the function,
 * for example if the language cannot handle certain argument types. To
 * signal an error, the validator function should use the ereport()
 * function. The return value of the function is ignored.
 *
 * @param PG_FUNCTION_ARGS The standard parameter list for fmgr-compatible
 * functions.
 *
 * @return The datum that can be stored in a PostgreSQL table.
 */
Datum plcsharp_validator(PG_FUNCTION_ARGS);

/**
 * @brief The main handler function, which receives an additional bool argument
 * to deal with both normal and inline calls.
 *
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param is_inline Wether the function is inline or not.
 *
 * @return The datum that can be stored in a PostgreSQL table.
 */
static Datum plcsharp_generic_handler(FunctionCallInfo fcinfo, bool is_inline);

/**
 * @brief This function starts to building the output paths into a static
 * pldotnet_PathConfig, then it parses the procedure data into a valid source
 * code. This second step produces a valid pldotnet_FunctionDecl which contains
 * useful information regarding the current function. While building
 * pldotnet_FunctionDecl, it tries to find a previous cached function aiming to
 * avoid reloading stuff from .NET.
 *
 * Case 1: If there is no previous cached function, it loads .NET, gets the
 * function pointers and calls Engine.Compile() and Engine.Run() to retrieve the
 * desired results. After calling the user function, it saves the current
 * pldotnet_FunctionDecl into a global hash table called procedures
 * (see pldotnet_common.h).
 *
 *  Case 2: if there is a previous cached function, then it just calls
 * Engine.Run()
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param is_inline Wether the function is inline or not.
 * @return The datum that can be stored in a PostgreSQL table.
 */
static Datum plcsharp_CompileAndRunUserFunction(const FunctionCallInfo fcinfo,
                                                bool is_inline);

/**
 * @brief Call the pldotnet_BuildPaths function (from pldotnet_common.c) if
 * the paths related to dotnet were not created yet.
 *
 * @param paths the pldotnet config paths.
 * @return true the paths were created.
 * @return false the paths could not be created.
 */
inline static bool plcsharp_BuildPaths(pldotnet_PathConfig *paths);

/**
 * @brief Returns the declared function or creates one.
 *
 * @param oid the function ID.
 * @param fcinfo
 * @param proc
 * @param is_inline whether it is a inline function or not
 * @param validation whether the function should be validated.
 * @return pldotnet_FunctionDecl*  the function created by the user.
 */
static pldotnet_FunctionDecl *plcsharp_GetFunctionDecl(Oid oid,
                                                       FunctionCallInfo fcinfo,
                                                       HeapTuple proc,
                                                       bool is_inline,
                                                       bool validation);

/**
 * @brief This function tries to build a valid pldotnet_FunctionDecl.
 * It searchs for information on PG SysCache and it stores the required data
 * into the last argument (pldotnet_FunctionDecl *function_decl) this structure
 * is meant to be saved into a hash table.
 *
 * @param oid the function ID.
 * @param fcinfo
 * @param proc
 * @param is_inline whether it is a inline function or not.
 * @param validation whether the function should be validated.
 * @param function_decl
 * @return true it it succeeds.
 * @return false otherwise.
 */
static bool plcsharp_BuildFunctionDecl(Oid oid, FunctionCallInfo fcinfo,
                                       HeapTuple proc, bool is_inline,
                                       bool validation,
                                       pldotnet_FunctionDecl *function_decl);

/**
 * @brief Validate the user function. This function is called in
 * plcsharp_validator(PG_FUNCTION_ARGS) function, which is executed after the
 * user creates a SQL function on PostgreSQL.
 *
 * @param oid The function ID
 * @param fcinfo The function information
 */
static void plcsharp_ValidateUserFunction(const Oid oid,
                                          const FunctionCallInfo fcinfo);

/**
 * @brief Set the function information to the plcsharp_GetSourceCode object
 * if the function is not null.
 *
 * @param fcinfo the function information
 * @param proc
 * @param procst
 * @param is_inline whether the functions is inline
 * @param validation whetter the function should be validated
 * @param user_function_decl the object that stores the function information
 * @return true if the process was successful
 * @return false if the source code could not be obtained
 */
static bool plcsharp_GetSourceCode(
    FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst,
    bool is_inline, bool validation,
    pldotnet_UserFunctionDeclaration *user_function_decl);

/*
 * END: declaring functions
 */

/*
 * START: implementing functions
 */

Datum plcsharp_call_handler(PG_FUNCTION_ARGS) {
    return plcsharp_generic_handler(fcinfo, false);
}

Datum plcsharp_inline_handler(PG_FUNCTION_ARGS) {
    return plcsharp_generic_handler(fcinfo, true);
}

Datum plcsharp_validator(PG_FUNCTION_ARGS) {
    MemoryContextWrapper memory_context;
    HeapTuple tuple;
    Oid funcoid = PG_GETARG_OID(0);

    if (!check_function_bodies)
        return (Datum)0;

    pldotnet_LoadHostFxrIfNeeded();

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        tuple = SearchSysCache1(PROCOID, ObjectIdGetDatum(funcoid));
        if (!HeapTupleIsValid(tuple))
            elog(ERROR, "cache lookup failed for function %u", funcoid);

        ReleaseSysCache(tuple);

        plcsharp_ValidateUserFunction(funcoid, fcinfo);

        /* REVERT PREV MEM CONTEXT */
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        /* Do the exception handling */
        elog(WARNING, "[pldotnet]: Exception on PG context");
        PG_RE_THROW();
    }
    PG_END_TRY();

    PG_RETURN_VOID();
}

static Datum plcsharp_generic_handler(FunctionCallInfo fcinfo, bool is_inline) {
    MemoryContextWrapper memory_context;
    Datum retval = 0;

    pldotnet_LoadHostFxrIfNeeded();

    if (!pldotnet_SPIReady())
        return retval;

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        retval = plcsharp_CompileAndRunUserFunction(fcinfo, is_inline);

        /* REVERT PREV MEM CONTEXT */
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "[pldotnet]: Exception on PG context");
        PG_RE_THROW();
    }

    PG_END_TRY();

    pldotnet_SPIFinish();

    return retval;
}

static Datum plcsharp_CompileAndRunUserFunction(const FunctionCallInfo fcinfo,
                                                bool is_inline) {
    HeapTuple proc;
    Form_pg_proc procst;
    pldotnet_FunctionDecl *function_decl = nullptr;
    void *arglist = nullptr;
    bool *nullmap = nullptr;
    pldotnet_Result output;
    output.value = (Datum)0;

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END
     * IF PROC != nullptr */
    /* START */
    proc = pldotnet_GetPostgresHeapTuple(fcinfo->flinfo->fn_oid);

    function_decl = plcsharp_GetFunctionDecl(fcinfo->flinfo->fn_oid, fcinfo,
                                             proc, is_inline, false);

    procst = (Form_pg_proc)GETSTRUCT(proc);
    /* END */

    pldotnet_ReleasePostgresHeapTuple(proc);

    if (nullptr == function_decl || nullptr == function_decl->call_user_method)
        elog(ERROR, "[pldotnet]: Could not load function_decl");

    arglist = pldotnet_BuildArgumentList(fcinfo, procst);
    nullmap = function_decl->user_decl.support_null_input
                  ? pldotnet_BuildNullArgumentList(fcinfo, procst)
                  : nullptr;

    function_decl->call_user_method(function_decl->user_decl.func_oid, arglist,
                                    &nullmap[0], (void *)&output);

    if (output.is_null)
        fcinfo->isnull = true;

    pldotnet_FreeGCHandle(arglist);
    if (nullmap)
        pfree(nullmap);

    return output.value;
}

inline static bool plcsharp_BuildPaths(pldotnet_PathConfig *paths) {
    static bool built = false;
    if (!built) {
        pldotnet_BuildPaths(true, paths);
        built = true;
    }
    return built;
}

static pldotnet_FunctionDecl *plcsharp_GetFunctionDecl(Oid oid,
                                                       FunctionCallInfo fcinfo,
                                                       HeapTuple proc,
                                                       bool is_inline,
                                                       bool validation) {
    pldotnet_FunctionDecl *decl = pldotnet_FindFunctionDecl(oid);
    bool found = nullptr != decl;

    if (found && !validation)
        return decl;

    decl = pldotnet_CreateFunctionDecl();

    plcsharp_BuildFunctionDecl(oid, fcinfo, proc, is_inline, validation, decl);

    pldotnet_SaveFunction(decl, !found);

    return decl;
}

static bool plcsharp_BuildFunctionDecl(Oid oid, FunctionCallInfo fcinfo,
                                       HeapTuple proc, bool is_inline,
                                       bool validation,
                                       pldotnet_FunctionDecl *function_decl) {
    Form_pg_proc procst;
    static pldotnet_PathConfig paths;

    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument, function_decl is null");

    if (!plcsharp_BuildPaths(&paths))
        elog(ERROR, "[pldotnet]: Could not build paths");

    if (!pldotnet_SetNetLoader(paths.config_path, paths.prefix))
        elog(ERROR, "[pldotnet]: Could not obtain .NET Loader");
    procst = (Form_pg_proc)GETSTRUCT(proc);

    if (!pldotnet_SetDotNetMethods(paths.library_path))
        elog(ERROR, "[pldotnet]: Could not obtain C# Methods");

    /* save some basic data */
    function_decl->user_decl.func_oid = (uint32_t)oid;
    function_decl->user_decl.support_null_input =
        procst->proisstrict ? false : true;
    function_decl->ret_type = procst->prorettype;

    if (!plcsharp_GetSourceCode(fcinfo, proc, procst, is_inline, validation,
                                &(function_decl->user_decl)))
        elog(ERROR, "[pldotnet]: Could not obtain the source code");

    if (!pldotnet_CompileUserFunction(&(function_decl->user_decl)))
        elog(ERROR,
             "[pldotnet]: Could not compile this "
             "function using the new method.");

    function_decl->call_user_method = pldotnet_GetUserDirectMethod(&paths);

    return nullptr != function_decl->call_user_method;
}

static void plcsharp_ValidateUserFunction(const Oid oid,
                                          const FunctionCallInfo fcinfo) {
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Could not obtain info about %u", oid);

    plcsharp_GetFunctionDecl(oid, fcinfo, proc, false, true);

    /* END */
    pldotnet_ReleasePostgresHeapTuple(proc);
}

static bool plcsharp_GetSourceCode(
    FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst,
    bool is_inline, bool validation,
    pldotnet_UserFunctionDeclaration *user_function_decl) {
    if (nullptr == user_function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument: user_function_decl is null");

    if (is_inline) {
        elog(ERROR, "[pldotnet]: Inline functions are not supported yet");
    } else {
        user_function_decl->language = "csharp";
        user_function_decl->func_name = NameStr(procst->proname);
        user_function_decl->func_rettype = procst->prorettype;
        user_function_decl->func_body = pldotnet_GetFunctionBody(proc, procst);
        user_function_decl->func_paramsName =
            pldotnet_GetSqlParamsName(proc, procst, true);
        user_function_decl->func_paramsType =
            pldotnet_GetSqlParamsType(proc, procst);
    }
    return true;
}

/*
 * END: implementing functions
 */
