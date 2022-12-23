CREATE TABLE pldotnettypes (
    bcol  BOOLEAN,
    i2col SMALLINT,
    i4col INTEGER,
    i8col BIGINT,
    f4col REAL,
    f8col DOUBLE PRECISION,
    ncol  NUMERIC,
    vccol VARCHAR
);
INSERT INTO pldotnettypes VALUES (
    true,
    CAST(1 as INT2),
    CAST(32767 as INT4),
    CAST(9223372036854775707 as BIGINT),
    CAST(1.4 as REAL),
    CAST(10.5000000000055 as DOUBLE PRECISION),
    CAST(1.2 as NUMERIC),
    'StringSample;'
);
CREATE TABLE usersavings(ssnum int8, name varchar, sname varchar, balance float4);
INSERT INTO usersavings VALUES (123456789,'Homer','Simpson',2304.55);
INSERT INTO usersavings VALUES (987654321,'Charles Montgomery','Burns',3000000.65);
CREATE OR REPLACE FUNCTION returnCompositeSumFSharp() RETURNS integer AS $$
let sum_record_cb (record : SPI.BpgsqlDbRecord) : int =
    let r = (record :> IDataRecord)
    (r.GetInt32(r.GetOrdinal("c"))) + (r.GetInt32(r.GetOrdinal("b")))
let records: List<SPI.BpgsqlDbRecord> = SPI.Execute "SELECT 3 as c, 7 as b" 1L

[for record in records do
    yield (sum_record_cb record)] |> List.sum |> Some
$$ LANGUAGE plfsharp;
SELECT returnCompositeSumFSharp() = integer '10';

CREATE OR REPLACE FUNCTION checkTypesFSharp() RETURNS boolean AS $$
let check_type (item : IDataRecord) : int =
    if item.["bcol"].GetType() = typeof<System.Boolean> &&
       item.["i2col"].GetType() = typeof<System.Int16> &&
       item.["i4col"].GetType() = typeof<System.Int32> &&
       item.["i8col"].GetType() = typeof<System.Int64> &&
       item.["f4col"].GetType() = typeof<System.Single> &&
       item.["f8col"].GetType() = typeof<System.Double> &&
       item.["ncol"].GetType() = typeof<System.Decimal>  &&
       item.["vccol"].GetType()  = typeof<System.String>
    then 0
    else 1
let records: List<SPI.BpgsqlDbRecord> = SPI.Execute "SELECT * from pldotnettypes" 1L
let result: int = [for x in records do yield (check_type x)] |> List.sum
0 = result |> Some
$$ LANGUAGE plfsharp;
SELECT checkTypesFSharp() is true;

CREATE OR REPLACE FUNCTION getUsersWithBalanceFSharp(searchbalance real) RETURNS varchar AS $$
let get_name (user: IDataRecord) : string =
    let name : obj = user.GetValue(user.GetOrdinal("name"))
    match name with
    | null -> ""
    | _ -> downcast name : string

let get_sname (user: IDataRecord) : string =
    let sname : obj = user.GetValue(user.GetOrdinal("sname"))
    match sname with
    | null -> ""
    | _ -> downcast sname : string

let get_ssnum (user: IDataRecord) : int64 =
    let ssnum : obj = user.GetValue(user.GetOrdinal("ssnum"))
    match ssnum with
    | null -> 0L
    | _ -> downcast ssnum : int64
let same_balance (user: IDataRecord) : string =
    sprintf ", %s %s (Social Security Number %d)" (get_name user) (get_sname user) (get_ssnum user)

match searchbalance with
| Some sb ->
    let records: List<SPI.BpgsqlDbRecord> = SPI.Execute "SELECT * from usersavings" 1L
    let res : string = sprintf "User(s) found with %.2f account balance" sb;
    let result : string =  [for record in records do yield (same_balance record)] |> List.fold (+) ""
    res + result + "." |> Some
| _ -> None
$$ LANGUAGE plfsharp;
SELECT getUsersWithBalanceFSharp(2304.55) = varchar 'User(s) found with 2304.55 account balance, Homer Simpson (Social Security Number 123456789).';

CREATE OR REPLACE FUNCTION getUserDescriptionFSharp(ssnum bigint) RETURNS varchar AS $$
let get_name (user: IDataRecord) : string =
    user.GetString(user.GetOrdinal("name"))
let get_sname (user: IDataRecord) : string =
    user.GetString(user.GetOrdinal("sname"))
let get_ssnum (user: IDataRecord) : int64 =
    user.GetInt64(user.GetOrdinal("ssnum"))
let get_balance (user: IDataRecord) : float32 =
    user.GetFloat(user.GetOrdinal("balance"))

let no_user : string option = Some "No user found"

let same_ssnum (_ssnum : int64) (user: IDataRecord) : bool =
    _ssnum = get_ssnum user

let format_str (user: IDataRecord) : string =
    let user_name : string = get_name user
    let user_sname : string = get_sname user
    let user_ssnum : int64 = get_ssnum user
    let user_balance : float32 = get_balance user
    sprintf "%s %s, Social security Number %d, has %.2f account balance." user_name user_sname user_ssnum user_balance

match ssnum with
| Some _ssnum ->
    let query = sprintf "SELECT * from usersavings WHERE ssnum=%d" _ssnum
    let records : List<SPI.BpgsqlDbRecord> = SPI.Execute query 1L
    let user = Seq.tryFind (same_ssnum _ssnum) records
    match user with
    | Some u -> u |> format_str |> Some
    | _ -> no_user
| _ -> no_user
$$ LANGUAGE plfsharp;
SELECT getUserDescriptionFSharp(123456789) = varchar 'Homer Simpson, Social security Number 123456789, has 2304.55 account balance.';
SELECT getUserDescriptionFSharp(987654321) = varchar 'Charles Montgomery Burns, Social security Number 987654321, has 3000000.75 account balance.';

DROP TABLE pldotnettypes CASCADE;
DROP TABLE usersavings CASCADE;
