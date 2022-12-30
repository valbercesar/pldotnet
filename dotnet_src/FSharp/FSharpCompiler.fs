// <copyright file="FSharpCompiler.fs" company="Brick Abode">
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

#if INTERACTIVE
#r "FSharp.Compiler.Service.dll"
#endif

namespace PlDotNET.FSharp

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Reflection
open System.Runtime.InteropServices
open System.Runtime.Loader
open System.Text

open FSharp.Compiler.CodeAnalysis
open FSharp.Compiler.Text

/// The FSharpCompiler type provides methods for compiling F# source code.
type FSharpCompiler() =

    /// C function declared in pldotnet_main.h.
    /// See ::pldotnet_Elog().
    [<DllImport("@PKG_LIBDIR/pldotnet.so", CallingConvention=CallingConvention.Cdecl)>]
    static extern void pldotnet_Elog(int level, string nessage)

    /// The F# checker used for compiling F# source code.
    ///
    /// @see FSharpChecker
    /// @see FSharpChecker.Create
    static member checker = FSharpChecker.Create()

    /// Compiles the given F# source code and returns the path to the generated DLL.
    /// If the compilation fails, an error message is logged and an empty string is returned.
    ///
    /// @param functionId The ID of the function.
    /// @param functionName The name of the function.
    /// @param sourceCode The F# source code to be compiled.
    /// @param extraAssemblies An array of strings containing the paths to any extra assemblies that should be included in the compilation.
    /// @return The path to the generated DLL, or an empty string if the compilation failed.
    static member CompileFSharpSourceCode (functionId: uint) (functionName: string) (sourceCode: string) (extraAssemblies: string[]) : string =
        let functionIdString = string functionId
        let inputFile : string = "/tmp/PlDotNET/fsharp/UserHandler_" + functionName + ".fs"
        let outputFile : string = "/tmp/PlDotNET/dlls/UserHandler_" + functionIdString + ".dll"
        let options = FSharpCompiler.GetAllFlags inputFile outputFile extraAssemblies

        let errors, exitCode =
            FSharpCompiler.checker.Compile(options)
                |> Async.RunSynchronously

        match (exitCode) with
        | 0 ->
            outputFile
        | _ ->
            let sb = new System.Text.StringBuilder()
            sb.AppendLine($"PL.NET could not compile the following F# generated code:") |> ignore
            sb.AppendLine($"**********") |> ignore
            sb.AppendLine(sourceCode) |> ignore
            sb.AppendLine($"**********") |> ignore
            sb.AppendLine($"Here are the compilation results:") |> ignore
            for e in errors do
                sb.AppendLine(e.ToString()) |> ignore
            pldotnet_Elog(19, sb.ToString())
            ""

    /// Generates an array of strings containing the command-line flags to be passed to the F# compiler.
    ///
    /// @param input The path to the input F# source file.
    /// @param output The path to the output DLL file.
    /// @param extraAssemblies An array of strings containing the paths to any extra assemblies that should be included in the compilation.
    /// @return An array of strings containing the command-line flags to be passed to the F# compiler.
    /// @remarks The generated array includes the paths to the required system assemblies and the F# Core library.
    static member GetAllFlags (input : string) (output : string) (extraAssemblies : string[]) =
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
                System.AppContext.BaseDirectory + "/FSharp/FSharp.Core.dll"
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
                   sysLib "System.Net.Primitives"
                   fsCore4300() ]
               for r in references do
                     yield "-r:" + r
               for r in extraAssemblies do
                     yield "-r:" + r |]
        allFlags
