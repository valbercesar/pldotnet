using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.BITOID, OID.BITARRAYOID)]
    public class varbit_handler : object_type_handler<BitArray>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumVarBitAttributes(IntPtr datum, ref int len, ref byte* dat);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumVarBit(int len, byte[] dat);

        public override unsafe BitArray input_value(IntPtr datum)
        {
            int bit_len = 0;
            byte* bit_dat = null;
            pldotnet_getDatumVarBitAttributes(datum, ref bit_len, ref bit_dat);

            int byte_len = bit_len / 8 + ((bit_len % 8) > 0 ? 1 : 0);
            byte[] bytes = new byte[byte_len];
            for (int i = 0; i < byte_len; i++)
            {
                bytes[i] = bit_dat[i];
            }

            // the reverse BitArray constructed from byte[]
            BitArray auxiliar = new BitArray(bytes);

            BitArray result = new BitArray(bit_len);
            for (int i = 0, cont = 0; i < byte_len; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bit_len)
                        break;
                    result[cont++] = auxiliar[i * 8 + j];
                }
            }

            return result;
        }

        public override IntPtr output_value(BitArray value)
        {
            int bit_len = value.Length;
            int byte_len = bit_len / 8 + ((bit_len % 8) > 0 ? 1 : 0);

            // the reverse BitArray; it will be used to call "CopyTo"
            BitArray auxiliar = new BitArray(byte_len * 8);
            for (int i = 0, cont = 0; i < byte_len; i++)
            {
                for (int j = 7; j >= 0; j--)
                {
                    if (cont == bit_len)
                        break;
                    auxiliar[i * 8 + j] = value[cont++];
                }
            }

            byte[] bytes = new byte[byte_len];
            auxiliar.CopyTo(bytes, 0);

            return pldotnet_createDatumVarBit(bit_len, bytes);
        }
    }
}