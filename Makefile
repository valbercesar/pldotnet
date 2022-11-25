# Makefile for PL/.NET

# General
# Get installed dotnet host host
DOTNET_VER = $(shell dotnet --info | grep 'Host' -A 3 | sed -n 's/Version: \(.*\)/\1/p' | xargs)
DOTNET_HOSTDIR ?= $(shell dpkg -L dotnet-apphost-pack-6.0 | grep hostfxr.h | head -1 | xargs dirname)
DOTNET_LIBDIR  ?= $(shell dpkg -L dotnet-apphost-pack-6.0 | grep hostfxr.h | head -1 | xargs dirname)
DOTNET_INCHOSTDIR ?= $(DOTNET_HOSTDIR) $(shell env > /tmp/pgdotnet-make-env)
DOTNET_HOSTLIB ?= -L$(DOTNET_LIBDIR) -lnethost
GLIB_INC := `pkg-config --cflags --libs glib-2.0`
PLNET_ENGINE_ROOT ?= /var/lib
PLNET_ENGINE_DIR := -D PLNET_ENGINE_DIR=$(PLNET_ENGINE_ROOT)/DotNetEngine
CSHARP_TEMPLATE_DIR =$(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp/templates

ifeq ("$(shell echo $(USE_DOTNETBUILD) | tr A-Z a-z)", "true")
	DEFINE_DOTNET_BUILD := -D USE_DOTNETBUILD
else
	GENERATE_CSHARP_BUILD_FILES := dotnet build $(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp -c Release
	GENERATE_FSHARP_BUILD_FILES := dotnet build $(PLNET_ENGINE_ROOT)/DotNetEngine/src/fsharp -c Release
endif

PG_CONFIG ?= pg_config
PKG_LIBDIR := $(shell $(PG_CONFIG) --pkglibdir)
PG_VER = $(shell pg_config --version | grep -Po '(?<=SQL )[0-9]+')
PG_10_OR_12PLUS = $(shell if [ ${PG_VER}  -lt "12" ]; then echo '10';  else echo '12plus'; fi)

MODULE_big = pldotnet
EXTENSION = pldotnet
DATA = pldotnet--0.0.1.sql

 REGRESS = \
	init-extension \
 	testfunc \
	testfsfunc \
	testintegers testnullintegers \
	testbool testnullbool \
	testnumeric \
	testfloats \
	testchar \
	testmixedtypes \
	testrecursive \
	testfsintegers testfsnullintegers \
	testfsbool testfsnullbool \
	testfsrecursive \
	testfschar \
	testfsfloats \
	testfsnumeric \
	testfsmixedtypes \
	testspi \
	testfsspi \
	testarray \
	testfsarray \
	testcomposites \
	testfscomposites \
	testvalidation_pg$(PG_10_OR_12PLUS) \
	testfsvalidation \
	testtrigger \
	testfstrigger \
	testelog \
	testfselog

OBJS = pldotnet_csharp.o pldotnet_hostfxr.o pldotnet.o pldotnet_common.o pldotnet_conversions.o

PG_CPPFLAGS = -I$(DOTNET_INCHOSTDIR) \
			  -Iinc -D LINUX $(DEFINE_DOTNET_BUILD) $(PLNET_ENGINE_DIR) \
			  $(GLIB_INC) -D PKG_LIBDIR=$(PKG_LIBDIR)

SHLIB_LINK = $(DOTNET_HOSTLIB) $(GLIB_INC)

PGXS := $(shell $(PG_CONFIG) --pgxs)

include $(PGXS)

plnet-install: install
	echo $(DOTNET_LIBDIR) > /etc/ld.so.conf.d/nethost_pldotnet.conf && ldconfig
	rm -rf $(PLNET_ENGINE_ROOT)/DotNetEngine
	cp -r DotNetEngine $(PLNET_ENGINE_ROOT) && chown -R postgres $(PLNET_ENGINE_ROOT)/DotNetEngine
	sed -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | sed 's/\//\\\//g')/' $(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp/Engine.cs
	sed -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | sed 's/\//\\\//g')/' $(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp/TypeHandlers/*.cs
	sed -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | sed 's/\//\\\//g')/' $(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp/templates/*.tcs
	sed -i 's/@CSHARP_TEMPLATE_DIR/$(shell echo $(CSHARP_TEMPLATE_DIR) | sed 's/\//\\\//g')/' $(PLNET_ENGINE_ROOT)/DotNetEngine/src/csharp/Engine.cs
	$(GENERATE_CSHARP_BUILD_FILES)

