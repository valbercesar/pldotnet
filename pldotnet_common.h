/* 
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as 
 *             procedural languages (PL)
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
#ifndef PLDOTNET_COMMON_H_
#define PLDOTNET_COMMON_H_

#define QUOTE(name) #name
#define STR(macro) QUOTE(macro)

#define nullptr ((void *)0)

#include "pldotnet_hostfxr.h"
#include <postgres.h>
#include <executor/spi.h>
#include <utils/syscache.h>
#include <catalog/pg_proc.h>
#include <catalog/pg_type.h>
#include <glib.h>
#include <utils/memutils.h>
#include <utils/numeric.h>
#include <utils/fmgrprotos.h>
#include <utils/array.h>
#include <utils/geo_decls.h>
#include <access/htup_details.h>

extern PGDLLIMPORT bool check_function_bodies;

extern GHashTable *procedures;

/* As a reminder snprintf is defined as pg_snprintf.  TODO - CHECK HERE
 * Check port.h into postgres codebase
 */ 
#define SNPRINTF(dst, size, fmt, ...)                                    \
    if ( snprintf(dst, size, fmt, __VA_ARGS__) >= size) {                   \
        elog(ERROR, "[pldotnet] (%s:%d) String too long for buffer: " fmt \
                        , __FILE__, __LINE__, __VA_ARGS__);                 \
    }

typedef struct MemoryContextWrapper {
    MemoryContext prev;
    MemoryContext curr;
} MemoryContextWrapper;

typedef struct pldotnet_PathConfig {
    char prefix[MAXPGPATH];
    char config_path[MAXPGPATH];
    char library_path[MAXPGPATH];
    char src_lib_path[MAXPGPATH];
} pldotnet_PathConfig;

typedef struct pldotnet_Result {
    Datum value;
    bool is_null;
} pldotnet_Result;

typedef struct pldotnet_ArgArrayInfo {
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
} pldotnet_ArgArrayInfo;

typedef struct pldotnet_FuncInOutInfo {
    int typesize_nullflags;
    int typesize_args;
    int typesize_result;
    pldotnet_ArgArrayInfo arrayinfo[32]; /* check max nr of args */
} pldotnet_FuncInOutInfo;

typedef struct pldotnet_UserFunctionDeclaration {
    const char *language;
    const char *func_name;
    int func_rettype;
    const char *func_paramsName;
    const int *func_paramsType;
    const char *func_body;
    int func_oid;
    bool support_null_input;
} pldotnet_UserFunctionDeclaration;

typedef struct pldotnet_FunctionDecl {
    int8_t *args;
    size_t args_length;
    Oid ret_type;
    pldotnet_FuncInOutInfo func_inout_info;
    component_entry_point_fn dotnet_method;
    pldotnet_UserFunctionDeclaration user_decl;
    user_method_delegate call_user_method;
} pldotnet_FunctionDecl;

/**
 * @brief 
 * 
 * @return true 
 * @return false 
 */
bool pldotnet_SPIReady(void);

/**
 * @brief 
 * 
 */
void pldotnet_SPIFinish(void);

/**
 * @brief 
 * 
 * @param config_path 
 * @param prefix 
 * 
 * @return true 
 * @return false 
 */
bool pldotnet_SetNetLoader(
    const char *config_path,
    const char* prefix
);

/**
 * @brief 
 * 
 * @param loader 
 * @param library_path 
 * 
 * @return true 
 * @return false 
 */
bool pldotnet_SetDotNetMethods(dotnet_loader loader, const char *library_path);

/**
 * @brief 
 * 
 * @param loader 
 * @param library_path 
 * @param dotnet_type 
 * @param dotnet_type_method 
 * @param delegate_type_name 
 * @return void* 
 */
void* pldotnet_GetDotNetMethod(
    dotnet_loader loader,
    const char *library_path,
    const char *dotnet_type,
    const char *dotnet_type_method,
    const char *delegate_type_name
);

/**
 * @brief Build the config paths related do .NET.
 * 
 * @param is_csharp whether the created function uses csharp.
 * @param paths the variable that stores the config paths related to dotnet.
 */
void pldotnet_BuildPaths(bool is_csharp, pldotnet_PathConfig *paths);

/**
 * @brief Return the Datum of an specific argument. 
 * 
 * @param fcinfo the function information
 * @param index the argument index 
 * @return Datum 
 */
Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index);

