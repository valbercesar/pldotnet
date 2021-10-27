#!/bin/bash
(runuser -u postgres -- initdb) || true
(runuser -u postgres -- pg_ctl start) || true
service postgresql start

exec "$@"