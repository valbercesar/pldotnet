\i sql/benchmark/includes.sql

\i sql/testchar.sql
\i sql/python/testchar.sql
--- \i sql/java/testchar.sql

\set runs 1000

SELECT
    'retVarChar',
    plbench('SELECT retVarChar(''Rodrigo'')', :runs) as plcsharp,
    plbench('SELECT retVarCharPython(''Rodrigo'')', :runs) as plpython;
    --- plbench('SELECT retVarCharJava(''Rodrigo'')', :runs) as pljava;

SELECT
    'retConcatVarChar',
    plbench('SELECT retConcatVarChar(''João '', ''da Silva'')', :runs) as plcsharp,
    plbench('SELECT retConcatVarCharPython(''João '', ''da Silva'')', :runs) as plpython;
    --- plbench('SELECT retConcatVarCharJava(''João '', ''da Silva'')', :runs) as pljava;

SELECT
    'retConcatText',
    plbench('SELECT retConcatText(''João '', ''da Silva'')', :runs) as plcsharp,
    plbench('SELECT retConcatTextPython(''João '', ''da Silva'')', :runs) as plpython;
    --- plbench('SELECT retConcatTextJava(''João '', ''da Silva'')', :runs) as pljava;

SELECT
    'retVarCharText',
    plbench('SELECT retVarCharText(''Homer Jay '', ''Simpson'')', :runs) as plcsharp,
    plbench('SELECT retVarCharTextPython(''Homer Jay '', ''Simpson'')', :runs) as plpython;
    --- plbench('SELECT retVarCharTextJava(''Homer Jay '', ''Simpson'')', :runs) as pljava,

SELECT
    'retChar',
    plbench('SELECT retChar(''R'')', :runs) as plcsharp,
    plbench('SELECT retCharPython(''R'')', :runs) as plpython;
    --- plbench('SELECT retCharJava(''R'')', :runs) as pljava,

SELECT
    'retConcatLetters',
    plbench('SELECT retConcatLetters(''R'', ''C'')', :runs) as plcsharp,
    plbench('SELECT retConcatLettersPython(''R'', ''C'')', :runs) as plpython;
    --- plbench('SELECT retConcatLettersJava(''R'', ''C'')', :runs) as pljava,

SELECT
    'retConcatChars',
    plbench('SELECT retConcatChars(''H.'', ''Simpson'')', :runs) as plcsharp,
    plbench('SELECT retConcatCharsPython(''H.'', ''Simpson'')', :runs) as plpython;
    --- plbench('SELECT retConcatCharsJava(''H.'', ''Simpson'')', :runs) as pljava,

SELECT
    'retConcatVarChars',
    plbench('SELECT retConcatVarChars(''H.'', ''Simpson'')', :runs) as plcsharp,
    plbench('SELECT retConcatVarCharsPython(''H.'', ''Simpson'')', :runs) as plpython;
    --- plbench('SELECT retConcatVarCharsJava(''H.'', ''Simpson'')', :runs) as pljava,

SELECT
    'retConcatVarChars(H. あ)',
    plbench('SELECT retConcatVarChars(''H. あ'', ''Simpson'')', :runs) as plcsharp,
    plbench('SELECT retConcatVarCharsPython(''H. あ'', ''Simpson'')', :runs) as plpython;
    --- plbench('SELECT retConcatVarCharsJava(''H. あ'', ''Simpson'')', :runs) as pljava,

SELECT
    'retNonRegularEncoding(漢字)',
    plbench('SELECT retNonRegularEncoding(''漢字'')', :runs) as plcsharp,
    plbench('SELECT retNonRegularEncodingPython(''漢字'')', :runs) as plpython;
    --- plbench('SELECT retNonRegularEncodingJava(''漢字'')', :runs) as pljava,

SELECT
    'retNonRegularEncoding',
    plbench('SELECT retNonRegularEncoding(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', :runs) as plcsharp,
    plbench('SELECT retNonRegularEncodingPython(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', :runs) as plpython;
    --- plbench('SELECT retNonRegularEncodingJava(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', :runs) as pljava,

SELECT
    'retNonRegularEncoding(ŁĄŻĘĆŃŚŹ)',
    plbench('SELECT retNonRegularEncoding(''ŁĄŻĘĆŃŚŹ'')', :runs) as plcsharp,
    plbench('SELECT retNonRegularEncodingPython(''ŁĄŻĘĆŃŚŹ'')', :runs) as plpython;
    --- plbench('SELECT retNonRegularEncodingJava(''ŁĄŻĘĆŃŚŹ'')', :runs) as pljava,

SELECT
    'retNonRegularEncoding(Unicode)',
    plbench('SELECT retNonRegularEncoding(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', :runs) as plcsharp,
    plbench('SELECT retNonRegularEncodingPython(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', :runs) as plpython;
    --- plbench('SELECT retNonRegularEncodingJava(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', :runs) as pljava,