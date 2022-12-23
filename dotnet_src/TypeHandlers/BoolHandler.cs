// <copyright file="BoolHandler.cs" company="Brick Abode">
//
// PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
//                      procedural languages (PL)
//
//
// Copyright 2019-2020 Brick Abode
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
        /// See ::pldotnet_GetBoolean().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool pldotnet_GetBoolean(IntPtr datum);

        /// <summary>
        /// C function declared in pldotnet_conversions.h.
        /// See ::pldotnet_CreateDatumBoolean().
        /// </summary>
        [DllImport("@PKG_LIBDIR/pldotnet.so")]
        public static extern IntPtr pldotnet_CreateDatumBoolean(bool value);

        /// <inheritdoc />
        public override bool InputValue(IntPtr datum)
        {
            return pldotnet_GetBoolean(datum);
        }

        /// <inheritdoc />
        public override IntPtr OutputValue(bool value)
        {
            return pldotnet_CreateDatumBoolean(value);
        }
    }
}