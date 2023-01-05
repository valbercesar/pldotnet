/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *                      procedural languages (PL)
 *
 *
 * Copyright 2023 Brick Abode
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
 * pldotnet.c - Postgres pldotnet extension init and deinit routines
 *
 */

#include "pldotnet_conversions.h"
#include "pldotnet_main.h"

#define DIR_SEPARATOR '/'

PG_MODULE_MAGIC;

/* Declare extension variables/structs here */
PGDLLEXPORT Datum _PG_init(PG_FUNCTION_ARGS);
PGDLLEXPORT Datum _PG_fini(PG_FUNCTION_ARGS);

#if PG_VERSION_NUM >= 100000

PG_FUNCTION_INFO_V1(_PG_init);

/**
 * @brief On startup, pldotnet initializes the function cache.
 */
Datum _PG_init(PG_FUNCTION_ARGS) {
    elog(LOG, "[plldotnet]: _PG_init");

    root_path = strdup(dnldir);
    if (root_path[strlen(root_path) - 1] == DIR_SEPARATOR)
        root_path[strlen(root_path) - 1] = 0;

    if (!pldotnet_LoadHostFxrIfNeeded())
        elog(ERROR, "[pldotnet]: Could not load host fxr.");

    if (!pldotnet_BuildPaths())
        elog(ERROR, "[pldotnet]: Could not build paths.");

    if (!pldotnet_SetNetLoader())
        elog(ERROR, "[pldotnet]: Could not obtain .NET Loader.");

    if (!pldotnet_SetDotNetMethods())
        elog(ERROR, "[pldotnet]: Could not obtain C# Methods.");

    procedures =
        g_hash_table_new_full(g_direct_hash, g_direct_equal, NULL, NULL);

    PG_RETURN_VOID();
}

PG_FUNCTION_INFO_V1(_PG_fini);
/**
 * @brief On startup, pldotnet removes the function cache.
 */
Datum _PG_fini(PG_FUNCTION_ARGS) {
    /* destroys the global hash table */
    g_hash_table_destroy(procedures);

    /* TODO: shutdown dotnet runtime */

    PG_RETURN_VOID();
}

#endif
