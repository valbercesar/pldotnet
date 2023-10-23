# Makefile for PL/.NET

UNAME = $(shell uname)
SED ?= sed
DBUSER ?= postgres

# General
# Get installed dotnet host host
DOTNET_VER = $(shell dotnet --info | grep 'Host' -A 3 | $(SED) -n 's/Version: \(.*\)/\1/p' | xargs)

PLDOTNET_ENGINE_DIR = -DPLDOTNET_ENGINE_DIR=$(PLDOTNET_ENGINE_ROOT)/PlDotNET
PLDOTNET_TEMPLATE_DIR = $(PLDOTNET_ENGINE_ROOT)/PlDotNET/Templates

# Linux support
ifeq ($(UNAME), Linux)
	DOTNET_HOSTDIR ?= $(shell dpkg -L dotnet-apphost-pack-6.0 | grep hostfxr.h | head -1 | xargs dirname)
	DOTNET_LIBDIR  ?= $(shell dpkg -L dotnet-apphost-pack-6.0 | grep hostfxr.h | head -1 | xargs dirname)
	DOTNET_HOSTLIB ?= -L$(DOTNET_LIBDIR) -lnethost -Wl,-rpath $(DOTNET_LIBDIR)
	PLDOTNET_ENGINE_ROOT ?= /var/lib
	PG_CONFIG = pg_config
	PKG_LIBDIR = $(shell $(PG_CONFIG) --pkglibdir)
	PG_INCDIR = $(shell $(PG_CONFIG) --includedir-server )
endif

# OSX support
ifeq ($(UNAME), Darwin)
	SED=gsed
	DBUSER=$(USER)
	DOTNET_HOSTDIR ?= $(shell find /usr/local/share/dotnet -name hostfxr.h | head -1 | xargs dirname)
	DOTNET_LIBDIR  ?= $(shell find /usr/local/share/dotnet -name hostfxr.h | head -1 | xargs dirname)
	DOTNET_HOSTLIB ?= -L$(DOTNET_LIBDIR) -Wl,-rpath,$(DOTNET_LIBDIR) -lglib-2.0 -lnethost
	PLDOTNET_ENGINE_ROOT = $(HOME)/dev/pldotnet/tmp/dotnet_install
	PG_CONFIG = $(HOME)/postgresql-15/bin/pg_config
	PKG_LIBDIR = $(HOME)/postgresql-15/lib/
	PG_INCDIR = $(HOME)/postgresql-15/include/server
endif

GLIB_INC := `pkg-config --cflags --libs glib-2.0`

ifeq ("$(shell echo $(USE_DOTNETBUILD) | tr A-Z a-z)", "true")
	DEFINE_DOTNET_BUILD := -DUSE_DOTNETBUILD
else
	BUILD_PLDOTNET_PROJECT := dotnet build $(PLDOTNET_ENGINE_ROOT)/PlDotNET -c Release
endif

PG_VER = $(shell pg_config --version | grep -Po '(?<=SQL )[0-9]+')
PG_10_OR_12PLUS = $(shell if [ ${PG_VER}  -lt "12" ]; then echo '10';  else echo '12plus'; fi)

MODULE_big = pldotnet
EXTENSION = pldotnet
DATA = pldotnet--0.9.sql

OBJS = src/pldotnet_hostfxr.o src/pldotnet.o src/pldotnet_conversions.o src/pldotnet_main.o

PG_CPPFLAGS = -I$(DOTNET_HOSTDIR) -I$(PG_INCDIR) $(GLIB_INC) \
			  -Iinc -DLINUX $(DEFINE_DOTNET_BUILD) $(PLDOTNET_ENGINE_DIR) \
			  -DPKG_LIBDIR=$(PKG_LIBDIR)
PGXS = $(shell $(PG_CONFIG) --pgxs)

ifeq ($(UNAME), Darwin)
	PG_CPPFLAGS = -isystem $(DOTNET_HOSTDIR) -isystem $(PG_INCDIR) $(GLIB_INC) \
			  -Iinc -DLINUX $(DEFINE_DOTNET_BUILD) $(PLDOTNET_ENGINE_DIR) \
			  -DPKG_LIBDIR=$(PKG_LIBDIR)
	PGXS = $(HOME)/postgresql-15/lib/pgxs/src/makefiles/pgxs.mk
