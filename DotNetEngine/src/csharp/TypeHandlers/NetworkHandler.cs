using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;

namespace PlDotNET_Handler
{
    [OIDHandler(OID.MACADDROID, OID.MACADDRARRAYOID)]
    public class macaddr_handler : object_type_handler<PhysicalAddress>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumMacAddressAttributes(IntPtr datum, int length, byte[] bytes);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumMacAddress(int length, byte[] bytes);

        public override PhysicalAddress input_value(IntPtr datum)
        {
            byte[] bytes = new byte[6];
            pldotnet_getDatumMacAddressAttributes(datum, 6, bytes);
            return new PhysicalAddress(bytes);
        }

        public override IntPtr output_value(PhysicalAddress value)
        {
            return pldotnet_createDatumMacAddress(6, value.GetAddressBytes());
        }
    }

    [OIDHandler(OID.MACADDR8OID, OID.MACADDR8ARRAYOID)]
    public class macaddr8_handler : object_type_handler<PhysicalAddress>
    {
        public override PhysicalAddress input_value(IntPtr datum)
        {
            byte[] bytes = new byte[8];
            macaddr_handler.pldotnet_getDatumMacAddressAttributes(datum, 8, bytes);
            return new PhysicalAddress(bytes);
        }

        public override IntPtr output_value(PhysicalAddress value)
        {
            return macaddr_handler.pldotnet_createDatumMacAddress(8, value.GetAddressBytes());
        }
    }

    [OIDHandler(OID.INETOID, OID.INETARRAYOID)]
    public class inet_handler : struct_type_handler<(IPAddress Address, int Netmask)>
    {
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumInetAttributes(IntPtr datum, ref int nelem, byte[] bytes, ref int netmask);

        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInet(int length, byte[] bytes, int netmask);

        public override (IPAddress Address, int Netmask) input_value(IntPtr datum)
        {
            int nelem = 0, netmask = 0;
            byte[] bytes = new byte[16];
            pldotnet_getDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
            byte[] new_bytes = new byte[nelem];
            for (int i = 0; i < nelem; i++)
            {
                new_bytes[i] = bytes[i];
            }
            return (new IPAddress(new_bytes), netmask);
        }

        public override IntPtr output_value((IPAddress Address, int Netmask) value)
        {
            return pldotnet_createDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask); ;
        }
    }

    [OIDHandler(OID.CIDROID, OID.CIDRARRAYOID)]
    public class cidr_handler : struct_type_handler<(IPAddress Address, int Netmask)>
    {
        public override (IPAddress Address, int Netmask) input_value(IntPtr datum)
        {
            int nelem = 0, netmask = 0;
            byte[] bytes = new byte[16];
            inet_handler.pldotnet_getDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
            byte[] new_bytes = new byte[nelem];
            for (int i = 0; i < nelem; i++)
            {
                new_bytes[i] = bytes[i];
            }
            return (new IPAddress(new_bytes), netmask);
        }

        public override IntPtr output_value((IPAddress Address, int Netmask) value)
        {
            elog.pldotnet_Elog(19, "\n\nWe still need to check if the result CIDR object is acceptable!!!\n\n");
            return inet_handler.pldotnet_createDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask); ;
        }
    }
}