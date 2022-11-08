using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
using System.Text.Unicode;

namespace PlDotNET_Handler
{
    /// <summary>
    /// A type handler for PostgreSQL character text data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.TEXTOID, OID.TEXTARRAYOID)]
    public class TextHandler : ObjectTypeHandler<string>
    {
        public TextHandler()
        {
            this.ElementOID = OID.TEXTOID;
            this.ArrayOID = OID.TEXTARRAYOID;
        }

        public static UTF8Encoding utf8_e = new UTF8Encoding();

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumTextAttributes(IntPtr datum, int* len, byte** buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumText(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int strlen;
            byte* str_p;
            pldotnet_getDatumTextAttributes(datum, &strlen, &str_p);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(str_p, strlen);
            String s1 = utf8_e.GetString(nativeSpan.ToArray(), 0, strlen);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            IntPtr datum;
            byte[] encodedBytes = utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            datum = pldotnet_createDatumText(len, encodedBytes);
            return datum;
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character char data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.BPCHAROID, OID.BPCHARARRAYOID)]
    public class CharHandler : ObjectTypeHandler<string>
    {
        public CharHandler()
        {
            this.ElementOID = OID.BPCHAROID;
            this.ArrayOID = OID.BPCHARARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumChar(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumCharAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = TextHandler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = TextHandler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumChar(len, encodedBytes);
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character varchar data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-character.html.
    /// </remarks>
    [OIDHandler(OID.VARCHAROID, OID.VARCHARARRAYOID)]
    public class CharVaryingHandler : ObjectTypeHandler<string>
    {
        public CharVaryingHandler()
        {
            this.ElementOID = OID.VARCHAROID;
            this.ArrayOID = OID.VARCHARARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumVarCharAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumVarChar(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumVarCharAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = TextHandler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = TextHandler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumVarChar(len, encodedBytes);
        }
    }

    /// <summary>
    /// A type handler for PostgreSQL character xml data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/datatype-xml.htm.
    /// </remarks>
    [OIDHandler(OID.XMLOID, OID.XMLARRAYOID)]
    public class XmlHandler : ObjectTypeHandler<string>
    {
        public XmlHandler()
        {
            this.ElementOID = OID.XMLOID;
            this.ArrayOID = OID.XMLARRAYOID;
        }

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumXmlAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumXml(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumXmlAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            String s1 = TextHandler.utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = TextHandler.utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumXml(len, encodedBytes);
        }
    }
}