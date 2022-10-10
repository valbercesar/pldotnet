CREATE OR REPLACE FUNCTION distance(pointa point, pointb point) RETURNS double precision AS $$
double dif_x = (pointa.X - pointb.X);
double dif_y = (pointa.Y - pointb.Y);
double distance = Math.Sqrt(dif_x*dif_x+dif_y*dif_y);
return distance;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST POINT: distance',  distance(POINT(1.5,2.75), POINT(3.0,4.75)) = double precision '2.5';

CREATE OR REPLACE FUNCTION checkPoints(pointa point, pointb point) RETURNS boolean AS $$
if(pointa.X == pointb.X && pointa.Y == pointb.Y)
{
    return true;
}
return false;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST POINT: checkPoints1',  checkPoints(POINT(2.555701574,8.7552345789),POINT(2.555701574,8.7552345789)) is true;
INSERT INTO results (testName, result)
SELECT 'TEST POINT: checkPoints2',  checkPoints(POINT(2.555701574,8.7552345789),POINT(2.555701574,8.7552345785)) is false;

CREATE OR REPLACE FUNCTION middlePoint(pointa point, pointb point) RETURNS point AS $$
double x = (pointa.X + pointb.X)*0.5;
double y = (pointa.Y + pointb.Y)*0.5;
var new_point = new NpgsqlPoint(x,y);
return new_point;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST POINT: middlePoint',  middlePoint(POINT(10.0,20.0),POINT(20.0,40.0)) ~= POINT(15.0,30.0);

