## Requirements

* [Java Development Kit (javac)](https://adoptopenjdk.net/), check installed with:
```
javac -version
```
* [Maven](https://maven.apache.org/install.html), check installed with:
```
mvn --version
```

## Install PL/Java

#### Ubuntu 18.04:

Add this PPA:
https://www.ubuntuupdates.org/ppa/postgresql?dist=bionic-pgdg

Install PL/Java

```
sudo apt install postgresql-10-pljava
```

#### Other distros

In case you are using another distro, you'll need to build and install PL/Java.

Here is the reference you should follow: https://tada.github.io/pljava/install/install.html

## Setup PL/Java API

If you have not built PL/Java, we will need to add the jar on our local maven repository.

First we will need to discover what the PL/Java API Path.

Discover its path by doing:
```shell
dpkg -L postgresql-10-pljava
```

It will look like this:

```shell
/usr/share/postgresql/10/pljava/pljava-api-1.5.5.jar
```

Then. add the jar file to the maven package:
```shell
mvn install:install-file \
   -Dfile=/usr/share/postgresql/10/pljava/pljava-api-1.5.5.jar \
   -DgroupId=org.postgresql \
   -DartifactId=pljava-api \
   -Dversion=1.5.5 \
   -Dpackaging=jar \
   -DgeneratePom=true
```

Now PL/Java API is accessible locally by maven.


## Build and Load

PL/Java requires the jars to be loaded inside Pg, then we need to build and load the tests insite Pg.

#### Building the Test Suite

Navigate to `{PATH-TO-THE-PROJECT}/sql/java/test-suite` and install the requirements for the build:
```
mvn install
```

Now build the package:
```
mvn clean package
```

If everything was successfully executed, the build will be located at `{PATH-TO-THE-PROJECT}/sql/java/test-suite/target`.

You can check its content by executing:
```
jar tf {PATH-TO-THE-PROJECT}/sql/java/test-suite/target/test-suite-1.0.0.jar
```

This is the expected content:
```
META-INF/
META-INF/MANIFEST.MF
com/
com/example/
com/example/proj/
pljava.ddr
com/example/proj/TestNumeric.class
com/example/proj/TestChar.class
com/example/proj/TestSpi.class
com/example/proj/TestRecursive.class
com/example/proj/TestFunc.class
com/example/proj/TestNullBool.class
com/example/proj/TestDo.class
com/example/proj/TestFloats.class
com/example/proj/TestMixedTypes.class
com/example/proj/TestIntegers.class
com/example/proj/TestArray.class
com/example/proj/TestNullInteger.class
com/example/proj/TestBool.class
com/example/proj/TestComposites.class
META-INF/maven/
META-INF/maven/com.pldotnet/
META-INF/maven/com.pldotnet/test-suite/
META-INF/maven/com.pldotnet/test-suite/pom.xml
META-INF/maven/com.pldotnet/test-suite/pom.properties
```

#### Loading the Test Suite inside PG

To load the Jars inside PG, replace the path with you project local path and execute:
```
select sqlj.install_jar('file:{PATH-TO-THE-PROJECT}/sql/java/test-suite/target/test-suite-1.0.0.jar', 'testsuite', true);
```

The results of this command will look like this:
```
# select sqlj.install_jar(
   'file:/home/rodrigo/pldotnet/sql/java/test-suite/target/test-suite-1.0.0.jar', 'testsuite', true);
 install_jar 
-------------
 
(1 row)
```
 
On PL/Java, there is a classpath for every schema, so now is the time to load our `testsuite` classpath:
```
select sqlj.set_classpath('public', 'testsuite');
```

Results in:
```
# select sqlj.set_classpath('public', 'testsuite');
 set_classpath 
---------------
 
(1 row)
```

If you need to build replace the Jar, you can use the `replace_jar` function:
```
select sqlj.replace_jar('file:{PATH-TO-THE-PROJECT}/sql/java/test-suite/target/test-suite-1.0.0.jar', 'testsuite', true);
```


#### Testing

Now try to call a function and check if the results are as expected, for instance:
```
postgres=# SELECT BooleanXorJava(false, false) is false;
?column? 
----------
t
(1 row)
```
