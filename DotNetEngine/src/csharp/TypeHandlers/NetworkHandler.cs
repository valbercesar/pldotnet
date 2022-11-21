using System;
using System.Runtime.InteropServices;
using System.Net.NetworkInformation;
using System.Net;

namespace PlDotNET.Handler
{
    /// <summary>
    /// A type handler for the PostgreSQL macaddr data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    /// </remarks>
    [OIDHandler(OID.MACADDROID, OID.MACADDRARRAYOID)]
    public class MacaddrHandler : ObjectTypeHandler<PhysicalAddress>
    {
        public MacaddrHandler()
        {
            this.ElementOID = OID.MACADDROID;
            this.ArrayOID = OID.MACADDRARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumMacAddressAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumMacAddressAttributes(IntPtr datum, int length, byte[] bytes);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumMacAddress().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumMacAddress(int length, byte[] bytes);

        /// <inheritdoc />
        public override PhysicalAddress InputValue(IntPtr datum)
        {
            byte[] bytes = new byte[6];
            pldotnet_getDatumMacAddressAttributes(datum, 6, bytes);
            return new PhysicalAddress(bytes);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(PhysicalAddress value)
        {
            return pldotnet_createDatumMacAddress(6, value.GetAddressBytes());
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL macaddr8 data types.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    /// </remarks>
    [OIDHandler(OID.MACADDR8OID, OID.MACADDR8ARRAYOID)]
    public class Macaddr8Handler : ObjectTypeHandler<PhysicalAddress>
    {
        public Macaddr8Handler()
        {
            this.ElementOID = OID.MACADDR8OID;
            this.ArrayOID = OID.MACADDR8ARRAYOID;
        }

        /// <inheritdoc />
        public override PhysicalAddress InputValue(IntPtr datum)
        {
            byte[] bytes = new byte[8];
            MacaddrHandler.pldotnet_getDatumMacAddressAttributes(datum, 8, bytes);
            return new PhysicalAddress(bytes);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(PhysicalAddress value)
        {
            return MacaddrHandler.pldotnet_createDatumMacAddress(8, value.GetAddressBytes());
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL inet data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    /// </remarks>
    [OIDHandler(OID.INETOID, OID.INETARRAYOID)]
    public class InetHandler : StructTypeHandler<(IPAddress Address, int Netmask)>
    {
        public InetHandler()
        {
            this.ElementOID = OID.INETOID;
            this.ArrayOID = OID.INETARRAYOID;
        }

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_getDatumInetAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_getDatumInetAttributes(IntPtr datum, ref int nelem, byte[] bytes, ref int netmask);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_createDatumInet().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_createDatumInet(int length, byte[] bytes, int netmask);

        /// <inheritdoc />
        public override (IPAddress Address, int Netmask) InputValue(IntPtr datum)
        {
            int nelem = 0, netmask = 0;
            byte[] bytes = new byte[16];
            pldotnet_getDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
            byte[] newBytes = new byte[nelem];
            for (int i = 0; i < nelem; i++)
            {
                newBytes[i] = bytes[i];
            }
            return (new IPAddress(newBytes), netmask);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue((IPAddress Address, int Netmask) value)
        {
            return pldotnet_createDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask); ;
        }
    }

    /// <summary>
    /// A type handler for the PostgreSQL cidr data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-net-types.html.
    /// </remarks>
    [OIDHandler(OID.CIDROID, OID.CIDRARRAYOID)]
    public class CidrHandler : StructTypeHandler<(IPAddress Address, int Netmask)>
    {
        public CidrHandler()
        {
            this.ElementOID = OID.CIDROID;
            this.ArrayOID = OID.CIDRARRAYOID;
        }

        /// <inheritdoc />
        public override (IPAddress Address, int Netmask) InputValue(IntPtr datum)
        {
            int nelem = 0, netmask = 0;
            byte[] bytes = new byte[16];
            InetHandler.pldotnet_getDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
            byte[] newBytes = new byte[nelem];
            for (int i = 0; i < nelem; i++)
            {
                newBytes[i] = bytes[i];
            }
            return (new IPAddress(newBytes), netmask);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue((IPAddress Address, int Netmask) value)
        {
            Elog.pldotnet_Elog(19, "\n\nWe still need to check if the result CIDR object is acceptable!!!\n\n");
            return InetHandler.pldotnet_createDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask); ;
        }
    }
}