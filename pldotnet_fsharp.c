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
 * pldotnet_fsharp.c - Postgres PL handlers for F# and functions
 *
 */
#include "pldotnet_fsharp.h"
#include <mb/pg_wchar.h> /* For UTF8 support */
#include <utils/numeric.h>

PGDLLEXPORT Datum plfsharp_call_handler(PG_FUNCTION_ARGS);
Datum plfsharp_call_handler1(PG_FUNCTION_ARGS);
PGDLLEXPORT Datum plfsharp_validator(PG_FUNCTION_ARGS);
#if PG_VERSION_NUM >= 90000
PGDLLEXPORT Datum plfsharp_inline_handler(PG_FUNCTION_ARGS);
#endif

static pldotnet_FuncInOutInfo func_inout_info;

static void plfsharp_GetStructFieldPrefix(Oid type, char *field_prefix);
static char   *plfsharp_BuildBlockArgsDecl(FunctionCallInfo fcinfo, Form_pg_proc procst);
static char   *plfsharp_BuildBlockUserFuncDecl(Form_pg_proc procst,
                                               HeapTuple proc);
static char   *plfsharp_BuildBlockCallFuncCall(FunctionCallInfo fcinfo, Form_pg_proc procst);

static Datum  plfsharp_GetNetResult(int8_t * libargs, Oid rettype, FunctionCallInfo fcinfo);

inline static bool plfsharp_BuildPaths(pldotnet_PathConfig *paths);
static char* plfsharp_GetUserSourceCode(FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst);
static char* plfsharp_GetInlineSourceCode(FunctionCallInfo fcinfo);
static bool plfsharp_GetSourceCode(FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst, bool is_inline, pldotnet_ArgsSource *source);
static bool plfsharp_CreateStructLibargs(const FunctionCallInfo fcinfo, const Form_pg_proc procst, pldotnet_FunctionDecl *function_decl);
static bool plfsharp_BuildFunctionDecl(FunctionCallInfo fcinfo, bool is_inline, pldotnet_FunctionDecl *function_decl);
static Datum plfsharp_CompileAndRunUserFunction(const FunctionCallInfo fcinfo, bool is_inline);
static bool  plfsharp_TypeSupported(Oid type);

static char* plfsharp_BuildNullFlagArray(uint32_t elems);
static void plfsharp_BuildStructField(Oid type, size_t index, char *currval);
static char* plfsharp_BuildStructFields(FunctionCallInfo fcinfo, Form_pg_proc procst);

static char fs_block_header[] = "\n\
namespace PlDotNETUserSpace\n\
open System.Runtime.InteropServices\n";
/****** fs_block_args_decl ******
[<Struct>]
[<StructLayout (LayoutKind.Sequential, Pack=1)>]
type LibArgs =
    struct\n";
 *      val mutable arg1:int
 *      val mutable arg2:int
 *      ...
 *      val mutable resu:int
 */
static char fs_block_userclass_header[] = "\n\
type UserClass =\n";
/********* fs_block_userfunc_decl ******
 *         static member <function_name> =
 *             <function_body>
 */
static char fs_block_callfunc[] = "\n\
    static member CallFunction (arg:System.IntPtr) (argLength:int) = \n";

static char fs_block_footer[] = "\n\
        Marshal.StructureToPtr(libargs, arg, false)\n\
        0";

/*
 * This function should be available while
 * we dont have support for all desired types
 * In the future, C# and F# should have the same
 * capabilities and so we will use
 * pldotnet_TypeSupported intead
 */
static bool
plfsharp_TypeSupported(Oid type)
{
    return pldotnet_IsSimpleType(type) ||
           BPCHAROID == type ||
           VARCHAROID == type ||
           TEXTOID == type;
}

