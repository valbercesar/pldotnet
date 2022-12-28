#!/bin/bash

FILENAME=$1

# Create header
echo "Test Case;plcsharp;plv8;plpython;plpgsql;pljava;plperl;pllua;pltcl;plr" >> $FILENAME

# Create the content
for file in tests/benchmark/bench-*.sql
do
    echo $file
    psql -f "$file" | grep "|" | grep -v "column" | sed "s/|/;/g" | sed s/'\s'//g  >> $FILENAME
done