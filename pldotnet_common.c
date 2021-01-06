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
#include "pldotnet_composites.h"

/*
 * Directories where C#/F# projects for user code are built when
 * USE_DOTNETBUILD is defined. Otherwise that is where our C#/F# compiler
 * projects are located. Default for Linux is /var/lib/DotNetEngine/
 */
char *root_path = NULL;
char *dnldir = STR(PLNET_ENGINE_DIR);

static void
pldotnet_FillArgArrayInfo(
    Datum datum,
    Form_pg_type typeinfo,
    uint32_t narg,
    const char *array_template,
    bool swap_variable_decl,
    pldotnet_ArgArrayInfo *parr_info);

bool pldotnet_ValidArgsSource(const pldotnet_ArgsSource *source)
{
    if (nullptr == source)
        return false;

    if (nullptr == source->source_code)
        return false;

    if (0 > source->func_oid)
        return false;

    /* TODO
     * Verify the case when the user function returns void
     */

    return true;
}

void
pldotnet_ResetFunctionDecl(pldotnet_FunctionDecl *function_decl)
{
    if (nullptr == function_decl)
        return;
    function_decl->source.source_code = nullptr;
    function_decl->source.func_oid = 0;
    function_decl->source.result = 1;
    function_decl->args = nullptr;
    function_decl->args_length = 0;
    function_decl->ret_type = InvalidOid;
    function_decl->dotnet_method = nullptr;
}

bool
pldotnet_ValidFunctionDecl(pldotnet_FunctionDecl *function_decl)
{
    if (nullptr == function_decl)
        return false;

    if (nullptr == function_decl->args && function_decl->args_length > 0)
        return false;

    return pldotnet_ValidArgsSource(&(function_decl->source));
}

bool
pldotnet_ValidCachedFunction(
    pldotnet_FunctionDecl *reference,
    pldotnet_FunctionDecl *candidate)
{
    if (nullptr == reference || nullptr == candidate)
        return false;
    if (reference->source.func_oid != candidate->source.func_oid)
        return false;
    if (reference->ret_type != candidate->ret_type)
        return false;
    if (reference->args_length != candidate->args_length)
        return false;
    if (nullptr == reference->source.source_code || nullptr == candidate->source.source_code)
        return false;
    if (0 != strcmp(reference->source.source_code, candidate->source.source_code))
        return false;
    if (nullptr == candidate->dotnet_method)
        return false;

    return true;
}

