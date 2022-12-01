// <copyright file="BitStringHandler.cs" company="Brick Abode">
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
using System.Collections;
using System.Runtime.InteropServices;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL var bit string data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-bit.html.
    /// </remarks>
    [OIDHandler(OID.VARBITOID, OID.VARBITARRAYOID)]
    public class VarBitStringHandler : ObjectTypeHandler<BitArray>
    {
        public VarBitStringHandler()
        {
            this.ElementOID = OID.VARBITOID;
            this.ArrayOID = OID.VARBITARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumVarBitAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumVarBitAttributes(IntPtr datum, ref int len, ref byte* dat);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumVarBit().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumVarBit(int len, byte[] dat);

        /// <summary>
        /// Creates a BitArray object from a PostgreSQL bit string data type.
        /// </summary>
        public static unsafe BitArray CreateBitArray(IntPtr datum)
        {
            int bitLen = 0;
            byte* bitDat = null;
            pldotnet_getDatumVarBitAttributes(datum, ref bitLen, ref bitDat);

            int byteLen = (bitLen / 8) + ((bitLen % 8) > 0 ? 1 : 0);
            byte[] bytes = new byte[byteLen];
            for (int i = 0; i < byteLen; i++)
            {
                bytes[i] = bitDat[i];
            }

            // the reverse BitArray constructed from byte[]
            BitArray auxiliar = new (bytes);

            BitArray result = new (bitLen);
            for (int i = 0, cont = 0; i < byteLen; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bitLen)
                    {
                        break;
                    }

                    result[cont++] = auxiliar[(i * 8) + j];
                }
            }

            return result;
        }

        /// <summary>
        /// Creates a PostgreSQL bit string data type from a BitArray object.
        /// </summary>
        public static IntPtr CreateDatum(BitArray value)
        {
            int bitLen = value.Length;
            int byteLen = (bitLen / 8) + ((bitLen % 8) > 0 ? 1 : 0);

            // the reverse BitArray; it will be used to call "CopyTo"
            BitArray auxiliar = new (byteLen * 8);
            for (int i = 0, cont = 0; i < byteLen; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bitLen)
                    {
                        break;
                    }

                    auxiliar[(i * 8) + j] = value[cont++];
                }
            }

            byte[] bytes = new byte[byteLen];
            auxiliar.CopyTo(bytes, 0);

            return pldotnet_createDatumVarBit(bitLen, bytes);
        }

        /// <inheritdoc />
        public override BitArray InputValue(IntPtr datum)
        {
            return CreateBitArray(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(BitArray value)
        {
            return CreateDatum(value);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL bit string data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-bit.html.
    /// </remarks>
    [OIDHandler(OID.BITOID, OID.BITARRAYOID)]
    public class BitStringHandler : ObjectTypeHandler<BitArray>
    {
        public BitStringHandler()
        {
            this.ElementOID = OID.BITOID;
            this.ArrayOID = OID.BITARRAYOID;
        }

        /// <inheritdoc />
        public override BitArray InputValue(IntPtr datum)
        {
            return VarBitStringHandler.CreateBitArray(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(BitArray value)
        {
            return VarBitStringHandler.CreateDatum(value);
        }
    }
}