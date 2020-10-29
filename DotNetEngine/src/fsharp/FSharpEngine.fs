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

    static member Run (args: IntPtr) (argLength: int) : int =
        let fid = uint (Marshal.ReadInt32(args, argLength))
        match fid <> FunctionCache.functionId with
        | false -> 
            try
                match FunctionCache.functionCache.TryGetValue fid with
                | true, (_, fn) ->
                    Engine.SetFunction fid fn  |> ignore
                    fn.Invoke(args, argLength) |> ignore
                    0
                | _ -> 1
            with
                | _ -> 2
        | _ ->
            FunctionCache.userFunction.Invoke(args, argLength) |> ignore
            0

    static member AddUserfunction (functionId: uint) (sourceCode: string) (assembly : Assembly) : int =
        let userClass = assembly.GetType("PlDotNETUserSpace.UserClass")
        let method = userClass.GetMethod("CallFunction")
        let funcType = typeof<System.Func<IntPtr, int, int>>
        FunctionCache.functionId <- functionId
        FunctionCache.userFunction <- Delegate.CreateDelegate(funcType, null, method) :?> (Func<IntPtr,int,int>)
        FunctionCache.functionCache.Add(functionId, (sourceCode, FunctionCache.userFunction))
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
        | 0, Some assembly -> Engine.AddUserfunction functionId sourceCode assembly
        | _ -> 1
    
    static member SetFunction (functionId : uint) (fn : Func<IntPtr, int, int>) : bool =
        FunctionCache.userFunction <- fn
        FunctionCache.functionId <- functionId
        true

    static member Compile (args: IntPtr) (argLength: int) : int = 
        let libArgs = Marshal.PtrToStructure<LibArgs>(args)
        let sourceCode = Marshal.PtrToStringAuto(libArgs.Source)
        let local = 
            try
                match FunctionCache.functionCache.TryGetValue libArgs.FunctionId with
                | true, (src, fn) ->
                    match sourceCode.Equals(src) with
                    | true -> Engine.SetFunction libArgs.FunctionId fn
                    | _ -> false
                | _ -> false
            with
                | _ -> false

        let remote = 
            match local with
            | false -> Engine.RetrieveFromRemoteStorage sourceCode libArgs.FunctionId
            | _ -> true
        match remote with
        | false -> Engine.CompileUserFunction libArgs.FunctionId sourceCode
        | _ -> 0

    static member InvokeAddProperty (arg:System.IntPtr) (argLength:int) : int = 0

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
                "FSharp.Core.dll"

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
                   sysLib "System.Runtime"
                   sysLib "System.Runtime.Numerics"
                   sysLib "System.Private.CoreLib"
                   sysLib "System.Collections"
                   sysLib "System.Net.Requests"
                   sysLib "System.Net.WebClient"
                   fsCore4300() ]
               for r in references do
                     yield "-r:" + r |]
        allFlags
