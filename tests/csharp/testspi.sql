CREATE OR REPLACE FUNCTION SPISumIntegers(a integer, b integer, c integer) RETURNS integer AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT {a} as a, {b} as b, {c} as c");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);

    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        int _b = reader.GetInt32(1);
        int _c = reader.GetInt32(2);
        sum += _a + _b + _c;
    }
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4-spi', 'SPISumIntegers1', SPISumIntegers(1, 2, 3) = 6;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4-spi', 'SPISumIntegers2', SPISumIntegers(4, 456, 2456) = 2916;

DROP TABLE IF EXISTS SPITEST;
CREATE TABLE SPITEST (
    BOOLCOL BOOLEAN,
    I2COL SMALLINT,
    I4COL INTEGER,
    I8COL BIGINT,
    F4COL FLOAT4,
    F8COL FLOAT8,
    POINTCOL POINT,
    LINECOL LINE,
    LSEGCOL LSEG,
    BOXCOL BOX,
    POLYGONCOL POLYGON,
    PATHCOL PATH,
    CIRCLECOL CIRCLE,
    DATECOL DATE,
    TIMECOL TIME,
    TIMETZCOL TIMETZ,
    TIMESTAMPCOL TIMESTAMP,
    TIMESTAMPTZCOL TIMESTAMP WITH TIME ZONE,
    INTERVALCOL INTERVAL,
    MACCOL MACADDR,
    MAC8COL MACADDR8,
    INETCOL INET,
    CIDRCOL CIDR,
    MONEYCOL MONEY,
    VARBITCOL BIT VARYING(64),
    BITCOL BIT(8),
    BYTEACOL BYTEA,
    TEXTCOL TEXT,
    CHARCOL CHAR(10),
    VARCHARCOL VARCHAR(10),
    XMLCOL XML,
    JSONCOL JSON,
    UUIDCOL UUID,
    I4RCOL INT4RANGE,
    I8RCOL INT8RANGE,
    TSRCOL TSRANGE,
    TSTZRCOL TSTZRANGE,
    DRCOL DATERANGE
);

INSERT INTO SPITEST(
    BOOLCOL,
    I2COL,
    I4COL,
    I8COL,
    F4COL,
    F8COL,
    POINTCOL,
    LINECOL,
    LSEGCOL,
    BOXCOL,
    POLYGONCOL,
    PATHCOL,
    CIRCLECOL,
    DATECOL,
    TIMECOL,
    TIMETZCOL,
    TIMESTAMPCOL,
    TIMESTAMPTZCOL,
    INTERVALCOL,
    MACCOL,
    MAC8COL,
    INETCOL,
    CIDRCOL,
    MONEYCOL,
    VARBITCOL,
    BITCOL,
    BYTEACOL,
    TEXTCOL,
    CHARCOL,
    VARCHARCOL,
    XMLCOL,
    JSONCOL,
    UUIDCOL,
    I4RCOL,
    I8RCOL,
    TSRCOL,
    TSTZRCOL,
    DRCOL
)
VALUES (
    TRUE,
    2023,
    327670,
    21474836470,
    10.20212,
    10.202120222023202,
    '(1.0,2.0)',
    '{1.0,2.0,3.0}',
    '((1.0,1.0),(2.0,2.0))',
    '((1.0,1.0),(2.0,2.0))',
    '((1.0,1.0),(2.0,2.0))',
    '( (1.0,1.0), (2.0,1.0), (2.0,2.0), (2.0,1.0) )',
    '<(1.0,1.0),0.5>',
    '1989-07-25',
    '12:00:01',
    '05:30-03:00',
    '1989-07-25 12:00:01',
    '1989-07-25 12:00:01 America/New_York',
    ('1999-03-24 19:18:17 America/Los_Angeles'::timestamp - '1989-07-25 12:00:01 America/New_York'::timestamp),
    'f6:30:00:00:00:00',
    'f6:30:00:ff:fe:00:00:00',
    '207.69.188.185/24',
    '207.69.188.185',
    '31415.92',
    '100111001',
    '10011001',
    '\x5468616e6b20796f7521',
    'hello',
    'good bye',
    'Bye Bye',
    '<?xml version="1.0" encoding="utf-8"?><title>Hello, World!</title>',
    '{"a":"Sunday", "b":"Monday", "c":"Tuesday"}',
    '123e4567-e89b-12d3-a456-426614174000',
    '(-2147483648,2147483644)',
    '[,9223372036854775804)',
    '[2010-01-01 14:30, 2010-01-01 15:30)',
    '["2013-10-01 07:00:00-03","2013-10-01 07:15:00-03")',
    '[2020-01-01,2021-01-01)'
);

CREATE OR REPLACE FUNCTION SPIReturnBoolValue() RETURNS BOOLEAN AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return reader.GetBoolean(reader.GetOrdinal("BOOLCOL"));
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-bool-spi', 'SPIReturnBoolValue', SPIReturnBoolValue() is true;

CREATE OR REPLACE FUNCTION SPIIncSmallInt(a SMALLINT) RETURNS SMALLINT AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return (short)(reader.GetInt16(reader.GetOrdinal("I2COL")) + a);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int2-spi', 'SPIIncSmallInt', SPIIncSmallInt('15'::SMALLINT) = '2038'::SMALLINT;

CREATE OR REPLACE FUNCTION SPIIncInt(a INTEGER) RETURNS INTEGER AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return (int)(reader.GetInt32(reader.GetOrdinal("I4COL")) + a);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4-spi', 'SPIIncInt', SPIIncInt(327670) = 655340;

