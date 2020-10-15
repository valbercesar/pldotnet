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
open AssemblyCache

[<Struct>]
[<StructLayout (LayoutKind.Sequential)>]
type LibArgs =
    struct
        val mutable FuncOid : int
        val mutable SourceCode : System.IntPtr
        new(foid:int, source:System.IntPtr) = { FuncOid = foid; SourceCode = source }
    end

type Engine() = 
    
    static member checker = FSharpChecker.Create()

    static member show_errors errors =
        for e in errors do
            printfn "%A" e
        
    static member RunUserFunction (asm: Assembly) (args: System.IntPtr) (argLength: int) : int = 
        let userClass = asm.GetType("PlDotNETUserSpace.UserClass")
        let method = userClass.GetMethod("CallFunction")
        try
            method.Invoke(null, [|args; argLength|]) :?> int
        with
            | _ -> -1
        
    static member Run (args: System.IntPtr) (argLength: int) : int = 0
        
    static member Compile (args: System.IntPtr) (argLength: int) : int =
        let libargs = Marshal.PtrToStructure<LibArgs>(args)
        let source_code = Marshal.PtrToStringAuto(libargs.SourceCode)
        let fakeInput : string = "/tmp/UserClass.fs"
        let fakeOutput : string = "/tmp/UserClass.dll"
        try
            match AssemblyCache.assemblyCache.TryGetValue libargs.FuncOid with
            | true, asm ->
                Engine.RunUserFunction asm args argLength
            | _ -> 
                let options = Engine.GetAllFlags fakeInput fakeOutput
                let files = [| (fakeInput, source_code); (fakeOutput, "") |]
                let vfs = PlDotNET.VirtualFileSystem.GetFileSystem files Shim.FileSystem
                let errors, exitCode, dynAssembly =
                    Engine.checker.CompileToDynamicAssembly(options, execute = None)
                    |> Async.RunSynchronously
                match (exitCode, dynAssembly) with
                | 0, Some asm1 -> 
                    AssemblyCache.assemblyCache.Add(libargs.FuncOid, asm1)
                    Engine.RunUserFunction asm1 args argLength
                | _ -> 1
        with
            | _ -> 
                1
        

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
                sysLib "FSharp.Core"
    
        let allFlags =
            [| yield "-o"; yield output;
               yield "-a"; yield input;
               yield "--optimize-";
               yield "--noframework";
               // yield "--debug:full";
               // yield "--define:DEBUG";
               yield "--simpleresolution";
               // yield "--doc:test.xml";
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
                   sysLib "System.Net.WebClient" ]
               for r in references do
                     yield "-r:" + r |]
        allFlags

module FSharpEngine = 
    
    [<EntryPoint>]
    let main(args) : int =
        let str = @"
namespace PlDotNETUserSpace

type UserClass =
    static member CallFunction (args: System.IntPtr) (argLength: int) : int =
        1223"
        let oid = 234l
        let abc = Marshal.StringToHGlobalAuto(str)
        let libargs = new LibArgs(oid, abc)
        let size = Marshal.SizeOf(typeof<LibArgs>)
        let ptr = Marshal.AllocHGlobal(size)
        Marshal.StructureToPtr(libargs, ptr, false)

        let timer = new System.Diagnostics.Stopwatch()
        let nanosecPerTick = (1000L*1000L*1000L) /  Stopwatch.Frequency

        let mutable total : double = 0.0

        for i in [0..1..100] do
            timer.Restart()
            let result = Engine.Compile ptr size
            let elapsed = 0.001 * ((double)(timer.ElapsedTicks * nanosecPerTick))
            total <- 
                match i with
                | 0 -> total
                | _ -> total + elapsed
            printfn "It: %A Result %A - Elapsed time: %A ns and %A ms" i result elapsed (timer.ElapsedMilliseconds)

        printfn "Average: %A ns" (total / 100.0)
        0
