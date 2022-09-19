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
 * pldotnet.c - Postgres pldotnet extension init and deinit routines
 *
 */
#include <postgres.h>
#include <funcapi.h>
#include <dlfcn.h>
#include <glib.h>
#include <glib/ghash.h>
#include <coreclr_delegates.h>

#define QUOTE(s) #s
#define DIR_SEPARATOR '/'

/*
 * Directories where C#/F# projects for user code are built when
 * USE_DOTNETBUILD is defined. Otherwise that is where our C#/F# compiler
 * projects are located. Default for Linux is /var/lib/DotNetEngine/
 */

char *root_path = NULL;
char *dnldir = QUOTE(PLNET_ENGINE_DIR);
GHashTable *procedures;
load_assembly_and_get_function_pointer_fn assembly_loader;

PG_MODULE_MAGIC;

/* Declare extension variables/structs here */
PGDLLEXPORT Datum _PG_init(PG_FUNCTION_ARGS);
PGDLLEXPORT Datum _PG_fini(PG_FUNCTION_ARGS);

#if PG_VERSION_NUM >= 90000
#define CODEBLOCK \
  ((InlineCodeBlock *) DatumGetPointer(PG_GETARG_DATUM(0)))->source_text
PG_FUNCTION_INFO_V1(_PG_init);
/** 
 * @brief On startup, pldotnet initializes the function cache.
 */
Datum _PG_init(PG_FUNCTION_ARGS) {
    elog(LOG, "[plldotnet]: _PG_init");

    root_path = strdup(dnldir);
    if (root_path[strlen(root_path) - 1] == DIR_SEPARATOR)
        root_path[strlen(root_path) - 1] = 0;

    procedures = g_hash_table_new_full(
        g_direct_hash,
        g_direct_equal,
        NULL,
        NULL);

    /* reset the assembly loader */
    assembly_loader = (load_assembly_and_get_function_pointer_fn)NULL;

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

    // dlclose(nethost_lib);
    PG_RETURN_VOID();
}
#endif