CREATE OR REPLACE FUNCTION SPIIncBigInt(a BIGINT) RETURNS BIGINT AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return (long)(reader.GetInt64(reader.GetOrdinal("I8COL")) + a);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int8-spi', 'SPIIncBigInt', SPIIncBigInt(214748364700) = 236223201170;


CREATE OR REPLACE FUNCTION SPIIncFloat(a FLOAT4) RETURNS FLOAT4 AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return (float)(reader.GetFloat(reader.GetOrdinal("F4COL")) + a);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-float4-spi', 'SPIIncFloat', SPIIncFloat('0.01252'::FLOAT4) = '10.21464'::FLOAT4;

CREATE OR REPLACE FUNCTION SPIIncDouble(a FLOAT8) RETURNS FLOAT8 AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    return (float)(reader.GetFloat(reader.GetOrdinal("F4COL")) + a);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-float8-spi', 'SPIIncDouble', SPIIncDouble('0.0125215699789'::FLOAT8) = ' 10.214641792002102'::FLOAT4;

CREATE OR REPLACE FUNCTION SPIIncPoint(a POINT) RETURNS POINT AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlPoint b = reader.GetFieldValue<NpgsqlPoint>(reader.GetOrdinal("POINTCOL"));
    b.X += ((NpgsqlPoint)a).X;
    b.Y += ((NpgsqlPoint)a).Y;
    return b;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-point-spi', 'SPIIncPoint', SPIIncPoint('(2.285,5.575)'::POINT) ~= '(3.285,7.575)'::POINT;

CREATE OR REPLACE FUNCTION SPIIncLine(a LINE) RETURNS LINE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlLine b = reader.GetFieldValue<NpgsqlLine>(reader.GetOrdinal("LINECOL"));
    b.A += ((NpgsqlLine)a).A;
    b.B += ((NpgsqlLine)a).B;
    b.C += ((NpgsqlLine)a).C;
    return b;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-line-spi', 'SPIIncLine', SPIIncLine('{3.0,2.0,1.0}'::LINE) = '{4.0,4.0,4.0}'::LINE;

CREATE OR REPLACE FUNCTION SPIModifyLSeg(a POINT) RETURNS LSEG AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlLSeg b = reader.GetFieldValue<NpgsqlLSeg>(reader.GetOrdinal("LSEGCOL"));
    b.End = (NpgsqlPoint)a;
    return b;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-lseg-spi', 'SPIModifyLSeg', SPIModifyLSeg('(3.0,3.0)'::POINT) = '((1.0,1.0),(3.0,3.0))'::LSEG;

CREATE OR REPLACE FUNCTION SPIIncBox(a FLOAT8) RETURNS BOX AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlBox b = reader.GetFieldValue<NpgsqlBox>(reader.GetOrdinal("BOXCOL"));
    return new NpgsqlBox(new NpgsqlPoint(b.UpperRight.X + (double)a, b.UpperRight.Y + (double)a), new NpgsqlPoint(b.LowerLeft.X + (double)a, b.LowerLeft.Y + (double)a));
    return b;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-box-spi', 'SPIIncBox', SPIIncBox('0.2575'::FLOAT8) = '((1.2575,1.2575),(2.2575,2.2575))'::BOX;

CREATE OR REPLACE FUNCTION SPIIncPolygon(a POINT) RETURNS POLYGON AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlPolygon b = reader.GetFieldValue<NpgsqlPolygon>(reader.GetOrdinal("POLYGONCOL"));
    int npts = b.Count;
    NpgsqlPolygon new_polygon = new NpgsqlPolygon(npts+1);
    for(int i = 0; i < npts; i++)
    {
        new_polygon.Add(b[i]);
    }
    new_polygon.Add(((NpgsqlPoint)a));
    return new_polygon;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-polygon-spi', 'SPIIncPolygon', SPIIncPolygon('(6.5,8.8)'::POINT) ~= '((1.0,1.0),(2.0,2.0),(6.5,8.8))'::POLYGON;

CREATE OR REPLACE FUNCTION SPIIncPath(a POINT) RETURNS PATH AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlPath b = reader.GetFieldValue<NpgsqlPath>(reader.GetOrdinal("PATHCOL"));
    int npts = b.Count;
    NpgsqlPath new_path = new NpgsqlPath(npts+1);
    for(int i = 0; i < npts; i++)
    {
        new_path.Add(b[i]);
    }
    new_path.Add(((NpgsqlPoint)a));
    return new_path;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-path-spi', 'SPIIncPath', SPIIncPath('(3.1415,6.2830)'::POINT) = '( (1.0,1.0), (2.0,1.0), (2.0,2.0), (2.0,1.0), (3.1415,6.2830) )'::PATH;

CREATE OR REPLACE FUNCTION SPIIncCircle(a POINT, b float8) RETURNS CIRCLE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlCircle c = reader.GetFieldValue<NpgsqlCircle>(reader.GetOrdinal("CIRCLECOL"));
    c.X += ((NpgsqlPoint)a).X;
    c.Y += ((NpgsqlPoint)a).Y;
    c.Radius += (double)b;
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-circle-spi', 'SPIIncCircle', SPIIncCircle('(1.5,2.5)'::POINT, '1.25354555'::FLOAT8) = '<(2.5,3.5),1.75354555>'::CIRCLE;

CREATE OR REPLACE FUNCTION SPIIncDate(d INTEGER, m INTEGER, y INTEGER) RETURNS DATE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    DateOnly date = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("DATECOL"));
    date = date.AddDays((int)d);
    date = date.AddMonths((int)m);
    date = date.AddYears((int)y);
    return date;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-date-spi', 'SPIIncDate', SPIIncDate(5,2,10) = '1999-09-30'::DATE;

