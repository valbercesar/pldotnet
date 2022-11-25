CREATE OR REPLACE FUNCTION moving_avg(query text, n int) returns float as $$
DECLARE
    t0 timestamp with time zone;
    t1 timestamp with time zone;
    average float;
    e float;
BEGIN
    t0 := clock_timestamp();
    average := 0;
    for i in 1 .. n loop
        execute query;
        t1 := clock_timestamp();
        e := extract(microseconds from (t1 - t0));
        t0 := t1;
        average := (average * (i - 1) + e) / i;
    end loop;
    return average / 1000000;
END;
$$ language plpgsql;
