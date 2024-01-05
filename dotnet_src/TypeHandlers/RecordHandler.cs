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
        public static BoolHandler BoolHandlerObj = new BoolHandler();
        public static ShortHandler ShortHandlerObj = new ShortHandler();
        public static IntHandler IntHandlerObj = new IntHandler();
        public static LongHandler LongHandlerObj = new LongHandler();
        public static FloatHandler FloatHandlerObj = new FloatHandler();
        public static DoubleHandler DoubleHandlerObj = new DoubleHandler();
        public static PointHandler PointHandlerObj = new PointHandler();
        public static LineHandler LineHandlerObj = new LineHandler();
        public static LineSegmentHandler LineSegmentHandlerObj = new LineSegmentHandler();
        public static BoxHandler BoxHandlerObj = new BoxHandler();
        public static PolygonHandler PolygonHandlerObj = new PolygonHandler();
        public static TextHandler TextHandlerObj = new TextHandler();
        public static PathHandler PathHandlerObj = new PathHandler();
        public static CircleHandler CircleHandlerObj = new CircleHandler();
        public static DateHandler DateHandlerObj = new DateHandler();
        public static TimeHandler TimeHandlerObj = new TimeHandler();
        public static TimeTzHandler TimeTzHandlerObj = new TimeTzHandler();
        public static TimestampHandler TimestampHandlerObj = new TimestampHandler();
        public static TimestampTzHandler TimestampTzHandlerObj = new TimestampTzHandler();
        public static IntervalHandler IntervalHandlerObj = new IntervalHandler();
        public static MacaddrHandler MacaddrHandlerObj = new MacaddrHandler();
        public static Macaddr8Handler Macaddr8HandlerObj = new Macaddr8Handler();
        public static InetHandler InetHandlerObj = new InetHandler();
        public static CidrHandler CidrHandlerObj = new CidrHandler();
        public static MoneyHandler MoneyHandlerObj = new MoneyHandler();
        public static VarBitStringHandler VarBitStringHandlerObj = new VarBitStringHandler();
        public static BitStringHandler BitStringHandlerObj = new BitStringHandler();
        public static ByteaHandler ByteaHandlerObj = new ByteaHandler();
        public static CharHandler CharHandlerObj = new CharHandler();
        public static CharVaryingHandler CharVaryingHandlerObj = new CharVaryingHandler();
        public static XmlHandler XmlHandlerObj = new XmlHandler();
        public static JsonHandler JsonHandlerObj = new JsonHandler();
        public static UuidHandler UuidHandlerObj = new UuidHandler();
        public static IntRangeHandler IntRangeHandlerObj = new IntRangeHandler();
        public static LongRangeHandler LongRangeHandlerObj = new LongRangeHandler();
        public static TimestampRangeHandler TimestampRangeHandlerObj = new TimestampRangeHandler();
        public static TimestampTzRangeHandler TimestampTzRangeHandlerObj = new TimestampTzRangeHandler();
        public static DateRangeHandler DateRangeHandlerObj = new DateRangeHandler();

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
                    return (NpgsqlDbType.Smallint, obj); // Note: Smallint is the equivalent of short in PostgreSQL. For byte, you might consider Bytea if you're dealing with a byte array.
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
                    return (NpgsqlDbType.Text, obj); // Else consider it TEXT
                case PhysicalAddress _:
                    return (NpgsqlDbType.MacAddr, obj);
                default:
                    throw new SystemException($"Unrecognized object for type conversion: ({obj.GetType().Name}){obj}");
            }
        }

        public static OID GetPostgreSQLOID(NpgsqlDbType type)
        {
            // this set is bigger than we need it to be, but that is actually good.
            switch (type)
            {
                case NpgsqlDbType.Boolean:
                    return OID.BOOLOID;
                case NpgsqlDbType.Smallint:
                    return OID.INT2OID;
                case NpgsqlDbType.Integer:
                    return OID.INT4OID;
                case NpgsqlDbType.Bigint:
                    return OID.INT8OID;
                case NpgsqlDbType.Real:
                    return OID.FLOAT4OID;
                case NpgsqlDbType.Double:
                    return OID.FLOAT8OID;
                case NpgsqlDbType.Text:
                    return OID.TEXTOID;
                case NpgsqlDbType.Varchar:
                    return OID.VARCHAROID;
                case NpgsqlDbType.MacAddr:
                    return OID.MACADDROID;
                default:
                    throw new InvalidOperationException($"Unsupported or unrecognized NpgsqlDbType: {type}");
            }
        }

