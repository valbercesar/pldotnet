package com.example.proj;

import java.sql.SQLException;
import org.postgresql.pljava.annotation.Function;

import java.util.logging.Logger;

public class TestIntegers {
    @Function
    public static short maxSmallIntJava() throws SQLException {
        return (short)32767;
    }

    @Function
    public static short sum2SmallIntJava(short a, short b) throws SQLException {
        return (short)(a + b);
    }

    @Function
    public static int maxIntegerJava() throws SQLException {
        return 2147483647;
    }

    @Function
    public static int sum2IntegerJava(int a, int b) throws SQLException {
        return a + b;
    }

    @Function
    public static long maxBigIntJava() throws SQLException {
        return Long.MAX_VALUE;
    }

    @Function
    public static long sum2BigIntJava(long a, long b) throws SQLException {
        return a + b;
    }

    @Function
    public static long mixedBigIntJava(int a, int b, long c) throws SQLException {
        return (long)a + (long)b + c;
    }

    @Function
    public static long mixedIntJava(short a, short b, int c) throws SQLException {
        return (int)a + (int)b + c;
    }

}