pldotnet_FunctionDecl*
pldotnet_CreateFunctionDecl(void)
{
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

pldotnet_FunctionDecl*
pldotnet_FindFunctionDecl(int function_id)
{
    gpointer value = g_hash_table_lookup(procedures, GUINT_TO_POINTER(function_id));

    if (nullptr != value)
        return (pldotnet_FunctionDecl*) value;

    return nullptr;
}

void pldotnet_SaveFunction(
    pldotnet_FunctionDecl *function,
    bool insert)
{
    if (insert)
        g_hash_table_insert(
            procedures,
            GUINT_TO_POINTER(function->source.func_oid),
            (gpointer) function
        );
    else
        g_hash_table_replace(
            procedures,
            GUINT_TO_POINTER(function->source.func_oid),
            (gpointer) function
        );
}

void
pldotnet_InsertFunctionDecl(pldotnet_FunctionDecl *function_decl, bool insert)
{
    MemoryContext mem;
    pldotnet_FunctionDecl *decl;

    mem = CurrentMemoryContext;

    /* change the mem context to save data in hash table */
    MemoryContextSwitchTo(TopMemoryContext);

    decl = pldotnet_CopyFunctionDecl(function_decl);

    if (insert)
        g_hash_table_insert(procedures, GUINT_TO_POINTER(decl->source.func_oid), (gpointer) decl);
    else
        g_hash_table_replace(procedures, GUINT_TO_POINTER(decl->source.func_oid), (gpointer) decl);

    /* revert */
    MemoryContextSwitchTo(mem);
}

pldotnet_FunctionDecl*
pldotnet_CopyFunctionDecl(const pldotnet_FunctionDecl *function_decl)
{
    pldotnet_FunctionDecl *decl;

    decl = (pldotnet_FunctionDecl*) palloc(sizeof(pldotnet_FunctionDecl));

    decl->ret_type = function_decl->ret_type;

    decl->args_length = function_decl->args_length;

    decl->source.func_oid = function_decl->source.func_oid;
    decl->source.result = function_decl->source.result;
    decl->source.source_code = (char*) palloc(sizeof(char) * strlen(function_decl->source.source_code));
    strcpy(decl->source.source_code, function_decl->source.source_code);
    decl->dotnet_method = function_decl->dotnet_method;

    return decl;
}

void
pldotnet_SaveFunctionDecl(
    dotnet_loader loader,
    pldotnet_PathConfig *paths,
    pldotnet_FunctionDecl *function_decl)
{
    bool insert;
    pldotnet_FunctionDecl *decl;

    if (nullptr == function_decl)
        return;

    decl = pldotnet_FindFunctionDecl(function_decl->source.func_oid);

    insert = nullptr == decl;

    if (!pldotnet_ValidCachedFunction(function_decl, decl))
    {
        function_decl->dotnet_method = pldotnet_GetUserMethod(loader, paths);
        pldotnet_InsertFunctionDecl(function_decl, insert);
    }
}

/*
 * This function builds the corresponding paths given the language flag
 * The second argument should never be null, take care o that
 */
void
pldotnet_BuildPaths(bool is_csharp, pldotnet_PathConfig *paths)
{
    char prefix[MAXPGPATH];
    const char json_path_suffix[] = "/PlDotNET.runtimeconfig.json";
    const char src_path_suffix[]  = "/Lib.cs";
    const char dll_path_suffix[]  = "/PlDotNET.dll";
    char lang[] = "csharp";

    if (!is_csharp)
        lang[0] = 'f';

    SNPRINTF(paths->prefix, MAXPGPATH, "%s%s", root_path, "/src/");
    SNPRINTF(prefix, MAXPGPATH, "%s%s", paths->prefix, lang);
    SNPRINTF(paths->config_path, MAXPGPATH, "%s%s", prefix, json_path_suffix);
    SNPRINTF(paths->library_path, MAXPGPATH, "%s%s", prefix, dll_path_suffix);
    SNPRINTF(paths->src_lib_path, MAXPGPATH, "%s%s", prefix, src_path_suffix);
}

bool
pldotnet_ValidPaths(const pldotnet_PathConfig *paths)
{
    if (nullptr == paths)
    {
        elog(ERROR, "[pldotnet]:[pldotnet_ValidPaths] Argument 'paths' is null");
        return false;
    }

    /* needs better validation? */
    return 0 < strlen(paths->prefix) &&
           0 < strlen(paths->config_path) &&
           0 < strlen(paths->library_path) &&
           0 < strlen(paths->src_lib_path);
}

void
pldotnet_StartNewMemoryContext(MemoryContextWrapper *config)
{
    config->prev = CurrentMemoryContext;
    config->curr = AllocSetContextCreate(TopMemoryContext,
                                    "PL/NET func_exec_ctx",
                                    ALLOCSET_SMALL_SIZES);

    if (nullptr == config->curr)
        elog(ERROR, "Could not create a new memory context");

    MemoryContextSwitchTo(config->curr);
}

void
pldotnet_ResetMemoryContext(MemoryContextWrapper *config)
{
    if (nullptr == config) return;

    if (config->prev)
        MemoryContextSwitchTo(config->prev);

    if (config->curr)
        MemoryContextDelete(config->curr);
}

const char *
pldotnet_GetNetTypeName(Oid id, bool hastypeconversion) {
    return pldotnet_GetCompatibleNetTypeName(id, hastypeconversion, true);
}

const char *
pldotnet_GetCompatibleNetTypeName(Oid id, bool hastypeconversion, bool is_csharp)
{
    Form_pg_type typeinfo;
    HeapTuple typ;
    char * composite_nm;

    switch (id)
    {
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
            return "string"; /* System.String */
        default:
            typ = SearchSysCache(TYPEOID,
                                  ObjectIdGetDatum(id), 0, 0, 0);
            if (!HeapTupleIsValid(typ))
            {
                elog(ERROR, "[pldotnet]: cache lookup failed for type %u", id);
            }
            typeinfo = (Form_pg_type) GETSTRUCT(typ);
            if (typeinfo->typtype == TYPTYPE_COMPOSITE)
            {
                composite_nm = NameStr(typeinfo->typname);
                ReleaseSysCache(typ);
                return composite_nm;
            }
            ReleaseSysCache(typ);
    }
    return "";
}

/* Native type size in bytes */
int
pldotnet_GetTypeSize(Oid id)
{
    switch (id)
    {
        case BOOLOID:
            return sizeof(bool);
        case INT4OID:
            return sizeof(int32_t);
        case INT8OID:
            return sizeof(int64_t);
        case INT2OID:
            return sizeof(int16_t);
        case FLOAT4OID:
            return sizeof(float4);
        case FLOAT8OID:
            return sizeof(float8);
        case NUMERICOID:
            return sizeof(void*);
        case BPCHAROID:
        case TEXTOID:
        case VARCHAROID:
            return sizeof(char *);
        default:
            return pldotnet_GetCompositeTypeSize(id);
    }
    return -1;
}

const char *
pldotnet_GetUnmanagedTypeName(Oid type)
{
    switch (type)
    {
        case BOOLOID:
            return "U1";
        case INT2OID:
            return "I2";
        case INT4OID:
            return "I4";
        case INT8OID:
            return "I8";
        case FLOAT4OID:
            return "R4";
        case FLOAT8OID:
            return "R8";
        case NUMERICOID:
            return "LPStr";
        case BPCHAROID:
        case VARCHAROID:
            return "LPUTF8Str";
        case TEXTOID:
            return "LPStr";
            /*
              return "LPUTF8Str";
              review why marshal is not working
              only in TEXTOID
            */
    }
    return  "";
}

int pldotnet_SetScalarValue(
    char *argp,
    Datum datum,
    FunctionCallInfo fcinfo,
    size_t arg_index,
    Oid type,
    bool *nullp)
{
    char * newstr;
    int len;
    bool isnull = false;

#if PG_VERSION_NUM >= 120000
    if (nullp)
        isnull = fcinfo->args[arg_index].isnull;
#else
    if (nullp)
        isnull=fcinfo->argnull[arg_index];
#endif

    switch (type)
    {
        case BOOLOID:
            *(bool *)(argp) = DatumGetBool(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case INT4OID:
            *(int32_t *)(argp) = DatumGetInt32(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case INT8OID:
            *(int64_t *)(argp) = DatumGetInt64(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case INT2OID:
            *(int16_t *)(argp) = DatumGetInt16(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case FLOAT4OID:
            *(float4 *)(argp) = DatumGetFloat4(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case FLOAT8OID:
            *(float8 *)(argp) = DatumGetFloat8(datum);
            if (nullp)
                *nullp = isnull;
            break;
        case NUMERICOID:
            /* C String encoding (numeric_out) is used here as it
             is a number. Unlikely to have encoding issues. */
            *(uint64_t *)(argp) = (uint64_t)
                DatumGetCString(DirectFunctionCall1(numeric_out, datum));
            if (nullp)
                *nullp = isnull;
            break;
        case BPCHAROID:
        case TEXTOID:
        case VARCHAROID:
            if (isnull)
            {
                *(uint64_t *)(argp) = (uint64_t) nullptr;
                if (nullp)
                    *nullp = isnull;
                break;
            }
            /* UTF8 encoding */
            len = VARSIZE( DatumGetTextP (datum) ) - VARHDRSZ;
            newstr = (char *)palloc0(len+1);
            memcpy(newstr, VARDATA( DatumGetTextP(datum) ), len);
            *(uint64_t *)(argp) = (uint64_t) pg_do_encoding_conversion(
                (unsigned char *)newstr,
                len+1,
                GetDatabaseEncoding(), PG_UTF8
            );

            /*  If you need C String encoding do like this:
                *(unsigned long *)argp =
                DirectFunctionCall1(cstrfunc, DatumGetCString(datum));
                where cstrfunc is bpcharout, textout or varcharout */
            break;
    }
    return 0;
}

Datum
pldotnet_GetScalarValue(
    char * result_ptr,
    char * resultnull_ptr,
    FunctionCallInfo fcinfo,
    Oid type
)
{
    Datum retval = 0;
    VarChar * res_varchar; /* For Unicode/UTF8 support */
    char * str_num;
    char * encoded_str;
    unsigned long * ret_ptr;
    int str_len;

    fcinfo->isnull = nullptr == resultnull_ptr ? false : *(bool *) resultnull_ptr;

    switch (type)
    {
        case BOOLOID:
            /* Recover flag for null result */
            if (fcinfo->isnull)
                return (Datum) 0;
            return BoolGetDatum  ( *(bool *)(result_ptr) );
        case INT4OID:
            if (fcinfo->isnull)
                return (Datum) 0;
            return Int32GetDatum ( *(int32_t *)(result_ptr) );
        case INT8OID:
            if (fcinfo->isnull)
                return (Datum) 0;
            return Int64GetDatum ( *(int64_t *)(result_ptr) );
        case INT2OID:
            if (fcinfo->isnull)
                return (Datum) 0;
            return  Int16GetDatum ( *(int16_t *)(result_ptr) );
        case FLOAT4OID:
            return Float4GetDatum ( *(float4 *)(result_ptr) );
        case FLOAT8OID:
            return Float8GetDatum ( *(double *)(result_ptr) );
        case NUMERICOID:
            if (fcinfo->isnull)
                return (Datum) 0;
            str_num = (char *)*(unsigned long *)(result_ptr);
            return NumericGetDatum(
                                   DirectFunctionCall3(numeric_in,
                                         CStringGetDatum(str_num),
                                         ObjectIdGetDatum(InvalidOid),
                                         Int32GetDatum(-1)));
        case TEXTOID:
             /* C String encoding
              * retval = DirectFunctionCall1(textin,
              *               CStringGetDatum(
              *                       *(unsigned long *)(libargs
              *                       + dotnet_cstruct_info.typesize_params)));
              */
        case BPCHAROID:
 /* https://git.brickabode.com/DotNetInPostgreSQL/pldotnet/issues/10#note_19223
         * We should try to get atttymod which is n size in char(n)
         * and use it in bpcharin (I did not find a way to get it)
         * case BPCHAROID:
         *    retval = DirectFunctionCall1(bpcharin,
         *                           CStringGetDatum(
         *                            *(unsigned long *)(libargs
         *                  + dotnet_cstruct_info.typesize_params)), attypmod);
         */
        case VARCHAROID:
             /* C String encoding
              * retval = DirectFunctionCall1(varcharin,
              *               CStringGetDatum(
              *                       *(unsigned long *)(libargs
              *                       + dotnet_cstruct_info.typesize_params)));
              */
            /* UTF8 encoding */
            ret_ptr = *(uint64_t **)(result_ptr);
            /* str_len = pg_mbstrlen(ret_ptr); */
            str_len = strlen((char*)ret_ptr);
            encoded_str = (char *)pg_do_encoding_conversion(
            (u_char*)ret_ptr, str_len, PG_UTF8, GetDatabaseEncoding() );
            res_varchar = (VarChar *)SPI_palloc(str_len + VARHDRSZ);
#if PG_VERSION_NUM < 80300
            /* Total size of structure, not just data */
            VARATT_SIZEP(res_varchar) = str_len + VARHDRSZ;
#else
            /* Total size of structure, not just data */
            SET_VARSIZE(res_varchar, str_len + VARHDRSZ);
#endif
            memcpy(VARDATA(res_varchar), encoded_str , str_len);
            /* pfree(encoded_str); */
            PG_RETURN_VARCHAR_P(res_varchar);
    }

    return retval;
}

bool
pldotnet_TypeSupported(Oid type)
{
   return (pldotnet_IsSimpleType(type) || pldotnet_IsTextType(type)
           || TYPTYPE_COMPOSITE);
}

bool
pldotnet_IsSimpleType(Oid type)
{
    return (type == INT2OID || type == INT4OID || type == INT8OID ||
            type == FLOAT4OID || type == FLOAT8OID || type == BOOLOID);
}

bool
pldotnet_IsTextType(Oid type)
{
    /* NUMERIC appears here because it is converted to a CString type */
    return (type == TEXTOID || type == VARCHAROID ||
            type == BPCHAROID || type == NUMERICOID);
}

bool
pldotnet_IsArray(int narg, pldotnet_FuncInOutInfo * funinout_info)
{
    return (funinout_info->arrayinfo[narg].ixarray == narg);
}

inline bool
pldotnet_IsNullable(Oid type)
{
    return (type == INT2OID || type == INT4OID || type == INT8OID || type == BOOLOID);
}

bool
pldotnet_IsNullValue(FunctionCallInfo fcinfo, size_t index)
{
#if PG_VERSION_NUM > 120000
    return fcinfo->args[index].isnull;
#else
    return fcinfo->argnull[index];
#endif
}

inline Datum
pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index)
{
#if PG_VERSION_NUM >= 120000
    return fcinfo->args[index].value;
#else
    return fcinfo->arg[index];
#endif
}

bool
pldotnet_NeedsIntPtr(Oid oid)
{
    return NUMERICOID == oid || pldotnet_IsTextType(oid);
}

static void
pldotnet_FillArgArrayInfo(
    Datum datum,
    Form_pg_type typeinfo,
    uint32_t narg,
    const char *array_template,
    bool swap_variable_decl,
    pldotnet_ArgArrayInfo *parr_info)
{
    const char *typename =
        pldotnet_NeedsIntPtr(typeinfo->typelem) ?
        "IntPtr" :
        pldotnet_GetCompatibleNetTypeName(
            typeinfo->typelem,
            true,
            !swap_variable_decl
        );

    parr_info->ixarray = narg;
    parr_info->typlen = typeinfo->typlen;
    parr_info->typbyval = typeinfo->typbyval;
    parr_info->typtype = typeinfo->typtype;
    parr_info->typelem = typeinfo->typelem;
    parr_info->typalign = typeinfo->typalign;

    if (swap_variable_decl)
        sprintf(parr_info->csharpdecl,
                array_template,
                narg,
                typename
        );
    else
        sprintf(parr_info->csharpdecl,
                array_template,
                typename,
                narg
        );
}

bool
pldotnet_IsPostgresArray(Oid oid)
{
    Form_pg_type typeinfo;
    bool is_array;
    HeapTuple tuple = SearchSysCache1(TYPEOID, ObjectIdGetDatum(oid));

    if (!HeapTupleIsValid(tuple))
        elog(ERROR, "[pldotnet]: (CheckArgIsArray) cache lookup failed for type %u", oid);

    typeinfo = (Form_pg_type) GETSTRUCT(tuple);

    is_array = (typeinfo->typelem != 0 && typeinfo->typlen == -1);

    ReleaseSysCache(tuple);

    return is_array;
}

bool
pldotnet_SetArrayInfo(
    Datum datum,
    Oid oid,
    uint32_t narg,
    const char *array_template,
    bool swap_variable_decl,
    pldotnet_FuncInOutInfo *func_inout_info)
{
    Form_pg_type typeinfo;
    HeapTuple tuple;
    bool isarr = false;

    if (nullptr == array_template)
        elog(ERROR, "[pldotnet]: Invalid argument: array_template is null");

    if (nullptr == func_inout_info)
        elog(ERROR, "[pldotnet]: Invalid argument: func_inout_info is null");

    tuple = SearchSysCache1(TYPEOID, ObjectIdGetDatum(oid));

    if (!HeapTupleIsValid(tuple))
        elog(ERROR, "[pldotnet]: (CheckArgIsArray) cache lookup failed for type %u", oid);

    typeinfo = (Form_pg_type) GETSTRUCT(tuple);

    isarr = (typeinfo->typelem != 0 && typeinfo->typlen == -1);

    if (isarr)
        pldotnet_FillArgArrayInfo(
            datum,
            typeinfo,
            narg,
            array_template,
            swap_variable_decl,
            &func_inout_info->arrayinfo[narg]
        );
    else
        func_inout_info->arrayinfo[narg].ixarray = -1;

    ReleaseSysCache(tuple);

    return isarr;
}

void
pldotnet_SetArraySize(Datum datum, pldotnet_ArgArrayInfo *parr_info)
{
    ArrayType *arr = DatumGetArrayTypeP(datum);
    parr_info->ndim = ARR_NDIM(arr);
    parr_info->dims = ARR_DIMS(arr);
    parr_info->nelems = ArrayGetNItems(ARR_NDIM(arr), ARR_DIMS(arr));
}

/*
 * This function creates a buffer to hold arguments and result data.
 * The buffer is sent to C#/F#$. The current user function may read
 * this buffer to obtain the arguments and/or write any ouput data.
 * TRICKY -> IN ORDER TO SPEED UP THE EXECUTION, THE FUNCTION OID FROM PG
 * IS ALSO APPENDED IN THE BUFFER, SO OUR C#/Engine.cs CAN FIND THE COMPILED
 * DELEGATE. SEE Engine.Run() at Engine.cs;
 */
int8_t*
pldotnet_CreateCStructLibargs(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    bool force_nullable_flags,
    pldotnet_FuncInOutInfo *func_inout_info)
{
    size_t i;
    size_t default_size;
    char *array_p;
    Datum array_element;
    pldotnet_ArgArrayInfo * arrinfo;
    ArrayType *arr;
    pldotnet_ArrayT *tmp;

    /* nullable related */
    bool nullable_arg_flag = false;
    bool *argsnull_ptr;
    Datum argdatum;

    int8_t *libargs_ptr = NULL;
    int8_t *cur_arg = NULL;
    Oid *argtype = procst->proargtypes.values;
    Oid rettype = procst->prorettype;

    func_inout_info->typesize_args = 0;
    func_inout_info->typesize_nullflags = 0;

    for (i = 0; i < procst->pronargs; i++)
    {
        if (pldotnet_IsArray((int) i, func_inout_info))
            func_inout_info->typesize_args += sizeof(pldotnet_ArrayT);
        else
            func_inout_info->typesize_args += pldotnet_GetTypeSize(argtype[i]);
        if (pldotnet_IsNullable(argtype[i]))
            nullable_arg_flag = true;
    }

    if (nullable_arg_flag || force_nullable_flags)
        func_inout_info->typesize_nullflags += sizeof(bool) * procst->pronargs;
    func_inout_info->typesize_nullflags += sizeof(bool);
    func_inout_info->typesize_result = pldotnet_GetTypeSize(rettype);

    default_size = (size_t) (
                      func_inout_info->typesize_nullflags
                    + func_inout_info->typesize_args
                    + func_inout_info->typesize_result);

    libargs_ptr = (int8_t*) palloc0(default_size + sizeof(uint32_t));
    argsnull_ptr = (bool *) libargs_ptr;
    cur_arg = libargs_ptr + func_inout_info->typesize_nullflags;

    for (i = 0; i < procst->pronargs; i++)
    {
        argdatum = pldotnet_GetArgDatum(fcinfo, i);
        if (pldotnet_IsArray(i, func_inout_info))
        {
            arrinfo = &(func_inout_info->arrayinfo[i]);
            arr = DatumGetArrayTypeP(argdatum);
            array_p = ARR_DATA_PTR(arr);

            if (arrinfo->ndim > 1)
                elog(ERROR, "Multidimensional array not supported.");

            pldotnet_SetArraySize(argdatum, arrinfo);

            tmp = (pldotnet_ArrayT*) cur_arg;

            tmp->element_size = NUMERICOID == arrinfo->typelem ? sizeof(void*) : pldotnet_GetTypeSize(arrinfo->typelem);
            tmp->buffer_size = arrinfo->nelems;
            tmp->buffer = (void*) palloc0(tmp->element_size * tmp->buffer_size);

            for (int j = 0; j < arrinfo->nelems; j++)
            {
                array_element = fetch_att(array_p, arrinfo->typbyval, arrinfo->typlen);

                /* This needs to reviewed: why for bittable/simple
                    types we need to pass the value. Makes sense
                    but it seems not to be necessary/used in others pl
                    extensions. */
                if (pldotnet_IsSimpleType(arrinfo->typelem))
                    array_element = (Datum) (*(Datum *) (array_element));

                pldotnet_SetScalarValue(
                        ((char*) tmp->buffer) + j * tmp->element_size,
                        array_element,
                        fcinfo,
                        j,
                        arrinfo->typelem,
                        nullptr
                );

                /* Iterate array */
                array_p = att_addlength_pointer(array_p, arrinfo->typlen,
                                                array_p);
                array_p = (char *) att_align_nominal(array_p,
                                                           arrinfo->typalign);
            }
            /* Iterate CLibargs */
            cur_arg += sizeof(pldotnet_ArrayT);
            continue;
        }
        else if ( !pldotnet_IsSimpleType(argtype[i]) &&
                  !pldotnet_IsTextType(argtype[i]) )
            pldotnet_FillCompositeValues((char*)cur_arg, argdatum, argtype[i], fcinfo, procst);
        else
            pldotnet_SetScalarValue(
                (char *)cur_arg,
                argdatum,
                fcinfo,
                i,
                argtype[i],
                procst->pronargs + 1 == func_inout_info->typesize_nullflags ? argsnull_ptr + i : nullptr
            );

        cur_arg += pldotnet_GetTypeSize(argtype[i]);
    }

    /* append the function id after usual libargs data */
    cur_arg = libargs_ptr + default_size;
    *((uint32_t*)cur_arg) = (uint32_t) fcinfo->flinfo->fn_oid;

    return libargs_ptr;
}

Oid
pldotnet_GetTypeAttribute(TupleDesc tupdesc, HeapTupleHeader tup, size_t index)
{
    bool isnull;

    GetAttributeByNum(tup, TupleDescAttr(tupdesc, index)->attnum, &isnull);
    if (!isnull)
        return TupleDescAttr(tupdesc, index)->atttypid;
    else
        return InvalidOid;
}

/*
 * This function was moved from csharp to common,
 * given that it now works in C# and F# functions
 * This function reads the libargs buffer and retrieves data
 * F# or C#
 */
Datum
pldotnet_GetNetResult(int8_t *libargs, Oid rettype, FunctionCallInfo fcinfo, pldotnet_FuncInOutInfo *func_inout_info)
{
    int8_t *result_ptr = libargs + func_inout_info->typesize_args
                                + func_inout_info->typesize_nullflags;
    int8_t *resultnull_ptr = libargs +
                           (func_inout_info->typesize_nullflags - sizeof(bool));

    if (!pldotnet_IsSimpleType(rettype) && !pldotnet_IsTextType(rettype))
    {
        /* TODO: review null composite values */
        fcinfo->isnull = *(bool *) (resultnull_ptr);
        if (fcinfo->isnull)
            return (Datum) 0;
        return pldotnet_CreateCompositeResult((char*) result_ptr, rettype, fcinfo);
    }

    return
        pldotnet_GetScalarValue((char*) result_ptr, (char*) resultnull_ptr, fcinfo, rettype);
}

bool
pldotnet_SPIReady(void)
{
    if (SPI_connect() != SPI_OK_CONNECT)
    {
        elog(ERROR, "[pldotnet]: could not connect to SPI manager");
        return false;
    }

    return true;
}

void
pldotnet_SPIFinish(void)
{
    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
}

bool
pldotnet_TriggerNotSupported(FunctionCallInfo fcinfo)
{
    if (CALLED_AS_TRIGGER(fcinfo))
    {
        elog(ERROR, "[pldotnet]: dotnet trigger not supported");
        return true;
    }
    return false;
}

HeapTuple
pldotnet_GetPostgresHeapTuple(Oid oid)
{
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Cache lookup failed for function %u", oid);
    return proc;
}

inline void
pldotnet_ReleasePostgresHeapTuple(HeapTuple proc)
{
    ReleaseSysCache(proc);
}

bool
pldotnet_SetNetLoader(const char *config_path, const char* prefix)
{
    if (nullptr != assembly_loader)
        return true;

    assembly_loader = GetNetLoadAssemblySetup(config_path, prefix);
    return nullptr != assembly_loader;
}

component_entry_point_fn
pldotnet_GetUserMethod(dotnet_loader loader, pldotnet_PathConfig *paths)
{
    int rc;
    component_entry_point_fn dotnet_method = nullptr;

    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Run";

    rc = loader(
        paths->library_path,
        dotnet_type,
        dotnet_type_method,
        nullptr,
        nullptr,
        (void**) &dotnet_method
    );

    if (0 != rc || nullptr == dotnet_method)
        elog(ERROR, "[pldotnet]: Could not load_assembly_and_get_function_pointer()");

    return dotnet_method;
}

bool
pldotnet_Run(
    dotnet_loader loader,
    const char *dotnet_type,
    const char *dotnet_type_method,
    const pldotnet_PathConfig *paths,
    int8_t *libargs,
    size_t args_length)
{
    component_entry_point_fn dotnet_method = nullptr;

    /* Function pointer to managed delegate */
    int rc = loader(
        paths->library_path,
        dotnet_type,
        dotnet_type_method,
        nullptr,
        nullptr,
        (void**) &dotnet_method
    );

    assert(rc == 0 && dotnet_method != nullptr && \
        "Failure: load_assembly_and_get_function_pointer()");
    return 0 == dotnet_method(libargs, args_length);
}

bool
pldotnet_CompileUserFunction(
    dotnet_loader loader,
    const pldotnet_PathConfig *paths,
    pldotnet_ArgsSource *source
)
{
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Compile";

    if (nullptr == loader)
        return false;

    if (!pldotnet_ValidPaths(paths))
        return false;

    return pldotnet_Run(
        loader,
        dotnet_type,
        dotnet_type_method,
        paths,
        (int8_t*) source,
        sizeof(pldotnet_ArgsSource)
    );
}

bool
pldotnet_RunUserFunction(
    dotnet_loader loader,
    const pldotnet_PathConfig *paths,
    int8_t *libargs,
    size_t args_length)
{
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Run";

    if (nullptr == loader)
        return (Datum) 1;

    if (!pldotnet_ValidPaths(paths))
        return (Datum) 1;

    if (nullptr != libargs)
    {
        return pldotnet_Run(
            loader,
            dotnet_type,
            dotnet_type_method,
            paths,
            libargs,
            args_length
        );
    }

    return pldotnet_Run(
        loader,
        dotnet_type,
        dotnet_type_method,
        paths,
        nullptr,
        0
    );
}
