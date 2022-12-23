// <copyright file="StringHandler.cs" company="Brick Abode">
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
    /// A type handler for PostgreSQL character data types (text, char, varchar, xml).
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    public class StringHandler : ObjectTypeHandler<string>
    {
        public static UTF8Encoding Utf8 = new ();

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumTextAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_GetDatumTextAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumText().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumText(int len, byte[] buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumCharAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_GetDatumCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumChar().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumChar(int len, byte[] buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumVarCharAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_GetDatumVarCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumVarChar().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumVarChar(int len, byte[] buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_GetDatumXmlAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_GetDatumXmlAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumXml().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumXml(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int strlen = 0;
            byte* str_p = null;
            switch ((int)this.ElementOID)
            {
                case (int)OID.TEXTOID:
                    pldotnet_GetDatumTextAttributes(datum, ref strlen, ref str_p);
                    break;
                case (int)OID.BPCHAROID:
                    pldotnet_GetDatumCharAttributes(datum, ref strlen, ref str_p);
                    break;
                case (int)OID.VARCHAROID:
                    pldotnet_GetDatumVarCharAttributes(datum, ref strlen, ref str_p);
                    break;
                case (int)OID.XMLOID:
                    pldotnet_GetDatumXmlAttributes(datum, ref strlen, ref str_p);
                    break;
                default:
                    throw new NotImplementedException($"StringConstructors doesn't support {(OID)this.ElementOID}");
            }

            ReadOnlySpan<byte> nativeSpan = new (str_p, strlen);
            return Utf8.GetString(nativeSpan.ToArray(), 0, strlen);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = Utf8.GetBytes(value);
            int len = encodedBytes.Length;
            return (int)this.ElementOID switch
            {
                (int)OID.TEXTOID => pldotnet_CreateDatumText(len, encodedBytes),
                (int)OID.BPCHAROID => pldotnet_CreateDatumChar(len, encodedBytes),
                (int)OID.VARCHAROID => pldotnet_CreateDatumVarChar(len, encodedBytes),
                (int)OID.XMLOID => pldotnet_CreateDatumXml(len, encodedBytes),
                _ => throw new NotImplementedException($"StringConstructors doesn't support {(OID)this.ElementOID}"),
            };
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character text data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.TEXTOID, OID.TEXTARRAYOID)]
    public class TextHandler : StringHandler
    {
        public TextHandler()
        {
            this.ElementOID = OID.TEXTOID;
            this.ArrayOID = OID.TEXTARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character char data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.BPCHAROID, OID.BPCHARARRAYOID)]
    public class CharHandler : StringHandler
    {
        public CharHandler()
        {
            this.ElementOID = OID.BPCHAROID;
            this.ArrayOID = OID.BPCHARARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character varchar data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.VARCHAROID, OID.VARCHARARRAYOID)]
    public class CharVaryingHandler : StringHandler
    {
        public CharVaryingHandler()
        {
            this.ElementOID = OID.VARCHAROID;
            this.ArrayOID = OID.VARCHARARRAYOID;
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character xml data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-xml.htm.
    /// </remarks>
    [OIDHandler(OID.XMLOID, OID.XMLARRAYOID)]
    public class XmlHandler : StringHandler
    {
        public XmlHandler()
        {
            this.ElementOID = OID.XMLOID;
            this.ArrayOID = OID.XMLARRAYOID;
        }
    }
}