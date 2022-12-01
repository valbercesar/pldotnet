// <copyright file="DateTimeHandler.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright 2019-2020 Brick Abode
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A class to control PostgreSQL parameters.
    /// </summary>
    public class ConfigDateTime
    {
        public static readonly bool DisableDateTimeInfinityConversions = true;
        public static readonly bool LegacyTimestampBehavior = true;
        public static readonly string InfinityExceptionMessage =
        "Can't read infinity value since UserClass.DisableDateTimeInfinityConversions is enabled";

        public static readonly long PostgresTimestampOffsetTicks = 630822816000000000L;
    }

    /// <summary>
    /// A type handler for the PostgreSQL date data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.DATEOID, OID.DATEARRAYOID)]
    public class DateHandler : StructTypeHandler<DateOnly>
    {
        public DateHandler()
        {
            this.ElementOID = OID.DATEOID;
            this.ArrayOID = OID.DATEARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumDateAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumDateAttributes(IntPtr datum, ref int date);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumDate().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumDate(int date);

        /// <inheritdoc />
        public override DateOnly InputValue(IntPtr datum)
        {
            int date = 0;
            pldotnet_getDatumDateAttributes(datum, ref date);
            return date switch
            {
                int.MaxValue => ConfigDateTime.DisableDateTimeInfinityConversions ?
                    throw new InvalidCastException(ConfigDateTime.InfinityExceptionMessage) : DateOnly.MaxValue,
                int.MinValue => ConfigDateTime.DisableDateTimeInfinityConversions ?
                    throw new InvalidCastException(ConfigDateTime.InfinityExceptionMessage) : DateOnly.MinValue,
                var value => DateOnly.FromDayNumber(value + 730119)
            };
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(DateOnly value)
        {
            return pldotnet_createDatumDate(value.DayNumber - 730119);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL time data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.TIMEOID, OID.TIMEARRAYOID)]
    public class TimeHandler : StructTypeHandler<TimeOnly>
    {
        public TimeHandler()
        {
            this.ElementOID = OID.TIMEOID;
            this.ArrayOID = OID.TIMEARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumTimeAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimeAttributes(IntPtr datum, ref long time);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumTime().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTime(long time);

        /// <inheritdoc />
        public override TimeOnly InputValue(IntPtr datum)
        {
            long time = 0;
            pldotnet_getDatumTimeAttributes(datum, ref time);
            return new TimeOnly(time * 10);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(TimeOnly value)
        {
            return pldotnet_createDatumTime(value.Ticks / 10);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL timetz data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.TIMETZOID, OID.TIMETZARRAYOID)]
    public class TimeTzHandler : StructTypeHandler<DateTimeOffset>
    {
        public TimeTzHandler()
        {
            this.ElementOID = OID.TIMETZOID;
            this.ArrayOID = OID.TIMETZARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumTimeTzAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimeTzAttributes(IntPtr datum, ref long time, ref int zone);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumTimeTz().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimeTz(long time, int zone);

        /// <inheritdoc />
        public override DateTimeOffset InputValue(IntPtr datum)
        {
            long time = 0;
            int zone = 0;
            pldotnet_getDatumTimeTzAttributes(datum, ref time, ref zone);
            return new DateTimeOffset((time * 10) + TimeSpan.TicksPerDay, new TimeSpan(0, 0, -zone));
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(DateTimeOffset value)
        {
            return pldotnet_createDatumTimeTz(value.TimeOfDay.Ticks / 10, -(int)(value.Offset.Ticks / TimeSpan.TicksPerSecond));
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL timestamp data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.TIMESTAMPOID, OID.TIMESTAMPARRAYOID)]
    public class TimestampHandler : StructTypeHandler<DateTime>
    {
        public TimestampHandler()
        {
            this.ElementOID = OID.TIMESTAMPOID;
            this.ArrayOID = OID.TIMESTAMPARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumTimestampAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimestampAttributes(IntPtr datum, ref long timestamp);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumTimestamp().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimestamp(long timestamp);

        /// <summary>
        /// Creates the DateTime object according to the provided parameters.
        /// </summary>
        public static DateTime CreateDateTimeObject(long timestamp, DateTimeKind kind)
        {
            try
            {
                return timestamp switch
                {
                    long.MaxValue => ConfigDateTime.DisableDateTimeInfinityConversions ? throw new InvalidCastException(ConfigDateTime.InfinityExceptionMessage) : DateTime.MaxValue,
                    long.MinValue => ConfigDateTime.DisableDateTimeInfinityConversions ? throw new InvalidCastException(ConfigDateTime.InfinityExceptionMessage) : DateTime.MinValue,
                    var value => new DateTime((value * 10) + ConfigDateTime.PostgresTimestampOffsetTicks, kind)
                };
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new InvalidCastException("Out of the range of DateTime (year must be between 1 and 9999)", e);
            }
        }

        /// <inheritdoc />
        public override DateTime InputValue(IntPtr datum)
        {
            long timestamp = 0;
            pldotnet_getDatumTimestampAttributes(datum, ref timestamp);
            return CreateDateTimeObject(timestamp, DateTimeKind.Unspecified);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(DateTime value)
        {
            if (!ConfigDateTime.DisableDateTimeInfinityConversions)
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

            return pldotnet_createDatumTimestamp((long)((value.Ticks - ConfigDateTime.PostgresTimestampOffsetTicks) / 10));
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL timestamptz data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.TIMESTAMPTZOID, OID.TIMESTAMPTZARRAYOID)]
    public class TimestampTzHandler : StructTypeHandler<DateTime>
    {
        public TimestampTzHandler()
        {
            this.ElementOID = OID.TIMESTAMPTZOID;
            this.ArrayOID = OID.TIMESTAMPTZARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumTimestampTzAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumTimestampTzAttributes(IntPtr datum, ref long timestamp);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumTimestampTz().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumTimestampTz(long timestamp);

        /// <inheritdoc />
        public override DateTime InputValue(IntPtr datum)
        {
            long timestamp = 0;
            pldotnet_getDatumTimestampTzAttributes(datum, ref timestamp);
            DateTime dateTime = TimestampHandler.CreateDateTimeObject(timestamp, DateTimeKind.Utc);
            return ConfigDateTime.LegacyTimestampBehavior && (ConfigDateTime.DisableDateTimeInfinityConversions || (dateTime != DateTime.MaxValue && dateTime != DateTime.MinValue))
            ? dateTime.ToLocalTime()
            : dateTime;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(DateTime value)
        {
            if (ConfigDateTime.LegacyTimestampBehavior)
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
            {
                Debug.Assert(value.Kind == DateTimeKind.Utc || value == DateTime.MinValue || value == DateTime.MaxValue);
            }

            if (!ConfigDateTime.DisableDateTimeInfinityConversions)
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

            return pldotnet_createDatumTimestampTz((long)((value.Ticks - ConfigDateTime.PostgresTimestampOffsetTicks) / 10));
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL date interval type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-datetime.html.
    /// </remarks>
    [OIDHandler(OID.INTERVALOID, OID.INTERVALARRAYOID)]
    public class IntervalHandler : StructTypeHandler<NpgsqlInterval>
    {
        public IntervalHandler()
        {
            this.ElementOID = OID.INTERVALOID;
            this.ArrayOID = OID.INTERVALARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumIntervalAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumIntervalAttributes(IntPtr datum, ref long time, ref int day, ref int month);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumInterval().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInterval(long time, int day, int month);

        /// <inheritdoc />
        public override NpgsqlInterval InputValue(IntPtr datum)
        {
            long time = 0;
            int day = 0, month = 0;
            pldotnet_getDatumIntervalAttributes(datum, ref time, ref day, ref month);
            return new NpgsqlInterval(month, day, time);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlInterval value)
        {
            return pldotnet_createDatumInterval(value.Time, value.Days, value.Months);
        }
    }
}
