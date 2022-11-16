using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace PlDotNET_Handler
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

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumVarBitAttributes(IntPtr datum, ref int len, ref byte* dat);

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

            int byteLen = bitLen / 8 + ((bitLen % 8) > 0 ? 1 : 0);
            byte[] bytes = new byte[byteLen];
            for (int i = 0; i < byteLen; i++)
            {
                bytes[i] = bitDat[i];
            }

            // the reverse BitArray constructed from byte[]
            BitArray auxiliar = new BitArray(bytes);

            BitArray result = new BitArray(bitLen);
            for (int i = 0, cont = 0; i < byteLen; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bitLen)
                        break;
                    result[cont++] = auxiliar[i * 8 + j];
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
            int byteLen = bitLen / 8 + ((bitLen % 8) > 0 ? 1 : 0);

            // the reverse BitArray; it will be used to call "CopyTo"
            BitArray auxiliar = new BitArray(byteLen * 8);
            for (int i = 0, cont = 0; i < byteLen; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bitLen)
                        break;
                    auxiliar[i * 8 + j] = value[cont++];
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