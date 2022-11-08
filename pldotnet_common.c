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
 * pldotnet_common.c - Common functions for PG <-> .NET type delivering
 *
 */

#include "pldotnet_common.h"

char *dnldir = STR(PLNET_ENGINE_DIR);

dotnet_loader assembly_loader;
compile_user_fn compile_user_function;
build_datum_list_t build_datum_list;
add_datum_to_list_t add_datum_to_list;
free_generic_gchandle_t free_generic_gchandle;

/*
 * START: implementing functions
 */

bool pldotnet_SetNetLoader(const char *config_path, const char *prefix) {
    if (nullptr != assembly_loader)
        return true;

    assembly_loader = GetNetLoadAssemblySetup(config_path, prefix);
    return nullptr != assembly_loader;
}

bool pldotnet_SPIReady(void) {
    if (SPI_connect() != SPI_OK_CONNECT) {
        elog(ERROR, "[pldotnet]: could not connect to SPI manager");
        return false;
    }

    return true;
}

void pldotnet_SPIFinish(void) {
    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
}

bool pldotnet_SetDotNetMethods(const char *library_path) {
    compile_user_function = (compile_user_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "CompileUserFunction",
        "PlDotNET.Engine+DelCompileUserFunction, PlDotNET");

    build_datum_list = (build_datum_list_t)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "BuildDatumList",
        "PlDotNET.Engine+DelBuildDatumList, PlDotNET");

    add_datum_to_list = (add_datum_to_list_t)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "AddDatumToList",
        "PlDotNET.Engine+DelAddDatumToList, PlDotNET");

    free_generic_gchandle = (free_generic_gchandle_t)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "FreeGenericGCHandle",
        "PlDotNET.Engine+DelFreeGenericGCHandle, PlDotNET");

    return nullptr != compile_user_function && nullptr != build_datum_list &&
           nullptr != add_datum_to_list && nullptr != free_generic_gchandle;
}

void *pldotnet_GetDotNetMethod(const char *library_path,
                               const char *dotnet_type,
                               const char *dotnet_type_method,
                               const char *delegate_type_name) {
    int rc;
    void *dotnet_method = nullptr;

    rc = assembly_loader(library_path, dotnet_type, dotnet_type_method,
                         delegate_type_name, nullptr, (void **)&dotnet_method);

    if (nullptr == dotnet_method)
        elog(ERROR, "[pldotnet]: Could not get_function_pointer(%s)",
             delegate_type_name);

    if (0 != rc)
        elog(ERROR,
             "[pldotnet]: Could not "
             "load_assembly_and_get_function_pointer(%s)",
             delegate_type_name);

    return dotnet_method;
}

void pldotnet_BuildPaths(bool is_csharp, pldotnet_PathConfig *paths) {
    char prefix[MAXPGPATH];
    const char json_path_suffix[] =
        "/bin/Release/net6.0/PlDotNET."
        "runtimeconfig.json";
    const char src_path_suffix[] = "/Lib.cs";
    const char dll_path_suffix[] = "/bin/Release/net6.0/PlDotNET.dll";
    char lang[] = "csharp";

    if (!is_csharp)
        lang[0] = 'f';

    SNPRINTF(paths->prefix, MAXPGPATH, "%s%s", root_path, "/src/");
    SNPRINTF(prefix, MAXPGPATH, "%s%s", paths->prefix, lang);
    SNPRINTF(paths->config_path, MAXPGPATH, "%s%s", prefix, json_path_suffix);
    SNPRINTF(paths->library_path, MAXPGPATH, "%s%s", prefix, dll_path_suffix);
    SNPRINTF(paths->src_lib_path, MAXPGPATH, "%s%s", prefix, src_path_suffix);
}

HeapTuple pldotnet_GetPostgresHeapTuple(Oid oid) {
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Cache lookup failed for function %u", oid);
    return proc;
}

inline void pldotnet_ReleasePostgresHeapTuple(HeapTuple proc) {
    ReleaseSysCache(proc);
}

pldotnet_FunctionDecl *pldotnet_CreateFunctionDecl(void) {
    pldotnet_FunctionDecl *decl;
    MemoryContext mem = CurrentMemoryContext;

    /* change to top mem context */
    MemoryContextSwitchTo(TopMemoryContext);

    decl = (pldotnet_FunctionDecl *)palloc(sizeof(pldotnet_FunctionDecl));

    pldotnet_ResetFunctionDecl(decl);

    /* revert to previous mem context */
    MemoryContextSwitchTo(mem);

    return decl;
}

pldotnet_FunctionDecl *pldotnet_FindFunctionDecl(int function_id) {
    gpointer value =
        g_hash_table_lookup(procedures, GUINT_TO_POINTER(function_id));

    if (nullptr != value)
        return (pldotnet_FunctionDecl *)value;

    return nullptr;
}

void pldotnet_ResetFunctionDecl(pldotnet_FunctionDecl *function_decl) {
    if (nullptr == function_decl)
        return;
    function_decl->args = nullptr;
    function_decl->args_length = 0;
    function_decl->ret_type = InvalidOid;
    function_decl->call_user_method = nullptr;

    /* this is the new user declaration */
    function_decl->user_decl.language = nullptr;
    function_decl->user_decl.func_name = nullptr;
    function_decl->user_decl.func_rettype = 0;
    function_decl->user_decl.func_paramsName = nullptr;
    function_decl->user_decl.func_paramsType = nullptr;
    function_decl->user_decl.func_body = nullptr;
    function_decl->user_decl.func_oid = 0;
    function_decl->user_decl.support_null_input = true;
}

