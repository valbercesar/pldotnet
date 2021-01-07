CREATE OR REPLACE FUNCTION returnCompositeSumFSharp() RETURNS integer AS $$
let (?) (expando: ExpandoObject) (s : string) =
    let d = expando :> IDictionary<string, obj>
    d.[s]
let (?<-) (expando: ExpandoObject) (s : string) (o: obj) =
    let d = expando :> IDictionary<string, obj>
    d.Remove(s) |> ignore
    d.Add(s, o)
let sum_cb (c : obj) (b : obj) =
    let _c = downcast c : int
    let _b = downcast b : int
    _c + _b
let expando: List<ExpandoObject> = SPI.Execute "SELECT 3 as c, 7 as b" 1L
[for x in expando do yield (sum_cb (x?c) (x?b) )] |> List.sum |> Some
$$ LANGUAGE plfsharp;
SELECT returnCompositeSumFSharp() = integer '10';

CREATE OR REPLACE FUNCTION checkTypesFSharp() RETURNS boolean AS $$
let (?) (expando: ExpandoObject) (s : string) =
    let d = expando :> IDictionary<string, obj>
    d.[s]
let check_type (item : ExpandoObject) : int =
    if item?bcol.GetType() = typeof<bool> &&
       item?i2col.GetType() = typeof<int16> &&
       item?i4col.GetType() = typeof<int32> &&
       item?i8col.GetType() = typeof<int64> &&
       item?f4col.GetType() = typeof<float32> &&
       item?f8col.GetType() = typeof<double> &&
       item?ncol.GetType() = typeof<decimal> &&
       item?vccol.GetType()  = typeof<string>
    then 0
    else 1
let expando: List<ExpandoObject> = SPI.Execute "SELECT * from pldotnettypes" 1L
let result: int = [for x in expando do yield (check_type x)] |> List.sum
0 = result |> Some
$$ LANGUAGE plfsharp;
SELECT checkTypesFSharp() is true;

CREATE OR REPLACE FUNCTION getUsersWithBalanceFSharp(searchbalance real) RETURNS varchar AS $$
let (?) (expando: ExpandoObject) (s : string) =
    let d = expando :> IDictionary<string, obj>
    d.[s]

let get_name (user: ExpandoObject) : string =
    let name : obj = user?name
    match name with
    | null -> ""
    | _ -> downcast name : string

let get_sname (user: ExpandoObject) : string =
    let sname : obj = user?sname
    match sname with
    | null -> ""
    | _ -> downcast sname : string

let get_ssnum (user: ExpandoObject) : int64 =
    let ssnum : obj = user?ssnum
    match ssnum with
    | null -> 0L
    | _ -> downcast ssnum : int64

let same_balance (user: ExpandoObject) : string =
    sprintf ", %s %s (Social Security Number %d)" (get_name user) (get_sname user) (get_ssnum user)

match searchbalance with
| Some sb ->
    let expando: List<ExpandoObject> = SPI.Execute "SELECT * from usersavings" 1L
    let res : string = sprintf "User(s) found with %.2f account balance" sb;
    let result : string =  [for x in expando do yield (same_balance x)] |> List.fold (+) ""
    res + result + "." |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT getUsersWithBalanceFSharp(2304.55) = varchar 'User(s) found with 2304.55 account balance, Homer Simpson (Social Security Number 123456789).';

CREATE OR REPLACE FUNCTION getUserDescription(ssnum bigint) RETURNS varchar AS $$
let (?) (expando: ExpandoObject) (s : string) =
    let d = expando :> IDictionary<string, obj>
    d.[s]
let get_name (user: ExpandoObject) : string =
    let name : obj = user?name
    match name with
    | null -> ""
    | _ -> downcast name : string

let get_sname (user: ExpandoObject) : string =
    let sname : obj = user?sname
    match sname with
    | null -> ""
    | _ -> downcast sname : string

let get_ssnum (user: ExpandoObject) : int64 =
    let ssnum : obj = user?ssnum
    match ssnum with
    | null -> 0L
    | _ -> downcast ssnum : int64

let get_balance (user: ExpandoObject) : float32 =
    let balance : obj = user?balance
    match balance with
    | null -> 0.0f
    | _ -> downcast balance : float32

let no_user : string option = Some "No user found"

let same_ssnum (_ssnum : int64) (user: ExpandoObject) : bool =
    _ssnum = get_ssnum user

let format_str (user: ExpandoObject) : string =
    let user_name : string = get_name user
    let user_sname : string = get_sname user
    let user_ssnum : int64 = get_ssnum user
    let user_balance : float32 = get_balance user
    sprintf "%s %s, Social security Number %d, has %.2f account balance." user_name user_sname user_ssnum user_balance

match ssnum with
| Some _ssnum ->
    let query = sprintf "SELECT * from usersavings WHERE ssnum=%d" _ssnum
    let expando = SPI.Execute query 1L
    let user = Seq.tryFind (same_ssnum _ssnum) expando
    match user with
    | Some u -> format_str u |> Some
    | _ -> no_user
| _ -> no_user
$$ LANGUAGE plfsharp;

SELECT getUserDescription(123456789) = varchar 'Homer Simpson, Social security Number 123456789, has 2304.55 account balance.';
SELECT getUserDescription(987654321) = varchar 'Charles Montgomery Burns, Social security Number 987654321, has 3000000.75 account balance.';
