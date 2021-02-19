module FunctionCache

open System
open System.IO
open System.Reflection
open System.Collections.Generic

type CachedFunction =
    struct
        val mutable sourceCode : string
        val mutable userFunction : Func<IntPtr, int, int>
        val mutable addProperty : Action<IntPtr, int>
        val mutable resetFuncRecords : Action
        new(
            src: string,
            userFunc: Func<IntPtr, int, int>,
            addProp: Action<IntPtr, int>,
            resetFunc : Action) = {
                sourceCode = src;
                userFunction = userFunc;
                addProperty = addProp;
                resetFuncRecords = resetFunc
            }
    end

let mutable funcBuiltCodeDict : Dictionary<uint, CachedFunction> = new Dictionary<uint, CachedFunction>()
let mutable cachedFunction : CachedFunction option = None
let mutable functionId : uint = 0u
let mutable needsReset : bool = false

let findCachedFunction (fid : uint) : CachedFunction option =
    try
        match funcBuiltCodeDict.TryGetValue fid with
        | true, cached -> Some cached
        | _ -> None
    with
        | _ -> None

let addToHashDict (fid : uint) (cached : CachedFunction option) : unit =
    match cached with
    | Some _cached ->
        funcBuiltCodeDict.[fid] <- _cached
    | _ -> ()

let removeFromHashDict (fid : uint) : unit =
    funcBuiltCodeDict.Remove fid |> ignore

let setCachedFunction (fid : uint) (cached: CachedFunction option) : unit =
    functionId <- fid
    cachedFunction <- cached
    needsReset <- true

let saveCachedFunction (fid : uint) (cached: CachedFunction option) : unit =
    setCachedFunction fid cached
    addToHashDict fid cached
