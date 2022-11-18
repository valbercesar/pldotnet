using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
using System.Text.Unicode;

namespace PlDotNET_Handler
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
        public JsonHandler()
        {
            this.ElementOID = OID.JSONOID;
            this.ArrayOID = OID.JSONARRAYOID;
        }

        public static UTF8Encoding utf8_e = new UTF8Encoding();

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumJsonAttributes(IntPtr datum, ref int len, ref byte* buf);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumJson(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe string InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumJsonAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            string s1 = utf8_e.GetString(nativeSpan.ToArray(), 0, len);
            return s1;
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(string value)
        {
            byte[] encodedBytes = utf8_e.GetBytes(value);
            int len = encodedBytes.Length;
            return pldotnet_createDatumJson(len, encodedBytes);
        }
    }
}