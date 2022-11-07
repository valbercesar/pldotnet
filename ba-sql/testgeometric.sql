--------- POINT 
CREATE OR REPLACE FUNCTION middlePoint(pointa point, pointb point) RETURNS point AS $$
double x = (pointa.X + pointb.X)*0.5;
double y = (pointa.Y + pointb.Y)*0.5;
var new_point = new NpgsqlPoint(x,y);
return new_point;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'POINT', 'middlePoint',  middlePoint(POINT(10.0,20.0),POINT(20.0,40.0)) ~= POINT(15.0,30.0);

CREATE OR REPLACE FUNCTION distanceBetweenPoints(pointa point, pointb point) RETURNS double precision AS $$
double dif_x = (pointa.X - pointb.X);
double dif_y = (pointa.Y - pointb.Y);
double distance = Math.Sqrt(dif_x*dif_x+dif_y*dif_y);
return distance;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'POINT', 'distanceBetweenPoints',  distanceBetweenPoints(POINT(1.5,2.75), POINT(3.0,4.75)) = double precision '2.5';

CREATE OR REPLACE FUNCTION checkPoints(pointa point, pointb point) RETURNS boolean AS $$
if(pointa.X == pointb.X && pointa.Y == pointb.Y)
{
    return true;
}
return false;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'POINT', 'checkPoints1',  checkPoints(POINT(2.555701574,8.7552345789),POINT(2.555701574,8.7552345789)) is true;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'POINT', 'checkPoints2',  checkPoints(POINT(2.555701574,8.7552345789),POINT(2.555701574,8.7552345785)) is false;

--------- LINE

CREATE OR REPLACE FUNCTION createLine(a double precision, b double precision, c double precision) RETURNS LINE AS $$
NpgsqlLine my_line = new NpgsqlLine(a,b,c);
return my_line;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'LINE', 'createLine', createLine(1.50,-2.750,3.25) = LINE '{1.50,-2.750,3.25}';

CREATE OR REPLACE FUNCTION modifyCoefficients(original_line LINE) RETURNS LINE AS $$
double a = original_line.A * -1.0;
double b = original_line.B * -1.0;
double c = original_line.C * -1.0;
NpgsqlLine my_line = new NpgsqlLine(a,b,c);
return my_line;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'LINE', 'modifyCoefficients', modifyCoefficients(LINE '{-1.5,2.75,-3.25}') = LINE '{1.50,-2.75,3.25}';

CREATE OR REPLACE FUNCTION getMinimumDistance(orig_line LINE, orig_point POINT) RETURNS double precision AS $$
double a = orig_line.A;
double b = orig_line.B;
double c = orig_line.C;
return Math.Abs((a*orig_point.X + b*orig_point.Y + c)/Math.Sqrt(a*a+b*b));
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'LINE', 'getMinimumDistance', getMinimumDistance(LINE '{4.0, 6.0, 2.0}', POINT(3.0,-6.0)) = double precision '3.05085107923876';

--------- LSEG

CREATE OR REPLACE FUNCTION createLineSegment(start_point POINT, end_point POINT) RETURNS LSEG AS $$
NpgsqlLSeg newLine = new NpgsqlLSeg(start_point, end_point);
return newLine;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'LSEG', 'createLineSegment', createLineSegment(POINT(0.088997,1.258456),POINT(5.456102,3.04561)) = LSEG '[(0.088997,1.258456),(5.456102,3.04561)]';

CREATE OR REPLACE FUNCTION getReverseLineSegment(my_line LSEG) RETURNS LSEG AS $$
NpgsqlPoint firstPoint = my_line.Start;
NpgsqlPoint secondPoint = my_line.End;
NpgsqlLSeg newLine = new NpgsqlLSeg(secondPoint, firstPoint);
return newLine;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'LSEG', 'getReverseLineSegment', getReverseLineSegment(LSEG(POINT(0.0,1.0),POINT(5.0,3.0))) = LSEG '[(5.0,3.0),(0.0,1.0)]';

--------- BOX

CREATE OR REPLACE FUNCTION testBox(my_box BOX) RETURNS BOX AS $$
return my_box;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOX', 'testBox', testBox(BOX '(0.025988, 1.021653), (2.052787, 3.005716)') = BOX '(0.025988, 1.021653), (2.052787, 3.005716)';

CREATE OR REPLACE FUNCTION createBox(high POINT, low POINT) RETURNS BOX AS $$
return new NpgsqlBox(high, low);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOX', 'createBox', createBox(POINT '(2.052787, 3.005716)', POINT '(0.025988, 1.021653)') = BOX '(2.052787, 3.005716), (0.025988, 1.021653)';

CREATE OR REPLACE FUNCTION returnWidth(high POINT, low POINT) RETURNS double precision AS $$
NpgsqlBox new_box = new NpgsqlBox(high, low);
return (double)Math.Abs(new_box.Width);
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'BOX', 'returnWidth', returnWidth(POINT '(0.025988, 1.021653)', POINT '(2.052787, 3.005716)') = double precision '2.026799';

--------- PATH

CREATE OR REPLACE FUNCTION returnPath(orig_path PATH) RETURNS PATH AS $$
return orig_path;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'PATH', 'returnPath - open', returnPath(PATH '[(1.5,2.75),(3.0,4.75),(5.0,5.0)]') <= PATH '[(1.5,2.75),(3.0,4.75),(5.0,5.0)]';
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'PATH', 'returnPath - close', returnPath(PATH '((1.5,2.75),(3.0,4.75),(5.0,5.0))') <= PATH '((1.5,2.75),(3.0,4.75),(5.0,5.0))';

--------- POLYGON

CREATE OR REPLACE FUNCTION addPointToPolygon(orig_polygon POLYGON, new_point POINT) RETURNS POLYGON AS $$
int npts = orig_polygon.Count;
NpgsqlPolygon new_polygon = new NpgsqlPolygon(npts+1);
for(int i = 0; i < npts; i++)
{
    new_polygon.Add(orig_polygon[i]);
}
new_polygon.Add(new_point);
return new_polygon;
$$ LANGUAGE plcsharp;
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'POLYGON', 'addPointToPolygon', addPointToPolygon(POLYGON '((1.5,2.75),(3.0,4.75),(5.0,5.0))', POINT '(6.5,8.8)') ~= POLYGON '((1.5,2.75),(3.0,4.75),(5.0,5.0),(6.5,8.8))';

--------- CIRCLE

CREATE OR REPLACE FUNCTION returnCircle(orig_circle CIRCLE) RETURNS CIRCLE AS $$
return orig_circle;
$$ LANGUAGE plcsharp; 
INSERT INTO results (FEATURE, TEST_NAME, RESULT)
SELECT 'CIRCLE', 'returnCircle', returnCircle(CIRCLE '2.5, 3.5, 12.78') ~= CIRCLE '<(2.5, 3.5), 12.78>';
