#!/bin/bash

TIMESTAMP=`date +%Y-%m-%d_%H-%M-%S`
FILENAME=automated_test_results/$TIMESTAMP.csv

touch $FILENAME
sudo chmod 777 $FILENAME

sudo su postgres -c "tests/benchmark/execute-bench.sh $FILENAME"