static void
plfsharp_GetStructFieldPrefix(Oid type, char *field_prefix)
{
    static const char unmanaged_template[] = "\
        [<MarshalAs(UnmanagedType.%s)>]\n%s";

    static const char val[] = "\
        val mutable";

    const char *unmanaged_name = pldotnet_GetUnmanagedTypeName(type);

    if (nullptr != unmanaged_name && 0 < strlen(unmanaged_name))
        snprintf(field_prefix, 1024, unmanaged_template, unmanaged_name, val);
    else
        SNPRINTF(field_prefix, 1024, "%s", val);
}

/*
 * This function aims to build a single field inside
 * a struct, including the required annotations and types.
 * For example:
 *     [<MarshalAs.Unmanaged.U1>]
 *     val mutable arg0 : bool
 *
 * @param [in] type A raw postgres type, used to fill the type name
 * @param [in] index The argument position in the procedure declared by users
 * @param [out] currval A buffer to hold the current field
 * @return Nothing
 */ 
static void
plfsharp_BuildStructField(Oid type, size_t index, char *currval)
{
    size_t length;
    char *str_ptr;
    plfsharp_GetStructFieldPrefix(type, currval);

    length = strlen(currval);
    str_ptr = currval + length;

    SNPRINTF(
        str_ptr,
        length + 1,
        " arg%lu: %s\n",
        index,
        pldotnet_GetCompatibleNetTypeName(type, true, false)
    );
}

/*
 * This functions aims to build all fields inside a struct,
 * including the required annotations and types
 * For example:
 *     [<MarshalAs.Unmanaged.U1>]
 *     val mutable arg0 : bool
 *     [<MarshalAs.Unmanaged.U1>]
 *     val mutable arg1 : bool
 *     [<MarshalAs.Unmanaged.U4>]
 *     val mutable arg2 : int
 *
 * @param [in] fcinfo Data passed to a fmgr-called function - it's from postgres
 * @param [in] procst A struct pointer containing the procedure information
 * @return a pointer to palloced string which contains the struct fields
 */
static char*
plfsharp_BuildStructFields(FunctionCallInfo fcinfo, Form_pg_proc procst)
{
    char *block2str;
    char *cursor;
    uint32_t i;
    Datum argdatum;
    bool isarr;
    Oid type;
    char currval[512];
    size_t pos = 0;
    uint32_t nargs = (uint32_t) fcinfo->nargs;
    Oid *argtype = procst->proargtypes.values;
    size_t totalsize = 0;

    static const char array_template[] = "\
        [<MarshalAs(UnmanagedType.ByValArray,ArraySubType=UnmanagedType.%s,SizeConst=%u)>]\n\
        val mutable arg%u : %s array";

    if (0 == nargs)
    {
        block2str = (char *) palloc0(1);
        SNPRINTF(block2str, 1, "%s", "\0");
        return block2str;
    }

    for (i = 0; i < nargs; ++i)
    {
        argdatum = pldotnet_GetArgDatum(fcinfo, i);
        isarr = pldotnet_SetArrayInfo( argdatum, argtype[i], i, array_template, true, &func_inout_info);

        type = isarr ? func_inout_info.arrayinfo[i].typelem : argtype[i];

        if (isarr)
            totalsize += strlen(func_inout_info.arrayinfo[i].csharpdecl) + 1;
        else
        {
            plfsharp_BuildStructField(type, i, currval);
            totalsize += strlen(currval) + 1;
        }
    }

    block2str = (char*) palloc0(totalsize);

    for (i = 0; i < nargs; ++i)
    {
        cursor = block2str + pos;
        if (pldotnet_IsArray(i, &func_inout_info))
        {
            SNPRINTF(cursor, totalsize - pos, "%s", func_inout_info.arrayinfo[i].csharpdecl);
        }
        else
        {
            plfsharp_BuildStructField(argtype[i], i, currval);
            SNPRINTF(cursor, totalsize - pos, "%s", currval);
        }
        pos += strlen(cursor);
    }

    return block2str;
}