endif

SHLIB_LINK = $(DOTNET_HOSTLIB) $(GLIB_INC)

CURRENT_DIR = $(shell pwd)

include $(PGXS)

CP_CHOWN = cp -R dotnet_src $(PLDOTNET_ENGINE_ROOT)/PlDotNET
ifeq ($(UNAME), Linux)
	CP_CHOWN += && chown -R postgres $(PLDOTNET_ENGINE_ROOT)/PlDotNET
endif

pldotnet-install: pldotnet-uninstall install
	$(CP_CHOWN)
	$(SED) -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | $(SED) 's/\//\\\//g')/' $(PLDOTNET_ENGINE_ROOT)/PlDotNET/Engine.cs
	$(SED) -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | $(SED) 's/\//\\\//g')/' $(PLDOTNET_ENGINE_ROOT)/PlDotNET/TypeHandlers/*.cs
	$(SED) -i 's/@PKG_LIBDIR/$(shell echo $(PKG_LIBDIR) | $(SED) 's/\//\\\//g')/' $(PLDOTNET_ENGINE_ROOT)/PlDotNET/FSharp/FSharpCompiler.fs
	$(SED) -i 's/@PLDOTNET_TEMPLATE_DIR/$(shell echo $(PLDOTNET_TEMPLATE_DIR) | $(SED) 's/\//\\\//g')/' $(PLDOTNET_ENGINE_ROOT)/PlDotNET/CodeGenerator.cs
	$(BUILD_PLDOTNET_PROJECT)

pldotnet-uninstall: uninstall
	rm -rf $(PLDOTNET_ENGINE_ROOT)/PlDotNET

pldotnet-install-dpkg:
	$(MAKE) documentation
	rm -f debian/packages/postgresql-*-pldotnet_*.deb
	-sudo -u $(DBUSER) pg_createcluster $(PG_VER) default
	service postgresql start
	pg_buildext updatecontrol
	debuild -b -uc -us --lintian-opts --suppress-tags=initial-upload-closes-no-bugs,custom-library-search-path --profile debian
	mkdir -p debian/packages
	cp ../postgresql-*-pldotnet_*.deb debian/packages/
	rm -rf ../postgresql-*-pldotnet_*.deb

