#!/bin/bash
(runuser -u postgres -- initdb) || true
(runuser -u postgres -- pg_ctl start) || true
service postgresql start

runuser -u postgres -- psql -c "DO \$\$ BEGIN CREATE ROLE root superuser createdb login createrole replication bypassrls; EXCEPTION WHEN duplicate_object THEN RAISE NOTICE '%, skipping', SQLERRM USING ERRCODE = SQLSTATE; END \$\$;"

make clean
make
make plnet-install