static char*
plfsharp_BuildNullFlagArray(uint32_t elems)
{
    size_t size;
    char *null_flag_array;
    const char *type_name;
    static const char array_template[] = "\n\
        [<MarshalAs(UnmanagedType.ByValArray,ArraySubType=UnmanagedType.U1,SizeConst=%u)>]\n\
        val argsnull : %s array";

    if (0 == elems)
    {
        null_flag_array = (char*) palloc0(1);
        null_flag_array[0] = '\0';
        return null_flag_array;
    }

    type_name = pldotnet_GetCompatibleNetTypeName(BOOLOID, true, false);

    size = strlen(array_template) + strlen(type_name) + 2;

    null_flag_array = (char*) palloc0(size);

    snprintf(
        null_flag_array,
        size,
        array_template,
        elems,
        type_name
    );

    return null_flag_array;
}

static char*
plfsharp_BuildBlockArgsDecl(FunctionCallInfo fcinfo, Form_pg_proc procst)
{
    char *values;
    char *block2string;
    char *null_flag_array;
    const char *rettype_name;
    const char *rettype_unmanaged_name;
    const char *bool_name;
    size_t totalsize;
    static const char struct_template[] = "\
[<Struct>]\n\
[<StructLayout (LayoutKind.Sequential, Pack=1)>]\n\
type LibArgs =\n\
    struct\n\
        %s\n\
        [<MarshalAs(UnmanagedType.U1)>]\n\
        val mutable resunull: %s\n%s\
        [<MarshalAs(UnmanagedType.%s)>]\n\
        val mutable resu: %s\n\
    end\n";

    if (!plfsharp_TypeSupported(procst->prorettype))
        elog(ERROR, "[pldotnet]: unsupported type on return");

    bool_name = pldotnet_GetCompatibleNetTypeName(BOOLOID, true, false);

    null_flag_array = plfsharp_BuildNullFlagArray(fcinfo->nargs);

    values = plfsharp_BuildStructFields(fcinfo, procst);

    rettype_name = pldotnet_GetCompatibleNetTypeName(procst->prorettype, true, false);
    rettype_unmanaged_name = pldotnet_GetUnmanagedTypeName(procst->prorettype);

    totalsize = strlen(struct_template)
              + strlen(null_flag_array)
              + strlen(bool_name)
              + strlen(values)
              + strlen(rettype_unmanaged_name)
              + strlen(rettype_name);

    block2string = (char*) palloc0(totalsize);

    snprintf(
        block2string,
        totalsize,
        struct_template,
        null_flag_array,
        bool_name,
        values,
        rettype_unmanaged_name,
        rettype_name
    );

    return block2string;
}

