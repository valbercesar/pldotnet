/* 
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as 
 * 			procedural languages (PL)
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
 * pldotnet_common.h
 *
 */
#ifndef PLDOTNETCOMMON_H
#define PLDOTNETCOMMON_H

/* PostgreSQL */
#include <postgres.h>
#include <fmgr.h>
#include <funcapi.h>
#include <access/heapam.h>
#include <access/xact.h>
#if PG_VERSION_NUM >= 90300
#include <access/htup_details.h>
#endif
#include <catalog/namespace.h>
#include <catalog/pg_proc.h>
#include <catalog/pg_type.h>
#include <commands/trigger.h>
#include <executor/spi.h>
#include <nodes/makefuncs.h>
#include <parser/parse_type.h>
#include <utils/array.h>
#include <utils/builtins.h>
#include <utils/datum.h>
#include <utils/lsyscache.h>
#include <utils/memutils.h>
#include <utils/rel.h>
#include <utils/syscache.h>
#include <utils/typcache.h>
#include <utils/numeric.h>
#include <mb/pg_wchar.h> /* For UTF8 support */

#include <assert.h>
#include "pldotnet_hostfxr.h"

#include <dlfcn.h>
#include <limits.h>
#include <glib.h>
#include <glib/ghash.h>

#include "pldotnet_helpers.h"

GHashTable *procedures;

#if PG_VERSION_NUM < 110000
    #define TupleDescAttr(tupdesc, i) ((tupdesc)->attrs[(i)])
    #define pg_create_context(name) \
            AllocSetContextCreate(TopMemoryContext, \
                name, ALLOCSET_DEFAULT_MINSIZE, ALLOCSET_DEFAULT_INITSIZE, \
                ALLOCSET_DEFAULT_MAXSIZE);
#else
    #define pg_create_context(name) \
            AllocSetContextCreate(TopMemoryContext, name, ALLOCSET_DEFAULT_SIZES);
#endif

/* As a reminder snprintf is defined as pg_snprintf.
 * Check port.h into postgres codebase
 */
#define SNPRINTF(dst, size, fmt, ...)                                    \
    if(snprintf(dst,size,fmt, __VA_ARGS__) >= size){                     \
        elog(ERROR,"[pldotnet] (%s:%d) String too long for buffer: " fmt \
                        ,__FILE__,__LINE__,__VA_ARGS__);                 \
    }

#define QUOTE(name) #name
#define STR(macro) QUOTE(macro)
#define CH(c) c
#define DIR_SEPARATOR '/'
#define MAX_PATH PATH_MAX

typedef struct pldotnet_ArgArrayInfo
{
    int ixarray;
    int typlen;
    bool typbyval;
    char typtype;
    Oid typelem;
    char typalign;
    int ndim;
    const int * dims;
    int nelems;
    char csharpdecl[256];
}pldotnet_ArgArrayInfo;

typedef struct pldotnet_FuncInOutInfo
{
    int typesize_nullflags;
    int typesize_args;
    int typesize_result;
    pldotnet_ArgArrayInfo arrayinfo[32]; /* check max nr of args */
}pldotnet_FuncInOutInfo;

typedef struct pldotnet_ArgsSource
{
    char* source_code;
    int result;
    uint32 func_oid;
}pldotnet_ArgsSource;

typedef struct pldotnet_FunctionDecl
{
    pldotnet_ArgsSource source;
    int8_t *args;
    size_t args_length;
    Oid ret_type;
    component_entry_point_fn dotnet_method;
} pldotnet_FunctionDecl;

typedef struct pldotnet_PathConfig
{
    char prefix[MAXPGPATH];
    char config_path[MAXPGPATH];
    char library_path[MAXPGPATH];
    char src_lib_path[MAXPGPATH];

} pldotnet_PathConfig;

typedef struct MemoryContextWrapper
{
    MemoryContext prev;
    MemoryContext curr;
} MemoryContextWrapper;


