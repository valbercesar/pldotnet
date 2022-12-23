package com.example.proj;

import java.sql.SQLException;
import org.postgresql.pljava.annotation.Function;

import java.util.logging.Logger;

public class TestFunc {
    @Function
    public static int returnXJava() throws SQLException {
        return 10;
    }

    @Function
    public static int inc2Java(int val) throws SQLException {
        return val + 2;
    }

    @Function
    public static int sum2Java(int a, int b) throws SQLException {
        return a + b;
    }

    @Function
    public static int sum3Java(int aaa, int bbb, int ccc) throws SQLException {
        return aaa + bbb + ccc;
    }

    @Function
    public static int sum4Java(int a, int b, int c, int d) throws SQLException {
        return a + b + c + d;
    }
}