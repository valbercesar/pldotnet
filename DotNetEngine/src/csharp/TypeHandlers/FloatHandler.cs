using System;
using System.Runtime.InteropServices;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL float data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    /// </remarks>
    [OIDHandler(OID.FLOAT4OID, OID.FLOAT4ARRAYOID)]
    public class FloatHandler : StructTypeHandler<float>
    {
        public FloatHandler()
        {
            this.ElementOID = OID.FLOAT4OID;
            this.ArrayOID = OID.FLOAT4ARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getFloat().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern float pldotnet_getFloat(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumFloat().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumFloat(float value);

        /// <inheritdoc />
        public override float InputValue(IntPtr datum)
        {
            return pldotnet_getFloat(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(float value)
        {
            return pldotnet_createDatumFloat(value);
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL double precision data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-numeric.html.
    /// </remarks>
    [OIDHandler(OID.FLOAT8OID, OID.FLOAT8ARRAYOID)]
    public class DoubleHandler : StructTypeHandler<double>
    {
        public DoubleHandler()
        {
            this.ElementOID = OID.FLOAT8OID;
            this.ArrayOID = OID.FLOAT8ARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDouble().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern double pldotnet_getDouble(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumDouble().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumDouble(double value);

        /// <inheritdoc />
        public override double InputValue(IntPtr datum)
        {
            return pldotnet_getDouble(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(double value)
        {
            return pldotnet_createDatumDouble(value);
        }
    }
}