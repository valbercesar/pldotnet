using System;
using System.Runtime.InteropServices;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.BOOLOID, OID.BOOLARRAYOID)]
    public class bool_handler : struct_type_handler<bool>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool pldotnet_getBoolean(IntPtr datum);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumBoolean(bool value);

        public override bool input_value(IntPtr datum)
        {
            return pldotnet_getBoolean(datum);
        }

        public override IntPtr output_value(bool value)
        {
            return pldotnet_createDatumBoolean(value);
        }
    }
}