CREATE OR REPLACE FUNCTION SPIIncTime(m INTEGER, h INTEGER) RETURNS TIME AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    TimeOnly time = reader.GetFieldValue<TimeOnly>(reader.GetOrdinal("TIMECOL"));
    time = time.AddMinutes((double)m);
    time = time.AddHours((double)h);
    return time;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-time-spi', 'SPIIncTime', SPIIncTime(24, 2) = '14:24:01'::TIME;

CREATE OR REPLACE FUNCTION SPIIncTimeWithTimeZonze(hours FLOAT4) RETURNS TIME WITH TIME ZONE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    DateTimeOffset timetz = reader.GetFieldValue<DateTimeOffset>(reader.GetOrdinal("TIMETZCOL"));
    return timetz.AddHours((double)hours);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-timetz-spi', 'SPIIncTimeWithTimeZonze', SPIIncTimeWithTimeZonze(1.75) = '07:15-03:00'::TIMETZ;

CREATE OR REPLACE FUNCTION SPIIncTimestamp(days INTEGER, hours INTEGER, minutes INTEGER) RETURNS TIMESTAMP AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    DateTime timestamp = reader.GetFieldValue<DateTime>(reader.GetOrdinal("TIMESTAMPCOL"));
    timestamp = timestamp.AddDays((double)days);
    timestamp = timestamp.AddHours((double)hours);
    timestamp = timestamp.AddMinutes((double)minutes);
    return timestamp;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-timestamp-spi', 'SPIIncTimestamp', SPIIncTimestamp(2, 6, 25) = '1989-07-27 18:25:01'::TIMESTAMP;

CREATE OR REPLACE FUNCTION SPIIncTimestamptz(days INTEGER, hours INTEGER, minutes INTEGER, seconds INTEGER) RETURNS TIMESTAMP WITH TIME ZONE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    DateTime timestamp = reader.GetFieldValue<DateTime>(reader.GetOrdinal("TIMESTAMPCOL"));
    timestamp = timestamp.AddDays((double)days);
    timestamp = timestamp.AddHours((double)hours);
    timestamp = timestamp.AddMinutes((double)minutes);
    timestamp = timestamp.AddSeconds((double)seconds);
    return timestamp;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-timestamptz-spi', 'SPIIncTimestamptz', SPIIncTimestamptz(2, 6, 25, 40) = '1989-07-27 18:25:41 America/New_York'::TIMESTAMP;

CREATE OR REPLACE FUNCTION SPIIncInterval(days INTEGER, months INTEGER) RETURNS INTERVAL AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlInterval orig_interval = reader.GetFieldValue<NpgsqlInterval>(reader.GetOrdinal("INTERVALCOL"));
    NpgsqlInterval new_interval = new NpgsqlInterval(orig_interval.Months + (int)months, orig_interval.Days + (int)days, orig_interval.Time);
    return new_interval;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-interval-spi', 'SPIIncInterval', SPIIncInterval(60, 1) = '1 month 3589 days 07:18:16'::INTERVAL;

CREATE OR REPLACE FUNCTION SPIIncMacAddress(inc INTEGER) RETURNS MACADDR AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    PhysicalAddress mac = reader.GetFieldValue<PhysicalAddress>(reader.GetOrdinal("MACCOL"));
    byte[] bytes = mac.GetAddressBytes();
    bytes[5] += (byte)inc;
    return new PhysicalAddress(bytes);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-macaddr-spi', 'SPIIncMacAddress', SPIIncMacAddress(5) = 'f6:30:00:00:00:05'::MACADDR;

CREATE OR REPLACE FUNCTION SPIIncMacAddress8(inc INTEGER) RETURNS MACADDR8 AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    PhysicalAddress mac = reader.GetFieldValue<PhysicalAddress>(reader.GetOrdinal("MAC8COL"));
    byte[] bytes = mac.GetAddressBytes();
    bytes[7] += (byte)inc;
    return new PhysicalAddress(bytes);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-macaddr8-spi', 'SPIIncMacAddress8', SPIIncMacAddress8(3) = 'f6:30:00:ff:fe:00:00:03'::MACADDR8;

CREATE OR REPLACE FUNCTION SPIIncInet(mask INTEGER, incip INTEGER) RETURNS INET AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    (IPAddress Address, int Netmask) inet = reader.GetFieldValue<(IPAddress Address, int Netmask)>(reader.GetOrdinal("INETCOL"));
    inet.Netmask += (int)mask;
    byte[] bytes = inet.Address.GetAddressBytes();
    bytes[bytes.Length-1] += (byte)incip;
    inet.Address = new IPAddress(bytes);
    return inet;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-inet-spi', 'SPIIncInet', SPIIncInet(3 , 20) = '207.69.188.205/27'::INET;

CREATE OR REPLACE FUNCTION SPIIncCIDR(incip INTEGER) RETURNS CIDR AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    (IPAddress Address, int Netmask) inet = reader.GetFieldValue<(IPAddress Address, int Netmask)>(reader.GetOrdinal("CIDRCOL"));
    byte[] bytes = inet.Address.GetAddressBytes();
    bytes[bytes.Length-1] += (byte)incip;
    inet.Address = new IPAddress(bytes);
    return inet;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-cidr-spi', 'SPIIncCIDR', SPIIncCIDR(45) = '207.69.188.230/32'::CIDR;

CREATE OR REPLACE FUNCTION SPIIncMoney(a MONEY) RETURNS MONEY AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    decimal money = reader.GetFieldValue<decimal>(reader.GetOrdinal("MONEYCOL"));
    return money + (decimal)a;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-money-spi', 'SPIIncMoney', SPIIncMoney('1315.23'::MONEY) = '32731.15'::MONEY;

