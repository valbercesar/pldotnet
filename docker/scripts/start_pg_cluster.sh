#!/bin/bash

PGVERSION=$(pg_config --version | grep -Po '(?<=SQL )[0-9]+')
PGDATA=${PGDATA:-"/var/lib/postgresql/data"}

# if cluster is not populated, copy from backup created in the building process
if [[ ! $(ls ${PGDATA}) ]]; then
    rsync -a /var/lib/postgresql/pgnet_backup_cluster/* ${PGDATA}
fi

# fixing permissions/ownership
chown postgres -R ${PGDATA}
chmod 700 -R ${PGDATA}

# starting cluster
/usr/bin/pg_ctlcluster \
${PGVERSION} pgnet \
start \
-- -l /var/log/postgresql/postgresql-main.log \
-D /etc/postgresql/${PGVERSION}/pgnet/ \
-s

# Setting postgres user password to POSTGRES_PASSWORD...
runuser -u postgres -- psql -c "ALTER USER ${POSTGRES_USER:-postgres} WITH PASSWORD '${POSTGRES_PASSWORD:-postgres}';"
echo "pldotnet.always_nullable = 'off'" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.print_source_code = 'off'" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.save_source_code = 'on'" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.compile_fsharp_with_fcs = 'off'" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.verbose_level = 0" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.path_to_save_source_code = '/tmp/PlDotNET/GeneratedCodes'" >> /etc/postgresql/14/pgnet/postgresql.conf;
echo "pldotnet.path_to_temporary_files = '/tmp/PlDotNET/'" >> /etc/postgresql/14/pgnet/postgresql.conf;
service postgresql restart
/usr/bin/pg_ctlcluster \
${PGVERSION} pgnet \
restart -- -l /var/log/postgresql/postgresql-main.log \
-D /etc/postgresql/${PGVERSION}/pgnet/ \
-s

exec "$@"