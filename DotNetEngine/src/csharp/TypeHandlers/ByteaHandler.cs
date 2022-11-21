using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Buffers;
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
        /// See ::pldotnet_getDatumByteaAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern unsafe void pldotnet_getDatumByteaAttributes(IntPtr datum, ref int len, ref byte* buf);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumBytea().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBytea(int len, byte[] buf);

        /// <inheritdoc />
        public override unsafe byte[] InputValue(IntPtr datum)
        {
            int len = 0;
            byte* buf = null;
            pldotnet_getDatumByteaAttributes(datum, ref len, ref buf);
            ReadOnlySpan<byte> nativeSpan = new ReadOnlySpan<byte>(buf, len);
            return nativeSpan.ToArray();
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(byte[] value)
        {
            return pldotnet_createDatumBytea(value.Length, value);
        }
    }
}