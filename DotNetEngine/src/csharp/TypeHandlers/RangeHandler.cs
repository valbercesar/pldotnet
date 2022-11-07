using System;
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace PlDotNET_Handler
{

    public class range_constructors
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern void pldotnet_getDatumRangeAttributes(
                IntPtr input_datum, byte* is_empty,
                IntPtr* lower_range, IntPtr* upper_range);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern void pldotnet_getDatumRangeBoundAttributes (
                IntPtr input_range, IntPtr* range_datum,
                byte* infinite, byte* inclusive, byte* lower);
    }

    [OIDHandler(OID.INT4RANGEOID, OID.INT4RANGEARRAYOID)]
    public class range_handler<T, THandler> : struct_type_handler<NpgsqlRange<T>>
            where THandler : base_type_handler<T>, new()
    {

        public static THandler handler_obj = new THandler();

        public override unsafe NpgsqlRange<T> input_value(IntPtr datum)
        {
            byte is_empty;
            IntPtr upper_range, lower_range;
            IntPtr upper_datum, lower_datum;
            byte upper_infinite, lower_infinite;
            byte upper_inclusive, lower_inclusive;
            byte upper_lower, lower_lower;
            T upper, lower;

            elog.pldotnet_Info("# DEBUG: Got range pointer: " + datum);
            range_constructors.pldotnet_getDatumRangeAttributes(datum, &is_empty, &lower_range, &upper_range);
            elog.pldotnet_Info($"# DEBUG: Got upper/lower pointers: {upper_range}, {lower_range}");

            range_constructors.pldotnet_getDatumRangeBoundAttributes(upper_range,
                            &upper_datum, &upper_infinite, &upper_inclusive, &upper_lower);
            range_constructors.pldotnet_getDatumRangeBoundAttributes(lower_range,
                            &lower_datum, &lower_infinite, &lower_inclusive, &lower_lower);

            // TODO: check upper_lower and lower_lower
            lower = handler_obj.input_value(lower_datum);
            upper = handler_obj.input_value(upper_datum);
            elog.pldotnet_Info($"# DEBUG: creating range with: {lower}, " +
                            $"{(lower_inclusive>0)}, {(lower_infinite > 0)} " +
                            $"{upper}, {(upper_inclusive>0)}, {(upper_infinite > 0)}");

            var retval = new NpgsqlRange<T>(lower, (lower_inclusive>0), (lower_infinite > 0),
                upper, (upper_inclusive>0), (upper_infinite > 0));
            elog.pldotnet_Info($"# DEBUG: returning range {retval}");
            return retval;
        }

        public override IntPtr output_value(NpgsqlRange<T> value)
        {
            // byte is_empty=0;
            // byte upper_infinite = value.UpperBoundIsInfinite ? 1 : 0;
            // byte lower_infinite = value.LowerBoundIsInfinite ? 1 : 0;
            // byte upper_inclusive = value.UpperBoundIsInclusive ? 1 : 0;
            // byte lower_inclusive = value.LowerBoundIsInclusive ? 1 : 0;
            // byte upper_lower = 0;
            // byte lower_lower = 1;
            // T upper = value.UpperBound;
            // T lower = value.LowerBound;
            // IntPtr upper_datum = handler_obj.output_value(upper_datum);
            // IntPtr lower_datum = handler_obj.output_value(lower_datum);
            // IntPtr upper_range, lower_range;
            // IntPtr retval;

            // TODO: now, actualy construct the range datum down in C

            return (IntPtr)0;
        }
    }

    public class    int_range_handler : range_handler<int,            int_handler>    {}
    public class   long_range_handler : range_handler<long,           long_handler>   {}
    public class   time_range_handler : range_handler<DateTime,       timestamp_handler>   {}
    public class timetz_range_handler : range_handler<DateTime, timestamptz_handler> {}
    public class   date_range_handler : range_handler<DateOnly,       date_handler>   {}
    // waiting on a handler implementation for Numeric
}

