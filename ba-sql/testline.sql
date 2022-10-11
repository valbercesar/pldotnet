CREATE OR REPLACE FUNCTION createLine(a double precision, b double precision, c double precision) RETURNS line AS $$
NpgsqlLine my_line = new NpgsqlLine(a,b,c);
return my_line;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST LINE: createLine', createLine(1.50,-2.750,3.25) = LINE '{1.50,-2.750,3.25}';

CREATE OR REPLACE FUNCTION modifyCoefficients(original_line LINE) RETURNS LINE AS $$
double a = original_line.A * -1.0;
double b = original_line.B * -1.0;
double c = original_line.C * -1.0;
NpgsqlLine my_line = new NpgsqlLine(a,b,c);
return my_line;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST LINE: modifyCoefficients', modifyCoefficients(LINE '{-1.5,2.75,-3.25}') = LINE '{1.50,-2.75,3.25}';

CREATE OR REPLACE FUNCTION getMinimumDistance(orig_line LINE, orig_point POINT) RETURNS double precision AS $$
double a = orig_line.A;
double b = orig_line.B;
double c = orig_line.C;
return Math.Abs((a*orig_point.X + b*orig_point.Y + c)/Math.Sqrt(a*a+b*b));
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST LINE: getMinimumDistance', getMinimumDistance(LINE '{4.0, 6.0, 2.0}', POINT(3.0,-6.0)) = double precision '3.05085107923876';

CREATE OR REPLACE FUNCTION createLineSegment(start_point POINT, end_point POINT) RETURNS LSEG AS $$
NpgsqlLSeg newLine = new NpgsqlLSeg(start_point, end_point);
return newLine;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST LSEG: createLineSegment', createLineSegment(POINT(0.088997,1.258456),POINT(5.456102,3.04561)) = LSEG '[(0.088997,1.258456),(5.456102,3.04561)]';

CREATE OR REPLACE FUNCTION getReverseLineSegment(my_line LSEG) RETURNS LSEG AS $$
NpgsqlPoint firstPoint = my_line.Start;
NpgsqlPoint secondPoint = my_line.End;
NpgsqlLSeg newLine = new NpgsqlLSeg(secondPoint, firstPoint);
return newLine;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST LSEG: getReverseLineSegment', getReverseLineSegment(LSEG(POINT(0.0,1.0),POINT(5.0,3.0))) = LSEG '[(5.0,3.0),(0.0,1.0)]';
