#if INTERACTIVE
#r "FSharp.Compiler.Service.dll"
#endif

namespace PlDotNET.FSharp

open System
open System.IO
open System.Text
open System.Reflection
open System.Diagnostics
open System.Runtime.InteropServices
open System.Runtime.Loader
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.AbstractIL.Internal
open FSharp.Compiler.AbstractIL.Internal.Library

open System.Collections.Generic

open PlDotNET.FSharp.VirtualFileSystem

type FSharpCompiler() =

    [<DllImport("@PKG_LIBDIR/pldotnet.so", CallingConvention=CallingConvention.Cdecl)>]
    static extern void pldotnet_Elog(int level, string nessage)

    static member checker = FSharpChecker.Create()

    static member CompileFSharpSourceCode (functionId: uint) (sourceCode: string) : string =
        pldotnet_Elog(17, "===========================");
        pldotnet_Elog(17, "Compiling F# source code");
        pldotnet_Elog(17, $"Source code:\n{sourceCode}");
        pldotnet_Elog(17, "===========================");
        let functionIdString = string functionId
        let inputFile : string = "/tmp/PlDotNET/UserFunction_" + functionIdString + ".fs"
        let outputFile : string = "/tmp/PlDotNET/UserFunction_" + functionIdString + ".dll"
        let options = FSharpCompiler.GetAllFlags inputFile outputFile
        let files = [| (inputFile, sourceCode); (outputFile, "") |]

        // set a virtual file system to avoid read/write to disk
        let vfs = VirtualFileSystem.SetVirtualFileSystem files Shim.FileSystem

        let errors, exitCode =
            FSharpCompiler.checker.Compile(options)
                |> Async.RunSynchronously

        match (exitCode) with
        | 0 ->
            outputFile
        | _ ->
            let sb = new System.Text.StringBuilder()
            sb.AppendLine("\n********ERROR************\n") |> ignore
            for e in errors do
                sb.AppendLine(e.ToString()) |> ignore
            sb.AppendLine("\n********ERROR************\n") |> ignore
            pldotnet_Elog(19, sb.ToString())
            ""

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
            [| yield "fsc.exe";
               yield "-o"; yield output;
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
                   sysLib "System.Net.NetworkInformation"
                   "/var/lib/DotNetEngine/src/csharp/bin/Release/net6.0/PlDotNET.dll"
                   "/var/lib/DotNetEngine/src/csharp/bin/Release/net6.0/Npgsql.dll"
                   fsCore4300() ]
               for r in references do
                     yield "-r:" + r |]
        allFlags