bool pldotnet_ValidArgsSource(const pldotnet_ArgsSource *args);
void pldotnet_ResetFunctionDecl(pldotnet_FunctionDecl *function_decl);
bool pldotnet_ValidFunctionDecl(pldotnet_FunctionDecl *function_decl);
bool pldotnet_ValidCachedFunction( pldotnet_FunctionDecl *reference, pldotnet_FunctionDecl *candidate);
pldotnet_FunctionDecl* pldotnet_FindFunctionDecl(int function_id);
void pldotnet_InsertFunctionDecl(pldotnet_FunctionDecl *function_decl, bool insert);
pldotnet_FunctionDecl* pldotnet_CopyFunctionDecl(const pldotnet_FunctionDecl *function_decl);

void pldotnet_SaveFunctionDecl(
    dotnet_loader loader,
    pldotnet_PathConfig *paths,
    pldotnet_FunctionDecl *function_decl);

void pldotnet_BuildPaths(bool is_csharp, pldotnet_PathConfig *paths);
bool pldotnet_ValidPaths(const pldotnet_PathConfig *paths);

void pldotnet_StartNewMemoryContext(MemoryContextWrapper *config);
void pldotnet_ResetMemoryContext(MemoryContextWrapper *config);

bool pldotnet_TypeSupported(Oid type);
const char * pldotnet_GetNetTypeName(Oid id, bool hastypeconversion);
const char * pldotnet_GetCompatibleNetTypeName(Oid id, bool hastypeconversion, bool is_csharp);
int pldotnet_GetTypeSize(Oid id);
const char * pldotnet_GetUnmanagedTypeName(Oid type);
int pldotnet_SetScalarValue(char *argp, Datum datum, FunctionCallInfo fcinfo,
                            size_t narg, Oid type, bool * nullp);
Datum pldotnet_GetScalarValue(char * result_ptr, char * resultnull_ptr,
                              FunctionCallInfo fcinfo, Oid type);
bool pldotnet_IsArray(int narg, pldotnet_FuncInOutInfo * funinout_info);
bool pldotnet_IsSimpleType(Oid type);
bool pldotnet_IsTextType(Oid type);
bool pldotnet_IsNullable(Oid type);
bool pldotnet_IsNullValue(FunctionCallInfo fcinfo, size_t index);

Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index);
bool pldotnet_SetArrayInfo(Datum datum, Oid oid, uint32_t narg, const char *attributeTemplate, bool swap_variable_decl, pldotnet_FuncInOutInfo *func_inout_info);
int8_t* pldotnet_CreateCStructLibargs(FunctionCallInfo fcinfo, Form_pg_proc procst, bool force_nullable_flags, pldotnet_FuncInOutInfo *func_inout_info);

bool pldotnet_SPIReady(void);
void pldotnet_SPIFinish(void);
bool pldotnet_TriggerNotSupported(FunctionCallInfo fcinfo);


HeapTuple pldotnet_GetPostgresHeapTuple(FunctionCallInfo fcinfo);
void pldotnet_ReleasePostgresHeapTuple(HeapTuple proc);

component_entry_point_fn pldotnet_GetUserMethod( dotnet_loader loader, pldotnet_PathConfig *paths);

bool
pldotnet_Run(
    dotnet_loader loader,
    const char *dotnet_type, 
    const char *dotnet_type_method, 
    const pldotnet_PathConfig *paths,
    int8_t *libargs,
    size_t args_length);

bool
pldotnet_CompileUserFunction(
    dotnet_loader loader,
    const FunctionCallInfo fcinfo,
    const pldotnet_PathConfig *paths,
    pldotnet_ArgsSource *source
);

bool
pldotnet_RunUserFunction(
    dotnet_loader loader,
    const pldotnet_PathConfig *paths,
    int8_t *libargs,
    size_t args_length
);

/*
 * Directories where C#/F# projects for user code are built when
 * USE_DOTNETBUILD is defined. Otherwise that is where our C#/F# compiler
 * projects are located. Default for Linux is /var/lib/DotNetEngine/
 */
char *root_path;
char *dnldir;

pldotnet_PathConfig *spi_paths;
dotnet_loader assembly_loader;

#endif /* PLDOTNETCOMMON_H */
