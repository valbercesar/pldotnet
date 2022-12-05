// <copyright file="ByteaHandler.cs" company="Brick Abode">
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
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL bytea data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-binary.html.
    /// </remarks>
    [OIDHandler(OID.BYTEAOID, OID.BYTEAARRAYOID)]
    public class ByteaHandler : ObjectTypeHandler<byte[]>
    {
        public ByteaHandler()
        {
            this.ElementOID = OID.BYTEAOID;
            this.ArrayOID = OID.BYTEAARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumByteaAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_GetDatumByteaAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumBytea().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumBytea(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe byte[] InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_GetDatumByteaAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new (buf, len);
            return nativeSpan.ToArray();
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(byte[] value)
        {
            return pldotnet_CreateDatumBytea(value.Length, value);
        }
    }
}