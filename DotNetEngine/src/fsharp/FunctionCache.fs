module FunctionCache

open System
open System.IO
open System.Reflection
open System.Collections.Generic

let mutable functionCache = new Dictionary<uint, (string * Func<IntPtr, int, int>)>()
let mutable functionId : uint = 0u
let mutable userFunction : Func<IntPtr, int, int> = new Func<IntPtr, int, int>(fun _ _ -> 1)
let mutable memStream : MemoryStream = new MemoryStream()
