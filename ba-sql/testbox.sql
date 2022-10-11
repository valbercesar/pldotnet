CREATE OR REPLACE FUNCTION testBox(my_box BOX) RETURNS BOX AS $$
return my_box;
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOX: testBox', testBox(BOX '(0.025988, 1.021653), (2.052787, 3.005716)') = BOX '(0.025988, 1.021653), (2.052787, 3.005716)';

CREATE OR REPLACE FUNCTION createBox(high POINT, low POINT) RETURNS BOX AS $$
return new NpgsqlBox(high, low);
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOX: createBox', createBox(POINT '(2.052787, 3.005716)', POINT '(0.025988, 1.021653)') = BOX '(2.052787, 3.005716), (0.025988, 1.021653)';

CREATE OR REPLACE FUNCTION returnWidth(high POINT, low POINT) RETURNS double precision AS $$
NpgsqlBox new_box = new NpgsqlBox(high, low);
return (double)Math.Abs(new_box.Width);
$$ LANGUAGE plcsharp;
INSERT INTO results (testName, result)
SELECT 'TEST BOX: returnWidth', returnWidth(POINT '(0.025988, 1.021653)', POINT '(2.052787, 3.005716)') = double precision '2.026799';