#nullable enable

        // not nullable; you must filter for nulls before calling
        public static object SingleValueInput(IntPtr datum, OID oid)
        {
            object obj;
            switch (oid)
            {
                case OID.BOOLOID:
                    obj = (object)BoolHandlerObj.InputValue(datum);
                    break;
                case OID.INT2OID:
                    obj = (object)ShortHandlerObj.InputValue(datum);
                    break;
                case OID.INT4OID:
                    obj = (object)IntHandlerObj.InputValue(datum);
                    break;
                case OID.INT8OID:
                    obj = (object)LongHandlerObj.InputValue(datum);
                    break;
                case OID.FLOAT4OID:
                    obj = (object)FloatHandlerObj.InputValue(datum);
                    break;
                case OID.FLOAT8OID:
                    obj = (object)DoubleHandlerObj.InputValue(datum);
                    break;
                case OID.POINTOID:
                    obj = (object)PointHandlerObj.InputValue(datum);
                    break;
                case OID.LINEOID:
                    obj = (object)LineHandlerObj.InputValue(datum);
                    break;
                case OID.LSEGOID:
                    obj = (object)LineSegmentHandlerObj.InputValue(datum);
                    break;
                case OID.BOXOID:
                    obj = (object)BoxHandlerObj.InputValue(datum);
                    break;
                case OID.POLYGONOID:
                    obj = (object)PolygonHandlerObj.InputValue(datum);
                    break;
                case OID.TEXTOID:
                    obj = (object)TextHandlerObj.InputValue(datum);
                    break;
                case OID.PATHOID:
                    obj = (object)PathHandlerObj.InputValue(datum);
                    break;
                case OID.CIRCLEOID:
                    obj = (object)CircleHandlerObj.InputValue(datum);
                    break;
                case OID.DATEOID:
                    obj = (object)DateHandlerObj.InputValue(datum);
                    break;
                case OID.TIMEOID:
                    obj = (object)TimeHandlerObj.InputValue(datum);
                    break;
                case OID.TIMETZOID:
                    obj = (object)TimeTzHandlerObj.InputValue(datum);
                    break;
                case OID.TIMESTAMPOID:
                    obj = (object)TimestampHandlerObj.InputValue(datum);
                    break;
                case OID.TIMESTAMPTZOID:
                    obj = (object)TimestampTzHandlerObj.InputValue(datum);
                    break;
                case OID.INTERVALOID:
                    obj = (object)IntervalHandlerObj.InputValue(datum);
                    break;
                case OID.MACADDROID:
                    obj = (object)MacaddrHandlerObj.InputValue(datum);
                    break;
                case OID.MACADDR8OID:
                    obj = (object)Macaddr8HandlerObj.InputValue(datum);
                    break;
                /* broken
                case OID.INETOID:
                    obj = (object)InetHandlerObj.InputValue(datum);
                    break;
                case OID.CIDROID:
                    obj = (object)CidrHandlerObj.InputValue(datum);
                    break;
                */
                case OID.MONEYOID:
                    obj = (object)MoneyHandlerObj.InputValue(datum);
                    break;
                /* broken
                case OID.VARBITOID:
                    obj = (object)VarBitStringHandlerObj.InputValue(datum);
                    break;
                case OID.BITOID:
                    obj = (object)BitStringHandlerObj.InputValue(datum);
                    break;
                */
                case OID.BYTEAOID:
                    obj = (object)ByteaHandlerObj.InputValue(datum);
                    break;
                case OID.BPCHAROID:
                    obj = (object)CharHandlerObj.InputValue(datum);
                    break;
                case OID.VARCHAROID:
                    obj = (object)CharVaryingHandlerObj.InputValue(datum);
                    break;
                case OID.XMLOID:
                    obj = (object)XmlHandlerObj.InputValue(datum);
                    break;
                case OID.JSONOID:
                    obj = (object)JsonHandlerObj.InputValue(datum);
                    break;
                case OID.UUIDOID:
                    obj = (object)UuidHandlerObj.InputValue(datum);
                    break;
                case OID.INT4RANGEOID:
                    obj = (object)IntRangeHandlerObj.InputValue(datum);
                    break;
                case OID.INT8RANGEOID:
                    obj = (object)LongRangeHandlerObj.InputValue(datum);
                    break;
                case OID.TSRANGEOID:
                    obj = (object)TimestampRangeHandlerObj.InputValue(datum);
                    break;
                case OID.TSTZRANGEOID:
                    obj = (object)TimestampTzRangeHandlerObj.InputValue(datum);
                    break;
                case OID.DATERANGEOID:
                    obj = (object)DateRangeHandlerObj.InputValue(datum);
                    break;
                default:
                    throw new InvalidOperationException($"Unrecognized OID: {oid}");
            }

            return obj;
        }

        // not nullable; you must filter for nulls before calling
        public static (IntPtr, OID) SingleValueOutput(object value)
        {
            IntPtr datum;
            (NpgsqlDbType dbt, object obj) = GetNpgsqlTypeAndValue(value);
            OID oid = GetPostgreSQLOID(dbt);
            switch (oid)
            {
                case OID.BOOLOID:
                    datum = BoolHandlerObj.OutputNullableValue((bool?)obj);
                    break;
                case OID.INT2OID:
                    datum = ShortHandlerObj.OutputNullableValue((short?)obj);
                    break;
                case OID.INT4OID:
                    datum = IntHandlerObj.OutputNullableValue((int?)obj);
                    break;
                case OID.INT8OID:
                    datum = LongHandlerObj.OutputNullableValue((long?)obj);
                    break;
                case OID.FLOAT4OID:
                    datum = FloatHandlerObj.OutputNullableValue((float?)obj);
                    break;
                case OID.FLOAT8OID:
                    datum = DoubleHandlerObj.OutputNullableValue((double?)obj);
                    break;
                case OID.POINTOID:
                    datum = PointHandlerObj.OutputNullableValue((NpgsqlPoint?)obj);
                    break;
                case OID.LINEOID:
                    datum = LineHandlerObj.OutputNullableValue((NpgsqlLine?)obj);
                    break;
                case OID.LSEGOID:
                    datum = LineSegmentHandlerObj.OutputNullableValue((NpgsqlLSeg?)obj);
                    break;
                case OID.BOXOID:
                    datum = BoxHandlerObj.OutputNullableValue((NpgsqlBox?)obj);
                    break;
                case OID.POLYGONOID:
                    datum = PolygonHandlerObj.OutputNullableValue((NpgsqlPolygon?)obj);
                    break;
                case OID.TEXTOID:
                    datum = TextHandlerObj.OutputNullableValue((string?)obj);
                    break;
                case OID.PATHOID:
                    datum = PathHandlerObj.OutputNullableValue((NpgsqlPath?)obj);
                    break;
                case OID.CIRCLEOID:
                    datum = CircleHandlerObj.OutputNullableValue((NpgsqlCircle?)obj);
                    break;
                case OID.DATEOID:
                    datum = DateHandlerObj.OutputNullableValue((DateOnly?)obj);
                    break;
                case OID.TIMEOID:
                    datum = TimeHandlerObj.OutputNullableValue((TimeOnly?)obj);
                    break;
                case OID.TIMETZOID:
                    datum = TimeTzHandlerObj.OutputNullableValue((DateTimeOffset?)obj);
                    break;
                case OID.TIMESTAMPOID:
                    datum = TimestampHandlerObj.OutputNullableValue((DateTime?)obj);
                    break;
                case OID.TIMESTAMPTZOID:
                    datum = TimestampTzHandlerObj.OutputNullableValue((DateTime?)obj);
                    break;
                case OID.INTERVALOID:
                    datum = IntervalHandlerObj.OutputNullableValue((NpgsqlInterval?)obj);
                    break;
                case OID.MACADDROID:
                    datum = MacaddrHandlerObj.OutputNullableValue((PhysicalAddress?)obj);
                    break;
                case OID.MACADDR8OID:
                    datum = Macaddr8HandlerObj.OutputNullableValue((PhysicalAddress?)obj);
                    break;
                /* broken
                case OID.INETOID:
                    datum = InetHandlerObj.OutputNullableValue(((IPAddress Address, int Netmask)?)obj);
                    break;
                case OID.CIDROID:
                    datum = CidrHandlerObj.OutputNullableValue(((IPAddress Address, int Netmask)?)obj);
                    break;
                */
                case OID.MONEYOID:
                    datum = MoneyHandlerObj.OutputNullableValue((decimal?)obj);
                    break;
                /* broken
                case OID.VARBITOID:
                    datum = VarBitStringHandlerObj.OutputNullableValue((BitArray?)obj);
                    break;
                case OID.BITOID:
                    datum = BitStringHandlerObj.OutputNullableValue((BitArray?)obj);
                    break;
                */
                case OID.BYTEAOID:
                    // arrays are inherently nullable
                    datum = ByteaHandlerObj.OutputNullableValue((byte[])obj);
                    break;
                case OID.BPCHAROID:
                    datum = CharHandlerObj.OutputNullableValue((string?)obj);
                    break;
                case OID.VARCHAROID:
                    datum = CharVaryingHandlerObj.OutputNullableValue((string?)obj);
                    break;
                case OID.XMLOID:
                    datum = XmlHandlerObj.OutputNullableValue((string?)obj);
                    break;
                case OID.JSONOID:
                    datum = JsonHandlerObj.OutputNullableValue((string?)obj);
                    break;
                case OID.UUIDOID:
                    datum = UuidHandlerObj.OutputNullableValue((Guid?)obj);
                    break;
                case OID.INT4RANGEOID:
                    datum = IntRangeHandlerObj.OutputNullableValue((NpgsqlRange<int>?)obj);
                    break;
                case OID.INT8RANGEOID:
                    datum = LongRangeHandlerObj.OutputNullableValue((NpgsqlRange<long>?)obj);
                    break;
                case OID.TSRANGEOID:
                    datum = TimestampRangeHandlerObj.OutputNullableValue((NpgsqlRange<DateTime>?)obj);
                    break;
                case OID.TSTZRANGEOID:
                    datum = TimestampTzRangeHandlerObj.OutputNullableValue((NpgsqlRange<DateTime>?)obj);
                    break;
                case OID.DATERANGEOID:
                    datum = DateRangeHandlerObj.OutputNullableValue((NpgsqlRange<DateOnly>?)obj);
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported or unrecognized OID: {oid}");
            }

            return (datum, oid);
        }

        /// <inheritdoc />
        public override object[] InputValue(IntPtr datum)
        {
            throw new SystemException($"`InputValue()` on a Record is unimplemented.");
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
                objects[i] = is_null ? null! : SingleValueInput(datum, oid);
            }

            return objects;
        }

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
