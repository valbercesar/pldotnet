\i sql/benchmark/includes.sql

\i sql/testchar.sql
\i sql/v8javascript/testchar.sql
\i sql/python/testchar.sql

SELECT
    'retVarChar',
    plbench('SELECT retVarChar(''Rodrigo'')', 300) as pldotnet,
    plbench('SELECT retVarCharV8(''Rodrigo'')', 300) as plv8,
    plbench('SELECT retVarCharPython(''Rodrigo'')', 300) as plpython;

SELECT
    'retConcatVarChar',
    plbench('SELECT retConcatVarChar(''João '', ''da Silva'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharV8(''João '', ''da Silva'')', 300) as plv8,
    plbench('SELECT retConcatVarCharPython(''João '', ''da Silva'')', 300) as plpython;

SELECT
    'retConcatText',
    plbench('SELECT retConcatText(''João '', ''da Silva'')', 300) as pldotnet,
    plbench('SELECT retConcatTextV8(''João '', ''da Silva'')', 300) as plv8,
    plbench('SELECT retConcatTextPython(''João '', ''da Silva'')', 300) as plpython;

SELECT
    'retVarCharText',
    plbench('SELECT retVarCharText(''Homer Jay '', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retVarCharTextV8(''Homer Jay '', ''Simpson'')', 300) as plv8,
    plbench('SELECT retVarCharTextPython(''Homer Jay '', ''Simpson'')', 300) as plpython;

SELECT
    'retChar',
    plbench('SELECT retChar(''R'')', 300) as pldotnet,
    plbench('SELECT retCharV8(''R'')', 300) as plv8,
    plbench('SELECT retCharPython(''R'')', 300) as plpython;

SELECT
    'retConcatLetters',
    plbench('SELECT retConcatLetters(''R'', ''C'')', 300) as pldotnet,
    plbench('SELECT retConcatLettersV8(''R'', ''C'')', 300) as plv8,
    plbench('SELECT retConcatLettersPython(''R'', ''C'')', 300) as plpython;

SELECT
    'retConcatChars',
    plbench('SELECT retConcatChars(''H.'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatCharsV8(''H.'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatCharsPython(''H.'', ''Simpson'')', 300) as plpython;

SELECT
    'retConcatVarChars',
    plbench('SELECT retConcatVarChars(''H.'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharsV8(''H.'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatVarCharsPython(''H.'', ''Simpson'')', 300) as plpython;


SELECT
    'retConcatVarChars(H. あ)',
    plbench('SELECT retConcatVarChars(''H. あ'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharsV8(''H. あ'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatVarCharsPython(''H. あ'', ''Simpson'')', 300) as plpython;

SELECT
    'retNonRegularEncoding(漢字)',
    plbench('SELECT retNonRegularEncoding(''漢字'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''漢字'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''漢字'')', 300) as plpython;

SELECT
    'retNonRegularEncoding',
    plbench('SELECT retNonRegularEncoding(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plpython;

SELECT
    'retNonRegularEncoding(ŁĄŻĘĆŃŚŹ)',
    plbench('SELECT retNonRegularEncoding(''ŁĄŻĘĆŃŚŹ'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''ŁĄŻĘĆŃŚŹ'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''ŁĄŻĘĆŃŚŹ'')', 300) as plpython;

SELECT
    'retNonRegularEncoding(Unicode)',
    plbench('SELECT retNonRegularEncoding(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plpython;