CREATE OR REPLACE FUNCTION SPIConcatenateVarBit(b BIT VARYING) RETURNS BIT VARYING AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    BitArray a = reader.GetFieldValue<BitArray>(reader.GetOrdinal("VARBITCOL"));
    BitArray c = new BitArray(a.Length+b.Length);
    for(int i = 0; i < a.Length;i++)
        c[i] = a[i];
    for(int i = 0, cont = a.Length; i < b.Length;i++)
        c[cont++] = b[i];
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-varbit-spi', 'SPIConcatenateVarBit', SPIConcatenateVarBit('111010111101111000'::BIT VARYING) = '100111001111010111101111000'::BIT VARYING;

CREATE OR REPLACE FUNCTION SPIConcatenateBit(b BIT(10)) RETURNS BIT AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    BitArray a = reader.GetFieldValue<BitArray>(reader.GetOrdinal("BITCOL"));
    BitArray c = new BitArray(a.Length+b.Length);
    for(int i = 0; i < a.Length;i++)
        c[i] = a[i];
    for(int i = 0, cont = a.Length; i < b.Length;i++)
        c[cont++] = b[i];
    return c;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-bit-spi', 'SPIConcatenateBit', SPIConcatenateBit('1110101'::BIT(10)) = '100110011110101000'::BIT(18);

CREATE OR REPLACE FUNCTION SPIConcatenateBytea(b BYTEA) RETURNS BYTEA AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    byte[] a = reader.GetFieldValue<byte[]>(reader.GetOrdinal("BYTEACOL"));
    UTF8Encoding utf8_e = new UTF8Encoding();
    string s1 = utf8_e.GetString(a, 0, a.Length);
    string s2 = utf8_e.GetString(b, 0, b.Length);
    Elog.Info($"s1 = {s1} | s2 = {s2}");
    string result = s1 + " " + s2;
    return utf8_e.GetBytes(result);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-bytea-spi', 'SPIConcatenateBytea', SPIConcatenateBytea('You are welcome!'::BYTEA) = 'Thank you! You are welcome!'::BYTEA;

CREATE OR REPLACE FUNCTION SPIUpperText() RETURNS TEXT AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    string a = reader.GetFieldValue<string>(reader.GetOrdinal("TEXTCOL"));
    return a.ToUpper();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-text-spi', 'SPIUpperText', SPIUpperText() = 'HELLO'::TEXT;

CREATE OR REPLACE FUNCTION SPIConcatenateBpchar(b BPCHAR) RETURNS BPCHAR AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    string a = reader.GetFieldValue<string>(reader.GetOrdinal("CHARCOL"));
    return a + b;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-bpchar-spi', 'SPIConcatenateBpchar', SPIConcatenateBpchar('my friend...'::BPCHAR) = 'good bye  my friend...'::TEXT;

CREATE OR REPLACE FUNCTION SPIConcatenateVarchar(b VARCHAR) RETURNS VARCHAR AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    string a = reader.GetFieldValue<string>(reader.GetOrdinal("VARCHARCOL"));
    return (a + " " + b).ToUpper();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-varchar-spi', 'SPIConcatenateVarchar', SPIConcatenateVarchar('my friend...'::VARCHAR) = 'BYE BYE MY FRIEND...'::TEXT;

CREATE OR REPLACE FUNCTION SPIReplaceXmlEnconding(enc TEXT) RETURNS XML AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    string xml = reader.GetFieldValue<string>(reader.GetOrdinal("XMLCOL"));
    return xml.Replace("utf-8", enc);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-xml-spi', 'SPIReplaceXmlEnconding', SPIReplaceXmlEnconding('US-ASCII'::text)::TEXT = '<?xml version="1.0" encoding="US-ASCII"?><title>Hello, World!</title>'::XML::TEXT;

CREATE OR REPLACE FUNCTION SPIAddKeyToJson(b TEXT, c TEXT) RETURNS XML AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    string json = reader.GetFieldValue<string>(reader.GetOrdinal("JSONCOL"));
    string new_value = $", \"{b}\":\"{c}\""+"}";
    return json.Replace("}", new_value);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-json-spi', 'SPIAddKeyToJson', SPIAddKeyToJson('d'::TEXT, 'Wednesday'::TEXT)::TEXT = '{"a":"Sunday", "b":"Monday", "c":"Tuesday", "d":"Wednesday"}'::JSON::TEXT;

CREATE OR REPLACE FUNCTION SPICombineUuid(b UUID) RETURNS UUID AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    Guid a = reader.GetFieldValue<Guid>(reader.GetOrdinal("UUIDCOL"));
    string aStr = a.ToString();
    string bStr = b.ToString();
    var aList = aStr.Split('-');
    var bList = bStr.Split('-');
    string newUuuidStr = aList[0] + aList[1] + aList[2] + bList[3] + bList[4];
    return new Guid(newUuuidStr);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-uuid-spi', 'SPICombineUuid', SPICombineUuid('a0eebc99-9c0b-4ef8-bb6d-6bb9bd380a11'::UUID) = '123e4567-e89b-12d3-bb6d-6bb9bd380a11'::UUID;

