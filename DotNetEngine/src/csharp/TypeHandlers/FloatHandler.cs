using System;
using System.Runtime.InteropServices;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.FLOAT4OID, OID.FLOAT4ARRAYOID)]
    public class float_handler : struct_type_handler<float>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern float pldotnet_getFloat(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumFloat(float value);

        public override float input_value(IntPtr datum)
        {
            return pldotnet_getFloat(datum);
        }

        public override IntPtr output_value(float value)
        {
            return pldotnet_createDatumFloat(value);
        }
    }

    [OIDHandler(OID.FLOAT8OID, OID.FLOAT8ARRAYOID)]
    public class double_handler : struct_type_handler<double>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern double pldotnet_getDouble(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumDouble(double value);

        public override double input_value(IntPtr datum)
        {
            return pldotnet_getDouble(datum);
        }

        public override IntPtr output_value(double value)
        {
            return pldotnet_createDatumDouble(value);
        }
    }
}