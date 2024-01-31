// <copyright file="RecordHandler.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright (c) 2023 Brick Abode
//
// This code is subject to the terms of the PostgreSQL License.
// The full text of the license can be found in the LICENSE file
// at the top level of the pldotnet repository.
//
// </copyright>
using System;
using System.Net.NetworkInformation; // for Macaddr
using System.Runtime.InteropServices;
using System.Text;
using NpgsqlTypes;
using PlDotNET.Common;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL record data type.
    /// </summary>
    /// <remarks>
    /// Normally when we need to convert dotnet values to PostgreSQL
    /// datum, we know the types at compile time and are able to
    /// generate precise code for the conversion.  However, Records
    /// can be dynamic, so we don't always know the types at compile
    /// time, so here we dynamically examine the objects and convert
    /// them to Datum, finally converting the entire set into a single
    /// Record datum.
    /// </remarks>
    [OIDHandler(OID.RECORDOID, OID.RECORDARRAYOID)]
    public class RecordHandler : ObjectTypeHandler<object[]>
    {
        public RecordHandler()
        {
            this.ElementOID = OID.RECORDOID;
            this.ArrayOID = OID.RECORDARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void resize_result(IntPtr output, int length);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_GetResultLength(IntPtr result);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_GetResult(
                              IntPtr result,
                              int offset,
                              out IntPtr value,
                              [MarshalAs(UnmanagedType.U1)] out bool is_null,
                              out OID oid);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_GetRecordAttributes(
                                IntPtr recordDatum,
                                int numAttrs,
                                IntPtr[] datums,
                                byte[] isNull,
                                OID[] oid);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_GetNumberOfRecordAttributes(IntPtr result);

        public static (NpgsqlDbType, object) GetNpgsqlTypeAndValue(object obj)
        {
            if (obj is BaseNpgsqlParameter param)
            {
                // we honor NpgsqlParameter's setting
                return (param.NpgsqlDbType, param.Value);
            }

            switch (obj)
            {
                case bool _:
                    return (NpgsqlDbType.Boolean, obj);
                case byte _:
                    return (NpgsqlDbType.Smallint, obj);
                case short _:
                    return (NpgsqlDbType.Smallint, obj);
                case int _:
                    return (NpgsqlDbType.Integer, obj);
                case long _:
                    return (NpgsqlDbType.Bigint, obj);
                case float _:
                    return (NpgsqlDbType.Real, obj);
                case double _:
                    return (NpgsqlDbType.Double, obj);
                case string _:
                    return (NpgsqlDbType.Text, obj);
                case PhysicalAddress _:
                    return (NpgsqlDbType.MacAddr, obj);
                default:
                    throw new SystemException(
                        $"Unrecognized object for type conversion: ({obj.GetType().Name}){obj}. Please use NpgsqlParameter to specify the type.");
            }
        }

#nullable enable

        /// <summary>
        /// Converts a single value to its corresponding PostgreSQL data type and returns the datum and OID.
        /// It is for not nullable values. You must filter for nulls before calling.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>A tuple containing the datum and OID.</returns>
        public static (IntPtr, OID) SingleValueOutput(object value)
        {
            (NpgsqlDbType dbt, object obj) = GetNpgsqlTypeAndValue(value);
            OID oid = (OID)NpgsqlHelper.FindOid(dbt);
            IntPtr datum = DatumConversion.OutputNullableValue(oid, obj);

            return (datum, oid);
        }

        /// <inheritdoc />
        public override object[] InputValue(IntPtr recordDatum)
        {
            // Return an empty array if the pointer is Null
            if (recordDatum == IntPtr.Zero)
            {
                return new object[0];
            }

            int len = pldotnet_GetNumberOfRecordAttributes(recordDatum);

            IntPtr[] datums = new IntPtr[len];
            byte[] nullmap = new byte[len];
            OID[] oids = new OID[len];

            if (pldotnet_GetRecordAttributes(recordDatum, len, datums, nullmap, oids) != 0)
            {
                throw new SystemException($"Could not get records attributes from record datum at {recordDatum.ToInt64():x}");
            }

            object[] objects = new object[len];

            for (int i = 0; i < len; i++)
            {
                objects[i] = nullmap[i] != 0 ? null! : DatumConversion.InputValue(datums[i], oids[i], true);
            }

            return objects;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(object[] values)
        {
            // for each
            throw new SystemException($"Do not call `OutputValue()` on a Record.");
        }

        /// <summary>
        /// Convert a C `pldotnet_Result*` into a C# `object[]`
        /// </summary>
        public object[] InputGetValue(IntPtr result)
        {
            // Return an empty array if the pointer is Null
            if (result == IntPtr.Zero)
            {
                return new object[0];
            }

            int len = pldotnet_GetResultLength(result);
            object[] objects = new object[len];

            for (int i = 0; i < len; i++)
            {
                IntPtr datum;
                bool is_null;
                OID oid;

                if (pldotnet_GetResult(result, i, out datum, out is_null, out oid) != 0)
                {
                    throw new SystemException($"Could not get value {i} from pldotnet_Result at {result.ToInt64():x}");
                }

                // We use the null-forgiving operator because `null` is correct here.
                objects[i] = is_null ? null! : DatumConversion.InputValue(datum, oid, true);
            }

            return objects;
        }

        /// <summary>
        /// Sets the field value in the pldotnet_Result pointer at the specified offset.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <param name="output">The pldotnet_Result pointer.</param>
        /// <param name="offset">The offset at which to set the field value.</param>
        public bool OutputSetField(object value, IntPtr output, int offset)
        {
            var (datum, oid) = SingleValueOutput(value);

            OutputResult.SetDatumResult(datum, false, output, offset, (uint)oid);
            return true;
        }

        /// <summary>
        /// Convert a C# `object[]` into a C `pldotnet_Result*`
        /// </summary>
        public bool OutputSetValue(object[] values, IntPtr output)
        {
            Elog.Info($"Entering OutputSetValue, output={output:x}, values=({string.Join(", ", values)})");

            if (values == null)
            {
                // FIXME, consider handling this better
                Elog.Info($"FIXME: returning 'true' on null input to OutputSetValue()");
                return true;
            }

            resize_result(output, values.Length);
            Elog.Info($"Result was resized");

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    // We don't know the OID of NULL, but pldotnet_main.c
                    // should handle this gracefully for NULL.
                    Elog.Info($"OutputResult.SetDatumResult(datum=NULL, isNull={true}, output={output:x}, i={i}, (uint)oid=0");
                    OutputResult.SetDatumResult((IntPtr)0, true, output, i, 0);
                }
                else
                {
                    bool isNull = false;
                    if (values[i] is BaseNpgsqlParameter param)
                    {
                        isNull = param.Value == null;
                    }

                    var (datum, oid) = SingleValueOutput(values[i]);
                    Elog.Info($"OutputResult.SetDatumResult(datum={datum:x}, isNull={isNull}, output={output:x}, i={i}, (uint)oid={(int)oid}");
                    OutputResult.SetDatumResult(datum, isNull, output, i, (uint)oid);
                }
            }

            Elog.Info($"Returning from OutputResult.SetDatumResult()");
            return true;
        }
    }
}
