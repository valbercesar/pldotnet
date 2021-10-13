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
#include "pldotnet_hostfxr.h" /* needed for pldotnet_LoadHostfxr() */
#include "pldotnet_composites.h"
#include <math.h>

/* Declare extension variables/structs here */
PGDLLEXPORT Datum plcsharp_call_handler(PG_FUNCTION_ARGS);
Datum plcsharp_call_handler1(PG_FUNCTION_ARGS);
PGDLLEXPORT Datum plcsharp_validator(PG_FUNCTION_ARGS);
#if PG_VERSION_NUM >= 90000
PGDLLEXPORT Datum plcsharp_inline_handler(PG_FUNCTION_ARGS);
#endif

static int
plcsharp_BuildBlockCompositesFromProcedure(
    char * composite_decls,
    FunctionCallInfo fcinfo,
    Form_pg_proc procst
);


static int plcsharp_BuildBlockComposites(char * composites_decl,
                                         FunctionCallInfo fcinfo,
                                         Form_pg_proc procst);
static const char*
plcsharp_GetTriggerDataDefinition(void);

static char*
plcsharp_BuildTriggerDataArgs(FunctionCallInfo fcinfo);

static char *plcsharp_BuildBlockArgsDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

const char*
plchsarp_GetTriggerArgs(FunctionCallInfo fcinfo);

static char*
plcsharp_BuildBlockTriggerFuncCall(FunctionCallInfo fcinfo, Form_pg_proc procst);

static char  *plcsharp_BuildBlockCallFuncCall(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
);

static const char *plcsharp_BuildBlockUserFuncDecl(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    HeapTuple proc,
    pldotnet_FuncInOutInfo *func_inout_info
);

static int   GetSizeNullableHeader(int argnm_size, Oid arg_type, int narg);
static int   GetSizeNullableFooter(Oid ret_type);
static int   GetSizeArgsNullArray(int nargs);
static int   pldotnet_PublicDeclSize(Oid type);
static const char* pldotnet_GetNullableTypeName(Oid id);

bool pldotnet_CheckArgIsArray(
    Datum datum,
    Oid oid,
    int narg,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

inline static bool plcsharp_BuildPaths(pldotnet_PathConfig *paths);
static bool plcsharp_CreateStructLibargs( const FunctionCallInfo fcinfo, const Form_pg_proc procst, pldotnet_FunctionDecl *function_decl);
static char* plcsharp_GetInlineSourceCode(FunctionCallInfo fcinfo);

static char* plcsharp_GetUserSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

static bool plcsharp_GetSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl
);

static pldotnet_FunctionDecl*
plcsharp_GetFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation
);

static bool plcsharp_BuildFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl
);

static void
plcsharp_ValidateUserFunction(const Oid oid, const FunctionCallInfo fcinfo);
static Datum plcsharp_CompileAndRunUserFunction(const FunctionCallInfo fcinfo, bool is_inline);

static Datum plcsharp_generic_handler(FunctionCallInfo fcinfo, bool is_inline);

dotnet_loader load_assembly_and_get_function_pointer;

bool hostfxr_loaded = false;
bool paths_defined = false;
pldotnet_PathConfig paths;

#if PG_VERSION_NUM >= 90000
#define CODEBLOCK \
  ((InlineCodeBlock *) DatumGetPointer(PG_GETARG_DATUM(0)))->source_text

const char public_bool[] = "\n[MarshalAs(UnmanagedType.U1)]\npublic ";
const char public_string_utf8[] = "\n[MarshalAs(UnmanagedType.LPUTF8Str)]public ";
const char public_struct[] = "\n[MarshalAs(UnmanagedType.Struct)]public ";
const char public_[] = "\npublic ";
/* nullable related constants */
const char resu_nullable_value[] = "libargs.resu = resu_nullable.GetValueOrDefault();\n";
const char resu_nullable_flag[] = "libargs.resunull = !resu_nullable.HasValue;\n";
const char resu_null_flag[] = "libargs.resunull = null == libargs.resu;\n";
const char argsnull_str[] = "libargs.argsnull";
const char nullable_suffix[] = "_nullable";
const char resu_flag_str[] = "bool resunull;";
const char arg_flag_str[] = "bool[] argsnull;";

/* C# CODE TEMPLATE */
static char cs_block_header[] = "           \n\
using System;                               \n\
using System.Linq;                          \n\
using System.Data.SqlClient;                \n\
using System.Data.Common;                   \n\
using System.Data;                          \n\
using System.Collections.Generic;           \n\
using System.Runtime.InteropServices;       \n\
using System.Globalization;                 \n\
using System.Diagnostics.CodeAnalysis;      \n\
namespace PlDotNETUserSpace                 \n\
{                                           \n\
    enum TypeOid                            \n\
    {                                       \n\
       BOOLOID    = 16,                     \n\
       INT8OID    = 20,                     \n\
       INT2OID    = 21,                     \n\
       INT4OID    = 23,                     \n\
       FLOAT4OID  = 700,                    \n\
       FLOAT8OID  = 701,                    \n\
       VARCHAROID = 1043,                   \n\
       NUMERICOID = 1700,                   \n\
    }                                       \n\
    public static class UserClass           \n\
    {                                       \n\
        static public Action<string> pldotnet_Info;\n\
        static public Action<string> pldotnet_Warning;\n\
        static public void SetInfo(Action<string> Info)\n\
        {\n\
            pldotnet_Info = Info;\n\
        }\n\
        static public void SetWarning(Action<string> Warning)\n\
        {\n\
            pldotnet_Warning = Warning;\n\
        }\n\
        [StructLayout(LayoutKind.Sequential, Pack=1)]\n\
        public struct ArrayT<T>\n\
        {\n\
            public IntPtr Buffer;\n\
            public uint ElementSize;\n\
            public uint BufferSize;\n\
        }\n\
        static decimal[] ArrayToDecimal(ArrayT<IntPtr> array)\n\
        {\n\
            var strings = UserClass.ArrayToString(array);\n\
            if (\"en-US\" != System.Globalization.CultureInfo.CurrentCulture.Name)\n\
            {\n\
                System.Globalization.CultureInfo.CurrentCulture = new CultureInfo(\"en-US\", false);\n\
            }\n\
            return Array.ConvertAll<string, decimal>(strings, Convert.ToDecimal);\n\
        }\n\
        static string[] ArrayToString(ArrayT<IntPtr> array)\n\
        {\n\
            var elementSize = Marshal.SizeOf(typeof(IntPtr));\n\
            string[] strs = new string[array.BufferSize];\n\
            for (int i = 0; i < array.BufferSize; ++i)\n\
            {\n\
                strs[i] = Marshal.PtrToStringUTF8(\n\
                    Marshal.ReadIntPtr(\n\
                        array.Buffer,\n\
                        i * elementSize\n\
                    )\n\
                );\n\
            }\n\
            return strs;\n\
        }\n\
        static SPI.BpgsqlDbRecord ArrayToDbRecord(ArrayT<SPI.PropertyValue> array)\n\
        {\n\
            if (1 > array.BufferSize)\n\
                return null;\n\
            var dbRecord = new SPI.BpgsqlDbRecord();\n\
            for (int i = 0; i < array.BufferSize; ++i)\n\
            {\n\
                var cursor = IntPtr.Add(array.Buffer, ((int) array.ElementSize) * i);\n\
                var propertyValue = Marshal.PtrToStructure<SPI.PropertyValue>(cursor);\n\
                dbRecord.AddProperty(propertyValue);\n\
            }\n\
            return dbRecord;\n\
        }\n\
        static T[] fromArrayT<T>(ArrayT<T> array)\n\
        {\n\
            var size = (int) (array.BufferSize * array.ElementSize);\n\
            var input = new T[array.BufferSize];\n\
            var bytes = new Byte[size];\n\
            Marshal.Copy(array.Buffer, bytes, 0, size);\n\
            System.Buffer.BlockCopy(bytes, 0, input, 0, size);\n\
            return input;\n\
        }\n\
        static ArrayT<T> toArrayT<T>(T[] output, uint ElementSize)\n\
         {\n\
            int size = (int) (output.Length * ElementSize);\n\
            var array = new ArrayT<T>();\n\
            array.ElementSize = ElementSize;\n\
            array.BufferSize = (uint) output.Length;\n\
            array.Buffer = Marshal.AllocHGlobal(size);\n\
            var bytes = new byte[size];\n\
            System.Buffer.BlockCopy(output, 0, bytes, 0, size);\n\
            Marshal.Copy(bytes, 0, array.Buffer, size);\n\
            return array;\n\
        }\n";

