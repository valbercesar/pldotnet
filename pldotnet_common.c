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
build_generic_list generic_list_constructor;
add_integer_to_generic_list add_integer_to_list;
add_small_integer_to_generic_list add_small_integer_to_list;
add_big_integer_to_generic_list add_big_integer_to_list;
add_float_to_generic_list add_float_to_list;
add_double_to_generic_list add_double_to_list;
add_boolean_to_generic_list add_boolean_to_list;

/*
 * START: implementing functions
 */

bool pldotnet_SetNetLoader(const char *config_path, const char *prefix) {
  if (nullptr != assembly_loader) return true;

  assembly_loader = GetNetLoadAssemblySetup(config_path, prefix);
  return nullptr != assembly_loader;
}

bool
pldotnet_SPIReady(void) {
    if (SPI_connect() != SPI_OK_CONNECT) {
        elog(ERROR, "[pldotnet]: could not connect to SPI manager");
        return false;
    }

    return true;
}

void
pldotnet_SPIFinish(void) {
    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
}

bool pldotnet_SetDotNetMethods(dotnet_loader loader, const char *library_path) {
    compile_user_function = (compile_user_fn) pldotnet_GetDotNetMethod(
        loader,
        library_path,
        "PlDotNET.Engine, PlDotNET",
        "CompileUserFunction",
        "PlDotNET.Engine+DelCompileUserFunction, PlDotNET");
    // TODO(rosicley) -- add the others methods here!

    generic_list_constructor = (build_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "BuildGenericList",
        "PlDotNET.ExperimentalBridge+DelBuildGenericList, PlDotNET");

    add_integer_to_list =
    (add_integer_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddIntegerToList",
        "PlDotNET.ExperimentalBridge+DelAddIntegerToList, PlDotNET");

    add_small_integer_to_list =
    (add_small_integer_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddSmallIntegerToList",
        "PlDotNET.ExperimentalBridge+DelAddSmallIntegerToList, PlDotNET");

    add_big_integer_to_list =
    (add_big_integer_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddBigIntegerToList",
        "PlDotNET.ExperimentalBridge+DelAddBigIntegerToList, PlDotNET");

    add_float_to_list =
    (add_float_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddFloatToList",
        "PlDotNET.ExperimentalBridge+DelAddFloatToList, PlDotNET");

    add_double_to_list =
    (add_double_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddDoubleToList",
        "PlDotNET.ExperimentalBridge+DelAddDoubleToList, PlDotNET");

    add_boolean_to_list =
    (add_boolean_to_generic_list) pldotnet_GetDotNetMethod(
        assembly_loader,
        library_path,
        "PlDotNET.ExperimentalBridge, PlDotNET",
        "AddBooleanToList",
        "PlDotNET.ExperimentalBridge+DelAddBooleanToList, PlDotNET");

    return nullptr != compile_user_function
    && nullptr != add_small_integer_to_list
    && nullptr != generic_list_constructor
    && nullptr != add_integer_to_list
    && nullptr != add_big_integer_to_list
    && nullptr != add_float_to_list
    && nullptr != add_double_to_list;
}

void *pldotnet_GetDotNetMethod(dotnet_loader loader,
                               const char *library_path,
                               const char *dotnet_type,
                               const char *dotnet_type_method,
                               const char *delegate_type_name) {
  int rc;
  void *dotnet_method = nullptr;

  elog(INFO, "type: %s, method: %s", dotnet_type, dotnet_type_method);
  elog(INFO, "delegate type name: %s", delegate_type_name);

  rc = loader(library_path, dotnet_type, dotnet_type_method, delegate_type_name,
              nullptr, (void **)&dotnet_method);

  if (nullptr == dotnet_method)
      elog(ERROR, "[pldotnet]: Could not get_function_pointer()");

  if (0 != rc)
      elog(ERROR, "[pldotnet]: Could not "
        "load_assembly_and_get_function_pointer()");

  return dotnet_method;
}

