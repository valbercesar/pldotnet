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
#include "pldotnet_hostfxr.h"
#include "postgres.h"
#include "fmgr.h"
#include "access/htup.h"
#include "access/htup_details.h"
#include "catalog/pg_proc.h"
#include "utils/syscache.h"

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
static Datum
plcsharp_generic_handler(
    FunctionCallInfo fcinfo,
    bool is_inline);

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
 *  Case 2: if there is a previous cached function, then it just calls Engine.Run()
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param is_inline Wether the function is inline or not.
 * @return The datum that can be stored in a PostgreSQL table.
 */
static Datum plcsharp_CompileAndRunUserFunction(
    const FunctionCallInfo fcinfo,
    bool is_inline);

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
    // MemoryContextWrapper memory_context;
    HeapTuple tuple;
    Oid funcoid = PG_GETARG_OID(0);

    //     if (!check_function_bodies)
    //         return (Datum)0;

    pldotnet_LoadHostFxrIfNeeded();

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        //         pldotnet_StartNewMemoryContext(&memory_context);

        tuple = SearchSysCache1(PROCOID, ObjectIdGetDatum(funcoid));
        if (!HeapTupleIsValid(tuple))
            elog(ERROR, "cache lookup failed for function %u", funcoid);

        ReleaseSysCache(tuple);

        //         plcsharp_ValidateUserFunction(funcoid, fcinfo);

        /* REVERT PREV MEM CONTEXT */
        //         pldotnet_ResetMemoryContext(&memory_context);
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

static Datum
plcsharp_generic_handler(FunctionCallInfo fcinfo,
                         bool is_inline) {
    // MemoryContextWrapper memory_context;
    Datum retval = 0;

    pldotnet_LoadHostFxrIfNeeded();

    // if (!pldotnet_SPIReady())
    //     return retval;

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        // pldotnet_StartNewMemoryContext(&memory_context);

        retval = plcsharp_CompileAndRunUserFunction(fcinfo, is_inline);

        /* REVERT PREV MEM CONTEXT */
        // pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "[pldotnet]: Exception on PG context");
        PG_RE_THROW();
    }

    PG_END_TRY();

    // pldotnet_SPIFinish();

    return retval;
}

static Datum
plcsharp_CompileAndRunUserFunction(
    const FunctionCallInfo fcinfo,
    bool is_inline) {
    /// TESTING
    perror("CompileAndRunUserFunction can not be executed now.\n");
    exit(EXIT_SUCCESS);
}

/*
 * END: implementing functions
 */