/********** cs_block_composites *********
 *   [StructLayout(LayoutKind.Sequential,Pack=1)]
 *   public struct CompositeName1;
 *   {
 *       public fielType1 fieldName1;
 *       public fielType2 fieldName2;
 *       ...
 *       public fielTypeN fieldNameN;
 *   }
 *
 *   [StructLayout(LayoutKind.Sequential,Pack=1)]
 *   public struct CompositeNameN;
 *    { ... }
 *
 */
static char cs_block_args_header[] = "\
        [StructLayout(LayoutKind.Sequential,Pack=1)]\n\
        public struct LibArgs               \n\
        {";
/*********** cs_block_args_decl **********
 *          public argType1 argname1;
 *          public argType2 argname2;
 *           ...
 *	        public returnT resu;
 */
static char cs_block_callfunc_header[] = "             \n\
        }\n\
        public static int CallFunction(IntPtr arg, int argLength)\n\
        {                                    \n\
            if (argLength != System.Runtime.InteropServices.Marshal.SizeOf(typeof(LibArgs)))\n\
                return 1;                    \n\
            LibArgs libargs = Marshal.PtrToStructure<LibArgs>(arg);\n";
/*********** cs_block_callfunc_call **********
 *          libargs.resu = FUNC(libargs.argname1, libargs.argname2, ...);
 */

/*********** cs_block_userfunc_decl **********
 *          returnT FUNC(argType1 argname1, argType2 argname2, ...)
 *          {
 *               What is in the SQL function code here
 *          }
 */
static char cs_block_footer[] = "              \n\
            Marshal.StructureToPtr<LibArgs>(libargs, arg, false);\n\
            return 0;                         \n\
        }                                     \n\
    }                                         \n\
}";


static char block_inline_header[] = "             \n\
using System;                               \n\
using System.Runtime.InteropServices;       \n\
namespace PlDotNETUserSpace                 \n\
{                                           \n\
    public static class UserClass           \n\
    {";
static char block_inline_callfunc[] = "             \n\
        public static int CallFunction(IntPtr arg, int argLength)\n\
        {";
/* block_inline_usercode  Function body */
static char block_inline_footer[] = "             \n\
	        return 0;\n\
	    }\n\
    }\n\
}";

static int
pldotnet_PublicDeclSize(Oid type)
{
    Oid id;
    Form_pg_type typeinfo;
    HeapTuple typ;

    switch (type)
    {
        case INT4OID:
        case INT8OID:
        case INT2OID:
        case FLOAT4OID:
        case FLOAT8OID:
        case NUMERICOID:
            return strlen(public_);
        case BOOLOID:
            return strlen(public_bool);
        case BPCHAROID:
        case VARCHAROID:
        case TRIGGEROID:
        case TEXTOID:
            return strlen(public_string_utf8);
        default:
            typ = SearchSysCache(TYPEOID,
                                  ObjectIdGetDatum(type), 0, 0, 0);
            if (!HeapTupleIsValid(typ))
            {
                elog(ERROR, "[pldotnet]: cache lookup failed for type %u",

        type);
            }
            typeinfo = (Form_pg_type) GETSTRUCT(typ);
            id = typeinfo->typtype;
            ReleaseSysCache(typ);
            if (id == TYPTYPE_COMPOSITE)
                return strlen(public_struct);
    }
    return  0;
}

char *
pldotnet_PublicDecl(Oid type)
{
    Oid id;
    Form_pg_type typeinfo;
    HeapTuple typ;

    switch (type)
    {
        case INT4OID:
        case INT8OID:
        case INT2OID:
        case FLOAT4OID:
        case FLOAT8OID:
        case NUMERICOID:
            return (char *)&public_;
        case BOOLOID:
            return (char *)&public_bool;
        case BPCHAROID:
        case VARCHAROID:
        case TRIGGEROID:
        case TEXTOID:
            return (char *)&public_string_utf8;
        default:
            typ = SearchSysCache(TYPEOID, ObjectIdGetDatum(type), 0, 0, 0);
            if (!HeapTupleIsValid(typ))
                elog(
                    ERROR,
                    "[pldotnet_PublicDecl]: cache lookup failed for type %u",
                    type
                );
            typeinfo = (Form_pg_type) GETSTRUCT(typ);
            id = typeinfo->typtype;
            ReleaseSysCache(typ);
            if (id == TYPTYPE_COMPOSITE)
                return (char *)&public_struct;
    }
    return  0;
}

static int
plcsharp_BuildBLockCompositesFromTriggerData(
    char* composite_decls,
    FunctionCallInfo fcinfo,
    Form_pg_proc procst
)
{
    const char *tg_definition = plcsharp_GetTriggerDataDefinition();

    snprintf(
        composite_decls,
        8198,
        "%s",
        tg_definition
    );

    return strlen(composite_decls);
}

static int
plcsharp_BuildBlockCompositesFromProcedure(
    char *composite_decls,
    FunctionCallInfo fcinfo,
    Form_pg_proc procst
)
{
    Form_pg_type typeinfo;
    HeapTuple type;
    TupleDesc tupdesc;
    int cursize = 0, i;
    Oid *argtype = procst->proargtypes.values;

    for (i = 0;i < procst->pronargs; i++)
    {
        /* TODO: review this */
        if ( pldotnet_IsSimpleType(argtype[i]) ||
             pldotnet_IsTextType(argtype[i]) )
            continue;

        type = SearchSysCache(TYPEOID, ObjectIdGetDatum(argtype[i]), 0, 0, 0);
        if (!HeapTupleIsValid(type))
            elog(
                ERROR,
                "[plcsharp_BuildBlockCompositesFromProcedure]: cache lookup failed for type %u",
                argtype[i]
            );
        typeinfo = (Form_pg_type) GETSTRUCT(type);
        if (typeinfo->typtype == TYPTYPE_COMPOSITE)
        {
            tupdesc = lookup_rowtype_tupdesc(argtype[i], typeinfo->typtypmod);
            pldotnet_GetStructFromCompositeTuple(
                composite_decls + cursize,
                1024 - cursize,
                NameStr(typeinfo->typname),
                tupdesc
            );

            ReleaseTupleDesc(tupdesc);
            cursize += strlen(composite_decls);
        }
        ReleaseSysCache(type);
    }
    return strlen(composite_decls);
}