static char *
plfsharp_BuildBlockUserFuncDecl(Form_pg_proc procst, HeapTuple proc)
{
    char *block2str, *str_ptr, *argnm, *source_text;
    int argnm_size, i, nnames, cursize=0, totalsize;
    bool isnull;
    char *func;
    size_t line_length;
    const char let[] = "let ";
    const char rec[] = "rec ";
    const char func_signature_indent[] = "        ";
    const char func_body_indent[] = "            ";
    const size_t body_indent_size = strlen(func_body_indent);
    char *user_line;
    const char end_fun_decl[] = " =\n";
    const char end_fun[] = "\n";
    int nargs = procst->pronargs;
    Datum *argname, argnames, prosrc;
    bool is_recursive = false;

    /* Function name */
    func = NameStr(procst->proname);

    /* Source code */
    prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    source_text = DatumGetCString(DirectFunctionCall1(textout, prosrc));

    is_recursive = pldotnet_FixFunctionName(func, source_text);

    argnames = SysCacheGetAttr(PROCOID, proc,
        Anum_pg_proc_proargnames, &isnull);

    if (!isnull)
      deconstruct_array(DatumGetArrayTypeP(argnames), TEXTOID, -1, false,
          'i', &argname, NULL, &nnames);

    /* Caculates the total amount in bytes of F# src text for 
     * the function declaration according nr of arguments 
     * and function body necessary indentation 
     */

    totalsize = strlen(func_signature_indent)
        + strlen(let) + strlen(rec) + strlen(func) + strlen(" ");

    for (i = 0; i < nargs; i++) 
    {
        argnm = DatumGetCString(DirectFunctionCall1(textout, argname[i]));

        argnm_size = strlen(argnm);
        /* +1 here is the space between type" "argname declaration */
        totalsize +=  1 + argnm_size;
    }

    user_line = source_text;

    /* tokenizes source_code into its lines for indentation insertion */
    while (*(user_line += strspn(user_line, "\n")) != '\0')
    {
        line_length = strcspn(user_line, "\n");
        totalsize += body_indent_size + line_length + 1;
        user_line += line_length;
    }

    totalsize += strlen(end_fun_decl) + strlen(end_fun) + 1;

    block2str = (char *)palloc0(totalsize);

    if (is_recursive)
    {
        SNPRINTF(block2str, totalsize - cursize, "%s%s%s%s"
            ,func_signature_indent, let, rec, func);
    }
    else
    {
        SNPRINTF(block2str, totalsize - cursize, "%s%s%s"
            ,func_signature_indent, let, func);
    }

    cursize = strlen(block2str);

    for (i = 0; i < nargs; i++)
    {
        argnm = DatumGetCString(DirectFunctionCall1(textout, argname[i]));

        argnm_size = strlen(argnm);
        str_ptr = (char *)(block2str + cursize);

        SNPRINTF(str_ptr, totalsize - cursize, " %s",argnm);
        cursize = strlen(block2str);
    }

    str_ptr = (char *)(block2str + cursize);
    SNPRINTF(str_ptr, totalsize - cursize, "%s", end_fun_decl);
    cursize = strlen(block2str);

    user_line = source_text;

    /* tokenizes source_code into its lines for indentation insertion */
    while (*(user_line += strspn(user_line, "\n")) != '\0')
    {
        line_length = strcspn(user_line, "\n");
        str_ptr = (char *)(block2str + cursize);

        SNPRINTF(str_ptr, totalsize - cursize, "%s", func_body_indent);
        str_ptr += body_indent_size;
        cursize += body_indent_size;

        for (i = 0; i < line_length; ++i)
            str_ptr[i] = user_line[i];

        str_ptr[line_length] = '\n';
        str_ptr += line_length + 1;
        cursize += line_length + 1;
        user_line += line_length;
    }

    str_ptr = (char *)(block2str + cursize);
    SNPRINTF(str_ptr, totalsize - cursize, "%s", end_fun);

    return block2str;
}

static char *
plfsharp_BuildBlockCallFuncCall(FunctionCallInfo fcinfo, Form_pg_proc procst)
{
    char *block2str, *str_ptr;
    char *func_call;
    size_t i, totalsize, call_func_size;
    size_t cursize = 0;
    char * func;
    char arg_template[] = " (wrap libargs.argsnull.[%d] libargs.arg%d)";
    size_t arg_size = strlen(arg_template);

    static const char body_template[] = "\
        let wrap (isnull: bool) a =\n\
            match isnull with\n\
            | true -> None\n\
            | _ -> Some a\n\
        let mutable libargs = Marshal.PtrToStructure<LibArgs> arg\n\
        let res =\n\
            try\n\
                %s\n\
            with\n\
                | _ -> None\n\
        libargs.resunull <- \n\
            match res with\n\
            | None -> true\n\
            | Some v ->\n\
                libargs.resu <- v\n\
                false";

    int nargs = procst->pronargs;

    /* Function name */
    func = NameStr(procst->proname);

    /* TODO:  review for nargs > 9 */
    if (nargs == 0)
    {
        int block_size = strlen(body_template) + strlen(func) + 1;
        block2str = (char *)palloc0(block_size);
        snprintf(block2str, block_size, body_template, func);
        return block2str;
    }

    call_func_size = strlen(func) + (arg_size + 10) * nargs;

    func_call = (char*) palloc0(call_func_size);

    SNPRINTF(func_call, call_func_size, "%s", func);
    cursize = strlen(func_call);

    for (i = 0; i < nargs; ++i)
    {
        str_ptr = (char*) (func_call + cursize);
        snprintf(str_ptr, call_func_size - cursize, arg_template, i, i);
        cursize += strlen(str_ptr);
    }

    totalsize = strlen(body_template) + call_func_size;

    block2str = (char *) palloc0(totalsize);

    snprintf(block2str, totalsize, body_template, func_call);

    return block2str;
}

