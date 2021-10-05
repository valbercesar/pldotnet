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
PGDLLEXPORT Datum plfsharp_validator(PG_FUNCTION_ARGS);
#if PG_VERSION_NUM >= 90000
PGDLLEXPORT Datum plfsharp_inline_handler(PG_FUNCTION_ARGS);
#endif

static void plfsharp_GetStructFieldPrefix(Oid type, char *field_prefix);
static char* plfsharp_BuildBlockUserFuncDecl(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    HeapTuple proc,
    pldotnet_FuncInOutInfo *func_inout_info
);

static void
plfsharp_BuildArrayArgument(
    const Oid oid,
    const size_t i,
    const size_t cursor,
    const char *template,
    char *str_ptr
);

static const char*
plfsharp_GetTriggerDataDefinition(void);

static char *plfsharp_BuildBlockArgsDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

static const char *plfsharp_BuildBlockCallFuncCall(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
);

inline static bool plfsharp_BuildPaths(pldotnet_PathConfig *paths);
const char* plfsharp_BuildBlockComposites(FunctionCallInfo fcinfo, Form_pg_proc procst);

static char* plfsharp_GetUserSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

static char* plfsharp_GetInlineSourceCode(FunctionCallInfo fcinfo);

static bool plfsharp_GetSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl
);
static bool plfsharp_CreateStructLibargs(const FunctionCallInfo fcinfo, const Form_pg_proc procst, pldotnet_FunctionDecl *function_decl);

static pldotnet_FunctionDecl*
plfsharp_GetFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation
);

static bool
plfsharp_BuildFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl);

static void plfsharp_ValidateUserFunction(const Oid oid, const FunctionCallInfo fcinfo);
static Datum plfsharp_CompileAndRunUserFunction(const FunctionCallInfo fcinfo, bool is_inline);

static char* plfsharp_BuildNullFlagArray(uint32_t elems);
static void plfsharp_BuildStructField(Oid type, const char *key, char *currval);
static char* plfsharp_BuildStructFields(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
);

static char* plfsharp_BuildStructFieldsFromTuple(TupleDesc tupdesc);
void plfsharp_BuildKeyFromIndex(size_t index, char *key);

static char*
plfsharp_GetStructFromComposite(const char *typname, TupleDesc tupdesc);

Datum
plfsharp_generic_handler(PG_FUNCTION_ARGS, bool is_inline);

