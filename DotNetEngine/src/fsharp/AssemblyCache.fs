module AssemblyCache

open System
open System.IO
open System.Reflection
open System.Collections.Generic

let mutable assemblyCache = new Dictionary<int, Assembly>()