static Datum
plfsharp_GetNetResult(int8_t *libargs, Oid rettype, FunctionCallInfo fcinfo)
{
    /* We have only Scalar values right now
     * TODO implement arrays and composite
     */
    char *args = (char*) libargs
                + func_inout_info.typesize_args
                + func_inout_info.typesize_nullflags;

    char *resnull_ptr = (char*) libargs
            + (func_inout_info.typesize_nullflags - sizeof(bool));

    return pldotnet_GetScalarValue(
        args,
        resnull_ptr,
        fcinfo,
        rettype
    );
}

inline static bool
plfsharp_BuildPaths(pldotnet_PathConfig *paths)
{
    static bool built = false;

    if (!built)
    {
        pldotnet_BuildPaths(false, paths);
        built = true;
    }

    spi_paths = paths;

    return built;
}

static char*
plfsharp_GetUserSourceCode(FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst)
{
    size_t source_code_size;
    char *fs_block_args_decl;
    char *fs_block_userfunc_decl;
    char *fs_block_callfunc_call;
    char *source_code = nullptr;

    fs_block_args_decl = plfsharp_BuildBlockArgsDecl(fcinfo, procst);
    fs_block_userfunc_decl = plfsharp_BuildBlockUserFuncDecl(procst, proc);
    fs_block_callfunc_call = plfsharp_BuildBlockCallFuncCall(fcinfo, procst);

    source_code_size = strlen(fs_block_header)
                     + strlen(fs_block_args_decl)
                     + strlen(fs_block_userclass_header)
                     + strlen(fs_block_callfunc)
                     + strlen(fs_block_userfunc_decl)
                     + strlen(fs_block_callfunc_call)
                     + strlen(fs_block_footer) + 1;

    source_code = (char*) palloc0(source_code_size);
    SNPRINTF(source_code, source_code_size, "%s%s%s%s%s%s%s",
                                            fs_block_header,
                                            fs_block_args_decl,
                                            fs_block_userclass_header,
                                            fs_block_callfunc,
                                            fs_block_userfunc_decl,
                                            fs_block_callfunc_call,
                                            fs_block_footer);
    return source_code;
}

static char*
plfsharp_GetInlineSourceCode(FunctionCallInfo fcinfo)
{
    /* TODO implement inline code handler */
    return nullptr;
}

static bool
plfsharp_GetSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool is_inline,
    pldotnet_ArgsSource *source)
{
    if (nullptr == source)
        elog(ERROR, "[pldotnet]: Invalid argument: source is null");

    if (nullptr != source->source_code)
        elog(ERROR, "[pldotnet]: ArgSouce.source code should be null at this point: \n%s", source->source_code);

    if (is_inline)
        source->source_code = plfsharp_GetInlineSourceCode(fcinfo);
    else
        source->source_code = plfsharp_GetUserSourceCode(fcinfo, proc, procst);

    return source->source_code != nullptr;
}

static bool
plfsharp_CreateStructLibargs(
    const FunctionCallInfo fcinfo,
    const Form_pg_proc procst,
    pldotnet_FunctionDecl *function_decl
)
{
    function_decl->args = pldotnet_CreateCStructLibargs(fcinfo, procst, true, &func_inout_info);
    function_decl->args_length = func_inout_info.typesize_nullflags +
                                 func_inout_info.typesize_args +
                                 func_inout_info.typesize_result;

    return nullptr != function_decl->args && function_decl->args_length > 0;
}