static char fs_block_header[] = "\n\
namespace PlDotNETUserSpace\n\
open System\n\
open System.Dynamic\n\
open System.Collections\n\
open System.Globalization\n\
open System.Collections.Generic\n\
open System.Runtime.InteropServices\n\
open System.Linq\n\
open System.Data\n\
open System.Data.Common\n\
open FSharp.Data\n\
\n\
\n\
[<Struct>]\n\
[<StructLayout (LayoutKind.Sequential, Pack=1)>]\n\
type ArrayT<'b> =\n\
    struct\n\
        val mutable Buffer: IntPtr\n\
        val mutable ElementSize: uint\n\
        val mutable BufferSize: uint\n\
    end\n\
\n\
module SPI =\n\
    type TypeOid =\n\
        | BOOLOID    = 16\n\
        | INT8OID    = 20\n\
        | INT2OID    = 21\n\
        | INT4OID    = 23\n\
        | FLOAT4OID  = 700\n\
        | FLOAT8OID  = 701\n\
        | NUMERICOID = 1700\n\
        | VARCHAROID = 1043\n\
    [<StructLayout(LayoutKind.Sequential, Pack=1)>]\n\
    type PropertyValue =\n\
        struct\n\
            val mutable value: IntPtr\n\
            [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
            val mutable name: string\n\
            [<MarshalAs(UnmanagedType.I4)>]\n\
            val mutable typ: int\n\
            [<MarshalAs(UnmanagedType.I4)>]\n\
            val mutable nrow: int\n\
        end\n\
    \n\
    type PropertyValueReader() =\n\
        class\n\
            member self.ReadValue<'T> (handle: IntPtr) : obj =\n\
                match handle with\n\
                | h when h = IntPtr.Zero -> null\n\
                | _ ->\n\
                    match typeof<'T> with\n\
                    | t when t = typeof<string> -> handle |> Marshal.PtrToStringUTF8 :> obj\n\
                    | t when t = typeof<decimal> -> handle |> Marshal.PtrToStringAnsi |> Convert.ToDecimal :> obj\n\
                    | _ -> handle |> Marshal.PtrToStructure<'T> :> obj\n\
            member self.Read (prop : PropertyValue) : obj =\n\
                match enum<TypeOid> prop.typ with\n\
                | TypeOid.BOOLOID -> self.ReadValue<bool> prop.value\n\
                | TypeOid.INT2OID -> self.ReadValue<int16> prop.value\n\
                | TypeOid.INT4OID -> self.ReadValue<int> prop.value\n\
                | TypeOid.INT8OID -> self.ReadValue<int64> prop.value\n\
                | TypeOid.FLOAT4OID -> self.ReadValue<float32> prop.value\n\
                | TypeOid.FLOAT8OID -> self.ReadValue<double> prop.value\n\
                | TypeOid.NUMERICOID -> self.ReadValue<decimal> prop.value\n\
                | TypeOid.VARCHAROID -> self.ReadValue<string> prop.value\n\
                | _ -> raise (new NotSupportedException())\n\
        end\n\
    type PropertyValueWriter() =\n\
        class\n\
            member self.WriteValue (value: 'T) (handle: IntPtr) : Unit =\n\
                match typeof<'T> with\n\
                | t when t = typeof<string> -> raise (new NotImplementedException())\n\
                | t when t = typeof<decimal> -> raise (new NotImplementedException())\n\
                | _ -> Marshal.StructureToPtr<'T>(value, handle, false)\n\
            member self.Write (value: obj) (prop: PropertyValue) : Unit =\n\
                match enum<TypeOid> prop.typ with\n\
                | TypeOid.BOOLOID -> self.WriteValue (value :?> bool) prop.value\n\
                | TypeOid.INT2OID -> self.WriteValue (value :?> int16)  prop.value\n\
                | TypeOid.INT4OID -> self.WriteValue (value :?> int) prop.value\n\
                | TypeOid.INT8OID -> self.WriteValue (value :?> int64) prop.value\n\
                | TypeOid.FLOAT4OID -> self.WriteValue (value :?> float32) prop.value\n\
                | TypeOid.FLOAT8OID -> self.WriteValue (value :?> double) prop.value\n\
                | TypeOid.NUMERICOID -> self.WriteValue (value :?> decimal) prop.value\n\
                | TypeOid.VARCHAROID -> self.WriteValue (value :?> string) prop.value\n\
                | _ -> raise (new NotSupportedException(\"Type not supported\"))\n\
        end\n\
    type BpgsqlDbRecord() =\n\
        class\n\
            let mutable Record : Dictionary<string, obj> = new Dictionary<string, obj>()\n\
            let mutable Values : List<obj> = new List<obj>()\n\
            let mutable Reader : PropertyValueReader = new PropertyValueReader()\n\
            let mutable ColumnNames : List<string> = new List<string>()\n\
            let mutable Writer : PropertyValueWriter = new PropertyValueWriter()\n\
            let mutable Properties: List<PropertyValue> = new List<PropertyValue>()\n\
            member self.GetRecord : outref<Dictionary<string, obj>> = &Record\n\
            member self.GetValue (name: string) : obj = Record.[name]\n\
            interface IDataRecord with\n\
                member self.GetValue (ordinal: int) : obj = Values.[ordinal]\n\
                member self.get_Item (ordinal: int) : obj = ordinal |> (self :> IDataRecord).GetValue\n\
                member self.get_Item (name: string) : obj = name |> self.GetValue\n\
                member self.FieldCount : int = Values.Count\n\
                member self.GetBoolean (ordinal: int) : bool = ordinal |> (self :> IDataRecord).GetValue :?> bool\n\
                member self.GetByte (ordinal: int) : byte = ordinal |> (self :> IDataRecord).GetValue :?> byte\n\
                member self.GetDateTime (ordinal: int): DateTime = ordinal |> (self :> IDataRecord).GetValue :?> DateTime\n\
                member self.GetDecimal (ordinal: int): decimal = ordinal |> (self :> IDataRecord).GetValue :?> decimal\n\
                member self.GetFieldType (ordinal: int) : Type = (ordinal |> (self :> IDataRecord).GetValue).GetType()\n\
                member self.GetGuid (ordinal: int) : Guid = ordinal |> (self :> IDataRecord).GetValue :?> Guid\n\
                member self.GetInt16 (ordinal: int) : int16 = ordinal |> (self :> IDataRecord).GetValue :?> int16\n\
                member self.GetInt32 (ordinal: int) : int = ordinal |> (self :> IDataRecord).GetValue :?> int\n\
                member self.GetInt64 (ordinal: int) : int64 = ordinal |> (self :> IDataRecord).GetValue :?> int64\n\
                member self.GetFloat (ordinal: int) : float32 = ordinal |> (self :> IDataRecord).GetValue :?> float32\n\
                member self.GetDouble (ordinal: int) : float = ordinal |> (self :> IDataRecord).GetValue :?> float\n\
                member self.GetName (ordinal: int) : string = ColumnNames.[ordinal]\n\
                member self.GetDataTypeName (ordinal: int) : string = (ordinal |> (self :> IDataRecord).GetFieldType).ToString()\n\
                member self.GetChar (ordinal: int) : char = ordinal |> (self :> IDataRecord).GetValue :?> char\n\
                member self.GetString (ordinal: int) : string = ordinal |> (self :> IDataRecord).GetValue :?> string\n\
                member self.GetValues (values: obj []) : int =\n\
                    let count : int = Math.Min(Values.Count, values.Length)\n\
                    for index in 0 .. count - 1 do\n\
                        values.[index] <- index |> (self :> IDataRecord).GetValue\n\
                    count\n\
                member self.GetOrdinal (name: string) : int = (fun e -> e = name) |> ColumnNames.FindIndex\n\
                member self.GetBytes (ordinal: int, fieldOffset: int64, buffer: byte [], bufferoffset: int, length: int) : int64 =\n\
                    raise (new NotImplementedException())\n\
                member self.GetChars (ordinal: int, fieldoffset: int64, buffer: char [], bufferoffset: int, length: int) : int64 =\n\
                    raise (new NotImplementedException())\n\
                member self.GetData (ordinal: int) : IDataReader = raise (new NotImplementedException())\n\
                member self.IsDBNull (ordinal: int) : bool = null = (ordinal |> (self :> IDataRecord).GetValue)\n\
\n\
            member self.GetDbDataReader(ordinal: int) : DbDataReader = raise (new NotSupportedException());\n\
            member self.SetBoolean (ordinal: int) (value: bool) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetByte (ordinal: int) (value: byte) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetChar (ordinal: int) (value: char) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetDataRecord (ordinal: int) (value: IDataRecord) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetDateTime (ordinal: int) (value: DateTime) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetDecimal (ordinal: int) (value: Decimal) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetDouble (ordinal: int) (value: float) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetFloat (ordinal: int) (value: float32) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetGuid (ordinal: int) (value: Guid) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetInt16 (ordinal: int) (value: int16) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetInt32 (ordinal: int) (value: int) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetInt64 (ordinal: int) (value: int64) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetString (ordinal: int) (value: string) : unit = self.SetValue ordinal (value :> obj)\n\
            member self.SetValue (ordinal: int) (value: obj) : unit = self.SetRecordValue ordinal value\n\
            member self.SetValues([<ParamArray>] (values: obj array)) : int =\n\
                let minValue = Math.Min(values.Length, (self :> IDataRecord).FieldCount)\n\
                for i in 0..minValue - 1 do\n\
                    self.SetRecordValue i (values.[i])\n\
                minValue\n\
            member self.SetDBNull (ordinal: int) = self.SetRecordValue ordinal DBNull.Value\n\
            member self.GetDataRecord (ordinal: int) : DbDataRecord  = ordinal |> (self :> IDataRecord).GetValue :?> DbDataRecord\n\
            member self.GetDataReader(ordinal: int) : DbDataReader = ordinal |> self.GetDbDataReader\n\
            member self.SetRecordValue (ordinal: int) (value: obj) =\n\
                Values.[ordinal] <- value\n\
                Record.[ColumnNames.[ordinal]] <- value\n\
                Writer.Write value Properties.[ordinal]\n\
            member self.AddProperty (property: PropertyValue) : unit =\n\
                let value = Reader.Read property\n\
                Record.[property.name] <- value\n\
                ColumnNames.Add(property.name)\n\
                Values.Add(value)\n\
                Properties.Add(property)\n\
                ()\n\
        end\n\
\n\
    [<DllImport(@\"" STR(PKG_LIBDIR) "/pldotnet.so\", CallingConvention=CallingConvention.Cdecl)>]\n\
    extern int pldotnet_SPIExecute(string cmd, int64 limit)\n\
\n\
    let mutable FuncRecords : List<BpgsqlDbRecord> = new List<BpgsqlDbRecord>()\n\
    let ResetFuncRecords () : unit =\n\
        match FuncRecords.Count with\n\
        | 0 -> ()\n\
        | _ ->\n\
            FuncRecords <- new List<BpgsqlDbRecord>()\n\
            ()\n\
\n\
    let AddProperty (arg: IntPtr) (funcoid: int) : unit =\n\
        let prop : PropertyValue = arg |> Marshal.PtrToStructure<PropertyValue>\n\
        match FuncRecords.Count < prop.nrow + 1 with\n\
        | true -> FuncRecords.Add(new BpgsqlDbRecord())\n\
        | _ -> ()\n\
        FuncRecords.[prop.nrow].AddProperty prop\n\
        ()\n\
    let Execute (cmd: string) (limit: int64) : List<BpgsqlDbRecord> =\n\
        pldotnet_SPIExecute(cmd, limit) |> ignore\n\
        FuncRecords\n\
\n\
module Helper =\n\
    let wrap (isnull: bool) a =\n\
        match isnull with\n\
        | true -> None\n\
        | false -> Some a\n\
    let toDecimal (isnull: bool) (str: string) : decimal option =\n\
        match isnull with\n\
        | true -> None\n\
        | false ->\n\
            match System.Decimal.TryParse(str) with\n\
            | true, v -> Some v\n\
            | _ -> None\n\
    let arrayToString (isnull : bool) (a : ArrayT<'b>) : string [] option =\n\
        match isnull with\n\
        | true -> None\n\
        | false ->\n\
            let elsize = typedefof<IntPtr> |> Marshal.SizeOf\n\
            let is = seq { 0..((int)a.BufferSize)-1 }\n\
            let ptrToStr (buffer : IntPtr) (offset : int) : string =\n\
                let str = Marshal.ReadIntPtr(buffer, offset) |> Marshal.PtrToStringUTF8\n\
                str\n\
            try\n\
                [|for i in is do yield (ptrToStr a.Buffer (i * elsize))|] |> Some\n\
            with\n\
                | _ -> None\n\
    let arrayToDbRecord (array: ArrayT<SPI.PropertyValue>)  : SPI.BpgsqlDbRecord option =\n\
        match 1u > array.BufferSize with\n\
        | true -> None\n\
        | false ->\n\
            let mutable dbRecord = new SPI.BpgsqlDbRecord()\n\
            for i in 0u..array.BufferSize-1u do\n\
                let cursor = IntPtr.Add(array.Buffer, (int) (array.ElementSize * i))\n\
                let prop = Marshal.PtrToStructure<SPI.PropertyValue>(cursor)\n\
                dbRecord.AddProperty(prop)\n\
            Some dbRecord\n\
    let arrayToDecimal (isnull: bool) (a : ArrayT<'b>) : decimal [] option =\n\
        match isnull with\n\
        | true -> None\n\
        | false ->\n\
            CultureInfo.CurrentCulture = new CultureInfo(\"en-US\", false) |> ignore\n\
            match arrayToString isnull a with\n\
            | Some strs ->\n\
                Array.ConvertAll<string, decimal>(strs, fun item -> Convert.ToDecimal(item)) |> Some\n\
            | _ -> None\n\
    let fromArrayT<'b> (isnull: bool) (a : ArrayT<'b>) : 'b[] option =\n\
        match isnull with\n\
        | true -> None\n\
        | false ->\n\
            let size = (int) (a.BufferSize * a.ElementSize)\n\
            let mutable input : 'b array = Array.zeroCreate ((int) a.BufferSize)\n\
            let mutable bytes : byte array = Array.zeroCreate size\n\
            Marshal.Copy(a.Buffer, bytes, 0, size)\n\
            System.Buffer.BlockCopy(bytes, 0, input, 0, size)\n\
            Some input\n";
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
    static member CallFunction (arg: System.IntPtr) (argLength: int) = \n\
        let mutable libargs = Marshal.PtrToStructure<LibArgs> arg\n";

static char fs_block_footer[] = "\n\
        Marshal.StructureToPtr(libargs, arg, false)\n\
        0";

static void
plfsharp_GetStructFieldPrefix(Oid type, char *field_prefix)
{
    static const char unmanaged_template[] = "\
        [<MarshalAs(UnmanagedType.%s)>]\n%s";

    static const char val[] = "\
        val mutable ";

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
plfsharp_BuildStructField(Oid type, const char *key, char *currval)
{
    size_t length;

    plfsharp_GetStructFieldPrefix(type, currval);

    length = strlen(currval);

    SNPRINTF(
        currval + length,
        length + 1,
        "%s: %s\n",
        key,
        pldotnet_GetCompatibleNetTypeName(type, true, false)
    );
}

void
plfsharp_BuildKeyFromIndex(size_t index, char *key)
{
    SNPRINTF(key, 128, "arg%lu", index);
}

static void
plfsharp_BuildStructValue(Oid type, size_t index, char *currval)
{
    char key[128];
    plfsharp_BuildKeyFromIndex(index, key);
    plfsharp_BuildStructField(type, key, currval);
}

/*
 * This function aims to build the trigger field inside a struct,
 * including the required annotations and types
 *
 * For example:
 *     [<MarshalAs(UnmanagedType.Struct)>]
 *     val mutable OLD : TriggerTuple
 *     [<MarshalAs(UnmanagedType.Struct)>]
 *     val mutable NEW : TriggerTuple
 *
 * @param [in] fcinfo Data passed to a fmgr-called function - it's from postgres
 * @param [in] procst A struct pointer containing the procedure information
 * @param [in] validation a boolean to indicate it we are in the validation mode
 * @return a pointer to palloced string which contains the struct fields
 */
static char*
plfsharp_BuildTriggerFields(FunctionCallInfo fcinfo)
{
    return "\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable tg_info: TriggerInfo\n";
}

/*
 * This function aims to build all fields inside a struct,
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
 * @param [in] validation a boolean to indicate it we are in the validation mode
 * @return a pointer to palloced string which contains the struct fields
 */
static char*
plfsharp_BuildNonTriggerFields(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
    )
{
    char *block2str;
    char *cursor;
    uint32_t i;
    Datum argdatum;
    bool isarr;
    Oid type;
    char currval[512];
    Oid *argtype = procst->proargtypes.values;
    uint32_t nargs = procst->pronargs;
    size_t pos = 0;
    size_t totalsize = 0;

    static const char array_template[] = "\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable arg%u : ArrayT<%s>\n";

    if (0 == nargs)
    {
        block2str = (char *) palloc0(1);
        SNPRINTF(block2str, 1, "%s", "\0");
        return block2str;
    }

    for (i = 0; i < nargs; ++i)
    {
        argdatum = pldotnet_GetArgDatum(fcinfo, i);
        isarr = pldotnet_SetArrayInfo(
            argdatum,
            argtype[i],
            i,
            array_template,
            true,
            func_inout_info
        );

        type = isarr ? func_inout_info->arrayinfo[i].typelem : argtype[i];

        if (isarr)
            totalsize += strlen(func_inout_info->arrayinfo[i].csharpdecl) + 1;
        else
        {
            plfsharp_BuildStructValue(type, i, currval);
            totalsize += strlen(currval) + 1;
        }
    }

    block2str = (char*) palloc0(totalsize);

    for (i = 0; i < nargs; ++i)
    {
        cursor = block2str + pos;
        if (pldotnet_IsArray(i, func_inout_info))
        {
            SNPRINTF(cursor, totalsize - pos, "%s", func_inout_info->arrayinfo[i].csharpdecl);
        }
        else
        {
            plfsharp_BuildStructValue(argtype[i], i, currval);
            SNPRINTF(cursor, totalsize - pos, "%s", currval);
        }
        pos += strlen(cursor);
    }

    return block2str;
}

static char*
plfsharp_BuildStructFields(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    if (CALLED_AS_TRIGGER(fcinfo))
        return plfsharp_BuildTriggerFields(fcinfo);

    return plfsharp_BuildNonTriggerFields(
        fcinfo,
        proc,
        procst,
        validation,
        func_inout_info
    );
}

static char*
plfsharp_BuildNullFlagArray(uint32_t elems)
{
    size_t size;
    char *null_flag_array;
    const char *type_name;
    static const char array_template[] = "\n\
        [<MarshalAs(UnmanagedType.ByValArray,ArraySubType=UnmanagedType.U1,SizeConst=%u)>]\n\
        val argsnull: %s array";

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

static const char*
plfsharp_GetTriggerDataDefinition(void)
{
    return "\n\
[<StructLayout(LayoutKind.Sequential,Pack=1)>]\n\
type TriggerInfo =\n\
    struct\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_name: string\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_table_name: string\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_table_schema: string\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_when: string\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_level: string\n\
        [<MarshalAs(UnmanagedType.LPUTF8Str)>]\n\
        val mutable tg_event: string\n\
        [<MarshalAs(UnmanagedType.U8)>]\n\
        val mutable tg_relid: uint64\n\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable tg_args_array: ArrayT<System.IntPtr>\n\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable tg_relatts_array: ArrayT<System.IntPtr>\n\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable tg_new_array: ArrayT<SPI.PropertyValue>\n\
        [<MarshalAs(UnmanagedType.Struct)>]\n\
        val mutable tg_old_array: ArrayT<SPI.PropertyValue>\n\
    end\n\
type TriggerTuples(tg_old_array: ArrayT<SPI.PropertyValue> byref, tg_new_array: ArrayT<SPI.PropertyValue> byref) =\n\
    class\n\
        member val NEW = Helper.arrayToDbRecord tg_new_array with get\n\
        member val OLD = Helper.arrayToDbRecord tg_old_array with get\n\
    end\n\
type TriggerData(tg_info: TriggerInfo byref) =\n\
    class\n\
        member val tg_args = Helper.arrayToString false tg_info.tg_args_array  with get\n\
        member val tg_relatts = Helper.arrayToString false tg_info.tg_relatts_array with get\n\
        member val tg_tuples = new TriggerTuples(ref tg_info.tg_old_array, ref tg_info.tg_new_array) with get\n\
        member val tg_info = tg_info\n\
    end\n\
\n";
}

static char*
plfsharp_BuildBlockArgsDecl(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *values;
    char *block2string;
    char *null_flag_array;
    const char *bool_name;
    char result[512];
    size_t totalsize;
    static const char struct_template[] = "\
[<Struct>]\n\
[<StructLayout (LayoutKind.Sequential, Pack=1)>]\n\
type LibArgs =\n\
    struct\n\
        %s\n\
        [<MarshalAs(UnmanagedType.U1)>]\n\
        val mutable resunull: %s\n%s%s\
    end\n";

    if (!pldotnet_TypeSupported(procst->prorettype))
        elog(ERROR, "[pldotnet]: unsupported type on return");

    bool_name = pldotnet_GetCompatibleNetTypeName(BOOLOID, true, false);

    null_flag_array = plfsharp_BuildNullFlagArray(procst->pronargs);

    values = plfsharp_BuildStructFields(fcinfo, proc, procst, validation, func_inout_info);

    plfsharp_BuildStructField(procst->prorettype, "resu", result);

    totalsize = strlen(struct_template)
              + strlen(null_flag_array)
              + strlen(bool_name)
              + strlen(values)
              + strlen(result);

    block2string = (char*) palloc0(totalsize);

    snprintf(
        block2string,
        totalsize,
        struct_template,
        null_flag_array,
        bool_name,
        values,
        result
    );

    return block2string;
}

static const char*
plfsharp_GetTriggerTuples(FunctionCallInfo fcinfo)
{
    if (CALLED_AS_TRIGGER(fcinfo))
    {
        return "\
        let mutable TD : TriggerData = new TriggerData(ref libargs.tg_info)\n";
    }

    return "";
}

static char *
plfsharp_BuildBlockUserFuncDecl(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    HeapTuple proc,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *block2str, *str_ptr, *argnm, *source_text;
    int argnm_size, i, nnames, cursize=0, totalsize;
    bool isnull;
    char *func;
    Oid type;
    const char *type_name;
    size_t line_length;
    const char let[] = "let ";
    const char rec[] = "rec ";
    const char func_signature_indent[] = "        ";
    const char func_body_indent[] = "            ";
    const size_t body_indent_size = strlen(func_body_indent);
    char *user_line;
    const char end_fun_decl[] = " =\n";
    const char end_fun[] = "\n";
    const char *tg_tuples = plfsharp_GetTriggerTuples(fcinfo);
    int nargs = procst->pronargs;
    Datum *argname, argnames, prosrc;
    Oid *argtypes = procst->proargtypes.values;

    /* Function name */
    func = NameStr(procst->proname);

    /* Source code */
    prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    source_text = DatumGetCString(DirectFunctionCall1(textout, prosrc));

    argnames = SysCacheGetAttr(PROCOID, proc,
        Anum_pg_proc_proargnames, &isnull);

    if (!isnull)
      deconstruct_array(DatumGetArrayTypeP(argnames), TEXTOID, -1, false,
          'i', &argname, NULL, &nnames);

    /* Calculates the total amount in bytes of F# src text for
     * the function declaration according nr of arguments
     * and function body necessary indentation
     */
    totalsize = strlen(func_signature_indent)
              + strlen(let)
              + strlen(rec)
              + strlen(tg_tuples)
              + strlen(func)
              + strlen(" ");

    for (i = 0; i < nargs; i++)
    {
        argnm = DatumGetCString(DirectFunctionCall1(textout, argname[i]));

        type = pldotnet_IsArray(i, func_inout_info) ? func_inout_info->arrayinfo[i].typelem : argtypes[i];

        argnm_size = strlen(argnm) + strlen(" (: [] option)") + strlen(pldotnet_GetCompatibleNetTypeName(type, false, false));
        /* +1 here is the space between type" "argname declaration */
        totalsize += 1 + argnm_size;
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

    SNPRINTF(
        block2str,
        totalsize - cursize,
        "%s%s%s%s",
        tg_tuples,
        func_signature_indent,
        let,
        func
    );

    cursize = strlen(block2str);

    for (i = 0; i < nargs; i++)
    {
        argnm = DatumGetCString(DirectFunctionCall1(textout, argname[i]));

        argnm_size = strlen(argnm);
        str_ptr = (char *)(block2str + cursize);

        type_name = pldotnet_GetCompatibleNetTypeName(argtypes[i], false, false);

        if (pldotnet_IsArray(i, func_inout_info))
        {
            type_name = pldotnet_GetCompatibleNetTypeName(func_inout_info->arrayinfo[i].typelem, false, false);
            SNPRINTF(str_ptr, totalsize - cursize, " (%s: %s [] option)", argnm, type_name);
        }
        else
        {
            SNPRINTF(str_ptr, totalsize - cursize, " (%s: %s option)", argnm, type_name);
        }
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

static void
plfsharp_BuildArrayArgument(
    const Oid oid,
    const size_t i,
    const size_t cursor,
    const char *template,
    char *str_ptr)
{
    static const char *arrayToDecimal = "arrayToDecimal";
    static const char *arrayToString = "arrayToString";

    if (NUMERICOID == oid)
        snprintf(
            str_ptr,
            cursor,
            template,
            arrayToDecimal,
            i,
            i
        );
    else if (pldotnet_IsTextType(oid))
        snprintf(
            str_ptr,
            cursor,
            template,
            arrayToString,
            i,
            i
        );
    else
        snprintf(
            str_ptr,
            cursor,
            " (Helper.fromArrayT<%s> libargs.argsnull.[%lu] libargs.arg%lu)",
            pldotnet_NeedsIntPtr(oid) ?
                "IntPtr" : pldotnet_GetCompatibleNetTypeName(
                    oid,
                    true,
                    false
                ),
            i,
            i
        );
}

static const char*
plfsharp_BuildBlockCallTrigger(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    const char *body_template
)
{
    const char *func = NameStr(procst->proname);
    size_t size = strlen(body_template)
                + strlen(func)
                + 1;

    char *block2str = (char*) palloc0(size);

    snprintf(
        block2str,
        size,
        body_template,
        func,
        "",
        ""
    );

    return block2str;
}

static const char*
plfsharp_BuildBlockCallNonTrigger(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    const char *body_template,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    /* Function name */
    const char *func = NameStr(procst->proname);
    static const char *toDecimal = "toDecimal";
    static const char *wrap = "wrap";
    const char *toString = NUMERICOID == procst->prorettype ? ".ToString()" : "";
    static const char *arg_template = " (Helper.%s libargs.argsnull.[%d] libargs.arg%d)";
    size_t arg_size = strlen(arg_template);
    size_t nargs = procst->pronargs;
    size_t cursize = 0;

    char *block2str, *str_ptr;
    char *func_call;
    size_t i, totalsize, call_func_size;

    /* TODO:  review for nargs > 9 */
    if (0 == nargs)
    {
        totalsize = strlen(body_template)
                   + strlen(func)
                   + 1;
        block2str = (char *)palloc0(totalsize);
        snprintf(
            block2str,
            totalsize,
            body_template,
            func,
            "",
            ""
        );
        return block2str;
    }

    call_func_size = strlen(func) + (arg_size + strlen("fromArray<>()") + 10) * nargs;

    func_call = (char*) palloc0(call_func_size);

    SNPRINTF(func_call, call_func_size, "%s", func);
    cursize = strlen(func_call);

    for (i = 0; i < nargs; ++i)
    {
        str_ptr = (char*) (func_call + cursize);
        if (pldotnet_IsArray(i, func_inout_info))
            plfsharp_BuildArrayArgument(
                func_inout_info->arrayinfo[i].typelem,
                i,
                call_func_size - cursize,
                arg_template,
                str_ptr
            );
        else if (NUMERICOID == procst->proargtypes.values[i])
            snprintf(str_ptr, call_func_size - cursize, arg_template, toDecimal, i, i);
        else
            snprintf(str_ptr, call_func_size - cursize, arg_template, wrap, i, i);
        cursize += strlen(str_ptr);
    }

    totalsize = strlen(body_template)
              + strlen(toString)
              + call_func_size;

    block2str = (char *) palloc0(totalsize);

    snprintf(
        block2str,
        totalsize,
        body_template,
        func_call,
        toString,
        ""
    );

    return block2str;
}

static const char *
plfsharp_BuildBlockCallFuncCall(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    static const char *body_template = "\
        let res =\n\
            try\n\
                %s\n\
            with\n\
                | _ -> None\n\
        libargs.resunull <- \n\
            match res with\n\
            | None -> true\n\
            | Some v ->\n\
                libargs.resu <- v%s\n\
                false\n\
        %s";

    if (CALLED_AS_TRIGGER(fcinfo))
        return plfsharp_BuildBlockCallTrigger(fcinfo, procst, body_template);
    else
        return plfsharp_BuildBlockCallNonTrigger(fcinfo, procst, body_template, func_inout_info);
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
plfsharp_BuildBlockCompositesFromProcedure(
    FunctionCallInfo fcinfo,
    Form_pg_proc procst
)
{
    HeapTuple type;
    Form_pg_type typeinfo;
    TupleDesc tupdesc;
    char *composite = nullptr;
    size_t composite_size = 0;
    size_t elems = 0;
    size_t pos = 0;
    Oid *argtype = procst->proargtypes.values;

    GSList *composite_list = nullptr, *next = nullptr;

    for (size_t i = 0; i < procst->pronargs; ++i)
    {
        /* TODO: review this */
        if (pldotnet_IsSimpleType(argtype[i]) || pldotnet_IsTextType(argtype[i]))
            continue;

        type = SearchSysCache1(TYPEOID, ObjectIdGetDatum(argtype[i]));
        if (!HeapTupleIsValid(type))
            elog(ERROR, "[plfsharp_BuildBlockCompositesFromProcedure]: cache lookup failed for type %u", argtype[i]);

        typeinfo = (Form_pg_type) GETSTRUCT(type);
        if (typeinfo->typtype == TYPTYPE_COMPOSITE)
        {
            tupdesc = lookup_rowtype_tupdesc(argtype[i], typeinfo->typtypmod);
            composite = plfsharp_GetStructFromComposite(
                NameStr(typeinfo->typname),
                tupdesc
            );
            composite_size += strlen(composite) + 1;
            composite_list = g_slist_prepend(composite_list, composite);

            ReleaseTupleDesc(tupdesc);
            elems += 1;
        }

        ReleaseSysCache(type);
    }

    if (0 < elems && nullptr != composite_list)
    {
        composite = (char*) palloc0(composite_size) + 1;

        next = g_slist_reverse(composite_list);
        while(nullptr != next)
        {
            SNPRINTF(composite, composite_size - pos, "%s", (char*) next->data);
            next = next->next;
        }

        g_slist_free(composite_list);
    }

    if (nullptr == composite) return "";

    return composite;
}

const char*
plfsharp_BuildBlockComposites(FunctionCallInfo fcinfo, Form_pg_proc procst)
{
    if (CALLED_AS_TRIGGER(fcinfo))
        return plfsharp_GetTriggerDataDefinition();

    return plfsharp_BuildBlockCompositesFromProcedure(
        fcinfo,
        procst
    );
}

static char*
plfsharp_GetUserSourceCode(
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    Form_pg_proc procst,
    bool validation,
    pldotnet_FuncInOutInfo *func_inout_info
)
{
    char *source_code = nullptr;
    const char * fs_block_composite_decl = plfsharp_BuildBlockComposites(fcinfo, procst);
    const char * fs_block_args_decl = plfsharp_BuildBlockArgsDecl(
        fcinfo,
        proc,
        procst,
        validation,
        func_inout_info
    );
    const char * fs_block_userfunc_decl = plfsharp_BuildBlockUserFuncDecl(fcinfo, procst, proc, func_inout_info);
    const char * fs_block_callfunc_call = plfsharp_BuildBlockCallFuncCall(fcinfo, procst, func_inout_info);

    size_t source_code_size = strlen(fs_block_header)
                     + strlen(fs_block_composite_decl)
                     + strlen(fs_block_args_decl)
                     + strlen(fs_block_userclass_header)
                     + strlen(fs_block_callfunc)
                     + strlen(fs_block_userfunc_decl)
                     + strlen(fs_block_callfunc_call)
                     + strlen(fs_block_footer) + 1;

    source_code = (char*) palloc0(source_code_size);
    SNPRINTF(source_code, source_code_size, "%s%s%s%s%s%s%s%s",
                                            fs_block_header,
                                            fs_block_composite_decl,
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
    bool validation,
    pldotnet_FunctionDecl *function_decl)
{
    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument: function_decl is null");

    if (nullptr != function_decl->source.source_code)
        elog(ERROR, "[pldotnet]: ArgSouce.source code should be null at this point: \n%s", function_decl->source.source_code);

    if (is_inline)
        function_decl->source.source_code = plfsharp_GetInlineSourceCode(fcinfo);
    else
        function_decl->source.source_code = plfsharp_GetUserSourceCode(
            fcinfo,
            proc, procst,
            validation,
            &(function_decl->func_inout_info)
        );

    return nullptr != function_decl->source.source_code;
}

static bool
plfsharp_CreateStructLibargs(
    const FunctionCallInfo fcinfo,
    const Form_pg_proc procst,
    pldotnet_FunctionDecl *function_decl
)
{
    pldotnet_FuncInOutInfo *func_inout_info = &(function_decl->func_inout_info);
    function_decl->args = pldotnet_CreateCStructLibargs(fcinfo, procst, true, func_inout_info);
    function_decl->args_length = func_inout_info->typesize_nullflags +
                                 func_inout_info->typesize_args +
                                 func_inout_info->typesize_result;

    return nullptr != function_decl->args && function_decl->args_length > 0;
}

static bool
plfsharp_BuildFunctionDecl(
    Oid oid,
    FunctionCallInfo fcinfo,
    HeapTuple proc,
    bool is_inline,
    bool validation,
    pldotnet_FunctionDecl *function_decl)
{
    Form_pg_proc procst;
    static pldotnet_PathConfig paths;
    static dotnet_loader loader = nullptr;

    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument, function_decl is null");

    if (!plfsharp_BuildPaths(&paths))
        elog(ERROR, "[pldotnet]: Could not build paths");

    if (nullptr == loader && nullptr == (loader = GetNetLoadAssemblySetup(paths.config_path, paths.prefix)))
        elog(ERROR, "[pldotnet]: Could not obtain .NET Loader");

    procst = (Form_pg_proc) GETSTRUCT(proc);

    /* save some basic data */
    function_decl->source.func_oid = (uint32_t) oid;
    function_decl->ret_type = procst->prorettype;

    if (!plfsharp_GetSourceCode(fcinfo, proc, procst, is_inline, validation, function_decl))
        elog(ERROR, "[pldotnet]: Could not obtain the source code");

    if (!pldotnet_CompileUserFunction(loader, &paths, &(function_decl->source)))
        elog(ERROR, "[pldotnet]: Could not compile this function. See the errors on /var/log/postgresql");

    function_decl->dotnet_method = pldotnet_GetUserMethod(loader, &paths);

    return nullptr != function_decl->dotnet_method;
}

static char*
plfsharp_BuildStructFieldsFromTuple(TupleDesc tupdesc)
{
    const char *key;
    Oid type_attr;
    char *cursor;
    char buffer[1024];
    size_t pos = 0;
    size_t totalsize = 0;
    char *fields = nullptr;

    buffer[0] = 0;

    for (size_t i = 0; i < tupdesc->natts; ++i)
    {
        Form_pg_attribute attr = TupleDescAttr(tupdesc, i);
        type_attr = attr->atttypid;

        if (InvalidOid != type_attr && !attr->attisdropped)
        {
            key = NameStr(TupleDescAttr(tupdesc, i)->attname);
            plfsharp_BuildStructField(type_attr, key, buffer);
            totalsize += strlen(buffer) + 1;
        }
    }

    buffer[0] = 0;

    fields = (char*) palloc(totalsize + 1);

    cursor = fields;

    for (size_t i = 0; i < tupdesc->natts; ++i)
    {
        Form_pg_attribute attr = TupleDescAttr(tupdesc, i);
        type_attr = attr->atttypid;

        if (InvalidOid != type_attr && !attr->attisdropped)
        {
            key = NameStr(TupleDescAttr(tupdesc, i)->attname);
            plfsharp_BuildStructField(type_attr, key, buffer);
            SNPRINTF(cursor, totalsize - pos, "%s", buffer);
            pos += strlen(buffer);
            cursor = fields + pos;
        }
    }

    return fields;
}

static char*
plfsharp_GetStructFromComposite(const char* typname, TupleDesc tupdesc)
{
    const char *template = "\n\
[<Struct>]\n\
[<StructLayout(LayoutKind.Sequential,Pack=1)>]\n\
type %s =\n\
    struct\n%s\
    end\n\n";

    char *output = nullptr;
    size_t output_size = 0;

    const char *fields = plfsharp_BuildStructFieldsFromTuple(tupdesc);

    output_size = strlen(template) + strlen(typname) + strlen(fields) + 1;

    output = (char*) palloc(output_size * sizeof(char));

    if (output_size <= snprintf(output, output_size, template, typname, fields))
        elog(ERROR, "[pldotnet]: String too long for buffer. ");
    return output;
}

static void
plfsharp_ValidateUserFunction(const Oid oid, const FunctionCallInfo fcinfo)
{
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Could not obtain info about %u", oid);

    plfsharp_GetFunctionDecl(
        oid,
        fcinfo,
        proc,
        false,
        true
    );

    /* END */
    pldotnet_ReleasePostgresHeapTuple(proc);
}

static Datum
plfsharp_CompileAndRunUserFunction(
    const FunctionCallInfo fcinfo,
    bool is_inline
)
{
    HeapTuple proc;
    Form_pg_proc procst;
    pldotnet_FunctionDecl *function_decl = nullptr;

    /* WARNING WE NEED TO RELEASE THE SYSCACHE AT THE END IF PROC != nullptr */
    /* START */
    proc = pldotnet_GetPostgresHeapTuple(fcinfo->flinfo->fn_oid);

    function_decl = plfsharp_GetFunctionDecl(
        fcinfo->flinfo->fn_oid,
        fcinfo,
        proc,
        is_inline,
        false
    );

    procst = (Form_pg_proc) GETSTRUCT(proc);

    if (!is_inline && !plfsharp_CreateStructLibargs(fcinfo, procst, function_decl))
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

static pldotnet_FunctionDecl*
plfsharp_GetFunctionDecl(
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

    plfsharp_BuildFunctionDecl(
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

PG_FUNCTION_INFO_V1(plfsharp_validator);
Datum plfsharp_validator(PG_FUNCTION_ARGS)
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

        plfsharp_ValidateUserFunction(funcoid, fcinfo);

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