static int
plcsharp_BuildBlockComposites(
    char * composite_decls,
    FunctionCallInfo fcinfo,
    Form_pg_proc procst
)
{
    if (CALLED_AS_TRIGGER(fcinfo))
        return plcsharp_BuildBLockCompositesFromTriggerData(
            composite_decls,
            fcinfo,
            procst
        );

    return plcsharp_BuildBlockCompositesFromProcedure(
        composite_decls,
        fcinfo,
        procst
    );
}

static const char*
plcsharp_GetTriggerDataDefinition(void)
{
    return "\n\
        [StructLayout(LayoutKind.Sequential,Pack=1)]\n\
        public class TriggerInfo\n\
        {\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_name;\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_table_name;\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_table_schema;\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_when;\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_level;\n\
            [MarshalAs(UnmanagedType.LPUTF8Str)]\n\
            public string tg_event;\n\
            [MarshalAs(UnmanagedType.U8)]\n\
            public ulong tg_relid;\n\
            [MarshalAs(UnmanagedType.Struct)]\n\
            public ArrayT<IntPtr> tg_args_array;\n\
            [MarshalAs(UnmanagedType.Struct)]\n\
            public ArrayT<IntPtr> tg_relatts_array;\n\
            [MarshalAs(UnmanagedType.Struct)]\n\
            public ArrayT<SPI.PropertyValue> tg_new_array;\n\
            [MarshalAs(UnmanagedType.Struct)]\n\
            public ArrayT<SPI.PropertyValue> tg_old_array;\n\
\n\
            public TriggerInfo(TriggerInfo td)\n\
            {\n\
                tg_name = td.tg_name;\n\
                tg_table_name = td.tg_table_name;\n\
                tg_table_schema = td.tg_table_schema;\n\
                tg_when = td.tg_when;\n\
                tg_level = td.tg_level;\n\
                tg_event = td.tg_event;\n\
                tg_relid = td.tg_relid;\n\
                tg_args_array = td.tg_args_array;\n\
                tg_relatts_array = td.tg_relatts_array;\n\
                tg_new_array = td.tg_new_array;\n\
                tg_old_array = td.tg_old_array;\n\
            }\n\
\n\
        }\n\
        public class TriggerTuples\n\
        {\n\
            public SPI.BpgsqlDbRecord NEW;\n\
            public SPI.BpgsqlDbRecord OLD;\n\
            public TriggerTuples(SPI.BpgsqlDbRecord _NEW, SPI.BpgsqlDbRecord _OLD)\n\
            {\n\
                NEW = _NEW;\n\
                OLD = _OLD;\n\
            }\n\
        }\n\
        public class TriggerData : TriggerInfo\n\
        {\n\
            public string[] tg_args;\n\
            public string[] tg_relatts;\n\
            public TriggerTuples tg_tuples;\n\
            public TriggerData(TriggerInfo TgInfo) : base(TgInfo)\n\
            {\n\
                tg_args = ArrayToString(TgInfo.tg_args_array);\n\
                tg_relatts = ArrayToString(TgInfo.tg_relatts_array);\n\
                var NEW = ArrayToDbRecord(TgInfo.tg_new_array);\n\
                var OLD = ArrayToDbRecord(TgInfo.tg_old_array);\n\
                tg_tuples = new TriggerTuples(NEW, OLD);\n\
            }\n\
        }\n";
}

static char*
plcsharp_BuildTriggerDataArgs(FunctionCallInfo fcinfo)
{
    return "\n\
[MarshalAs(UnmanagedType.Struct)]\n\
public TriggerInfo TgInfo;";
}

static char *
plcsharp_BuildBlockArgsDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *block2str, *str_ptr;
    Oid *argtype = procst->proargtypes.values; /* Indicates the args type */
    Oid rettype = procst->prorettype; /* Indicates the return type */
    int nargs = procst->pronargs;
    const char semicon[] = ";";
    char argname[] = " argN";
    char result[] = " resu"; /*  have to be same size argN */
    int i, cursize = 0, totalsize = 0;
    char *tg_data;
    /* nullable related */
    bool nullable_arg_flag = false;
    int null_flags_size = 0, return_null_flag_size = 0;
    bool isarr = false;
    Oid type;

    if (!pldotnet_TypeSupported(rettype))
        elog(ERROR, "[pldotnet]: unsupported type on return");

    for (i = 0; i < nargs; i++)
    {
        isarr = pldotnet_CheckArgIsArray(
            pldotnet_GetArgDatum(fcinfo, i), 
            argtype[i],
            i,
            validation,
            func_inout_info
        );

        type = isarr ? func_inout_info->arrayinfo[i].typelem : argtype[i];

        if (!pldotnet_TypeSupported(type))
        {
            elog(ERROR, "[pldotnet]: unsupported type on arg %d", i);
            return 0;
        }

        if (isarr)
            totalsize += strlen(func_inout_info->arrayinfo[i].csharpdecl);
        else
        {
            totalsize += pldotnet_PublicDeclSize(type) +
                         strlen(pldotnet_GetNetTypeName(type, true)) +
                         strlen(argname) + strlen(semicon);
        }

        if (pldotnet_IsNullable(argtype[i]))
            nullable_arg_flag = true;
    }

    return_null_flag_size = strlen(public_bool) + strlen(resu_flag_str);

    if (nullable_arg_flag && 0 < nargs)
        null_flags_size = GetSizeArgsNullArray(nargs);

    if (CALLED_AS_TRIGGER(fcinfo))
    {
        tg_data = plcsharp_BuildTriggerDataArgs(fcinfo);
        totalsize += strlen(tg_data);
    }

    totalsize += pldotnet_PublicDeclSize(rettype)
               + strlen(pldotnet_GetNetTypeName(rettype, true))
               + null_flags_size
               + return_null_flag_size
               + strlen(result)
               + strlen(semicon) + 1;

    block2str = (char *) palloc0(totalsize);

    if (nullable_arg_flag && 0 < nargs)
    {
        SNPRINTF(
            block2str,
            totalsize,
            "\n[MarshalAs(UnmanagedType.ByValArray,ArraySubType=UnmanagedType.U1,SizeConst=%d)]public %s",
            nargs,
            arg_flag_str
        );
        cursize = strlen(block2str);
    }

    str_ptr = (char *)(block2str + cursize);
    SNPRINTF(
        str_ptr,
        totalsize - cursize,
        "%s%s",
        public_bool,
        resu_flag_str
    );

    cursize += strlen(str_ptr);

    if (CALLED_AS_TRIGGER(fcinfo))
    {
        size_t cts = strlen(tg_data) + 1;
        str_ptr = (char*)(block2str + cursize);
        SNPRINTF(
            str_ptr,
            cts,
            "%s",
            tg_data
        );
        cursize = strlen(block2str);
    }

    for (i = 0; i < nargs; i++)
    {
        str_ptr = (char *)(block2str + cursize);
        if (pldotnet_IsArray(i, func_inout_info))
        {
            SNPRINTF(str_ptr, totalsize - cursize,"%s",
                     func_inout_info->arrayinfo[i].csharpdecl);
        }
        else
        {
            /* review nargs > 9 */
            SNPRINTF(argname, strlen(argname)+1, " arg%d", i);
            SNPRINTF(str_ptr,totalsize - cursize,"%s%s%s%s"
                        , pldotnet_PublicDecl(argtype[i])
                        , pldotnet_GetNetTypeName(argtype[i], true)
                        , argname, semicon);
        }
        cursize += strlen(str_ptr);
    }

    /* result */
    str_ptr = (char *)(block2str + cursize);

    SNPRINTF(
        str_ptr,
        totalsize - cursize,
        "%s%s%s%s",
        pldotnet_PublicDecl(rettype),
        pldotnet_GetNetTypeName(rettype, true),
        result,
        semicon
    );

    return block2str;
}