# plnet-uninstall: uninstall
# 	rm -rf $(PLNET_ENGINE_ROOT)/DotNetEngine

plnet-install-dpkg:
	rm -f debian/packages/postgresql-*-pldotnet_*.deb
	-sudo -u postgres pg_createcluster $(PG_VER) default
	service postgresql start
	pg_buildext updatecontrol
	debuild -b -uc -us --lintian-opts --profile debian
	mkdir -p debian/packages
	cp ../postgresql-*-pldotnet_*.deb debian/packages/
	rm -rf ../postgresql-*-pldotnet_*.deb

cpplint:
	cpplint --filter=-readability/casting,-build/include_subdir,-runtime/int,-runtime/printf *.c *.h

doxygen:
	rm -rf documentation
	doxygen Doxyfile

clean-docker:
	# These might fail if there are no containers and/or images
	# first you remove the containers
	-docker ps -a|grep -v CREATED|awk '{print $$1}'| xargs docker rm
	# second, you remove the images
	-docker images|grep -v CREATED|awk '{print $$3}'| xargs docker rmi
	rm -rf postgres-data

pldotnet-ubuntu:
	docker-compose run --rm pldotnet-ubuntu22 bash

plnet-postgres:
	make clean && make && make plnet-install
	sudo -u postgres psql

build-package:
	docker-compose -f docker-compose-build.yml up pldotnet-build | tee build-log.txt

build-package-bash:
	make build-package
	docker-compose -f docker-compose-build.yml run --rm pldotnet-build bash

tests:
	rm -rf results
	mkdir results
	echo 'DROP TABLE results;CREATE TABLE results(FEATURE TEXT, TEST_NAME TEXT, RESULT boolean);' | (sudo -u postgres  psql)
	cat ba-sql/testbit.sql | (sudo -u postgres  psql 2>&1) | tee results/testbit.out
	cat ba-sql/testbool.sql | (sudo -u postgres  psql 2>&1) | tee results/testbool.out
	cat ba-sql/testbytea.sql | (sudo -u postgres  psql 2>&1) | tee results/testbytea.out
	cat ba-sql/testdatetime.sql | (sudo -u postgres  psql 2>&1) | tee results/testdatetime.out
	cat ba-sql/testfloats.sql | (sudo -u postgres  psql 2>&1) | tee results/testfloats.out
	cat ba-sql/testgeometric.sql | (sudo -u postgres  psql 2>&1) | tee results/testgeometric.out
	cat ba-sql/testintegers.sql | (sudo -u postgres  psql 2>&1) | tee results/testintegers.out
	cat ba-sql/testjson.sql | (sudo -u postgres  psql 2>&1) | tee results/testjson.out
	cat ba-sql/testmoney.sql | (sudo -u postgres  psql 2>&1) | tee results/testmoney.out
	cat ba-sql/testnetwork.sql | (sudo -u postgres  psql 2>&1) | tee results/testnetwork.out
	cat ba-sql/testrange.sql | (sudo -u postgres  psql 2>&1) | tee results/testrange.out
	cat ba-sql/teststring.sql | (sudo -u postgres  psql 2>&1) | tee results/teststring.out
	cat ba-sql/testuuid.sql | (sudo -u postgres  psql 2>&1) | tee results/testuuid.out
	cat ba-sql/testdo.sql | (sudo -u postgres  psql 2>&1) | tee results/testdo.out
	cat ba-sql/testprocedure.sql | (sudo -u postgres  psql 2>&1) | tee results/testprocedure.out
	cat ba-sql/testcreate.sql | (sudo -u postgres  psql 2>&1) | tee results/testcreate.out
	cat ba-sql/testcall.sql | (sudo -u postgres  psql 2>&1) | tee results/testcall.out
	echo 'SELECT FEATURE, TEST_NAME, RESULT from results;' | (sudo -u postgres  psql 2>&1) | tee results/results.out

stress-test:
	rm -rf results
	mkdir results
	echo 'DROP TABLE results;CREATE TABLE results(FEATURE TEXT, TEST_NAME TEXT, RESULT boolean);' | (sudo -u postgres psql)
	sudo bash stress_test.sh
