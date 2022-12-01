// <copyright file="RangeHandler.cs" company="Brick Abode">
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
using System.Runtime.InteropServices;
using NpgsqlTypes;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A generic type handler for the PostgreSQL range.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    public abstract class RangeHandler<T, THandler> : StructTypeHandler<NpgsqlRange<T>>
            where THandler : BaseTypeHandler<T>, new()
    {
        public static THandler HandlerObj = new ();

        /// <inheritdoc />
        public override unsafe NpgsqlRange<T> InputValue(IntPtr datum)
        {
            byte isEmpty;
            IntPtr upperDange, lower_range;
            IntPtr upperDatum, lowerDatum;
            byte upperInfinite, lowerInfinite;
            byte upperInclusive, lowerInclusive;
            byte upperLower, lowerLower;
            T upper, lower;

            RangeConstructors.pldotnet_getDatumRangeAttributes(datum, &isEmpty, &lower_range, &upperDange);

            RangeConstructors.pldotnet_getDatumRangeBoundAttributes(
                upperDange, &upperDatum, &upperInfinite, &upperInclusive, &upperLower);
            RangeConstructors.pldotnet_getDatumRangeBoundAttributes(
                lower_range, &lowerDatum, &lowerInfinite, &lowerInclusive, &lowerLower);

            // TODO: check upperLower and lowerLower
            lower = HandlerObj.InputValue(lowerDatum);
            upper = HandlerObj.InputValue(upperDatum);

            return new NpgsqlRange<T>(
                lower,
                lowerInclusive > 0,
                lowerInfinite > 0,
                upper,
                upperInclusive > 0,
                upperInfinite > 0);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(NpgsqlRange<T> value)
        {
            if (value.IsEmpty)
            {
                return RangeConstructors.pldotnet_createEmptyDatumRange(this.ElementOID);
            }

            byte upperInfinite = (byte)(value.UpperBoundInfinite ? 1 : 0);
            byte lowerInfinite = (byte)(value.LowerBoundInfinite ? 1 : 0);
            byte upperInclusive = (byte)(value.UpperBoundIsInclusive ? 1 : 0);
            byte lowerInclusive = (byte)(value.LowerBoundIsInclusive ? 1 : 0);
            T upper = value.UpperBound;
            T lower = value.LowerBound;
            IntPtr upperDatum = HandlerObj.OutputValue(upper);
            IntPtr lowerDatum = HandlerObj.OutputValue(lower);

            // TODO: now, actually construct the range datum down in C
            // - Construct the two RangeBound objects for upper and lower
            // - Combine them to make a Range
            return RangeConstructors.pldotnet_createDatumRange(
                this.ElementOID,
                lowerDatum,
                lowerInfinite,
                lowerInclusive,
                upperDatum,
                upperInfinite,
                upperInclusive);
        }
    }

    /// <summary>
    /// This class contains the C methods used to convert from PostgreSQL range for NpgsqlRange,
    /// as well as the opposite way.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    public class RangeConstructors
    {
        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumRangeAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern void pldotnet_getDatumRangeAttributes(
                IntPtr inputDatum,
                byte* isEmpty,
                IntPtr* lowerRange,
                IntPtr* upperDange);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumRangeBoundAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern void pldotnet_getDatumRangeBoundAttributes(
                IntPtr inputRange,
                IntPtr* rangeDatum,
                byte* infinite,
                byte* inclusive,
                byte* lower);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumRange().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern IntPtr pldotnet_createDatumRange(
            OID rtOid,
            IntPtr lowerDatum,
            byte lowerInfinite,
            byte lowerInclusive,
            IntPtr upperDatum,
            byte upperInfinite,
            byte upperInclusive);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createEmptyDatumRange().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static unsafe extern IntPtr pldotnet_createEmptyDatumRange(OID rangeTypeId);
    }

    /// <summary>
    /// A type handler for the PostgreSQL range of integer.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    [OIDHandler(OID.INT4RANGEOID, OID.INT4RANGEARRAYOID)]
    public class IntRangeHandler : RangeHandler<int, IntHandler>
    {
        public IntRangeHandler()
        {
            this.ElementOID = OID.INT4RANGEOID;
            this.ArrayOID = OID.INT4RANGEARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL range of bigint.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    [OIDHandler(OID.INT8RANGEOID, OID.INT8RANGEARRAYOID)]
    public class LongRangeHandler : RangeHandler<long, LongHandler>
    {
        public LongRangeHandler()
        {
            this.ElementOID = OID.INT8RANGEOID;
            this.ArrayOID = OID.INT8RANGEARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL range of timestamp without time zone.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    [OIDHandler(OID.TSRANGEOID, OID.TSRANGEARRAYOID)]
    public class TimestampRangeHandler : RangeHandler<DateTime, TimestampHandler>
    {
        public TimestampRangeHandler()
        {
            this.ElementOID = OID.TSRANGEOID;
            this.ArrayOID = OID.TSRANGEARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL range of timestamp with time zone.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    [OIDHandler(OID.TSTZRANGEOID, OID.TSTZRANGEARRAYOID)]
    public class TimestampTzRangeHandler : RangeHandler<DateTime, TimestampTzHandler>
    {
        public TimestampTzRangeHandler()
        {
            this.ElementOID = OID.TSTZRANGEOID;
            this.ArrayOID = OID.TSTZRANGEARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL range of date.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/rangetypes.html.
    /// </remarks>
    [OIDHandler(OID.DATERANGEOID, OID.DATERANGEARRAYOID)]
    public class DateRangeHandler : RangeHandler<DateOnly, DateHandler>
    {
        public DateRangeHandler()
        {
            this.ElementOID = OID.DATERANGEOID;
            this.ArrayOID = OID.DATERANGEARRAYOID;
        }
    }
}