/*
 * This function builds the parameters to the function 
 * or stored procedure. A trigger can call a function and
 * pass some arguments not related to the function
 * definition.
 * Example: CREATE FUNCTION my_function() RETURNS TRIGGER ...
 * my_function does not accept any argument in its definition
 * but triggers can pass some variables, including both
 * NEW and OLD tuples in a typical UPDATE query.
 * This list of special args here is a list of strings.
 */
const char*
plchsarp_GetTriggerArgs(FunctionCallInfo fcinfo)
{
    return "";
}

static char*
plcsharp_BuildBlockTriggerFuncCall(FunctionCallInfo fcinfo, Form_pg_proc procst)
{
    const char *template = "\n\
            libargs.resu = %s(%s%s);\n\
            libargs.resunull = null == libargs.resu;\n";

    const char *args = plchsarp_GetTriggerArgs(fcinfo);
    const char *tg_data = "new TriggerData(libargs.TgInfo)";
    char *func = NameStr(procst->proname);

    size_t size = strlen(template)
                + strlen(func)
                + strlen(args)
                + strlen(tg_data)
                + 1;

    char *block2str = (char*) palloc0(size);

    snprintf(
        block2str,
        size,
        template,
        func,
        args,
        tg_data
    );

    return block2str;
}

static char*
plcsharp_BuildBlockNonTriggerFuncCall(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *block2str, *str_ptr, *resu_var;
    int cursize = 0, i, totalsize;
    const char beginFun[] = "(";
    char * func;
    const char libargs[] = "libargs.";
    const char strconvert[] = ".ToString()"; /* Converts func return */
    const char todecimal[] = "Convert.ToDecimal(";
    const char arr_todecimal[] = "UserClass.ArrayToDecimal(";
    const char arr_tostring[] = "UserClass.ArrayToString(";
    const char result[] = "            libargs.resu=";
    const char nullable_result[] = "            resu_nullable=";
    const char fromArrayT[] = "UserClass.fromArrayT<%s>(%s%s)";
    const char comma[] = ",";
    char argname[] = "argN";
    const char end_fun[] = ")";
    const char semicolon[] = ";";
    int nargs = procst->pronargs;
    Oid *argtype = procst->proargtypes.values; /* Indicates the args type */
    Oid rettype = procst->prorettype; /* Indicates the return type */

    /* Function name */
    func = NameStr(procst->proname);

    if (pldotnet_IsNullable(rettype))
    {
        int resu_var_size =
            strlen(pldotnet_GetNullableTypeName(rettype))
          + strlen(nullable_result)
          + 1;

        resu_var = (char *)palloc0(resu_var_size);
        SNPRINTF(
            resu_var,
            resu_var_size,
            "%s%s",
            pldotnet_GetNullableTypeName(rettype),
            nullable_result
        );
    }
    else
        resu_var = (char *)result;

    /* TODO:  review for nargs > 9 */
    if (nargs == 0)
    {
         int block_size;

         if (rettype == NUMERICOID)
         {
            block_size = strlen(resu_var) + strlen(func) + strlen(beginFun)
                                 + strlen(end_fun) + strlen(strconvert)
                                 + strlen(semicolon) + 1;
            block2str = (char *)palloc0(block_size);
            SNPRINTF(block2str,block_size,"%s%s%s%s%s%s"
                       , resu_var, func, beginFun
                       , end_fun, strconvert, semicolon);
         }
         else
         {
            block_size = strlen(resu_var) + strlen(func) + strlen(beginFun)
                                 + strlen(end_fun) + strlen(semicolon) + 1;
            block2str = (char *)palloc0(block_size);
            SNPRINTF(block2str,block_size,"%s%s%s%s%s"
                        ,resu_var, func, beginFun, end_fun, semicolon);
         }
         return block2str;
    }

    totalsize = strlen(resu_var) + strlen(func) + strlen(beginFun) +
                    (strlen(libargs) + strlen(argname)) * nargs
                     + strlen(end_fun) + strlen(semicolon) + 1;

    for (i = 0; i < nargs; i++) /* Get number of Numeric argr */
    {
        if (argtype[i] == NUMERICOID)
            totalsize += strlen(todecimal) + strlen(end_fun);
        else if (pldotnet_IsArray(i, func_inout_info))
        {
            if (func_inout_info->arrayinfo[i].typelem == NUMERICOID)
                totalsize += strlen(arr_todecimal) + strlen(end_fun);
            else if (pldotnet_IsTextType(func_inout_info->arrayinfo[i].typelem))
                totalsize += strlen(arr_tostring) + strlen(end_fun);
            else
                totalsize += strlen(fromArrayT) + strlen(end_fun);
        }
    }

    if (rettype == NUMERICOID)
        totalsize += strlen(strconvert);

    if (nargs > 1)
        totalsize += (nargs - 1) * strlen(comma);

    block2str = (char *) palloc0(totalsize);
    SNPRINTF(block2str, totalsize - cursize, "%s%s%s", resu_var, func, beginFun);
    cursize = strlen(resu_var) + strlen(func) + strlen(beginFun);

    for (i = 0; i < nargs; i++)
    {
        SNPRINTF(argname, strlen(argname)+1, "arg%d", i); /* review nargs > 9 */
        str_ptr = (char *)(block2str + cursize);

        if (argtype[i] == NUMERICOID)
        {
            SNPRINTF(str_ptr,totalsize-cursize,"%s%s%s%s",
                     todecimal, libargs, argname, end_fun);
        }
        else if (pldotnet_IsArray(i, func_inout_info))
        {
            if (func_inout_info->arrayinfo[i].typelem == NUMERICOID)
            {
                SNPRINTF(
                    str_ptr,
                    totalsize - cursize,
                    "%s%s%s%s",
                    arr_todecimal,
                    libargs,
                    argname,
                    end_fun
                );
            }
            else if (pldotnet_IsTextType(func_inout_info->arrayinfo[i].typelem))
            {
                SNPRINTF(
                    str_ptr,
                    totalsize - cursize,
                    "%s%s%s%s",
                    arr_tostring,
                    libargs,
                    argname,
                    end_fun
                );
            }
            else
            {
                SNPRINTF(
                    str_ptr,
                    totalsize - cursize,
                    "UserClass.fromArrayT<%s>(%s%s)",
                    pldotnet_GetCompatibleNetTypeName(
                        func_inout_info->arrayinfo[i].typelem,
                        true,
                        true
                    ),
                    libargs,
                    argname
                );
            }
        }
        else
        {
            SNPRINTF(str_ptr, totalsize-cursize, "%s%s", libargs, argname);
        }

        if (i + 1 < nargs)  /* comma required */
        {
            cursize = strlen(block2str);
            str_ptr = (char *)(block2str + cursize);
            SNPRINTF(str_ptr, totalsize-cursize, "%s", comma);
        }
        cursize = strlen(block2str);
    }

    str_ptr = (char *)(block2str + cursize);
    if (rettype == NUMERICOID)
    {
        SNPRINTF(str_ptr, totalsize-cursize, "%s%s%s"
                   , end_fun, strconvert, semicolon);
    }
    else
    {
        SNPRINTF(str_ptr, totalsize-cursize, "%s%s", end_fun, semicolon);
    }
    return block2str;
}

