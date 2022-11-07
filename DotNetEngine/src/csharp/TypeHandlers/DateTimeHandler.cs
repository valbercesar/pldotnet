using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using NpgsqlTypes;

namespace PlDotNET_Handler
{
    public class CONFIG_DATE_TIME
    {
        public static readonly bool DisableDateTimeInfinityConversions = true;
        public static readonly bool LegacyTimestampBehavior = true;
        public static readonly string InfinityExceptionMessage =
        "Can't read infinity value since UserClass.DisableDateTimeInfinityConversions is enabled";
        public static readonly long PostgresTimestampOffsetTicks = 630822816000000000L;
    }

    [OIDHandler(OID.DATEOID, OID.DATEARRAYOID)]
    public class date_handler : struct_type_handler<DateOnly>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumDateAttributes(IntPtr datum, ref int date);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumDate(int date);

        public override DateOnly input_value(IntPtr datum)
        {
            int date = 0;
            pldotnet_getDatumDateAttributes(datum, ref date);
            return date switch
            {
                int.MaxValue => CONFIG_DATE_TIME.DisableDateTimeInfinityConversions ?
                    throw new InvalidCastException(CONFIG_DATE_TIME.InfinityExceptionMessage) : DateOnly.MaxValue,
                int.MinValue => CONFIG_DATE_TIME.DisableDateTimeInfinityConversions ?
                    throw new InvalidCastException(CONFIG_DATE_TIME.InfinityExceptionMessage) : DateOnly.MinValue,
                var value => DateOnly.FromDayNumber(value + 730119)
            };
        }

