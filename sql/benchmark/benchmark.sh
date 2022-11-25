#!/bin/bash

TIMESTAMP=`date +%Y-%m-%d_%H-%M-%S`
FILENAME=results/$TIMESTAMP.csv

cd ../../
touch $FILENAME
sudo chmod 777 $FILENAME

make install-pls
sudo su postgres -c "sql/benchmark/execute-bench.sh $FILENAME"