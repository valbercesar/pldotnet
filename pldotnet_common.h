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
#include <utils/fmgrprotos.h>
#include <access/htup_details.h>
#include <utils/array.h>

extern PGDLLIMPORT bool check_function_bodies;

extern GHashTable *procedures;

extern char *root_path;
extern char *dnldir;

extern dotnet_loader assembly_loader;

/* As a reminder snprintf is defined as pg_snprintf.  TODO - CHECK HERE
 * Check port.h into postgres codebase
 */
#define SNPRINTF(dst, size, fmt, ...)                                      \
    if (snprintf(dst, size, fmt, __VA_ARGS__) >= size) {                   \
        elog(ERROR, "[pldotnet] (%s:%d) String too long for buffer: " fmt, \
             __FILE__, __LINE__, __VA_ARGS__);                             \
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
    pldotnet_UserFunctionDeclaration user_decl;
    user_method_delegate call_user_method;
} pldotnet_FunctionDecl;

/**
 * @brief Connects to SPI manager.
 *
 * @return true if the connection was successful.
 * @return false if the connection was not successful.
 */
bool pldotnet_SPIReady(void);

/**
 * @brief Disconnect from SPI manager.
 *
 */
void pldotnet_SPIFinish(void);

/**
 * @brief Sets the assembly_loader object.
 *
 * @param config_path
 * @param prefix
 *
 * @return true if the assembly_loader was already defined or was found
 * correctly.
 * @return false if the assembly_loader was not found.
 */
bool pldotnet_SetNetLoader(const char *config_path, const char *prefix);

/**
 * @brief Sets the .NET methods for the C function pointers.
 *
 * @param library_path The .NET library.
 *
 * @return true if all the .NET functions were found.
 * @return false if any .NET functions were not found.
 */
bool pldotnet_SetDotNetMethods(const char *library_path);

/**
 * @brief Find and return a specified .NET method.
 *
 * @param library_path The .NET project path.
 * @param dotnet_type The .NET type (the namespace and class).
 * @param dotnet_type_method The .NET function name.
 * @param delegate_type_name The .NET delegate function name.
 * @return void* the C function pointer that points to the specified .NET method
 */
void *pldotnet_GetDotNetMethod(const char *library_path,
                               const char *dotnet_type,
                               const char *dotnet_type_method,
                               const char *delegate_type_name);

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
pldotnet_FunctionDecl *pldotnet_CreateFunctionDecl(void);

/**
 * @brief Finds the pldotnet function declared by the user through its ID.
 *
 * @param function_id the function ID.
 * @return pldotnet_FunctionDecl*
 */
pldotnet_FunctionDecl *pldotnet_FindFunctionDecl(int function_id);

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
 * @brief Saves or replaces the user function in the hash table.
 *
 * @param function the object that contains the user function information
 * @param insert whether the function should be inserted. On the other hand,
 * it will be replaced.
 */
void pldotnet_SaveFunction(pldotnet_FunctionDecl *function, bool insert);

/**
 * @brief Returns the function body as a char*.
 *
 * @param proc
 * @param procst
 * @return const char* the function body.
 */
const char *pldotnet_GetFunctionBody(HeapTuple proc, Form_pg_proc procst);

/**
 * @brief Returns the SQL parameter names as a char*.
 *
 * @param proc the HeapTuple object.
 * @param procst the Form_pg_proc object.
 * @param is_csharp whether the user functions uses C#.
 * @return const char* the SQL parameters.
 */
const char *pldotnet_GetSqlParamsName(HeapTuple proc, Form_pg_proc procst,
                                      bool is_csharp);

/**
 * @brief Returns the OIDs of the SQL function as an int*.
 *
 * @param proc the HeapTuple object.
 * @param procst the Form_pg_proc object.
 * @return int* that points to an array with the OID of the arguments.
 */
const int *pldotnet_GetSqlParamsType(HeapTuple proc, Form_pg_proc procst);

/**
 * @brief Calls the CompileUserFunction function from the .NET environment.
 * Thus, this function will generate and dynamically compile the user code,
 * which is defined through the "declaration" argument.
 *
 * @param declaration The object that contains the user's function information.
 * @return true if the process successful.
 */
bool pldotnet_CompileUserFunction(
    pldotnet_UserFunctionDeclaration *declaration);

/**
 * @brief Calls the "pldotnet_GetDotNetMethod" function to return the function
 * pointer that points to the "RunUserFunction" function, which is a .NET
 * function and runs the user function.
 *
 * @param paths The .NET config paths
 * @return user_method_delegate The function pointer that points to the
 * .NET RunUserFunction function.
 */
user_method_delegate pldotnet_GetUserDirectMethod(pldotnet_PathConfig *paths);

/**
 * @brief Calls the "elog" function to report a message os PostgreSQL.
 *
 * @param level The message level. For example, INFO, ERROR, WARNING...
 * @param message The message that will be reported.
 */
extern void pldotnet_Elog(int level, char *message);

/**
 * @brief This functions is called from the dynamic C# code to set the result
 * Datum of the user function.
 *
 * @param value a Datum object.
 * @param isnull whether the Datum is null.
 * @param native_result a "pldotnet_Result" object created on
 * "plcsharp_CompileAndRunUserFunction" and contains the Datum that will be
 * returned.
 */
extern void pldotnet_SetDatumResult(void *value, bool isnull,
                                    void *native_result);

/**
 * @brief Creates a list of IntPtr, adds the user arguments in this list and
 * returns the created list.
 *
 * @param fcinfo the FunctionCallInfo object.
 * @param procst the Form_pg_proc object.
 * @return void* the List<IntPtr> that contains the Datums.
 */
void *pldotnet_BuildArgumentList(FunctionCallInfo fcinfo, Form_pg_proc procst);

/**
 * @brief Maps the NULL Datums in the user argument list.
 *
 * @return bool* the pointer that points to the nullmap of user arguments.
 */
bool *pldotnet_BuildNullArgumentList(FunctionCallInfo fcinfo,
                                     Form_pg_proc procst);

/**
 * @brief Calls the "FreeGenericGCHandle" function to free a GCHandle object
 * previous allocated in the .NET environment.
 *
 * @param gchandle the GCHandle object that will be free on .NET.
 */
void pldotnet_FreeGCHandle(void *gchandle);

/**
 * @brief Calls the "UnloadAssemblies" function to unload the assemblies related
 * to the specified function.
 *
 * @param functionId the function ID.
 */
void pldotnet_UnloadAssemblies(int functionId);

#endif  // PLDOTNET_COMMON_H_
