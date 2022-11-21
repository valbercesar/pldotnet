using System;
using System.Runtime.InteropServices;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL bool data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-boolean.html.
    /// </remarks>
    [OIDHandler(OID.BOOLOID, OID.BOOLARRAYOID)]
    public class BoolHandler : StructTypeHandler<bool>
    {
        public BoolHandler()
        {
            this.ElementOID = OID.BOOLOID;
            this.ArrayOID = OID.BOOLARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getBoolean().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool pldotnet_getBoolean(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumBoolean().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBoolean(bool value);

        /// <inheritdoc />
        public override bool InputValue(IntPtr datum)
        {
            return pldotnet_getBoolean(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(bool value)
        {
            return pldotnet_createDatumBoolean(value);
        }
    }
}