void pldotnet_BuildPaths(bool is_csharp, pldotnet_PathConfig *paths) {
    char prefix[MAXPGPATH];
    const char json_path_suffix[] = "/bin/Release/net6.0/PlDotNET."
        "runtimeconfig.json";
    const char src_path_suffix[]  = "/Lib.cs";
    const char dll_path_suffix[]  = "/bin/Release/net6.0/PlDotNET.dll";
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

pldotnet_FunctionDecl* pldotnet_CreateFunctionDecl(void) {
    pldotnet_FunctionDecl *decl;
    MemoryContext mem = CurrentMemoryContext;

    /* change to top mem context */
    MemoryContextSwitchTo(TopMemoryContext);

    decl = (pldotnet_FunctionDecl*) palloc(sizeof(pldotnet_FunctionDecl));

    pldotnet_ResetFunctionDecl(decl);

    /* revert to previous mem context */
    MemoryContextSwitchTo(mem);

    return decl;
}

pldotnet_FunctionDecl* pldotnet_FindFunctionDecl(int function_id) {
    gpointer value = g_hash_table_lookup(procedures,
        GUINT_TO_POINTER(function_id));

    if (nullptr != value)
        return (pldotnet_FunctionDecl*) value;

    return nullptr;
}

void pldotnet_ResetFunctionDecl(pldotnet_FunctionDecl *function_decl) {
    if (nullptr == function_decl)
        return;
    function_decl->source.source_code = nullptr;
    function_decl->source.func_oid = 0;
    function_decl->source.result = 1;
    function_decl->args = nullptr;
    function_decl->args_length = 0;
    function_decl->ret_type = InvalidOid;
    function_decl->dotnet_method = nullptr;
    function_decl->call_user_method = nullptr;

    /* this is the new user declaration */
    function_decl->user_decl.language = nullptr;
    function_decl->user_decl.func_name = nullptr;
    function_decl->user_decl.func_rettype = nullptr;
    function_decl->user_decl.func_params = nullptr;
    function_decl->user_decl.func_body = nullptr;
}

void pldotnet_StartNewMemoryContext(MemoryContextWrapper *config) {
    config->prev = CurrentMemoryContext;
    config->curr = AllocSetContextCreate(TopMemoryContext,
                                    "PL/NET func_exec_ctx",
                                    ALLOCSET_SMALL_SIZES);

    if (nullptr == config->curr)
        elog(ERROR, "Could not create a new memory context");

    MemoryContextSwitchTo(config->curr);
}

void pldotnet_ResetMemoryContext(MemoryContextWrapper *config) {
    if (nullptr == config) return;

    if (config->prev)
        MemoryContextSwitchTo(config->prev);

    if (config->curr)
        MemoryContextDelete(config->curr);
}

void pldotnet_SaveFunction(pldotnet_FunctionDecl *function, bool insert) {
    if (insert)
        g_hash_table_insert(
            procedures,
            GUINT_TO_POINTER(function->source.func_oid),
            (gpointer) function);
    else
        g_hash_table_replace(
            procedures,
            GUINT_TO_POINTER(function->source.func_oid),
            (gpointer) function);
}


const char* pldotnet_GetCompatibleNetTypeName(Oid id,
                                              bool hastypeconversion,
                                              bool is_csharp) {
    Form_pg_type typeinfo;
    HeapTuple typ;
    char * composite_nm;

    switch (id) {
        case BOOLOID:
            return "bool";   /* System.Boolean */
        case INT4OID:
            return "int";    /* System.Int32 */
        case INT8OID:
            return is_csharp ? "long" : "int64";   /* System.Int64 */
        case INT2OID:
            return is_csharp ? "short" : "int16";  /* System.Int16 */
        case FLOAT4OID:
            return is_csharp ? "float" : "float32";  /* System.Single */
        case FLOAT8OID:
            return "double"; /* System.Double */
        case NUMERICOID:     /* System.Decimal */
            return hastypeconversion ? "string" : "decimal";
        case BPCHAROID:
        case TEXTOID:
        case VARCHAROID:
        case TRIGGEROID:
            return "string"; /* System.String */
        case POINTOID:
            return "NpgsqlPoint";
        case POLYGONOID:
            return "NpgsqlPolygon";
        default:
            typ = SearchSysCache(TYPEOID,
                                  ObjectIdGetDatum(id), 0, 0, 0);
            if (!HeapTupleIsValid(typ)) {
                elog(ERROR, "[pldotnet]: cache lookup failed for type %u", id);
            }
            typeinfo = (Form_pg_type) GETSTRUCT(typ);
            if (typeinfo->typtype == TYPTYPE_COMPOSITE) {
                composite_nm = NameStr(typeinfo->typname);
                ReleaseSysCache(typ);
                return composite_nm;
            }
            ReleaseSysCache(typ);
    }
    return "";
}

const char* pldotnet_GetFunctionBody(HeapTuple proc, Form_pg_proc procst) {
    bool isnull = false;
    Datum prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    const char *body = DatumGetCString(DirectFunctionCall1(textout, prosrc));
    return body;
}

const char* pldotnet_GetSqlParams(HeapTuple proc,
                                  Form_pg_proc procst,
                                  bool is_csharp) {
    int nnames = 0;
    bool isnull = false;
    const char **argnames_array = nullptr;
    Datum *argnames = nullptr;
    char *sql_params = nullptr;
    Datum argname = SysCacheGetAttr(PROCOID,
        proc,
        Anum_pg_proc_proargnames,
        &isnull);
    Oid *argtypes = procst->proargtypes.values;
    size_t buffer_size = 0;

    if (!isnull)
        deconstruct_array(DatumGetArrayTypeP(argname),
        TEXTOID,
        -1,
        false,
        'i',
        &argnames,
        NULL,
        &nnames);
    else
        return nullptr;

    argnames_array = (const char**)
        palloc0(sizeof(char*) * procst->pronargs * 2);

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        /* get the arg name */
        const char *name = DatumGetCString(DirectFunctionCall1(textout,
            argnames[i]));

        /* get the arg type name without the type converstion */
        const char *type = pldotnet_GetCompatibleNetTypeName(argtypes[i],
            false,
            is_csharp);

        argnames_array[i * 2] = name;
        argnames_array[i * 2 + 1] = type;

        buffer_size += strlen(name) + strlen(type) + 3;
    }

    sql_params = (char*) palloc0(buffer_size);

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        /* copy the arg name */
        strcat(sql_params, argnames_array[i * 2]);
            strcat(sql_params, " ");

        /* copy the arg type name */
        strcat(sql_params, argnames_array[i * 2 + 1]);
        if (i < procst->pronargs - 1)
            strcat(sql_params, ",");
    }
    return sql_params;
}