static bool
plfsharp_BuildFunctionDecl(
    FunctionCallInfo fcinfo,
    bool is_inline,
    pldotnet_FunctionDecl *function_decl)
{
    HeapTuple proc;
    Form_pg_proc procst;
    pldotnet_FunctionDecl *decl;
    bool result = true;

    if (nullptr == function_decl)
    {
        elog(ERROR, "[pldotnet]: Invalid argument, function decl is null");
        return false;
    }

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    if (nullptr == (proc = pldotnet_GetPostgresHeapTuple(fcinfo)))
        return false;

    procst = (Form_pg_proc) GETSTRUCT(proc);

    /* save some basic data */
    function_decl->source.func_oid = (uint32_t) fcinfo->flinfo->fn_oid;
    function_decl->ret_type = procst->prorettype;

    if (!plfsharp_GetSourceCode(fcinfo, proc, procst, is_inline, &(function_decl->source)))
        result = false;
    else if (!is_inline && !plfsharp_CreateStructLibargs(fcinfo, procst, function_decl))
    {
        result = false;
    }

    if (result)
    {
        /* Try to find the current function in the hash table
         * we still need the source code and other information to compare
         * the current function and the candidate in the hash table
         */
        decl = pldotnet_FindFunctionDecl(function_decl->source.func_oid);

        /* we need to validate the candidate (e.g. the source code may have changed) */
        if (pldotnet_ValidCachedFunction(function_decl, decl))
            function_decl->dotnet_method = decl->dotnet_method;
    }

    /* END */
    pldotnet_ReleasePostgresHeapTuple(proc);

    return result;
}

static Datum
plfsharp_CompileAndRunUserFunction(
    const FunctionCallInfo fcinfo,
    bool is_inline)
{
    pldotnet_FunctionDecl function_decl;
    static pldotnet_PathConfig paths;
    static dotnet_loader loader = nullptr;

    pldotnet_ResetFunctionDecl(&function_decl);

    if (!plfsharp_BuildPaths(&paths))
        return (Datum) 0;

    if (!plfsharp_BuildFunctionDecl(fcinfo, is_inline, &function_decl))
        return (Datum) 0;

    if (nullptr == function_decl.dotnet_method)
    {
        if (nullptr == loader && nullptr == (loader = GetNetLoadAssemblySetup(paths.config_path, paths.prefix)))
        {
            elog(ERROR, "[pldotnet]: Could not obtain .NET Loader");
            return (Datum) 0;
        }
        if (!pldotnet_CompileUserFunction(loader, fcinfo, &paths, &(function_decl.source)))
            return (Datum) 0;
        if (!pldotnet_RunUserFunction(loader, &paths, function_decl.args, function_decl.args_length))
            return (Datum) 0;

        pldotnet_SaveFunctionDecl(loader, &paths, &function_decl);
    }
    else if ((Datum) 0 != function_decl.dotnet_method(function_decl.args, function_decl.args_length)) 
        return (Datum) 0;

    return plfsharp_GetNetResult(function_decl.args, function_decl.ret_type, fcinfo);
}

Datum
plfsharp_generic_handler(PG_FUNCTION_ARGS, bool is_inline);