static char *
plcsharp_BuildBlockCallFuncCall(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    if (CALLED_AS_TRIGGER(fcinfo))
        return plcsharp_BuildBlockTriggerFuncCall(fcinfo, procst);

    return plcsharp_BuildBlockNonTriggerFuncCall(fcinfo, procst, func_inout_info);
}

static int
GetSizeArgsNullArray(int nargs)
{
    const char public_bool_array[] =
        "\n[MarshalAs(UnmanagedType.ByValArray,ArraySubType=UnmanagedType.U1,SizeConst=)]public ";
    int n_digits_args = 0;

    if (nargs > 0)
        n_digits_args = floor(log10(abs(nargs))) + 1;

    return (strlen(public_bool_array) + n_digits_args + strlen(arg_flag_str));
}

/*
 * Returns the size of typical C# line converting
 * a struct argument to a nullable C# type argument
 */
static int
GetSizeNullableHeader(int argnm_size, Oid arg_type, int narg)
{
    int total_size = 0;
    char *question_mark = "?";
    char *equal_char = "=";
    char *parenthesis_char = "("; /* same for ')' */
    char *square_bracket_char = "["; /* same for ']' */
    char *colon_char = ":";
    char *semicolon_char = ";";
    char *newline_char = "\n";
    char *null_str = "null";
    int n_digits_arg = 0;

    if (narg == 0)
        /* Edge case treatment since log10(0) == -HUGE_VAL */
        n_digits_arg = floor(log10(abs(1))) + 1;
    else
        n_digits_arg = floor(log10(abs(narg))) + 1;

    switch (arg_type)
    {
        case INT2OID:
        case INT4OID:
        case INT8OID:
        case BOOLOID:
            /* template: 
             * bool? <arg>=argsnull[i]? (bool?)null : <arg>_nullable; */
            total_size = strlen(pldotnet_GetNullableTypeName(arg_type))
                + argnm_size + strlen(equal_char) + strlen(argsnull_str)
                + strlen(square_bracket_char) + n_digits_arg +
                + strlen(square_bracket_char) + strlen(question_mark)
                + strlen(parenthesis_char)
                + strlen(pldotnet_GetNullableTypeName(arg_type))
                + strlen(parenthesis_char) + strlen(null_str)
                + strlen(colon_char) + argnm_size + strlen(nullable_suffix)
                + strlen(semicolon_char) + strlen(newline_char);
            break;
    }

    return total_size;
}

/*
 * Returns the size of typical C# line converting
 * a nullable C# type return to a struct return
 */
static int
GetSizeNullableFooter(Oid ret_type)
{
    switch (ret_type)
    {
        case INT2OID:
        case INT4OID:
        case INT8OID:
        case BOOLOID:
            return strlen(resu_nullable_value)
                 + strlen(resu_nullable_flag);
        case TRIGGEROID:
            return strlen(resu_null_flag);
        default:
            return 0;
    }
}

static const char*
plcsharp_BuildBlockTriggerFuncDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst
)
{
    bool isnull = false;
    const char *template = "\n\
            %s %s(%s)\n\
            {\n\
                %s\n\
            }\n";

    char *func = NameStr(procst->proname);
    Datum prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    const char *source_text = DatumGetCString(DirectFunctionCall1(textout, prosrc));
    const char *rettypname = pldotnet_GetCompatibleNetTypeName(
        procst->prorettype,
        true,
        true
    );

    const char *tg_data = "TriggerData TD";

    size_t size = strlen(template)
                + strlen(rettypname)
                + strlen(func) * 2
                + strlen(tg_data)
                + strlen(source_text) + 1;

    char *block2str = (char*) palloc0(size);

    snprintf(
        block2str,
        size,
        template,
        rettypname,
        func,
        tg_data,
        source_text,
        func
    );

    return block2str;
}