/**
 * @brief Check that the datum argument is null. 
 * 
 * @param fcinfo the function information
 * @param index the argument index 
 * @return true if the referent argument is null.
 * @return false if the referent argument is non-null.  
 */
bool pldotnet_CheckNullArgument(FunctionCallInfo fcinfo, size_t index);

/**
 * @brief Retuns the Postgres Heap.
 * 
 * @param oid the function ID.
 * @return HeapTuple
 */
HeapTuple pldotnet_GetPostgresHeapTuple(Oid oid);

/**
 * @brief Calls the ReleaseSysCache function from utils/syscache.h.
 * 
 * @param proc the HeapTuple object
 */
void pldotnet_ReleasePostgresHeapTuple(HeapTuple proc);

/**
 * @brief 
 * 
 * @return pldotnet_FunctionDecl* 
 */
pldotnet_FunctionDecl* pldotnet_CreateFunctionDecl(void);

/**
 * @brief Finds the pldotnet function declared by the user through its ID.
 * 
 * @param function_id the function ID.
 * @return pldotnet_FunctionDecl*
 */
pldotnet_FunctionDecl* pldotnet_FindFunctionDecl(int function_id);

/**
 * @brief Reset the pldotnet function.
 * 
 * @param function_decl the function that will be reset
 */
void pldotnet_ResetFunctionDecl(pldotnet_FunctionDecl *function_decl);

/**
 * @brief Creates a new memory context.
 * 
 * @param config 
 */
void pldotnet_StartNewMemoryContext(MemoryContextWrapper *config);

/**
 * @brief Reverts the created memory context.
 * 
 * @param config 
 */
void pldotnet_ResetMemoryContext(MemoryContextWrapper *config);

/**
 * @brief 
 * 
 * @param function 
 * @param insert 
 */
void pldotnet_SaveFunction(pldotnet_FunctionDecl *function, bool insert);

/**
 * @brief Returns the C# or F# type that is compatible with the SQL type.
 * 
 * @param id the SQL type
 * @param hastypeconversion 
 * @param is_csharp whether the created function uses C#
 * @return const char* the compatible type in C# or F#
 */
const char * pldotnet_GetCompatibleNetTypeName(Oid id,
    bool hastypeconversion,
    bool is_csharp);

/**
 * @brief Returns the function body as a char*.
 * 
 * @param proc 
 * @param procst 
 * @return const char* the function body.
 */
const char* pldotnet_GetFunctionBody(HeapTuple proc, Form_pg_proc procst);

/**
 * @brief Returns the SQL parameters as a char*.
 * 
 * @param proc 
 * @param procst 
 * @param is_csharp 
 * @return const char* the SQL parameters.
 */
const char* pldotnet_GetSqlParamsName(HeapTuple proc,
                                  Form_pg_proc procst,
                                  bool is_csharp);

/**
 * @brief 
 * 
 * @param proc 
 * @param procst 
 * @return 
 */
const int* pldotnet_GetSqlParamsType(HeapTuple proc,
                                  Form_pg_proc procst);

/**
 * @brief 
 * 
 * @param loader 
 * @param function_id 
 * @param declaration 
 * @return true 
 * @return false 
 */
bool pldotnet_CompileUserFunction(dotnet_loader loader,
                                pldotnet_UserFunctionDeclaration *declaration);
/**
 * @brief 
 * 
 * @param loader 
 * @param paths 
 * @return user_method_delegate 
 */
user_method_delegate pldotnet_GetUserDirectMethod(dotnet_loader loader,
    pldotnet_PathConfig *paths);

/**
 * @brief 
 * 
 * @param level 
 * @param message 
 */
extern void pldotnet_Elog(int level, char *message);

/**
 * @brief 
 * 
 * @param value 
 * @param isnull 
 * @param native_result 
 */
extern void pldotnet_SetDatumResult(void* value,
    bool isnull, void *native_result);

/**
 * @brief 
 * 
 * @return void* 
 */
void* pldotnet_BuildArgumentList(FunctionCallInfo, Form_pg_proc);

/**
 * @brief 
 * 
 * @return void* 
 */
bool* pldotnet_BuildNullArgumentList(FunctionCallInfo, Form_pg_proc);

extern char *root_path;
extern char *dnldir;
// extern pldotnet_PathConfig *spi_paths;
extern dotnet_loader assembly_loader;

#endif  // PLDOTNET_COMMON_H_
