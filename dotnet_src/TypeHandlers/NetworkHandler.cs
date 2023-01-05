// <copyright file="NetworkHandler.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright 2023 Brick Abode
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>

using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

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
        /// See ::pldotnet_GetDatumMacAddressAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_GetDatumMacAddressAttributes(IntPtr datum, int length, byte[] bytes);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumMacAddress().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumMacAddress(int length, byte[] bytes);

        /// <inheritdoc />
        public override PhysicalAddress InputValue(IntPtr datum)
        {
            byte[] bytes = new byte[6];
            pldotnet_GetDatumMacAddressAttributes(datum, 6, bytes);
            return new PhysicalAddress(bytes);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(PhysicalAddress value)
        {
            return pldotnet_CreateDatumMacAddress(6, value.GetAddressBytes());
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
            MacaddrHandler.pldotnet_GetDatumMacAddressAttributes(datum, 8, bytes);
            return new PhysicalAddress(bytes);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(PhysicalAddress value)
        {
            return MacaddrHandler.pldotnet_CreateDatumMacAddress(8, value.GetAddressBytes());
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
        /// See ::pldotnet_GetDatumInetAttributes().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern void pldotnet_GetDatumInetAttributes(IntPtr datum, ref int nelem, byte[] bytes, ref int netmask);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumInet().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumInet(int length, byte[] bytes, int netmask);

        /// <inheritdoc />
        public override (IPAddress Address, int Netmask) InputValue(IntPtr datum)
        {
            int nelem = 0, netmask = 0;
            byte[] bytes = new byte[16];
            pldotnet_GetDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
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
            return pldotnet_CreateDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask);
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
            InetHandler.pldotnet_GetDatumInetAttributes(datum, ref nelem, bytes, ref netmask);
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
            return InetHandler.pldotnet_CreateDatumInet(value.Address.GetAddressBytes().Length, value.Address.GetAddressBytes(), value.Netmask);
        }
    }
}