static const char*
plcsharp_BuildBlockNonTriggerFuncDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *block2str, *str_ptr, *source_argnm, *source_text;
    int argnm_size, i, nnames, cursize = 0, source_size, totalsize;
    bool isnull;
    const char begin_fun_decl[] = "(";
    char *func_name;
    const char comma[] = ",";
    const char end_fun_decl[] = "){";
    const char end_fun[] = "}\n";
    const char newline[] = "\n";
    const char arrbrackets[] = "[]";
    char argnm[64];
    int nargs = procst->pronargs;
    Oid rettype = procst->prorettype;
    Datum *argname, argnames, prosrc;
    Oid *argtype = procst->proargtypes.values; /* Indicates the args type */
    /* nullable related */
    char *header_nullable, *header_nullable_ptr;
    int header_size=0, cur_header_size, footer_size=0;

    /* Function name */
    func_name = NameStr(procst->proname);

    /* Source code */
    prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    source_text = DatumGetCString(DirectFunctionCall1(textout, prosrc));
    source_size = strlen(source_text);

    argnames = SysCacheGetAttr(PROCOID, proc,
                               Anum_pg_proc_proargnames, &isnull);

    if (!isnull)
        deconstruct_array(DatumGetArrayTypeP(argnames), TEXTOID,
                          -1, false, 'i', &argname, NULL, &nnames);

    /* Calculates the total amount in bytes of C# src text for
     * the function declaration according nr of arguments
     * their types and the function return type
     */
    if (pldotnet_IsNullable(rettype))
    {
        totalsize = (2 * strlen(newline))
            + strlen(pldotnet_GetNullableTypeName(rettype))
            + strlen(" ") + strlen(func_name) + strlen(begin_fun_decl);
    }
    else
    {
        totalsize = (2 * strlen(newline))
            + strlen(pldotnet_GetNetTypeName(rettype, false))
            + strlen(" ") + strlen(func_name) + strlen(begin_fun_decl);
    }

    for (i = 0; i < nargs; i++)
    {
        source_argnm = DatumGetCString( DirectFunctionCall1(textout,
                                                            argname[i]) );
        if (pldotnet_IsNullable(argtype[i]))
        {
            header_size += GetSizeNullableHeader( strlen(source_argnm),
                                                  argtype[i],i );
            argnm_size = strlen(source_argnm) + strlen("_nullable");
        }
        else
            argnm_size = strlen(source_argnm);

        if (pldotnet_IsArray(i, func_inout_info))
        {
            totalsize += strlen( pldotnet_GetNetTypeName(
                                 func_inout_info->arrayinfo[i].typelem, false) )
                         + strlen(arrbrackets) + 1 + argnm_size;
        }
        else
        {
             /* +1 here is the space between type" "argname declaration */
            totalsize += strlen(pldotnet_GetNetTypeName(argtype[i], false)) +
                         1 + argnm_size;
        }

    }
    if (nargs > 1)
         totalsize += (nargs - 1) * strlen(comma); /* commas size */

    footer_size = GetSizeNullableFooter(rettype);

    totalsize += strlen(end_fun_decl) + header_size + source_size
        + strlen(end_fun) + footer_size + 1;

    block2str = (char *)palloc0(totalsize);

    if (pldotnet_IsNullable(rettype))
    {
        SNPRINTF(block2str, totalsize, "%s%s%s %s%s",newline
                   , newline, pldotnet_GetNullableTypeName(rettype)
                   , func_name, begin_fun_decl);
    }
    else
    {
        SNPRINTF(block2str, totalsize, "%s%s%s %s%s",newline
                   , newline,  pldotnet_GetNetTypeName(rettype, false)
                   , func_name, begin_fun_decl);
    }

    cursize = strlen(block2str);

    header_nullable = (char *)palloc0(header_size + 1);
    cur_header_size = strlen(header_nullable);

    for (i = 0; i < nargs; i++)
    {
        source_argnm = DatumGetCString( DirectFunctionCall1(textout,
                                                            argname[i]) );
        if (pldotnet_IsNullable(argtype[i]))
        {
            header_nullable_ptr = (char *) (header_nullable + cur_header_size);
            SNPRINTF(header_nullable_ptr, (header_size - cur_header_size) + 1
                       , "%s%s=%s[%d]?(%s)null:%s%s;\n"
                       , pldotnet_GetNullableTypeName(argtype[i])
                       , source_argnm, argsnull_str, i
                       , pldotnet_GetNullableTypeName(argtype[i])
                       , source_argnm, nullable_suffix);
            cur_header_size = strlen(header_nullable);
            SNPRINTF(argnm, strlen(source_argnm) + strlen("_nullable") + 1
                            , "%s_nullable", source_argnm);
        }
        else
            SNPRINTF(argnm, strlen(source_argnm) + 1, "%s", source_argnm);

        str_ptr = (char *)(block2str + cursize);
        if (pldotnet_IsArray(i, func_inout_info))
        {
            SNPRINTF( str_ptr, totalsize - cursize, "%s%s",
            pldotnet_GetNetTypeName(func_inout_info->arrayinfo[i].typelem,
                                    false), arrbrackets );
        }
        else
        {
            SNPRINTF( str_ptr, totalsize - cursize, "%s",
                      pldotnet_GetNetTypeName(argtype[i], false) );

        }
        cursize = strlen(block2str);
        str_ptr = (char *)(block2str + cursize);
        SNPRINTF( str_ptr, totalsize - cursize, " %s", argnm);
        if (i + 1 < nargs)
        {  /* last no comma */
            cursize = strlen(block2str);
            str_ptr = (char *)(block2str + cursize);
            SNPRINTF(str_ptr, totalsize - cursize, "%s", comma);
        }

        cursize = strlen(block2str);
        bzero(argnm, sizeof(argnm));
    }

    str_ptr = (char *)(block2str + cursize);
    SNPRINTF(str_ptr, totalsize - cursize, "%s", end_fun_decl);
    cursize = strlen(block2str);

    if (header_size > 0)
    {
        str_ptr = (char *)(block2str + cursize);
        SNPRINTF(str_ptr, totalsize - cursize, "%s",header_nullable);
        cursize = strlen(block2str);
    }

    str_ptr = (char *)(block2str + cursize);
    SNPRINTF(str_ptr, totalsize - cursize, "%s%s", source_text, end_fun);
    cursize = strlen(block2str);

    if (footer_size > 0)
    {
        str_ptr = (char *)(block2str + cursize);
        SNPRINTF(str_ptr, totalsize - cursize, "%s%s"
            , resu_nullable_value, resu_nullable_flag);
    }

    return block2str;
}

static const char *
plcsharp_BuildBlockUserFuncDecl(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    HeapTuple proc,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    if (CALLED_AS_TRIGGER(fcinfo))
        return plcsharp_BuildBlockTriggerFuncDecl(
            fcinfo,
            proc,
            procst
        );

    return plcsharp_BuildBlockNonTriggerFuncDecl(
        fcinfo,
        proc,
        procst,
        func_inout_info
    );
}

/* Postgres Datum type to C# nullable type name */
static const char *
pldotnet_GetNullableTypeName(Oid id)
{
    switch (id)
    {
        case BOOLOID:
            return "bool?"; /* Nullable<System.Boolean> */
        case INT2OID:
            return "short?";/* Nullable<System.Int16> */
        case INT4OID:
            return "int?";  /* Nullable<System.Int32> */
        case INT8OID:
            return "long?"; /* Nullable<System.Int64> */
        case TRIGGEROID:
            return "string?";
    }
    return "";
}

bool
pldotnet_CheckArgIsArray(
    Datum datum,
    Oid oid,
    int narg,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    HeapTuple typetuple;
    Form_pg_type typeinfo;
    bool isarr = false;
    pldotnet_ArgArrayInfo * parr_info;

    typetuple = SearchSysCache(TYPEOID, ObjectIdGetDatum(oid), 0, 0, 0);
    if (!HeapTupleIsValid(typetuple)) {
        elog(ERROR,
          "[pldotnet]: (CheckArgIsArray) cache lookup failed for type %u", oid);
    }
    typeinfo = (Form_pg_type) GETSTRUCT(typetuple);

    isarr = (typeinfo->typelem != 0 && typeinfo->typlen == -1);

    if (isarr)
    {
        parr_info = &(func_inout_info->arrayinfo[narg]);
        parr_info->ixarray = narg;
        parr_info->typlen = typeinfo->typlen;
        parr_info->typbyval = typeinfo->typbyval;
        parr_info->typtype = typeinfo->typtype;
        parr_info->typelem = typeinfo->typelem;
        parr_info->typalign = typeinfo->typalign;

        if (!validation)
            pldotnet_SetArraySize(datum, parr_info);

        /* Review for nargs > 9*/
        sprintf(parr_info->csharpdecl, "\n\
            [MarshalAs(UnmanagedType.Struct)]\n\
            public ArrayT<%s> arg%d;\n\
            ",
            pldotnet_NeedsIntPtr(typeinfo->typelem) ? "IntPtr" :  pldotnet_GetNetTypeName(typeinfo->typelem, true),
            narg
        );
    }
    else
        func_inout_info->arrayinfo[narg].ixarray = -1;
    ReleaseSysCache(typetuple);
    return isarr;
}

inline static bool
plcsharp_BuildPaths(pldotnet_PathConfig *paths)
{
    static bool built = false;

    if (!built)
    {
        pldotnet_BuildPaths(true, paths);
        built = true;
    }

    spi_paths = paths;

    return built;
}

