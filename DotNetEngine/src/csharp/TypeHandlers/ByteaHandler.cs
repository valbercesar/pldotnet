using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
using System.Text.Unicode;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.BYTEAOID, OID.BYTEAARRAYOID)]
    public class bytea_handler : object_type_handler<byte[]>
    {
        public static UTF8Encoding utf8_e = new UTF8Encoding();

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumByteaAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBytea(int len, byte[] buf);

        public override unsafe byte[] input_value(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumByteaAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            return nativeSpan.ToArray();
        }

        public override IntPtr output_value(byte[] value)
        {
            return pldotnet_createDatumBytea(value.Length, value);
        }
    }
}