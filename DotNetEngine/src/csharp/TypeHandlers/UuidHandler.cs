// <copyright file="UuidHandler.cs" company="Brick Abode">
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

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL UUID data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-uuid.html.
    /// </remarks>
    [OIDHandler(OID.UUIDOID, OID.UUIDARRAYOID)]
    public class UuidHandler : StructTypeHandler<Guid>
    {
        public UuidHandler()
        {
            this.ElementOID = OID.UUIDOID;
            this.ArrayOID = OID.UUIDARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumUuidAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_GetDatumUuidAttributes(IntPtr datum, byte[] data);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumUuid().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumUuid(byte[] data);

        /// <inheritdoc />
        public override Guid InputValue(IntPtr datum)
        {
            byte[] data = new byte[16];
            pldotnet_GetDatumUuidAttributes(datum, data);

            byte[] data1 = data[0..4];
            byte[] data2 = data[4..6];
            byte[] data3 = data[6..8];
            Array.Reverse(data1);
            Array.Reverse(data2);
            Array.Reverse(data3);

            return new Guid(
                BitConverter.ToInt32(data1, 0),
                BitConverter.ToInt16(data2, 0),
                BitConverter.ToInt16(data3, 0),
                data[8..]);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(Guid value)
        {
            byte[] data = value.ToByteArray();
            byte[] data1 = data[0..4];
            byte[] data2 = data[4..6];
            byte[] data3 = data[6..8];
            Array.Reverse(data1);
            Array.Reverse(data2);
            Array.Reverse(data3);

            byte[] psql_data = new byte[16];
            data1.CopyTo(psql_data, 0);
            data2.CopyTo(psql_data, 4);
            data3.CopyTo(psql_data, 6);
            data[8..].CopyTo(psql_data, 8);

            return pldotnet_CreateDatumUuid(psql_data);
        }
    }
}