CREATE OR REPLACE FUNCTION SPIModifyInt4Range(lower BOOLEAN, value INT4) RETURNS INT4RANGE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlRange<int> range = reader.GetFieldValue<NpgsqlRange<int>>(reader.GetOrdinal("I4RCOL"));
    int lowerBound = (bool)lower ? range.LowerBound + (int)value : range.LowerBound;
    int upperBound = (bool)lower ? range.UpperBound : range.UpperBound + (int)value;
    return new NpgsqlRange<int>(lowerBound, range.LowerBoundIsInclusive, range.LowerBoundInfinite, upperBound, range.UpperBoundIsInclusive, range.UpperBoundInfinite);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4range-spi', 'SPIModifyInt4Range1', SPIModifyInt4Range(true, 48) = '(-2147483600,2147483644)'::INT4RANGE;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4range-spi', 'SPIModifyInt4Range2', SPIModifyInt4Range(false, -10) = '(-2147483648,2147483634)'::INT4RANGE;

CREATE OR REPLACE FUNCTION SPIModifyInt8Range(lower BOOLEAN, value INT8) RETURNS INT8RANGE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlRange<long> range = reader.GetFieldValue<NpgsqlRange<long>>(reader.GetOrdinal("I8RCOL"));
    long lowerBound = (bool)lower ? range.LowerBound + (long)value : range.LowerBound;
    long upperBound = (bool)lower ? range.UpperBound : range.UpperBound + (long)value;
    return new NpgsqlRange<long>(lowerBound, range.LowerBoundIsInclusive, range.LowerBoundInfinite && lowerBound == 0, upperBound, range.UpperBoundIsInclusive, range.UpperBoundInfinite && upperBound == 0);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int8range-spi', 'SPIModifyInt8Range1', SPIModifyInt8Range(true, 2147483657) = '(2147483657,9223372036854775804)'::INT8RANGE;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int8range-spi', 'SPIModifyInt8Range2', SPIModifyInt8Range(false, -10) = '[,9223372036854775794)'::INT8RANGE;

CREATE OR REPLACE FUNCTION SPIModifyTimeRange(lower BOOLEAN, days_to_add INT, minutes_do_add INT) RETURNS TSRANGE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlRange<DateTime> range = reader.GetFieldValue<NpgsqlRange<DateTime>>(reader.GetOrdinal("TSRCOL"));
    DateTime lowerBound = (bool)lower ? range.LowerBound.AddDays((int)days_to_add).AddMinutes((double)minutes_do_add) : range.LowerBound;
    DateTime upperBound = (bool)lower ? range.UpperBound : range.UpperBound.AddDays((int)days_to_add).AddMinutes((double)minutes_do_add);
    return new NpgsqlRange<DateTime>(lowerBound, range.LowerBoundIsInclusive, range.LowerBoundInfinite, upperBound, range.UpperBoundIsInclusive, range.UpperBoundInfinite);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-tsrange-spi', 'SPIModifyTimeRange1', SPIModifyTimeRange(true, 0, 15) = '[2010-01-01 14:45, 2010-01-01 15:30)'::TSRANGE;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-tsrange-spi', 'SPIModifyTimeRange2', SPIModifyTimeRange(false, 10, 25) = '[2010-01-01 14:30, 2010-01-11 15:55)'::TSRANGE;

CREATE OR REPLACE FUNCTION SPIModifyTimeTzRange(lower BOOLEAN, days_to_add INT, minutes_do_add INT) RETURNS TSTZRANGE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlRange<DateTime> range = reader.GetFieldValue<NpgsqlRange<DateTime>>(reader.GetOrdinal("TSTZRCOL"));
    DateTime lowerBound = (bool)lower ? range.LowerBound.AddDays((int)days_to_add).AddMinutes((double)minutes_do_add) : range.LowerBound;
    DateTime upperBound = (bool)lower ? range.UpperBound : range.UpperBound.AddDays((int)days_to_add).AddMinutes((double)minutes_do_add);
    return new NpgsqlRange<DateTime>(lowerBound, range.LowerBoundIsInclusive, range.LowerBoundInfinite, upperBound, range.UpperBoundIsInclusive, range.UpperBoundInfinite);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-tstzrange-spi', 'SPIModifyTimeTzRange1', SPIModifyTimeTzRange(true, 0, 10) = '["2013-10-01 07:10:00-03","2013-10-01 07:15:00-03")'::TSTZRANGE;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-tstzrange-spi', 'SPIModifyTimeTzRange2', SPIModifyTimeTzRange(false, 15, 40) = '["2013-10-01 07:00:00-03","2013-10-16 07:55:00-03")'::TSTZRANGE;

CREATE OR REPLACE FUNCTION SPIModifyDateRange(lower BOOLEAN, days INTEGER, months INTEGER, years INTEGER) RETURNS DATERANGE AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPITEST");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    reader.Read();
    NpgsqlRange<DateOnly> range = reader.GetFieldValue<NpgsqlRange<DateOnly>>(reader.GetOrdinal("DRCOL"));
    DateOnly lowerBound = (bool)lower ? range.LowerBound.AddDays((int)days).AddMonths((int)months).AddYears((int)years) : range.LowerBound;
    DateOnly upperBound = (bool)lower ? range.UpperBound : range.UpperBound.AddDays((int)days).AddMonths((int)months).AddYears((int)years);
    return new NpgsqlRange<DateOnly>(lowerBound, range.LowerBoundIsInclusive, range.LowerBoundInfinite, upperBound, range.UpperBoundIsInclusive, range.UpperBoundInfinite);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-daterange-spi', 'SPIModifyDateRange1', SPIModifyDateRange(true, 10, 5, 0) = '[2020-06-11,2021-01-01)'::DATERANGE;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-daterange-spi', 'SPIModifyDateRange2', SPIModifyDateRange(false, 15, 14, 2) = '[2020-01-01,2024-03-16)'::DATERANGE;

--- NULL tests

