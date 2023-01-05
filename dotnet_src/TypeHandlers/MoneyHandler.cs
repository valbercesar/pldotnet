// <copyright file="MoneyHandler.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright 2023 Brick Abode
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

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL money data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-money.html.
    /// </remarks>
    [OIDHandler(OID.MONEYOID, OID.MONEYARRAYOID)]
    public class MoneyHandler : StructTypeHandler<decimal>
    {
        public MoneyHandler()
        {
            this.ElementOID = OID.MONEYOID;
            this.ArrayOID = OID.MONEYARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumMoneyAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_GetDatumMoneyAttributes(IntPtr datum, ref long value);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumMoney().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumMoney(long value);

        /// <summary>
        /// Checks the limits of the decimal value before converting it into PostgreSQL money data type.
        /// </summary>
        public static void CheckLimits(decimal value)
        {
            if (value < -92233720368547758.08M || value > 92233720368547758.07M)
            {
                throw new OverflowException($"The supplied value ({value}) is outside the range for a PostgreSQL money value.");
            }
        }

        /// <inheritdoc />
        public override decimal InputValue(IntPtr datum)
        {
            long value = 0;
            pldotnet_GetDatumMoneyAttributes(datum, ref value);
            decimal datumValue = new (value);
            return datumValue / 100.0M;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(decimal value)
        {
            CheckLimits(value);
            return pldotnet_CreateDatumMoney(decimal.ToInt64(Math.Round(100.0M * value)));
        }
    }
}