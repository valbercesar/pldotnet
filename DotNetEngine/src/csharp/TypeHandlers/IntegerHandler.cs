using System;
using System.Runtime.InteropServices;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.INT2OID, OID.INT2ARRAYOID)]
    public class short_handler : struct_type_handler<short>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern short pldotnet_getInt16(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt16(short value);

        public override short input_value(IntPtr datum)
        {
            return pldotnet_getInt16(datum);
        }

        public override IntPtr output_value(short value)
        {
            return pldotnet_createDatumInt16(value);
        }
    }

    [OIDHandler(OID.INT4OID, OID.INT4ARRAYOID)]
    public class int_handler : struct_type_handler<int>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern int pldotnet_getInt32(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt32(int value);

        public override int input_value(IntPtr datum)
        {
            return pldotnet_getInt32(datum);
        }

        public override IntPtr output_value(int value)
        {
            return pldotnet_createDatumInt32(value);
        }
    }

    [OIDHandler(OID.INT8OID, OID.INT8ARRAYOID)]
    public class long_handler : struct_type_handler<long>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern long pldotnet_getInt64(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInt64(long value);

        public override long input_value(IntPtr datum)
        {
            return pldotnet_getInt64(datum);
        }

        public override IntPtr output_value(long value)
        {
            return pldotnet_createDatumInt64(value);
        }
    }
}