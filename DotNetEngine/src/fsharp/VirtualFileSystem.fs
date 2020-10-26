namespace PlDotNET

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
