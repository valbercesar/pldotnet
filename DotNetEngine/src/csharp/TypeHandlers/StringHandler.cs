using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
using System.Text.Unicode;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.TEXTOID, OID.TEXTARRAYOID)]
    public class text_handler : object_type_handler<string>
    {
        public static UTF8Encoding utf8_e = new UTF8Encoding();

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumTextAttributes(IntPtr datum, int* len, byte** buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumText(int len, byte[] buf);

        public override unsafe string input_value(IntPtr datum)
        {
            int strlen;
            byte* str_p;
            pldotnet_getDatumTextAttributes(datum, &strlen, &str_p);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(str_p, strlen);
            String s1 = utf8_e.GetString(nativeSpan.ToArray(), 0, strlen);
            return s1;
        }

        public override IntPtr output_value(string value)
        {
            IntPtr datum;
            byte[] encodedBytes = utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            datum = pldotnet_createDatumText(len, encodedBytes);
            return datum;
        }
    }

    [OIDHandler(OID.BPCHAROID, OID.BPCHARARRAYOID)]
    public class bpchar_handler : object_type_handler<string>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumChar(int len, byte[] buf);

        public override unsafe string input_value(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumCharAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = text_handler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        public override IntPtr output_value(string value)
        {
            byte[] encodedBytes = text_handler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumChar(len, encodedBytes);
        }
    }

    [OIDHandler(OID.VARCHAROID, OID.VARCHARARRAYOID)]
    public class varchar_handler : object_type_handler<string>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumVarCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumVarChar(int len, byte[] buf);

        public override unsafe string input_value(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumVarCharAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = text_handler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        public override IntPtr output_value(string value)
        {
            byte[] encodedBytes = text_handler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumVarChar(len, encodedBytes);
        }
    }

    [OIDHandler(OID.XMLOID, OID.XMLARRAYOID)]
    public class xml_handler : object_type_handler<string>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumXmlAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumXml(int len, byte[] buf);

        public override unsafe string input_value(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumXmlAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = text_handler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        public override IntPtr output_value(string value)
        {
            byte[] encodedBytes = text_handler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumXml(len, encodedBytes);
        }
    }
}