DROP TABLE IF EXISTS SPINULLS;
CREATE TABLE SPINULLS (
    BOOLCOL BOOLEAN, -- basic type
    I4COL INTEGER, -- basic type
    F8COL FLOAT8, -- basic type
    DATECOL DATE, -- complex struct
    TEXTCOL TEXT, -- object
    MAC8COL MACADDR8 -- object
);
INSERT INTO SPINULLS(
    BOOLCOL,
    I4COL,
    F8COL,
    DATECOL,
    TEXTCOL,
    MAC8COL
)
VALUES (
    NULL::BOOLEAN,
    NULL::INTEGER,
    NULL::FLOAT8,
    '2023-02-01'::DATE,
    'Hello Terrible World!'::TEXT,
    'ab-01-2b-31-41-fa-ab-ac'::MACADDR8
);
INSERT INTO SPINULLS(
    BOOLCOL,
    I4COL,
    F8COL,
    DATECOL,
    TEXTCOL,
    MAC8COL
)
VALUES (
    'TRUE'::BOOLEAN,
    '2023'::INTEGER,
    '3.141592653'::FLOAT8,
    NULL::DATE,
    NULL::TEXT,
    NULL::MACADDR8
);

CREATE OR REPLACE FUNCTION SPINullBool() RETURNS BOOLEAN[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<bool?> bools = new ();
    while(reader.Read())
    {
        bools.Add(reader.GetFieldValue<bool?>(reader.GetOrdinal("BOOLCOL")));
    }
    return bools.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-bool-null-spi', 'SPINullBool', SPINullBool() = ARRAY[NULL::BOOLEAN, TRUE];

CREATE OR REPLACE FUNCTION SPINullInt4() RETURNS INTEGER[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<int?> ints = new ();
    while(reader.Read())
    {
        ints.Add(reader.GetFieldValue<int?>(reader.GetOrdinal("I4COL")));
    }
    return ints.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int4-null-spi', 'SPINullInt4', SPINullInt4() = ARRAY[NULL::INTEGER, 2023];

CREATE OR REPLACE FUNCTION SPINullFloat8() RETURNS FLOAT8[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<double?> doubles = new ();
    while(reader.Read())
    {
        doubles.Add(reader.GetFieldValue<double?>(reader.GetOrdinal("F8COL")));
    }
    return doubles.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-float8-null-spi', 'SPINullFloat8', SPINullFloat8() = ARRAY[NULL::FLOAT8, '3.141592653'::FLOAT8];

CREATE OR REPLACE FUNCTION SPINullDate() RETURNS DATE[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<DateOnly?> dates = new ();
    while(reader.Read())
    {
        dates.Add(reader.GetFieldValue<DateOnly?>(reader.GetOrdinal("DATECOL")));
    }
    return dates.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-date-null-spi', 'SPINullDate', SPINullDate() = ARRAY['2023-02-01'::DATE, NULL::DATE];

CREATE OR REPLACE FUNCTION SPINullString() RETURNS TEXT[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<string?> strings = new ();
    while(reader.Read())
    {
        strings.Add(reader.GetFieldValue<string?>(reader.GetOrdinal("TEXTCOL")));
    }
    return strings.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-string-null-spi', 'SPINullString', SPINullString() = ARRAY['Hello Terrible World!'::TEXT, NULL::TEXT];

CREATE OR REPLACE FUNCTION SPINullMac8() RETURNS MACADDR8[] AS $$
    var dataSource = NpgsqlMultiHostDataSource.Create();
    var cmd = dataSource.CreateCommand($"SELECT * FROM SPINULLS");
    var reader = cmd.ExecuteDbDataReader(CommandBehavior.Default);
    List<PhysicalAddress?> addresses = new ();
    while(reader.Read())
    {
        addresses.Add(reader.GetFieldValue<PhysicalAddress?>(reader.GetOrdinal("MAC8COL")));
    }
    return addresses.ToArray();
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-macaddr8-null-spi', 'SPINullMac8', SPINullMac8() = ARRAY['ab:01:2b:31:41:fa:ab:ac'::MACADDR8, NULL::MACADDR8];

CREATE OR REPLACE FUNCTION SPISumIntegers2(a integer, b integer, c integer) RETURNS integer AS $$
    using var conn = new NpgsqlConnection();
    var command = new NpgsqlCommand($"SELECT {a} as a, {b} as b, {c} as c; SELECT {2*a} as a, {2*b} as b, {2*c} as c", conn);
    var reader = command.ExecuteReader();

    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        int _b = reader.GetInt32(1);
        int _c = reader.GetInt32(2);
        Elog.Info($"Returned values of the FIRST query: {_a} | {_b} | {_c}");
        sum += _a + _b + _c;
    }

    reader.NextResult();

    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        int _b = reader.GetInt32(1);
        int _c = reader.GetInt32(2);
        Elog.Info($"Returned values of the SECOND query: {_a} | {_b} | {_c}");
        sum += _a + _b + _c;
    }
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'SPISumIntegers2', SPISumIntegers2(1, 2, 3) = 18;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'SPISumIntegers2', SPISumIntegers2(4, 0, 5) = 27;

CREATE OR REPLACE FUNCTION MultiQueriesTest(num INTEGER) RETURNS BOOLEAN AS $$
try
{
    var sb = new StringBuilder();
    for (var i = 0; i < num; i++)
    {
        sb.Append($"SELECT @p{i};");
    }
    using var cmd = new NpgsqlCommand(sb.ToString());
    for (var i = 0; i < num; i++)
    {
        cmd.Parameters.AddWithValue($"p{i}", NpgsqlDbType.Integer, i);
    }
    using var reader = cmd.ExecuteReader();
    for (var i = 0; i < num; i++)
    {
        reader.Read();
        int returned_value = reader.GetInt32(0);
        Elog.Info($"Query {i} | Returned value = {returned_value}");
        if (returned_value != i)
        {
            throw new InvalidOperationException("The value is different!");
        }
        reader.NextResult();
    }
    return true;
}
catch (Exception e)
{
    Elog.Warning($"ERROR: {e.ToString()}");
    return false;
}
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'MultiQueriesTest1', MultiQueriesTest(1);
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'MultiQueriesTest2', MultiQueriesTest(10);
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'MultiQueriesTest3', MultiQueriesTest(500);

CREATE OR REPLACE FUNCTION SPISUMIntegersPositionParameters(num INTEGER) RETURNS INTEGER AS $$
    var cmd = new NpgsqlCommand($"SELECT $1; SELECT $2")
    {
        Parameters = { new() { Value = num }, new() { Value = 2*num } }
    };

    var reader = cmd.ExecuteReader();

    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        sum += _a;
    }

    reader.NextResult();

    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        sum += _a;
    }
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'SPISUMIntegersPositionParameters1', SPISUMIntegersPositionParameters(1) = 3;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery', 'SPISUMIntegersPositionParameters2', SPISUMIntegersPositionParameters(10) = 30;

CREATE OR REPLACE FUNCTION SPITestingCompoudParameters() RETURNS INTEGER AS $$
    var sb = new StringBuilder();
    sb.Append("DROP TABLE IF EXISTS SPI_COMPOUD_TESTS;");
    sb.Append("CREATE TABLE IF NOT EXISTS SPI_COMPOUD_TESTS (ID INTEGER);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@a);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@b);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@c);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@d);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@e);");
    sb.Append("UPDATE SPI_COMPOUD_TESTS SET ID = 2 * ID;");
    sb.Append("DELETE FROM SPI_COMPOUD_TESTS WHERE ID = @f;");
    sb.Append("SELECT * FROM SPI_COMPOUD_TESTS;");

    using var conn = new NpgsqlConnection();
    var cmd = new NpgsqlCommand(sb.ToString(), conn);

    cmd.Parameters.AddWithValue("a", NpgsqlDbType.Integer, 1);
    cmd.Parameters.AddWithValue("b", NpgsqlDbType.Integer, 2);
    cmd.Parameters.AddWithValue("c", NpgsqlDbType.Integer, 3);
    cmd.Parameters.AddWithValue("d", NpgsqlDbType.Integer, 4);
    cmd.Parameters.AddWithValue("e", NpgsqlDbType.Integer, 5);

    cmd.Parameters.AddWithValue("f", NpgsqlDbType.Integer, 8);

    var reader = cmd.ExecuteReader();

    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();

    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        Elog.Info($"Returned value = {_a}");
        sum += _a;
    }
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery-compoud', 'SPITestingCompoudParameters', SPITestingCompoudParameters() = 22;

