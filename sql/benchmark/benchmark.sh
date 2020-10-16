#!/bin/bash

TIMESTAMP=`date +%Y-%m-%d_%H-%M-%S`
FILENAME=logs/$TIMESTAMP.csv

cd ../../
touch $FILENAME
sudo chmod 777 $FILENAME

sudo su postgres -c "sql/benchmark/execute-bench.sh $FILENAME"