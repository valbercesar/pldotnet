\i sql/benchmark/includes.sql

\i sql/testchar.sql
\i sql/v8javascript/testchar.sql
\i sql/python/testchar.sql
\i sql/pgsql/testchar.sql
\i sql/java/testchar.sql
\i sql/perl/testchar.sql
\i sql/tcl/testchar.sql
\i sql/r/testchar.sql
\i sql/testfschar.sql

SELECT
    'retVarChar',
    plbench('SELECT retVarChar(''Rodrigo'')', 300) as pldotnet,
    plbench('SELECT retVarCharV8(''Rodrigo'')', 300) as plv8,
    plbench('SELECT retVarCharPython(''Rodrigo'')', 300) as plpython,
    plbench('SELECT retVarCharPg(''Rodrigo'')', 300) as plpgsql,
    plbench('SELECT retVarCharJava(''Rodrigo'')', 300) as pljava,
    plbench('SELECT retVarCharPerl(''Rodrigo'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retVarCharTcl(''Rodrigo'')', 300) as pltcl,
    plbench('SELECT retVarCharR(''Rodrigo'')', 300) as plr,
    plbench('SELECT retVarCharFSharp(''Rodrigo'')', 300) as plfsharp;

SELECT
    'retConcatVarChar',
    plbench('SELECT retConcatVarChar(''João '', ''da Silva'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharV8(''João '', ''da Silva'')', 300) as plv8,
    plbench('SELECT retConcatVarCharPython(''João '', ''da Silva'')', 300) as plpython,
    plbench('SELECT retConcatVarCharPg(''João '', ''da Silva'')', 300) as plpgsql,
    plbench('SELECT retConcatVarCharJava(''João '', ''da Silva'')', 300) as pljava,
    plbench('SELECT retConcatVarCharPerl(''João '', ''da Silva'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatVarCharTcl(''João '', ''da Silva'')', 300) as pltcl,
    plbench('SELECT retConcatVarCharR(''João '', ''da Silva'')', 300) as plr,
    plbench('SELECT retConcatVarCharFSharp(''João '', ''da Silva'')', 300) as plfsharp;

SELECT
    'retConcatText',
    plbench('SELECT retConcatText(''João '', ''da Silva'')', 300) as pldotnet,
    plbench('SELECT retConcatTextV8(''João '', ''da Silva'')', 300) as plv8,
    plbench('SELECT retConcatTextPython(''João '', ''da Silva'')', 300) as plpython,
    plbench('SELECT retConcatTextPg(''João '', ''da Silva'')', 300) as plpgsql,
    plbench('SELECT retConcatTextJava(''João '', ''da Silva'')', 300) as pljava,
    plbench('SELECT retConcatTextPerl(''João '', ''da Silva'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatTextTcl(''João '', ''da Silva'')', 300) as pltcl,
    plbench('SELECT retConcatTextR(''João '', ''da Silva'')', 300) as plr,
    plbench('SELECT retConcatTextFSharp(''João '', ''da Silva'')', 300) as plfsharp;

SELECT
    'retVarCharText',
    plbench('SELECT retVarCharText(''Homer Jay '', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retVarCharTextV8(''Homer Jay '', ''Simpson'')', 300) as plv8,
    plbench('SELECT retVarCharTextPython(''Homer Jay '', ''Simpson'')', 300) as plpython,
    plbench('SELECT retVarCharTextPg(''Homer Jay '', ''Simpson'')', 300) as plpgsql,
    plbench('SELECT retVarCharTextJava(''Homer Jay '', ''Simpson'')', 300) as pljava,
    plbench('SELECT retVarCharTextPerl(''Homer Jay '', ''Simpson'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retVarCharTextTcl(''Homer Jay '', ''Simpson'')', 300) as pltcl,
    plbench('SELECT retVarCharTextR(''Homer Jay '', ''Simpson'')', 300) as plr,
    plbench('SELECT retVarCharTextFSharp(''Homer Jay '', ''Simpson'')', 300) as plfsharp;

SELECT
    'retChar',
    plbench('SELECT retChar(''R'')', 300) as pldotnet,
    plbench('SELECT retCharV8(''R'')', 300) as plv8,
    plbench('SELECT retCharPython(''R'')', 300) as plpython,
    plbench('SELECT retCharPg(''R'')', 300) as plpgsql,
    plbench('SELECT retCharJava(''R'')', 300) as pljava,
    plbench('SELECT retCharPerl(''R'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retCharTcl(''R'')', 300) as pltcl,
    plbench('SELECT retCharR(''R'')', 300) as plr,
    plbench('SELECT retCharFSharp(''R'')', 300) as plfsharp;

SELECT
    'retConcatLetters',
    plbench('SELECT retConcatLetters(''R'', ''C'')', 300) as pldotnet,
    plbench('SELECT retConcatLettersV8(''R'', ''C'')', 300) as plv8,
    plbench('SELECT retConcatLettersPython(''R'', ''C'')', 300) as plpython,
    plbench('SELECT retConcatLettersPg(''R'', ''C'')', 300) as plpgsql,
    plbench('SELECT retConcatLettersJava(''R'', ''C'')', 300) as pljava,
    plbench('SELECT retConcatLettersPerl(''R'', ''C'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatLettersTcl(''R'', ''C'')', 300) as pltcl,
    plbench('SELECT retConcatLettersR(''R'', ''C'')', 300) as plr,
    plbench('SELECT retConcatLettersFSharp(''R'', ''C'')', 300) as plfsharp;

SELECT
    'retConcatChars',
    plbench('SELECT retConcatChars(''H.'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatCharsV8(''H.'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatCharsPython(''H.'', ''Simpson'')', 300) as plpython,
    plbench('SELECT retConcatCharsPg(''H.'', ''Simpson'')', 300) as plpgsql,
    plbench('SELECT retConcatCharsJava(''H.'', ''Simpson'')', 300) as pljava,
    plbench('SELECT retConcatCharsPerl(''H.'', ''Simpson'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatCharsTcl(''H.'', ''Simpson'')', 300) as pltcl,
    plbench('SELECT retConcatCharsR(''H.'', ''Simpson'')', 300) as plr,
    plbench('SELECT retConcatCharsFSharp(''H.'', ''Simpson'')', 300) as plfsharp;

SELECT
    'retConcatVarChars',
    plbench('SELECT retConcatVarChars(''H.'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharsV8(''H.'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatVarCharsPython(''H.'', ''Simpson'')', 300) as plpython,
    plbench('SELECT retConcatVarCharsPg(''H.'', ''Simpson'')', 300) as plpgsql,
    plbench('SELECT retConcatVarCharsJava(''H.'', ''Simpson'')', 300) as pljava,
    plbench('SELECT retConcatVarCharsPerl(''H.'', ''Simpson'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatVarCharsTcl(''H.'', ''Simpson'')', 300) as pltcl,
    plbench('SELECT retConcatVarCharsR(''H.'', ''Simpson'')', 300) as plr,
    plbench('SELECT retConcatVarCharsFSharp(''H.'', ''Simpson'')', 300) as plfsharp;

SELECT
    'retConcatVarChars(H. あ)',
    plbench('SELECT retConcatVarChars(''H. あ'', ''Simpson'')', 300) as pldotnet,
    plbench('SELECT retConcatVarCharsV8(''H. あ'', ''Simpson'')', 300) as plv8,
    plbench('SELECT retConcatVarCharsPython(''H. あ'', ''Simpson'')', 300) as plpython,
    plbench('SELECT retConcatVarCharsPg(''H. あ'', ''Simpson'')', 300) as plpgsql,
    plbench('SELECT retConcatVarCharsJava(''H. あ'', ''Simpson'')', 300) as pljava,
    plbench('SELECT retConcatVarCharsPerl(''H. あ'', ''Simpson'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retConcatVarCharsTcl(''H. あ'', ''Simpson'')', 300) as pltcl,
    plbench('SELECT retConcatVarCharsR(''H. あ'', ''Simpson'')', 300) as plr,
    plbench('SELECT retConcatVarCharsFSharp(''H. あ'', ''Simpson'')', 300) as plfsharp;

SELECT
    'retNonRegularEncoding(漢字)',
    plbench('SELECT retNonRegularEncoding(''漢字'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''漢字'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''漢字'')', 300) as plpython,
    plbench('SELECT retNonRegularEncodingPg(''漢字'')', 300) as plpgsql,
    plbench('SELECT retNonRegularEncodingJava(''漢字'')', 300) as pljava,
    plbench('SELECT retNonRegularEncodingPerl(''漢字'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retNonRegularEncodingTcl(''漢字'')', 300) as pltcl,
    plbench('SELECT retNonRegularEncodingR(''漢字'')', 300) as plr,
    plbench('SELECT retNonRegularEncodingFsharp (''漢字'')', 300) as plfsharp;

SELECT
    'retNonRegularEncoding',
    plbench('SELECT retNonRegularEncoding(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plpython,
    plbench('SELECT retNonRegularEncodingPg(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plpgsql,
    plbench('SELECT retNonRegularEncodingJava(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as pljava,
    plbench('SELECT retNonRegularEncodingPerl(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retNonRegularEncodingTcl(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as pltcl,
    plbench('SELECT retNonRegularEncodingR(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plr,
    plbench('SELECT retNonRegularEncodingFSharp(''ｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿﾀﾁﾂﾃ'')', 300) as plfsharp;

SELECT
    'retNonRegularEncoding(ŁĄŻĘĆŃŚŹ)',
    plbench('SELECT retNonRegularEncoding(''ŁĄŻĘĆŃŚŹ'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''ŁĄŻĘĆŃŚŹ'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''ŁĄŻĘĆŃŚŹ'')', 300) as plpython,
    plbench('SELECT retNonRegularEncodingPg(''ŁĄŻĘĆŃŚŹ'')', 300) as plpgsql,
    plbench('SELECT retNonRegularEncodingJava(''ŁĄŻĘĆŃŚŹ'')', 300) as pljava,
    plbench('SELECT retNonRegularEncodingPerl(''ŁĄŻĘĆŃŚŹ'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retNonRegularEncodingTcl(''ŁĄŻĘĆŃŚŹ'')', 300) as pltcl,
    plbench('SELECT retNonRegularEncodingR(''ŁĄŻĘĆŃŚŹ'')', 300) as plr,
    plbench('SELECT retNonRegularEncodingFSharp(''ŁĄŻĘĆŃŚŹ'')', 300) as plfsharp;

SELECT
    'retNonRegularEncoding(Unicode)',
    plbench('SELECT retNonRegularEncoding(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as pldotnet,
    plbench('SELECT retNonRegularEncodingV8(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plv8,
    plbench('SELECT retNonRegularEncodingPython(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plpython,
    plbench('SELECT retNonRegularEncodingPg(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plpgsql,
    plbench('SELECT retNonRegularEncodingJava(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as pljava,
    plbench('SELECT retNonRegularEncodingPerl(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plperl,
    '-' as pllua,
    plbench('SELECT retNonRegularEncodingTcl(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as pltcl,
    plbench('SELECT retNonRegularEncodingR(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plr,
    plbench('SELECT retNonRegularEncodingFSharp(''Unicode, которая состоится 10-12 марта 1997 года в Майнце в Германии.'')', 300) as plfsharp;
