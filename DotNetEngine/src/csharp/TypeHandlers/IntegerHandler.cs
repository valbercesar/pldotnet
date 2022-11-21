using System;
using System.Runtime.InteropServices;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL smallint data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    /// </remarks>
    [OIDHandler(OID.INT2OID, OID.INT2ARRAYOID)]
    public class ShortHandler : StructTypeHandler<short>
    {
        public ShortHandler()
        {
            this.ElementOID = OID.INT2OID;
            this.ArrayOID = OID.INT2ARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getInt16().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern short pldotnet_getInt16(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumInt16().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt16(short value);

        /// <inheritdoc />
        public override short InputValue(IntPtr datum)
        {
            return pldotnet_getInt16(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(short value)
        {
            return pldotnet_createDatumInt16(value);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL integer data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    /// </remarks>
    [OIDHandler(OID.INT4OID, OID.INT4ARRAYOID)]
    public class IntHandler : StructTypeHandler<int>
    {
        public IntHandler()
        {
            this.ElementOID = OID.INT4OID;
            this.ArrayOID = OID.INT4ARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getInt32().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_getInt32(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumInt32().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt32(int value);

        /// <inheritdoc />
        public override int InputValue(IntPtr datum)
        {
            return pldotnet_getInt32(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(int value)
        {
            return pldotnet_createDatumInt32(value);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL bigint data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    /// </remarks>
    [OIDHandler(OID.INT8OID, OID.INT8ARRAYOID)]
    public class LongHandler : StructTypeHandler<long>
    {
        public LongHandler()
        {
            this.ElementOID = OID.INT8OID;
            this.ArrayOID = OID.INT8ARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getInt64().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern long pldotnet_getInt64(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumInt64().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt64(long value);

        /// <inheritdoc />
        public override long InputValue(IntPtr datum)
        {
            return pldotnet_getInt64(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(long value)
        {
            return pldotnet_createDatumInt64(value);
        }
    }
}