CREATE OR REPLACE FUNCTION SPITestingCompoudPositionalParameters() RETURNS INTEGER AS $$
    var sb = new StringBuilder();
    sb.Append("DROP TABLE IF EXISTS SPI_COMPOUD_TESTS;");
    sb.Append("CREATE TABLE IF NOT EXISTS SPI_COMPOUD_TESTS (ID INTEGER);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($2);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($3);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($4);");
    sb.Append("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($5);");
    sb.Append("UPDATE SPI_COMPOUD_TESTS SET ID = 2 * ID;");
    sb.Append("DELETE FROM SPI_COMPOUD_TESTS WHERE ID = $6;");
    sb.Append("SELECT * FROM SPI_COMPOUD_TESTS;");

    using var conn = new NpgsqlConnection();
    var cmd = new NpgsqlCommand(sb.ToString(), conn)
    {
        Parameters = { new() { Value = 1 }, new() { Value = 2 }, new() { Value = 3 }, new() { Value = 4 }, new() { Value = 5 }, new() { Value = 8 } }
    };

    var reader = cmd.ExecuteReader();

    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();

    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        Elog.Info($"Returned value = {_a}");
        sum += _a;
    }
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-multiquery-compoud', 'SPITestingCompoudPositionalParameters', SPITestingCompoudPositionalParameters() = 22;