static bool
plcsharp_CreateStructLibargs(
    const FunctionCallInfo fcinfo,
    const Form_pg_proc procst,
    pldotnet_FunctionDecl *function_decl
)
{
    pldotnet_FuncInOutInfo *func_inout_info = &(function_decl->func_inout_info);
    
    function_decl->args = (int8_t*) pldotnet_CreateCStructLibargs(
        fcinfo,
        procst,
        false,
        func_inout_info
    );
    function_decl->args_length = func_inout_info->typesize_nullflags +
                                 func_inout_info->typesize_args +
                                 func_inout_info->typesize_result;

    return nullptr != function_decl->args && function_decl->args;
}

/*
 * This function parses the information from the arguments
 * into a valid source code. In this case. an INLINE function called in a DO block
 */
static char*
plcsharp_GetInlineSourceCode(FunctionCallInfo fcinfo)
{
    size_t source_code_size;
    char *source_code = nullptr;
    char *block_inline_usercode = CODEBLOCK;

    source_code_size = strlen(block_inline_header)
                     + strlen(block_inline_callfunc)
                     + strlen(block_inline_usercode)
                     + strlen(block_inline_footer) + 1;

    source_code = (char*) palloc0(source_code_size);

    SNPRINTF(source_code,
                source_code_size,
                "%s%s%s%s",
                block_inline_header,
                block_inline_callfunc,
                block_inline_usercode,
                block_inline_footer);

    return source_code;
}

/*
 * This function parses the information from the arguments
 * into a valid source code. In this case, an user function saved in the database
 */
static char*
plcsharp_GetUserSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    size_t source_code_size;
    const char *cs_block_args_decl;
    const char *cs_block_userfunc_decl;
    const char *cs_block_callfunc_call;
    char *source_code = nullptr;
    char cs_block_composite_decl[8198];
    cs_block_composite_decl[0] = 0;

    plcsharp_BuildBlockComposites(cs_block_composite_decl, fcinfo, procst);
    
    cs_block_args_decl = plcsharp_BuildBlockArgsDecl(
        fcinfo,
        proc,
        procst,
        validation,
        func_inout_info
    );

    cs_block_callfunc_call = plcsharp_BuildBlockCallFuncCall(fcinfo, procst, func_inout_info);
    cs_block_userfunc_decl = plcsharp_BuildBlockUserFuncDecl(fcinfo, procst, proc, func_inout_info);

    source_code_size = strlen(cs_block_header)
                     + strlen(cs_block_composite_decl)
                     + strlen(cs_block_args_header)
                     + strlen(cs_block_args_decl)
                     + strlen(cs_block_callfunc_header)
                     + strlen(cs_block_callfunc_call)
                     + strlen(cs_block_userfunc_decl)
                     + strlen(cs_block_footer) + 1;

    source_code = palloc0(source_code_size);

    SNPRINTF(source_code,
            source_code_size,
            "%s%s%s%s%s%s%s%s",
            cs_block_header,
            cs_block_composite_decl,
            cs_block_args_header,
            cs_block_args_decl,
            cs_block_callfunc_header,
            cs_block_callfunc_call,
            cs_block_userfunc_decl,
            cs_block_footer
    );
    return source_code;
}

static bool
plcsharp_GetSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl
)
{
    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument: function_decl is null");

    if (nullptr != function_decl->source.source_code)
        elog(
            ERROR,
            "[pldotnet]: ArgSouce.source_code should be null at this point: \n%s",
            function_decl->source.source_code
        );

    if (is_inline)
        function_decl->source.source_code = plcsharp_GetInlineSourceCode(fcinfo);
    else
        function_decl->source.source_code = plcsharp_GetUserSourceCode(
            fcinfo,
            proc,
            procst,
            validation,
            &(function_decl->func_inout_info)
        );

    return nullptr != function_decl->source.source_code;
}

/*
 * This function tries to build a valid pldotnet_FunctionDecl
 * It searchs for information on PG SysCache and it stores
 * the required data into the last argument (pldotnet_FunctionDecl *function_decl)
 * this structure is meant to be saved into a hash table
 * If it succeeds, the it returns true, otherwise false
 */
static bool
plcsharp_BuildFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl)
{
    Form_pg_proc procst;
    static pldotnet_PathConfig paths;

    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument, function_decl is null");

    if (!plcsharp_BuildPaths(&paths))
        elog(ERROR, "[pldotnet]: Could not build paths");

    if (!pldotnet_SetNetLoader(paths.config_path, paths.prefix))
        elog(ERROR, "[pldotnet]: Could not obtain .NET Loader");

    procst = (Form_pg_proc) GETSTRUCT(proc);

    /* save some basic data */
    function_decl->source.func_oid = (uint32_t) oid;
    function_decl->ret_type = procst->prorettype;

    if (!plcsharp_GetSourceCode(fcinfo, proc, procst, is_inline, validation, function_decl))
        elog(ERROR, "[pldotnet]: Could not obtain the source code");

    if (!pldotnet_CompileUserFunction(assembly_loader, &paths, &(function_decl->source)))
        elog(ERROR, "[pldotnet]: Could not compile this function.");

    function_decl->dotnet_method = pldotnet_GetUserMethod(assembly_loader, &paths);

    return nullptr != function_decl->dotnet_method;
}

static pldotnet_FunctionDecl*
plcsharp_GetFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation)
{
    pldotnet_FunctionDecl *decl = pldotnet_FindFunctionDecl(oid);
    bool found = nullptr != decl;

    if (found && !validation)
        return decl;

    decl = pldotnet_CreateFunctionDecl();

    plcsharp_BuildFunctionDecl(
        oid,
        fcinfo,
        proc,
        is_inline,
        validation,
        decl
    );

    pldotnet_SaveFunction(
        decl,
        !found
    );

    return decl;
}

static void
plcsharp_ValidateUserFunction(const Oid oid, const FunctionCallInfo fcinfo)
{
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Could not obtain info about %u", oid);

    plcsharp_GetFunctionDecl(
        oid,
        fcinfo,
        proc,
        false,
        true
    );

    /* END */
    pldotnet_ReleasePostgresHeapTuple(proc);
}

/*
 * This function starts to building the output paths into a static
 * pldotnet_PathConfig, then it parses the procedure data into a
 * valid source code. This second step produces a valid pldotnet_FunctionDecl
 * which contains useful information regarding the current function.
 * While building pldotnet_FunctionDecl, it tries to find a previous cached
 * function aiming to avoid reloading stuff from .NET.
 *
 *  Case 1: If there is no previous cached function, it loads .NET, gets the
 *  function pointers and calls Engine.Compile() and Engine.Run() to retrieve
 *  the desired results. After calling the user function, it saves the current
 *  pldotnet_FunctionDecl into a global hash table called procedures
 *  (see pldotnet_common.h)
 *
 *  Case 2: if there is a previous cached function, then it just calls Engine.Run()
 */