cpplint:
	cpplint --filter=-readability/casting,-build/include_subdir,-runtime/int,-runtime/printf,-build/header_guard src/*.c src/*.h

documentation:
	doxygen docs/Doxyfile

clean-docker:
	# These might fail if there are no containers and/or images
	# first you remove the containers
	-docker ps -a|grep -v CREATED|awk '{print $$1}'| xargs docker rm
	# second, you remove the images
	-docker images|grep -v CREATED|awk '{print $$3}'| xargs docker rmi
	rm -rf postgres-data

pldotnet-ubuntu:
	docker-compose run --rm pldotnet-ubuntu22 bash

pldotnet-postgres:
	make clean && make && make pldotnet-install
	sudo -u $(DBUSER) psql

build-package:
	docker-compose up pldotnet-build | tee package-build-log.txt

build-package-bash:
	make build-package
	docker-compose run --rm pldotnet-build bash

build-package-arm:
	docker-compose up pldotnet-build-arm | tee package-build-arm-log.txt

build-package-arm-bash:
	make build-package-arm
	docker-compose run --rm pldotnet-build-arm bash

pre-tests-script:
	dotnet build $(CURRENT_DIR)/tests/csharp/DotNetTestProject -c Release
	dotnet build $(CURRENT_DIR)/tests/fsharp/DotNetTestProject -c Release
	rm -rf automated_test_results
	mkdir -p automated_test_results
	echo 'DROP TABLE IF EXISTS automated_test_results;CREATE TABLE automated_test_results(FEATURE TEXT, TEST_NAME TEXT, RESULT boolean);' | (sudo -u $(DBUSER)  psql)

post-tests-script:
	cd $(CURRENT_DIR)/tests/csharp/DotNetTestProject/ && rm -rf bin obj
	cd $(CURRENT_DIR)/tests/fsharp/DotNetTestProject/ && rm -rf bin obj
	cd $(CURRENT_DIR)/
	echo 'SELECT FEATURE, TEST_NAME, RESULT from automated_test_results;' | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/automated_test_results.out
	echo 'SELECT RESULT, COUNT(1) FROM automated_test_results GROUP BY RESULT;' | (sudo -u $(DBUSER)  psql)

csharp-tests-cats:
	cat tests/csharp/testbit.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testbit.out
	cat tests/csharp/testbool.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testbool.out
	cat tests/csharp/testbytea.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testbytea.out
	cat tests/csharp/testdatetime.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testdatetime.out
	cat tests/csharp/testdll.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testdll.out
	cat tests/csharp/testfloats.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfloats.out
	cat tests/csharp/testgeometric.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testgeometric.out
	cat tests/csharp/testintegers.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testintegers.out
	cat tests/csharp/testjson.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testjson.out
	cat tests/csharp/testmoney.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testmoney.out
	cat tests/csharp/testnetwork.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testnetwork.out
	cat tests/csharp/testrange.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testrange.out
	cat tests/csharp/teststring.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/teststring.out
	cat tests/csharp/testuuid.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testuuid.out
	cat tests/csharp/testdo.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testdo.out
	cat tests/csharp/testprocedure.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testprocedure.out
	cat tests/csharp/testcreate.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testcreate.out
	cat tests/csharp/testcall.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testcall.out
	cat tests/csharp/testinout.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testinout.out

fsharp-tests-cats:
	cat tests/fsharp/testfsbit.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsbit.out
	cat tests/fsharp/testfsbool.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsbool.out
	cat tests/fsharp/testfsbytea.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsbytea.out
	cat tests/fsharp/testfsdate.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsdate.out
	cat tests/fsharp/testfsdatetime.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsdatetime.out
	cat tests/fsharp/testfsdo.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsdo.out
	cat tests/fsharp/testfsprocedure.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsprocedure.out
	cat tests/fsharp/testfsdll.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsdll.out
	cat tests/fsharp/testfsfloats.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsfloats.out
	cat tests/fsharp/testfsgeometric.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsgeometric.out
	cat tests/fsharp/testfsintegers.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsintegers.out
	cat tests/fsharp/testfsjson.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsjson.out
	cat tests/fsharp/testfsnetwork.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsnetwork.out
	cat tests/fsharp/testfsrange.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsrange.out
	cat tests/fsharp/testfsstring.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsstring.out
	cat tests/fsharp/testfsuuid.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsuuid.out
	cat tests/fsharp/testfscreate.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfscreate.out
	cat tests/fsharp/testfscall.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfscall.out
	cat tests/fsharp/testfsinout.sql | (sudo -u $(DBUSER)  psql 2>&1) | tee automated_test_results/testfsinout.out

pldotnet-tests:
	make pre-tests-script
	make fsharp-tests-cats
	make csharp-tests-cats
	make post-tests-script

csharp-tests:
	make pre-tests-script
	make csharp-tests-cats
	make post-tests-script

fsharp-tests:
	make pre-tests-script
	make fsharp-tests-cats
	make post-tests-script

stress-test:
	rm -rf automated_test_results
	mkdir -p automated_test_results
	echo 'DROP TABLE automated_test_results;CREATE TABLE automated_test_results(FEATURE TEXT, TEST_NAME TEXT, RESULT boolean);' | (sudo -u $(DBUSER) psql)
	sudo bash tests/stress_test/stress_test.sh

benchmark-tests:
	mkdir -p automated_test_results
	cat tests/benchmark/python/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/java/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/perl/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/lua/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/tcl/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/r/init-extension.sql | (sudo -u $(DBUSER) psql)
	cat tests/benchmark/v8javascript/init-extension.sql | (sudo -u $(DBUSER) psql)
	cd $(CURRENT_DIR)/tests/benchmark/java/test-suite && mvn install && cd $(CURRENT_DIR)/
	echo "select sqlj.install_jar('file:$(CURRENT_DIR)/tests/benchmark/java/test-suite/target/test-suite-1.0.0.jar', 'testsuite', true);" | (sudo -u $(DBUSER) psql)
	echo "select sqlj.set_classpath('public', 'testsuite');" | (sudo -u $(DBUSER) psql)
	bash tests/benchmark/benchmark.sh $(NRUNS)
