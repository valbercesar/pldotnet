#!/bin/bash

FILENAME=$1

# Create header
echo "Test Case;pldotnet;plv8;plpython;plpgsql" >> $FILENAME

# Create the content
for file in sql/benchmark/bench-*.sql
do
    echo $file
    psql -f "$file" | grep "|" | grep -v "column" | sed "s/|/;/g" | sed s/'\s'//g  >> $FILENAME
done