static Datum
plcsharp_CompileAndRunUserFunction(
    const FunctionCallInfo fcinfo,
    bool is_inline)
{
    HeapTuple proc;
    Form_pg_proc procst;
    pldotnet_FunctionDecl *function_decl = nullptr;

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    proc = pldotnet_GetPostgresHeapTuple(fcinfo->flinfo->fn_oid);

    function_decl = plcsharp_GetFunctionDecl(
        fcinfo->flinfo->fn_oid,
        fcinfo,
        proc,
        is_inline,
        false
    );

    procst = (Form_pg_proc) GETSTRUCT(proc);

    if (!is_inline && !plcsharp_CreateStructLibargs(fcinfo, procst, function_decl))
        elog(ERROR, "[pldotnet]: Could not create struct buffer");

    /* END */
    pldotnet_ReleasePostgresHeapTuple(proc);

    if (nullptr == function_decl || nullptr == function_decl->dotnet_method)
        elog(ERROR, "[pldotnet]: Could not load function_decl");

    function_decl->dotnet_method(function_decl->args, function_decl->args_length);

    return pldotnet_GetNetResult(
        function_decl->args,
        function_decl->ret_type,
        fcinfo,
        &(function_decl->func_inout_info)
    );
}

/*
 * This is the main handler function
 * It receives and additional bool is_inline argument
 * to deal with both normal and inline calls
 * This flag is used internally to generate propper source code
 */
static Datum
plcsharp_generic_handler(FunctionCallInfo fcinfo, bool is_inline)
{
    MemoryContextWrapper memory_context;
    Datum retval = 0;

    pldotnet_LoadHostFxrIfNeeded();

    if (!pldotnet_SPIReady())
        return retval;

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        retval = plcsharp_CompileAndRunUserFunction(fcinfo, is_inline);

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

PG_FUNCTION_INFO_V1(plcsharp_call_handler);

Datum
plcsharp_call_handler(PG_FUNCTION_ARGS)
{
    return plcsharp_generic_handler(fcinfo, false);
}

PG_FUNCTION_INFO_V1(plcsharp_validator);
Datum
plcsharp_validator(PG_FUNCTION_ARGS)
{
    MemoryContextWrapper memory_context;
    HeapTuple tuple;
    Oid funcoid = PG_GETARG_OID(0);

    /* if (!CheckFunctionValidatorAccess(funcoid, fcinfo->flinfo->fn_oid))
     *    PG_RETURN_VOID();
    */

    if (!check_function_bodies)
        return (Datum) 0;

    pldotnet_LoadHostFxrIfNeeded();

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        tuple = SearchSysCache1(PROCOID, ObjectIdGetDatum(funcoid));
	    if (!HeapTupleIsValid(tuple))
		    elog(ERROR, "cache lookup failed for function %u", funcoid);

        ReleaseSysCache(tuple);

        plcsharp_ValidateUserFunction(funcoid, fcinfo);

        /* REVERT PREV MEM CONTEXT */
        pldotnet_ResetMemoryContext(&memory_context);
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

PG_FUNCTION_INFO_V1(plcsharp_inline_handler);
Datum
plcsharp_inline_handler(PG_FUNCTION_ARGS)
{
    return plcsharp_generic_handler(fcinfo, true);
}

/*
 * This function should not be used in production
 * It compiles directly into disk so it's very slow
 * We can rely on Roslyn compiler,
 */
int 
plcsharp_CompileFunctionNetBuild(char * source_code)
{
    FILE *output_file;
    int compile_resp;
    char *cmd;
    output_file = fopen(paths.src_lib_path, "w");
    if (!output_file)
    {
        fprintf(stderr, "Cannot open file: '%s'\n", paths.src_lib_path);
        exit(-1);
    }
    if (fputs(source_code, output_file) == EOF)
    {
        fprintf(stderr, "Cannot write to file: '%s'\n", paths.src_lib_path);
        exit(-1);
    }
    fclose(output_file);
    setenv("DOTNET_CLI_HOME", dnldir, 1);
    cmd = palloc0(strlen("dotnet build ")
        + strlen(dnldir) + strlen("/src/csharp > null") + 1);
    SNPRINTF(cmd, strlen("dotnet build ") + strlen(dnldir) +
                  strlen("/src/csharp > null") + 1,
                  "dotnet build %s/src/csharp > null", dnldir);
    compile_resp = system(cmd);
    assert(compile_resp != -1 && "Failure: Cannot compile C# source code");
    return 0; /* VOID */
 }

/*
 *  DEPRECATED see pldotnet_CompileUserFunction on pldotnet_common.c
 */
int 
plcsharp_CompileFunction(char * src, FunctionCallInfo fcinfo)
{
    int rc;
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Compile";

    static component_entry_point_fn csharp_method = nullptr;

    pldotnet_ArgsSource args;
    args.source_code = src;
    args.func_oid = (int) fcinfo->flinfo->fn_oid;
    args.result = 1;

    /* Function pointer to managed delegate */
    if (nullptr == csharp_method)
    {
        rc = load_assembly_and_get_function_pointer(
            paths.library_path,
            dotnet_type,
            dotnet_type_method,
            nullptr /* delegate_type_name */,
            nullptr,
            (void**)&csharp_method);

        assert(rc == 0 && csharp_method != nullptr && \
            "Failure: load_assembly_and_get_function_pointer()");
    }

    return csharp_method((char*)&args, sizeof(pldotnet_ArgsSource));
}

/*
 *  DEPRECATED see pldotnet_RunUserFunction on pldotnet_common.c
 */
Datum 
plcsharp_RunFunction(
    char *libargs,
    FunctionCallInfo fcinfo,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    int rc;
    Datum retval = 0;
    component_entry_point_fn csharp_method = nullptr;
#ifdef USE_DOTNETBUILD
    char dotnet_type[]  = "PlDotNETUserSpace.UserClass, PlDotNETUserSpace";
    char dotnet_type_method[64] = "CallFunction";
    FILE *output_file;
#else
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Run";
#endif

    if (libargs != NULL)  /* Regular functions */
    {
        rc = load_assembly_and_get_function_pointer(
            paths.library_path,
            dotnet_type,
            dotnet_type_method,
            nullptr /* delegate_type_name */,
            nullptr,
            (void**)&csharp_method);

        assert(rc == 0 && csharp_method != nullptr && \
            "Failure: load_assembly_and_get_function_pointer()");

        retval = plcsharp_Run(dotnet_type, dotnet_type_method, libargs,
                 func_inout_info->typesize_nullflags +
                 func_inout_info->typesize_args +
                 func_inout_info->typesize_result);
    }
    else  /* Inlines */
        retval = plcsharp_Run(dotnet_type, dotnet_type_method, NULL, 0);

    return retval;
}

/*
 *  DEPRECATED see pldotnet_Run on pldotnet_common.c
 */
int
plcsharp_Run(char * dotnet_type, char * dotnet_type_method, char * libargs,
                                                                 int args_size)
{
    int rc;
    component_entry_point_fn csharp_method = nullptr;

    /* Function pointer to managed delegate */
    rc = load_assembly_and_get_function_pointer(
        paths.library_path,
        dotnet_type,
        dotnet_type_method,
        nullptr /* delegate_type_name */,
        nullptr,
        (void**)&csharp_method);

    assert(rc == 0 && csharp_method != nullptr && \
            "Failure: load_assembly_and_get_function_pointer()");

    return csharp_method(libargs, args_size);
}
#endif