bool pldotnet_CompileUserFunction(dotnet_loader loader,
    uint32_t function_id,
    pldotnet_UserFunctionDeclaration *declaration) {
    int test = compile_user_function(
        function_id,
        (void*) declaration->func_name,
        (void*) declaration->func_rettype,
        (void*) declaration->func_params,
        (void*) declaration->func_body);
    return 0 == test;
}

user_method_delegate pldotnet_GetUserDirectMethod(dotnet_loader loader,
                                                  pldotnet_PathConfig *paths) {
    elog(INFO, "library: %s", paths->library_path);
    return (user_method_delegate) pldotnet_GetDotNetMethod(
        loader,
        paths->library_path,
        "PlDotNET.Engine, PlDotNET",
        "RunUserFunction",
        "PlDotNET.Engine+DelRunUserFunction, PlDotNET");
}

void* pldotnet_BuildArgumentList(FunctionCallInfo fcinfo,
 Form_pg_proc procst) {
    void *argument;
    void *list = generic_list_constructor();

    for (int16_t i = 0; i < procst->pronargs; ++i) {
        Datum argdatum = pldotnet_GetArgDatum(fcinfo, i);

        switch (procst->proargtypes.values[i]) {
            case INT4OID:
                add_integer_to_list(list, DatumGetInt32(argdatum));
                break;
            case INT2OID:
                add_small_integer_to_list(list, DatumGetInt16(argdatum));
                break;
            case INT8OID:
                add_big_integer_to_list(list, DatumGetInt64(argdatum));
                break;
            case FLOAT4OID:
                add_float_to_list(list, DatumGetFloat4(argdatum));
                break;
            case FLOAT8OID:
                add_double_to_list(list, DatumGetFloat8(argdatum));
                break;
            case BOOLOID:
                add_boolean_to_list(list, DatumGetBool(argdatum));
                break;
            // case POINTOID:
            // {
            //     Point *point;
            //     point = DatumGetPointP(argdatum);
            //     argument = point_constructor(point->x, point->y);
            //     add_element_to_list(list, argument);
            //     break;
            // }
            default:
                elog(ERROR, "[pldotnet]: Unsupported argument type. "
                    "Check the pldotnet_BuildArgumentList function. "
                    "The defined type is %d\n", procst->proargtypes.values[i]);
                break;
        }
    }

    return list;
}

void pldotnet_Elog(int level, char *message) {
    elog(level, "%s", message);
}

inline Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index) {
#if PG_VERSION_NUM >= 120000
    return fcinfo->args[index].value;
#else
    return fcinfo->arg[index];
#endif
}

/* The following functions should be called from .NET */
/* It is used to set the result from the user function */

void pldotnet_SetInt32Result(int32_t value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetInt32Result: %d", value);
    result->is_null = isnull;
    result->value = Int32GetDatum(value);
}

void pldotnet_SetInt16Result(int16_t value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetInt16Result: %d", value);
    result->is_null = isnull;
    result->value = Int16GetDatum(value);
}

void pldotnet_SetInt64Result(int64_t value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetInt64Result: %ld", (long)value);
    result->is_null = isnull;
    result->value = Int64GetDatum(value);
}

void pldotnet_SetFloatResult(float value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetFloatResult: %f", value);
    result->is_null = isnull;
    result->value = Float4GetDatum(value);
}

void pldotnet_SetDoubleResult(double value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetDoubleResult: %f", value);
    result->is_null = isnull;
    result->value = Float8GetDatum(value);
}

void pldotnet_SetBooleanResult(bool value, bool isnull, void *native_result) {
    pldotnet_Result *result = (pldotnet_Result*) native_result;

    elog(INFO, "SetBooleanResult: %s", value ? "true" : "false");
    result->is_null = isnull;
    result->value = BoolGetDatum(value);
}
/*
 * END: implementing functions
 */