Datum
plfsharp_generic_handler(PG_FUNCTION_ARGS, bool is_inline)
{
    MemoryContextWrapper memory_context;
    Datum retval = 0;

    pldotnet_LoadHostFxrIfNeeded();

    if (!pldotnet_SPIReady())
        return retval;

    PG_TRY();
    {
        /* TODO we need to support trigger function */
        if (pldotnet_TriggerNotSupported(fcinfo))
        {
            pldotnet_SPIFinish();
            return retval;
        }

        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        retval = plfsharp_CompileAndRunUserFunction(fcinfo, is_inline);

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

/****** FSharp handlers ******/
PG_FUNCTION_INFO_V1(plfsharp_call_handler);
Datum plfsharp_call_handler(PG_FUNCTION_ARGS)
{
    return plfsharp_generic_handler(fcinfo, false);
}

Datum plfsharp_call_handler1(PG_FUNCTION_ARGS)
{
    bool istrigger;
    char *source_code,
         *fs_block_args_decl,
         *fs_block_userfunc_decl,
         *fs_block_callfunc_call;
    int8_t *libargs;
    int source_code_size;
    HeapTuple proc;
    Form_pg_proc procst;
    Datum retval = 0;
    Oid rettype;

    /* .NET HostFxr declarations */
    char dotnet_type[]  = "PlDotNET.UserClass, PlDotNET";
    char dotnet_type_method[64] = "CallFunction";
    FILE *output_file;
    int rc;
    load_assembly_and_get_function_pointer_fn
                                         load_assembly_and_get_function_pointer;
    component_entry_point_fn fsharp_method = nullptr;

    char *cmd;

    const char json_path_suffix[] = "/src/fsharp/PlDotNET.runtimeconfig.json";
    const char src_path_suffix[] = "/src/fsharp/Engine.fs";
    const char dll_path_suffix[] = "/src/fsharp/PlDotNET.dll";

    char fsharp_config_path[MAXPGPATH];
    char fsharp_lib_path[MAXPGPATH];
    char fsharp_srclib_path[MAXPGPATH];

    int compile_resp;

    if (SPI_connect() != SPI_OK_CONNECT)
        elog(ERROR, "[pldotnet]: could not connect to SPI manager");
    istrigger = CALLED_AS_TRIGGER(fcinfo);
    if (istrigger)
    {
        ereport(ERROR,
              (errcode(ERRCODE_FEATURE_NOT_SUPPORTED),
               errmsg("[pldotnet]: dotnet trigger not supported")));
    }
    PG_TRY();
    {
        MemoryContext oldcontext = CurrentMemoryContext;
        MemoryContext func_cxt = NULL;
        func_cxt = AllocSetContextCreate(TopMemoryContext,
                                    "PL/NET func_exec_ctx",
                                    ALLOCSET_SMALL_SIZES);
        MemoryContextSwitchTo(func_cxt);
        proc = SearchSysCache(PROCOID
            , ObjectIdGetDatum(fcinfo->flinfo->fn_oid), 0, 0, 0);
        if (!HeapTupleIsValid(proc))
            elog(ERROR, "[pldotnet]: cache lookup failed for function %u"
                , (Oid) fcinfo->flinfo->fn_oid);
        procst = (Form_pg_proc) GETSTRUCT(proc);

        /* Build the source code */
        fs_block_args_decl = plfsharp_BuildBlockArgsDecl(fcinfo, procst);
        fs_block_userfunc_decl = plfsharp_BuildBlockUserFuncDecl(procst, proc);
        fs_block_callfunc_call = plfsharp_BuildBlockCallFuncCall(fcinfo, procst);

        source_code_size = strlen(fs_block_header)
                         + strlen(fs_block_args_decl)
                         + strlen(fs_block_userclass_header)
                         + strlen(fs_block_userfunc_decl)
                         + strlen(fs_block_callfunc)
                         + strlen(fs_block_callfunc_call)
                         + strlen(fs_block_footer) + 1;

        source_code = palloc0(source_code_size);
        SNPRINTF(source_code, source_code_size, "%s%s%s%s%s%s%s",
                                                fs_block_header,
                                                fs_block_args_decl,
                                                fs_block_userclass_header,
                                                fs_block_userfunc_decl,
                                                fs_block_callfunc,
                                                fs_block_callfunc_call,
                                                fs_block_footer);
        rettype = procst->prorettype;

        ReleaseSysCache(proc);

        SNPRINTF(fsharp_srclib_path, MAXPGPATH, "%s%s", dnldir,
                                                               src_path_suffix);
        output_file = fopen(fsharp_srclib_path, "w");
        if (!output_file)
        {
            fprintf(stderr, "Cannot open file: '%s'\n", fsharp_srclib_path);
            exit(-1);
        }
        if (fputs(source_code, output_file) == EOF)
        {
            fprintf(stderr, "Cannot write to file: '%s'\n", fsharp_srclib_path);
            exit(-1);
        }
        fclose(output_file);
        setenv("DOTNET_CLI_HOME", dnldir, 1);
        cmd = palloc0(strlen("dotnet build ")
                        + strlen(dnldir) + strlen("/src/fsharp > null") + 1);
        SNPRINTF(cmd
            , strlen("dotnet build ") +
              strlen(dnldir) + 
              strlen("/src/fsharp > null") + 1
            , "dotnet build %s/src/fsharp > null", dnldir);
        compile_resp = system(cmd);
        assert(compile_resp != -1 && "Failure: Cannot compile C# source code");

        /*
         * STEP 1: Load HostFxr and get exported hosting functions
         */
        if (!pldotnet_LoadHostfxr())
            assert(0 && "Failure: pldotnet_LoadHostfxr()");

        /*
         * STEP 2: Initialize and start the .NET Core runtime
         */
        SNPRINTF(fsharp_config_path, MAXPGPATH, "%s%s", root_path,
                                                              json_path_suffix);
        load_assembly_and_get_function_pointer =
                                         GetNetLoadAssembly(fsharp_config_path);
        assert(load_assembly_and_get_function_pointer != nullptr && \
            "Failure: GetNetLoadAssembly()");

        /*
         * STEP 3: Load managed assembly and 
         *         get function pointer to a managed method
         */
        SNPRINTF(fsharp_lib_path, MAXPGPATH, "%s%s", root_path,
                                                               dll_path_suffix);

        /* Function pointer to managed delegate */
        rc = load_assembly_and_get_function_pointer(
            fsharp_lib_path,
            dotnet_type,
            dotnet_type_method,
            nullptr /* delegate_type_name */,
            nullptr,
            (void**)&fsharp_method);
        assert(rc == 0 && fsharp_method != nullptr && \
            "Failure: load_assembly_and_get_function_pointer()");

        libargs = pldotnet_CreateCStructLibargs(fcinfo, procst, true, &func_inout_info);
        fsharp_method(libargs, func_inout_info.typesize_nullflags +
                               func_inout_info.typesize_args +
                               func_inout_info.typesize_result);
        
        retval = plfsharp_GetNetResult(libargs, rettype, fcinfo);
        if (libargs != NULL)
            pfree(libargs);
        pfree(source_code);
        MemoryContextSwitchTo(oldcontext);
        if (func_cxt)
            MemoryContextDelete(func_cxt);
	}
    PG_CATCH();
    {
        /* Do the excption handling */
        elog(WARNING, "Exception");
        PG_RE_THROW();
    }
    PG_END_TRY();
    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
    return retval;
}

PG_FUNCTION_INFO_V1(plfsharp_validator);
Datum plfsharp_validator(PG_FUNCTION_ARGS)
{
    /* return DotNET_validator( additional args, PG_GETARG_OID(0)); */
    if (SPI_connect() != SPI_OK_CONNECT)
        elog(ERROR, "[pldotnet]: could not connect to SPI manager");
    PG_TRY();
    {
        /* Do some dotnet checking ?? */
    }
    PG_CATCH();
    {
        /* Do the excption handling */
        PG_RE_THROW();
    }
    PG_END_TRY();
    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
    return 0; /* VOID */
}

PG_FUNCTION_INFO_V1(plfsharp_inline_handler);
Datum plfsharp_inline_handler(PG_FUNCTION_ARGS)
{
    /*  return DotNET_inlinehandler( additional args,CODEBLOCK); */
    if (SPI_connect() != SPI_OK_CONNECT)
        elog(ERROR, "[plldotnet]: could not connect to SPI manager");

    PG_TRY();
    {
        /* Do F# inline handler here */
    }
    PG_CATCH();
    {
        /* Exception handling */
        PG_RE_THROW();
    }
    PG_END_TRY();

    if (SPI_finish() != SPI_OK_FINISH)
        elog(ERROR, "[pldotnet]: could not disconnect from SPI manager");
    PG_RETURN_VOID();
}