CREATE OR REPLACE FUNCTION SPIBatchCompoudParameters(init INTEGER, inc INTEGER) RETURNS INTEGER AS $$
    using var conn = new NpgsqlConnection();
    var batch = conn.CreateBatch();
    batch.BatchCommands.Add(new ("DROP TABLE IF EXISTS SPI_COMPOUD_TESTS;"));
    batch.BatchCommands.Add(new ("CREATE TABLE IF NOT EXISTS SPI_COMPOUD_TESTS (ID INTEGER);"));
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@a);") { Parameters = { new("a", init) } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@b);") { Parameters = { new("b", init) } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@c);") { Parameters = { new("c", init) } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@d);") { Parameters = { new("d", init) } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES(@e);") { Parameters = { new("e", init) } });
    init += inc;
    batch.BatchCommands.Add(new ("UPDATE SPI_COMPOUD_TESTS SET ID = 2 * ID;"));
    batch.BatchCommands.Add(new ("SELECT * FROM SPI_COMPOUD_TESTS;"));
    var reader = batch.ExecuteReader();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    int sum = 0;
    while (reader.Read())
    {
        int _a = reader.GetInt32(0);
        Elog.Info($"Returned value = {_a}");
        sum += _a;
    }
    Elog.Info($"Calling SPIBatchCompoudParameters with integer arguments.");
    return sum;
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-batch-query-compoud-parameters', 'SPIBatchCompoudParameters', SPIBatchCompoudParameters(1, 1) = 30;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int-spi-batch-query-compoud-parameters', 'SPIBatchCompoudParameters', SPIBatchCompoudParameters(5, 2) = 90;

CREATE OR REPLACE FUNCTION SPIBatchCompoudParameters(init FLOAT8, inc FLOAT8) RETURNS FLOAT8 AS $$
    using var conn = new NpgsqlConnection();
    var batch = conn.CreateBatch();
    batch.BatchCommands.Add(new ("DROP TABLE IF EXISTS SPI_COMPOUD_TESTS;"));
    batch.BatchCommands.Add(new ("CREATE TABLE IF NOT EXISTS SPI_COMPOUD_TESTS (ID FLOAT8);"));
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);") { Parameters = { new() { Value = init} } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);") { Parameters = { new() { Value = init} } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);") { Parameters = { new() { Value = init} } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);") { Parameters = { new() { Value = init} } });
    init += inc;
    batch.BatchCommands.Add(new ("INSERT INTO SPI_COMPOUD_TESTS (ID) VALUES($1);") { Parameters = { new() { Value = init} } });
    init += inc;
    batch.BatchCommands.Add(new ("UPDATE SPI_COMPOUD_TESTS SET ID = 2.0 * ID;"));
    batch.BatchCommands.Add(new ("SELECT * FROM SPI_COMPOUD_TESTS;"));
    var reader = batch.ExecuteReader();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    reader.NextResult();
    double sum = 0.0;
    while (reader.Read())
    {
        double _a = reader.GetDouble(0);
        Elog.Info($"Returned value = {_a}");
        sum += _a;
    }
    Elog.Info($"Calling SPIBatchCompoudParameters with double arguments.");
    return Math.Round(sum, 5);
$$ LANGUAGE plcsharp;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-float8-spi-batch-query-compoud-parameters', 'SPIBatchCompoudParameters', SPIBatchCompoudParameters('1.5'::FLOAT8, '0.25'::FLOAT8) = 20;
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-float8-spi-batch-query-compoud-parameters', 'SPIBatchCompoudParameters', SPIBatchCompoudParameters('1.5'::FLOAT8, '0.1'::FLOAT8) = 17;

CREATE OR REPLACE PROCEDURE SPITransactionTestCommitFirst() AS $$
    NpgsqlCommand command;
    NpgsqlDataReader reader;
    int i;

    var conn = new NpgsqlConnection();
    var batch = conn.CreateBatch();
    batch.BatchCommands.Add(new ("DROP TABLE IF EXISTS TRANSACTION_TEST;"));
    batch.BatchCommands.Add(new ("CREATE TABLE IF NOT EXISTS TRANSACTION_TEST (a INTEGER, b text);"));
    batch.ExecuteReader();
    var transaction = conn.BeginTransaction();
    for(i = 0; i < 10; i++)
    {
        Elog.Warning($"DEBUG: i={i}");
        command = new NpgsqlCommand($"INSERT INTO TRANSACTION_TEST (a) VALUES ({i})", conn);
        reader = command.ExecuteReader();
        if (i % 2 == 0)
        {
            transaction.Commit();
        }
        else
        {
            transaction.Rollback();
        }
    }
    command = new NpgsqlCommand("SELECT * FROM TRANSACTION_TEST", conn);
    reader = command.ExecuteReader();
    i = 0;
    while(reader.Read())
    {
        int value = reader.GetInt32(0);
        Elog.Info($"Row[{i++}] = {value}");
    }
$$ LANGUAGE plcsharp;
CALL SPITransactionTestCommitFirst();
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int32-spi-transaction', 'SPITransactionTestCommitFirst', CASE WHEN SUM(a) = 20 THEN TRUE ELSE FALSE END AS RESULT FROM TRANSACTION_TEST;

CREATE OR REPLACE PROCEDURE SPITransactionTestRollbackFirst() AS $$
    NpgsqlCommand command;
    NpgsqlDataReader reader;
    int i;

    var conn = new NpgsqlConnection();
    var batch = conn.CreateBatch();
    batch.BatchCommands.Add(new ("DROP TABLE IF EXISTS TRANSACTION_TEST;"));
    batch.BatchCommands.Add(new ("CREATE TABLE IF NOT EXISTS TRANSACTION_TEST (a INTEGER, b text);"));
    batch.ExecuteReader();
    var transaction = conn.BeginTransaction();
    for(i = 0; i < 10; i++)
    {
        Elog.Warning($"DEBUG: i={i}");
        command = new NpgsqlCommand($"INSERT INTO TRANSACTION_TEST (a) VALUES ({i})", conn);
        reader = command.ExecuteReader();
        if (i % 2 != 0)
        {
            transaction.Commit();
        }
        else
        {
            transaction.Rollback();
        }
    }
    command = new NpgsqlCommand("SELECT * FROM TRANSACTION_TEST", conn);
    reader = command.ExecuteReader();
    i = 0;
    while(reader.Read())
    {
        int value = reader.GetInt32(0);
        Elog.Info($"Row[{i++}] = {value}");
    }
$$ LANGUAGE plcsharp;
CALL SPITransactionTestRollbackFirst();
INSERT INTO automated_test_results (FEATURE, TEST_NAME, RESULT)
SELECT 'c#-int32-spi-transaction', 'SPITransactionTestRollbackFirst', CASE WHEN SUM(a) = 25 THEN TRUE ELSE FALSE END AS RESULT FROM TRANSACTION_TEST;
