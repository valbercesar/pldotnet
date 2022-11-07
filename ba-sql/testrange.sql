CREATE OR REPLACE FUNCTION rangedemo_4(a int4range) RETURNS int AS $$
elog.pldotnet_Info("# DEBUG: got range " + a);
return 1;
$$ LANGUAGE plcsharp STRICT;

CREATE OR REPLACE FUNCTION rangedemo_8(a int8range) RETURNS int AS $$
elog.pldotnet_Info("# DEBUG: got range " + a);
return 1;
$$ LANGUAGE plcsharp STRICT;

CREATE OR REPLACE FUNCTION rangedemo_ts(a tsrange) RETURNS int AS $$
elog.pldotnet_Info("# DEBUG: got range " + a);
return 1;
$$ LANGUAGE plcsharp STRICT;

SELECT rangedemo_4('[2,6)'::int4range);
SELECT rangedemo_4('(3,7]'::int4range);
SELECT rangedemo_4('(4,8)'::int4range);
SELECT rangedemo_4('[5,9]'::int4range);
SELECT rangedemo_4('[,11)'::int4range);
SELECT rangedemo_4('(12,]'::int4range);
SELECT rangedemo_4('(,13]'::int4range);
SELECT rangedemo_4('[14,)'::int4range);
SELECT rangedemo_4('[15,]'::int4range);
SELECT rangedemo_4('(16,)'::int4range);
SELECT rangedemo_4('[,17]'::int4range);
SELECT rangedemo_4('(,18)'::int4range);

SELECT rangedemo_8('[2,6)'::int8range);
SELECT rangedemo_8('(3,7]'::int8range);
SELECT rangedemo_8('(4,8)'::int8range);
SELECT rangedemo_8('[5,9]'::int8range);
SELECT rangedemo_8('[,11)'::int8range);
SELECT rangedemo_8('(12,]'::int8range);
SELECT rangedemo_8('(,13]'::int8range);
SELECT rangedemo_8('[14,)'::int8range);
SELECT rangedemo_8('[15,]'::int8range);
SELECT rangedemo_8('(16,)'::int8range);
SELECT rangedemo_8('[,17]'::int8range);
SELECT rangedemo_8('(,18)'::int8range);

SELECT rangedemo_ts( '[2021-01-01 14:30, 2021-01-01 15:30]' );
SELECT rangedemo_ts( '[2021-01-01 14:30, 2021-01-01 15:30)' );
SELECT rangedemo_ts( '(2021-01-01 14:30, 2021-01-01 15:30]' );
SELECT rangedemo_ts( '(2021-01-01 14:30, 2021-01-01 15:30)' );
SELECT rangedemo_ts( '[2021-01-01 14:30,]' );
SELECT rangedemo_ts( '[2021-01-01 14:30,)' );
SELECT rangedemo_ts( '(2021-01-01 14:30,]' );
SELECT rangedemo_ts( '(2021-01-01 14:30,)' );
SELECT rangedemo_ts( '[, 2021-01-01 15:30 ]' );
SELECT rangedemo_ts( '[, 2021-01-01 15:30 )' );
SELECT rangedemo_ts( '(, 2021-01-01 15:30 ]' );
SELECT rangedemo_ts( '(, 2021-01-01 15:30 )' );