#if INTERACTIVE
#r "FSharp.Compiler.Service.dll"
#endif

namespace PlDotNET

open System
open System.IO
open System.Text
open System.Reflection
open System.Diagnostics
open System.Runtime.InteropServices
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.AbstractIL.Internal
open FSharp.Compiler.AbstractIL.Internal.Library

open System.Collections.Generic

open PlDotNET.VirtualFileSystem
open FunctionCache

[<Struct>]
[<StructLayout (LayoutKind.Sequential)>]
type LibArgs =
    struct
        val mutable Source : IntPtr
        val mutable Result : int
        val mutable FunctionId: uint
        new(src: IntPtr, res: int, fid : uint) = { Source = src; Result = res; FunctionId = fid }
    end

type Engine() =

    static member checker = FSharpChecker.Create()

    static member RetrieveFromRemoteStorage (sourceCode : string) (functionId : uint) : bool = false
    static member SendToRemoteStorage (sourceCode: string) (functionId: uint) (assembly: Assembly) : bool = true
    static member RunCachedFunction (cached: CachedFunction option) (args: IntPtr) (argLength: int) : int =
        match cached with
        | Some _cached ->
            _cached.userFunction.Invoke(args, argLength) |> ignore
            FunctionCache.needsReset <- true
            0
        | _ -> 1

    static member Run (args: IntPtr) (argLength: int) : int =
        let fid = uint (Marshal.ReadInt32(args, argLength))
        match fid <> FunctionCache.functionId with
        | true ->
            match FunctionCache.findCachedFunction fid with
            | None -> 1
            | cached ->
                FunctionCache.setCachedFunction fid cached
                Engine.RunCachedFunction cached args argLength
        | _ -> Engine.RunCachedFunction FunctionCache.cachedFunction args argLength

    static member SetDelegate (functionId: uint) (sourceCode: string) (assembly : Assembly) : int =
        let procClassType1 = assembly.GetType("PlDotNETUserSpace.UserClass")
        let method1 = procClassType1.GetMethod("CallFunction")

        let procClassType2 = assembly.GetType("PlDotNETUserSpace.SPI")
        let method2 = procClassType2.GetMethod("AddProperty")
        let method3 = procClassType2.GetMethod("ResetFuncRecords")

        let userFuncType = typeof<System.Func<IntPtr, int, int>>
        let addPropType = typeof<System.Action<IntPtr, int>>
        let rstPropType = typeof<System.Action>

        let userFunction = Delegate.CreateDelegate(userFuncType, null, method1) :?> System.Func<IntPtr,int,int>
        let addProperty = Delegate.CreateDelegate(addPropType, null, method2) :?> (System.Action<IntPtr,int>)
        let resetFuncRecords = Delegate.CreateDelegate(rstPropType, null, method3) :?> (System.Action)

        let cached = new CachedFunction(sourceCode, userFunction, addProperty, resetFuncRecords) |> Some

        FunctionCache.saveCachedFunction functionId cached
        Engine.SendToRemoteStorage sourceCode functionId assembly |> ignore
        0

    static member CompileUserFunction (functionId: uint) (sourceCode: string) : int =
        let fakeInput : string = "/tmp/UserClass.fs"
        let fakeOutput : string = "/tmp/UserClass.dll"
        let options = Engine.GetAllFlags fakeInput fakeOutput
        let files = [| (fakeInput, sourceCode); (fakeOutput, "") |]

        // set a virtual file system to avoid read/write to disk
        let vfs = VirtualFileSystem.SetVirtualFileSystem files Shim.FileSystem

        let errors, exitCode, dynAssembly =
            Engine.checker.CompileToDynamicAssembly(options, execute = None)
             |> Async.RunSynchronously
        match (exitCode, dynAssembly) with
        | 0, Some assembly -> Engine.SetDelegate functionId sourceCode assembly
        | _ ->
            printfn "%s" "\n********ERROR************\n"
            for e in errors do
                printfn "=======\n%A\n========" e
            printfn "%s" "\n********ERROR************\n"
            1

    static member VerifyCachedFunction (functionId: uint) (cached : CachedFunction option) (sourceCode : string) : bool =
        match cached with
        | Some _cached ->
            match _cached.sourceCode.Equals(sourceCode) with
            | true ->
                FunctionCache.setCachedFunction functionId cached
                true
            | _ ->
                FunctionCache.removeFromHashDict functionId
                false
        | _ -> false

    static member Compile (args: IntPtr) (argLength: int) : int =
        let libArgs = Marshal.PtrToStructure<LibArgs>(args)
        let sourceCode = Marshal.PtrToStringAuto(libArgs.Source)
        let cached = FunctionCache.findCachedFunction libArgs.FunctionId
        let remote =
            match Engine.VerifyCachedFunction libArgs.FunctionId cached sourceCode with
            | false -> Engine.RetrieveFromRemoteStorage sourceCode libArgs.FunctionId
            | _ -> true
        match remote with
        | false -> Engine.CompileUserFunction libArgs.FunctionId sourceCode
        | _ -> 0

    static member InvokeAddProperty (arg: System.IntPtr) (argLength: int) : int =
        match FunctionCache.cachedFunction with
        | Some cached ->
            match FunctionCache.needsReset with
            | true ->
                cached.resetFuncRecords.Invoke()
                FunctionCache.needsReset <- false
            | _ -> ()
            cached.addProperty.Invoke(arg, argLength) |> ignore
            0
        | _ -> 1

    static member GetAllFlags (input : string) (output : string) =
        let sysLib nm =
            if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then // file references only valid on Windows
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) +
                @"\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0\" + nm + ".dll"
            else
                let sysDir = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory()
                let (++) a b = System.IO.Path.Combine(a,b)
                sysDir ++ nm + ".dll"

        let fsCore4300() =
            if System.Environment.OSVersion.Platform = System.PlatformID.Win32NT then // file references only valid on Windows
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFilesX86) +
                @"\Reference Assemblies\Microsoft\FSharp\.NETFramework\v4.0\4.3.0.0\FSharp.Core.dll"
            else
                System.AppContext.BaseDirectory + "fsharp/FSharp.Core.dll"

        let allFlags =
            [| yield "-o"; yield output;
               yield "-a"; yield input;
               yield "--optimize-";
               yield "--noframework";
               yield "--debug:full";
               yield "--define:DEBUG";
               yield "--simpleresolution";
               yield "--doc:test.xml";
               yield "--warn:3";
               yield "--fullpaths";
               yield "--flaterrors";
               let references =
                 [ sysLib "mscorlib"
                   sysLib "System"
                   sysLib "System.Core"
                   sysLib "System.Linq"
                   sysLib "System.Data"
                   sysLib "System.Data.Common"
                   sysLib "System.Linq.Expressions"
                   sysLib "System.Runtime"
                   sysLib "System.Runtime.Numerics"
                   sysLib "System.Private.CoreLib"
                   sysLib "System.Collections"
                   sysLib "System.Net.Requests"
                   sysLib "System.Net.WebClient"
                   sysLib "System.Globalization"
                   sysLib "System.Runtime.InteropServices"
                   sysLib "System.Runtime.Extensions"
                   fsCore4300() ]
               for r in references do
                     yield "-r:" + r |]
        allFlags