        public override IntPtr output_value(DateOnly value)
        {
            return pldotnet_createDatumDate(value.DayNumber - 730119);
        }
    }

    [OIDHandler(OID.TIMEOID, OID.TIMEARRAYOID)]
    public class time_handler : struct_type_handler<TimeOnly>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimeAttributes(IntPtr datum, ref long time);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTime(long time);

        public override TimeOnly input_value(IntPtr datum)
        {
            long time = 0;
            pldotnet_getDatumTimeAttributes(datum, ref time);
            return new TimeOnly(time * 10);
        }

        public override IntPtr output_value(TimeOnly value)
        {
            return pldotnet_createDatumTime(value.Ticks / 10);
        }
    }

    [OIDHandler(OID.TIMETZOID, OID.TIMETZARRAYOID)]
    public class timetz_handler : struct_type_handler<DateTimeOffset>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimeTzAttributes(IntPtr datum, ref long time, ref int zone);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimeTz(long time, int zone);

        public override DateTimeOffset input_value(IntPtr datum)
        {
            long time = 0;
            int zone = 0;
            pldotnet_getDatumTimeTzAttributes(datum, ref time, ref zone);
            return new DateTimeOffset(time * 10 + TimeSpan.TicksPerDay, new TimeSpan(0, 0, -zone));
        }

        public override IntPtr output_value(DateTimeOffset value)
        {
            return pldotnet_createDatumTimeTz(value.TimeOfDay.Ticks / 10, -(int)(value.Offset.Ticks / TimeSpan.TicksPerSecond)); ;
        }
    }

    [OIDHandler(OID.TIMESTAMPOID, OID.TIMESTAMPARRAYOID)]
    public class timestamp_handler : struct_type_handler<DateTime>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimestampAttributes(IntPtr datum, ref long timestamp);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimestamp(long timestamp);

        public static DateTime createDateTimeObject(long timestamp, DateTimeKind kind)
        {
            try
            {
                return timestamp switch
                {
                    long.MaxValue => CONFIG_DATE_TIME.DisableDateTimeInfinityConversions ? throw new InvalidCastException(CONFIG_DATE_TIME.InfinityExceptionMessage) : DateTime.MaxValue,
                    long.MinValue => CONFIG_DATE_TIME.DisableDateTimeInfinityConversions ? throw new InvalidCastException(CONFIG_DATE_TIME.InfinityExceptionMessage) : DateTime.MinValue,
                    var value => new DateTime(value * 10 + CONFIG_DATE_TIME.PostgresTimestampOffsetTicks, kind)
                };
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new InvalidCastException("Out of the range of DateTime (year must be between 1 and 9999)", e);
            }
        }

        public override DateTime input_value(IntPtr datum)
        {
            long timestamp = 0;
            pldotnet_getDatumTimestampAttributes(datum, ref timestamp);
            return createDateTimeObject(timestamp, DateTimeKind.Unspecified);
        }

        public override IntPtr output_value(DateTime value)
        {
            if (!CONFIG_DATE_TIME.DisableDateTimeInfinityConversions)
            {
                if (value == DateTime.MaxValue)
                {
                    return pldotnet_createDatumTimestamp(long.MaxValue);
                }
                if (value == DateTime.MinValue)
                {
                    return pldotnet_createDatumTimestamp(long.MinValue);
                }
            }
            return pldotnet_createDatumTimestamp((long)((value.Ticks - CONFIG_DATE_TIME.PostgresTimestampOffsetTicks) / 10));
        }
    }

    [OIDHandler(OID.TIMESTAMPTZOID, OID.TIMESTAMPTZARRAYOID)]
    public class timestamptz_handler : struct_type_handler<DateTime>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimestampTzAttributes(IntPtr datum, ref long timestamp);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimestampTz(long timestamp);

        public override DateTime input_value(IntPtr datum)
        {
            long timestamp = 0;
            pldotnet_getDatumTimestampTzAttributes(datum, ref timestamp);
            DateTime dateTime = timestamp_handler.createDateTimeObject(timestamp, DateTimeKind.Utc);
            return CONFIG_DATE_TIME.LegacyTimestampBehavior && (CONFIG_DATE_TIME.DisableDateTimeInfinityConversions || dateTime != DateTime.MaxValue && dateTime != DateTime.MinValue)
            ? dateTime.ToLocalTime()
            : dateTime;
        }

        public override IntPtr output_value(DateTime value)
        {
            if (CONFIG_DATE_TIME.LegacyTimestampBehavior)
            {
                switch (value.Kind)
                {
                    case DateTimeKind.Unspecified:
                    case DateTimeKind.Utc:
                        break;
                    case DateTimeKind.Local:
                        value = value.ToUniversalTime();
                        break;
                    default:
                        throw new InvalidOperationException($"Internal Npgsql bug: unexpected value {value.Kind} of enum {nameof(DateTimeKind)}. Please file a bug.");
                }
            }
            else
                Debug.Assert(value.Kind == DateTimeKind.Utc || value == DateTime.MinValue || value == DateTime.MaxValue);
            if (!CONFIG_DATE_TIME.DisableDateTimeInfinityConversions)
            {
                if (value == DateTime.MaxValue)
                {
                    return pldotnet_createDatumTimestampTz(long.MaxValue);
                }
                if (value == DateTime.MinValue)
                {
                    return pldotnet_createDatumTimestampTz(long.MinValue);
                }
            }
            return pldotnet_createDatumTimestampTz((long)((value.Ticks - CONFIG_DATE_TIME.PostgresTimestampOffsetTicks) / 10));
        }
    }

    [OIDHandler(OID.INTERVALOID, OID.INTERVALARRAYOID)]
    public class interval_handler : struct_type_handler<NpgsqlInterval>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumIntervalAttributes(IntPtr datum, ref long time, ref int day, ref int month);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInterval(long time, int day, int month);

        public override NpgsqlInterval input_value(IntPtr datum)
        {
            long time = 0;
            int day = 0, month = 0;
            pldotnet_getDatumIntervalAttributes(datum, ref time, ref day, ref month);
            return new NpgsqlInterval(month, day, time);
        }

        public override IntPtr output_value(NpgsqlInterval value)
        {
            return pldotnet_createDatumInterval(value.Time, value.Days, value.Months);
        }
    }
}
