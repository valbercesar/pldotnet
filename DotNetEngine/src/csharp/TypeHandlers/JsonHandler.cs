// <copyright file="JsonHandler.cs" company="Brick Abode">
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
    /// A type handler for the PostgreSQL json data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-json.html.
    /// </remarks>
    [OIDHandler(OID.JSONOID, OID.JSONARRAYOID)]
    public class JsonHandler : ObjectTypeHandler<string>
    {
        public static UTF8Encoding Utf8E = new ();

        public JsonHandler()
        {
            this.ElementOID = OID.JSONOID;
            this.ArrayOID = OID.JSONARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumJsonAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumJsonAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumJson().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumJson(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumJsonAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new (buf, len);
            string s1 = Utf8E.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = Utf8E.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumJson(len, encodedBytes);
        }
    }
}