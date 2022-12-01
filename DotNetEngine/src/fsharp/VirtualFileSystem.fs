// <copyright file="VirtualFileSystem.fs" company="Brick Abode">
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

namespace PlDotNET.FSharp

open System
open System.IO
open System.Text
open System.Collections.Generic
open FSharp.Compiler.AbstractIL.Internal
open FSharp.Compiler.AbstractIL.Internal.Library

module VirtualFileSystem =

    let mutable defaultFileSystem : IFileSystem = Shim.FileSystem

    type VFileSystem(container: (string * string) [], defaultFileSystem: IFileSystem) =
        let files = dict container
        interface IFileSystem with
            // Implement the service to open files for reading and writing
            member __.FileStreamReadShim(f) =
                match files.TryGetValue f with
                | true, text ->  new MemoryStream(Encoding.UTF8.GetBytes(text)) :> Stream
                | _ -> defaultFileSystem.FileStreamReadShim(f)

            member __.FileStreamCreateShim(f) =
                defaultFileSystem.FileStreamCreateShim(f)

            member __.FileStreamWriteExistingShim(f) =
                defaultFileSystem.FileStreamWriteExistingShim(f)

            member __.ReadAllBytesShim(f) =
                defaultFileSystem.ReadAllBytesShim(f)

            // Implement the service related to temporary paths and file time stamps
            member __.GetTempPathShim() =
                defaultFileSystem.GetTempPathShim()

            member __.GetLastWriteTimeShim(f) =
                defaultFileSystem.GetLastWriteTimeShim(f)

            member __.GetFullPathShim(f) =
                defaultFileSystem.GetFullPathShim(f)

            member __.IsInvalidPathShim(f) =
                defaultFileSystem.IsInvalidPathShim(f)

            member __.IsPathRootedShim(f) =
                defaultFileSystem.IsPathRootedShim(f)

            member __.IsStableFileHeuristic(f) =
                defaultFileSystem.IsStableFileHeuristic(f)

            // Implement the service related to file existence and deletion
            member __.SafeExists(f) =
                files.ContainsKey(f) || defaultFileSystem.SafeExists(f)

            member __.FileDelete(f) =
                defaultFileSystem.FileDelete(f)

            // Implement the service related to assembly loading, used to load type providers
            // and for F# interactive.
            member __.AssemblyLoadFrom(f) =
                defaultFileSystem.AssemblyLoadFrom f

            member __.AssemblyLoad(assemblyName) =
                defaultFileSystem.AssemblyLoad assemblyName

    let SetVirtualFileSystem (files : (string * string) []) (shim: IFileSystem) =
        let mvfs = VFileSystem(files, shim)
        Shim.FileSystem <- mvfs
        mvfs

    let RestoreFileSystem : int =
        Shim.FileSystem <- defaultFileSystem
        0