void pldotnet_StartNewMemoryContext(MemoryContextWrapper *config) {
    config->prev = CurrentMemoryContext;
    config->curr = AllocSetContextCreate(
        TopMemoryContext, "PL/NET func_exec_ctx", ALLOCSET_SMALL_SIZES);

    if (nullptr == config->curr)
        elog(ERROR, "Could not create a new memory context");

    MemoryContextSwitchTo(config->curr);
}

void pldotnet_ResetMemoryContext(MemoryContextWrapper *config) {
    if (nullptr == config)
        return;

    if (config->prev)
        MemoryContextSwitchTo(config->prev);

    if (config->curr)
        MemoryContextDelete(config->curr);
}

void pldotnet_SaveFunction(pldotnet_FunctionDecl *function, bool insert) {
    if (insert)
        g_hash_table_insert(procedures,
                            GUINT_TO_POINTER(function->user_decl.func_oid),
                            (gpointer)function);
    else
        g_hash_table_replace(procedures,
                             GUINT_TO_POINTER(function->user_decl.func_oid),
                             (gpointer)function);
}

const char *pldotnet_GetFunctionBody(HeapTuple proc, Form_pg_proc procst) {
    bool isnull = false;
    Datum prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    const char *body = DatumGetCString(DirectFunctionCall1(textout, prosrc));
    return body;
}

const char *pldotnet_GetSqlParamsName(HeapTuple proc, Form_pg_proc procst,
                                      bool is_csharp) {
    int nnames = 0;
    bool isnull = false;
    const char **argnames_array = nullptr;
    Datum *argnames = nullptr;
    char *sql_params = nullptr;
    Datum argname =
        SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_proargnames, &isnull);
    size_t buffer_size = 0;
    const char *space = " ";
    size_t space_size = strlen(space);

    if (!isnull)
        deconstruct_array(DatumGetArrayTypeP(argname), TEXTOID, -1, false, 'i',
                          &argnames, NULL, &nnames);
    else
        return nullptr;

    argnames_array =
        (const char **)palloc0(sizeof(char *) * procst->pronargs * 2);

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        /* get the arg name */
        const char *name =
            DatumGetCString(DirectFunctionCall1(textout, argnames[i]));

        argnames_array[i * 2] = name;

        buffer_size += strlen(name) + space_size;
    }

    sql_params = (char *)palloc0(buffer_size);

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        /* copy the arg name */
        strcat(sql_params, argnames_array[i * 2]);
        if (i < procst->pronargs - 1)
            strcat(sql_params, space);
    }
    return sql_params;
}

const int *pldotnet_GetSqlParamsType(HeapTuple proc, Form_pg_proc procst) {
    int nnames = 0;
    bool isnull = false;
    Datum *argnames = nullptr;
    Datum argname =
        SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_proargnames, &isnull);
    Oid *argtypes = procst->proargtypes.values;
    int *sql_types = (int *)palloc0(sizeof(int) * procst->pronargs);

    if (!isnull)
        deconstruct_array(DatumGetArrayTypeP(argname), TEXTOID, -1, false, 'i',
                          &argnames, NULL, &nnames);
    else
        return nullptr;

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        sql_types[i] = argtypes[i];
    }
    return sql_types;
}

bool pldotnet_CompileUserFunction(
    pldotnet_UserFunctionDeclaration *declaration) {
    int test = compile_user_function(
        declaration->func_oid, (void *)declaration->func_name,
        (int)declaration->func_rettype, (void *)declaration->func_paramsName,
        (int *)declaration->func_paramsType, (void *)declaration->func_body,
        declaration->support_null_input);
    return 0 == test;
}

user_method_delegate pldotnet_GetUserDirectMethod(pldotnet_PathConfig *paths) {
    return (user_method_delegate)pldotnet_GetDotNetMethod(
        paths->library_path, "PlDotNET.Engine, PlDotNET", "RunUserFunction",
        "PlDotNET.Engine+DelRunUserFunction, PlDotNET");
}

void *pldotnet_BuildArgumentList(FunctionCallInfo fcinfo, Form_pg_proc procst) {
    void *list = build_datum_list();
    for (int16_t i = 0; i < procst->pronargs; ++i) {
        Datum argdatum = pldotnet_GetArgDatum(fcinfo, i);
        add_datum_to_list(list, (void *)argdatum);
    }
    return list;
}

bool *pldotnet_BuildNullArgumentList(FunctionCallInfo fcinfo,
                                     Form_pg_proc procst) {
    int nargums = procst->pronargs;
    bool *isnull = (bool *)palloc(sizeof(bool) * nargums);

    for (int i = 0; i < nargums; ++i) {
        isnull[i] = pldotnet_CheckNullArgument(fcinfo, i);
    }
    return isnull;
}

void pldotnet_Elog(int level, char *message) { elog(level, "%s", message); }

inline Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index) {
#if PG_VERSION_NUM >= 120000
    return fcinfo->args[index].value;
#else
    return fcinfo->arg[index];
#endif
}

inline bool pldotnet_CheckNullArgument(FunctionCallInfo fcinfo, size_t index) {
#if PG_VERSION_NUM >= 120000
    return fcinfo->args[index].isnull;
#else
    return fcinfo->argnull[index];
#endif
}

void pldotnet_SetDatumResult(void *value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result *)native_result;
    result->is_null = isnull;
    result->value = (Datum)value;
}

void pldotnet_FreeGCHandle(void *gchandle) { free_generic_gchandle(gchandle); }

/*
 * END: implementing functions
 */
