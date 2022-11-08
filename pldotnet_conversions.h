/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *             procedural languages (PL)
 *
 *
 * Copyright 2019-2020 Brick Abode
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * pldotnet_conversion.h
 *
 */

#ifndef PLDOTNET_CONVERSIONS_H_
#define PLDOTNET_CONVERSIONS_H_

#include "pldotnet_common.h"
#include <utils/rangetypes.h>

////////////////////////////////////
//// Datum -> Npgsql or C# type ////
////////////////////////////////////

/**
 * @brief Returns the int16_t value that corresponds to a PostgreSQL small
 * integer. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return int16_t value.
 */
extern int16_t pldotnet_getInt16(void *datum);

/**
 * @brief Returns the int32_t value that corresponds to a PostgreSQL integer. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return int32_t value.
 */
extern int32_t pldotnet_getInt32(void *datum);

/**
 * @brief Returns the int64_t value that corresponds to a PostgreSQL big
 * integer. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return int64_t value.
 */
extern int64_t pldotnet_getInt64(void *datum);

/**
 * @brief Returns the float value that corresponds to a PostgreSQL float. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return float value.
 */
extern float pldotnet_getFloat(void *datum);

/**
 * @brief Returns the float value that corresponds to a PostgreSQL double. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return double value.
 */
extern double pldotnet_getDouble(void *datum);

/**
 * @brief Returns the boolean value that corresponds to a PostgreSQL boolean. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @return bool value.
 */
extern bool pldotnet_getBoolean(void *datum);

/**
 * @brief Modifies the arguments with the coordinates of a PostgreSQL Point. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param x the x coordinate.
 * @param y the y coordinate.
 */
extern void pldotnet_getDatumPointAttributes(void *datum, double *x, double *y);

/**
 * @brief Modifies the arguments with the parameters of a PostgreSQL Line. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param a the line parameter.
 * @param b the line parameter.
 * @param c the line parameter.
 */
extern void pldotnet_getDatumLineAttributes(void *datum, double *a, double *b,
                                            double *c);

/**
 * @brief Modifies the arguments with the point coordinates that make up a
 * PostgreSQL Line Segment. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param x1 the x coordinate of the first point.
 * @param y1 the y coordinate of the first point.
 * @param x2 the x coordinate of the second point.
 * @param y2 the y coordinate of the second point.
 */
extern void pldotnet_getDatumLineSegmentAttributes(void *datum, double *x1,
                                                   double *y1, double *x2,
                                                   double *y2);
/**
 * @brief Modifies the arguments with the point coordinates that make up a
 * PostgreSQL Box. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param x1 the x coordinate of the upper right corner.
 * @param y1 the y coordinate of the upper right corner.
 * @param x2 the x coordinate of the lower left corner.
 * @param y2 the y coordinate of the lower left corner.
 */
extern void pldotnet_getDatumBoxAttributes(void *datum, double *x1, double *y1,
                                           double *x2, double *y2);

/**
 * @brief Modifies the arguments with the number of points and whether the
 * PostgreSQL Path is closed or not. It is used to convert from PostgreSQL type
 * to C#.
 *
 * @param datum the datum object.
 * @param pointNumber the number of point of PostgreSQL Path.
 * @param closed whether the Path is closed or not.
 */
extern void pldotnet_getDatumPathAttributes(void *datum, int *pointNumber,
                                            int *closed);

/**
 * @brief Sets the coordinates of the points that make up the PostgreSQL Path to
 * two arrays. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param xCoordinates the array with the x coordinates.
 * @param yCoordinates the array with the y coordinates.
 */
extern void pldotnet_getDatumPathCoordinates(void *datum, double *xCoordinates,
                                             double *yCoordinates);

/**
 * @brief Modifies the argument with the number of points that make up the
 * PostgreSQL Polygon. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param pointNumber an pointer to the number of points.
 */
extern void pldotnet_getDatumPolygonAttributes(void *datum, int *pointNumber);

/**
 * @brief Sets the coordinates of the points that make up the PostgreSQL Path to
 * two arrays. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param xCoordinates the array with the x coordinates.
 * @param yCoordinates the array with the y coordinates.
 */
extern void pldotnet_getDatumPolygonCoordinates(void *datum,
                                                double *xCoordinates,
                                                double *yCoordinates);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Circle. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param x the x coordinate of the center point.
 * @param y the y coordinate of the center point.
 * @param r the radius of the circle.
 */
extern void pldotnet_getDatumCircleAttributes(void *datum, double *x, double *y,
                                              double *r);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Text. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_getDatumTextAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Character
 * (n). It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_getDatumCharAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Character
 * Varying (n). It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_getDatumVarCharAttributes(void *datum, int *len,
                                               char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Bytea. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_getDatumByteaAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Xml. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_getDatumXmlAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Date. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param date the int32 value that represents a Date in PostgreSQL.
 */
extern void pldotnet_getDatumDateAttributes(void *datum, int *date);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Time. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param time the int64 value that represents a Date in PostgreSQL.
 */
extern void pldotnet_getDatumTimeAttributes(void *datum, long *time);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Time. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param time the int64 value that represents a Time in PostgreSQL.
 */
extern void pldotnet_getDatumTimeTzAttributes(void *datum, long *time,
                                              int *zone);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Timestamp
 * without time noe. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param time the int64 value that represents a Timestamp in PostgreSQL.
 */
extern void pldotnet_getDatumTimestampAttributes(void *datum, long *timestamp);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Timestamp wit
 * time noe. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param time the int64 value that represents a Timestamp in PostgreSQL.
 */
extern void pldotnet_getDatumTimestampTzAttributes(void *datum,
                                                   long *timestamp);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Interval.
 * It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param time the ticks of time.
 * @param day the day.
 * @param month the month.
 */
extern void pldotnet_getDatumIntervalAttributes(void *datum, long *time,
                                                int *day, int *month);

/**
 * @brief Modifies an argument with the properties of a PostgreSQL Mac Address.
 * The MAC address length must be 6 or 8. It is used to convert from PostgreSQL
 * type to C#.
 *
 * @param datum the datum object.
 * @param length the MAC address length.
 * @param bytes an array with the byte value that make up the MAC address.
 */
extern void pldotnet_getDatumMacAddressAttributes(void *datum, int length,
                                                  unsigned char *bytes);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL INET or
 * CIDR. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param nelem the number of elements.
 * @param bytes the array with byte values.
 * @param netmask the number of bits in the netmask.
 */
extern void pldotnet_getDatumInetAttributes(void *datum, int *nelem,
                                            unsigned char *bytes, int *netmask);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Money. It is
 * used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param value the long integer that represents the PostgreSQL Money.
 */
extern void pldotnet_getDatumMoneyAttributes(void *datum, long *value);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Bit or
 * VarBit. It is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param len the length of the bit string.
 * @param dat an pointer that points to an array of unsigned char.
 */
extern void pldotnet_getDatumVarBitAttributes(void *datum, int *len,
                                              bits8 **dat);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Array. It
 * is used to convert from PostgreSQL type to C#.
 *
 * @param datum the datum object.
 * @param element_typeid the OID of the elements.
 * @param ndims the number of dimensions.
 * @param dims the array with the lengths of each dimension.
 * @param nullmap the pointer that points to an array with the nullmap of the
 * PostgreSQL Array.
 */
extern void pldotnet_getArrayAttributes(void *datum, int *element_typeid,
                                        int *ndims, int *dims,
                                        uint8_t **nullmap);

/**
 * @brief Get maximum number of dimensions of an PostgreSQL Array.
 *
 * @return the maximum number of dimensions of an PostgreSQL Array.
 */
extern int get_maxdim(void);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Array.
 *
 * @param array_datum the PostgreSQL Array.
 * @param results the flat array with the datum objects.
 * @param nelems the number of elements.
 * @param element_typeid the OID of the elements.
 * @return int an integer to control errors.
 */
extern int pldotnet_getArrayDatum(Datum array_datum, Datum *results, int nelems,
                                  int element_typeid);

extern void pldotnet_getDatumRangeBoundAttributes(RangeBound *input_range,
                                                  Datum *range_datum,
                                                  bool *infinite,
                                                  bool *inclusive, bool *lower);

extern void pldotnet_getDatumRangeAttributes(Datum input_datum, bool *is_empty,
                                             RangeBound **lower_range,
                                             RangeBound **upper_range);

////////////////////////////////////
//// Npgsql or C# type -> Datum ////
////////////////////////////////////

/**
 * @brief Creates a PostgreSQL small integer from a int16_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumInt16(int16_t dotnetValue);

/**
 * @brief Creates a PostgreSQL integer from a int32_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumInt32(int32_t dotnetValue);

/**
 * @brief Creates a PostgreSQL big integer from a int64_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumInt64(int64_t dotnetValue);

/**
 * @brief Creates a PostgreSQL float from a float value. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumFloat(float dotnetValue);

/**
 * @brief Creates a PostgreSQL double from a double value. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumDouble(double dotnetValue);

/**
 * @brief Creates a PostgreSQL double from a double. It is used to convert from
 * a .NET type to a PostgreSQL Datum.
 *
 * @param dotnetValue
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumBoolean(bool dotnetValue);

/**
 * @brief Creates a PostgreSQL Point from its coordinates. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param x
 * @param y
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumPoint(double x, double y);

/**
 * @brief Creates a PostgreSQL Line from its parameters. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param a
 * @param b
 * @param c
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumLine(double a, double b, double c);

/**
 * @brief Creates a PostgreSQL Line Segment from its point coordinates. It is
 * used to convert from a .NET type to a PostgreSQL Datum.
 *
 * @param x1 the x coordinate of the first point.
 * @param y1 the y coordinate of the first point.
 * @param x2 the x coordinate of the second point.
 * @param y2 the y coordinate of the second point.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumLineSegment(double x1, double y1, double x2,
                                             double y2);

/**
 * @brief Creates a PostgreSQL Box from its point coordinates. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param x1 the x coordinate of the upper right corner.
 * @param y1 the y coordinate of the upper right corner.
 * @param x2 the x coordinate of the lower left corner.
 * @param y2 the y coordinate of the lower left corner.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumBox(double x1, double y1, double x2,
                                     double y2);

/**
 * @brief Creates a PostgreSQL Path from its point coordinates. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param pointNumber the number of points.
 * @param closed whether the path is closed or not
 * @param xCoordinates the array with the x coordinate of the points.
 * @param yCoordinates the array with the y coordinate of the points.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumPath(int pointNumber, int closed,
                                      double *xCoordinates,
                                      double *yCoordinates);

/**
 * @brief Creates a PostgreSQL Polygon from its point coordinates. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param pointNumber the number of points.
 * @param xCoordinates the array with the x coordinate of the points.
 * @param yCoordinates the array with the y coordinate of the points.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumPolygon(int pointNumber, double *xCoordinates,
                                         double *yCoordinates);

/**
 * @brief Creates a PostgreSQL Circle from its properties. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param x the x coordinate of center point.
 * @param y the y coordinate of center point.
 * @param r the radius of the circle.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumCircle(double x, double y, double r);

/**
 * @brief Creates a PostgreSQL Text. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumText(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Character(n). It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumChar(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Character Varying(n). It is used to convert from
 * a .NET type to a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumVarChar(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Bytea. It is used to convert from a .NET type to
 * a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumBytea(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Xml. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumXml(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Date. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param date the integer value that represents a Date in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumDate(int date);

/**
 * @brief Creates a PostgreSQL Time. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param time the long integer value that represents a Time in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumTime(long time);

/**
 * @brief Creates a PostgreSQL Time with time zone. It is used to convert from a
 * .NET type to a PostgreSQL Datum.
 *
 * @param time the long integer value that represents a Time in PostgreSQL.
 * @param zone the zone value.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumTimeTz(long time, int zone);

/**
 * @brief Creates a PostgreSQL Timestamp without time zone. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param time the long integer value that represents a Timestamp in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumTimestamp(long timestamp);

/**
 * @brief Creates a PostgreSQL Timestamp with time zone. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param time the long integer value that represents a Timestamp in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumTimestampTz(long timestamp);

/**
 * @brief Creates a PostgreSQL Interval. It is used to convert from a .NET type
 * to a PostgreSQL Datum.
 *
 * @param time the long integer value that represents a time in PostgreSQL.
 * @param day the number of days.
 * @param month the number of months.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumInterval(long time, int day, int month);

/**
 * @brief Creates a PostgreSQL MAC Address. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param length the length of the MAC address; it must be 6 or 8.
 * @param bytes the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumMacAddress(int length, unsigned char *bytes);

/**
 * @brief Creates a PostgreSQL INET. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param length the number of elements.
 * @param bytes the array with byte values.
 * @param netmask  the number of bits in the netmask.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumInet(int length, unsigned char *bytes,
                                      int netmask);

/**
 * @brief Creates a PostgreSQL Money. It is used to convert from a .NET type to
 * a PostgreSQL Datum.
 *
 * @param value the long integer that represents the money in PostgreSQL
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumMoney(long value);

/**
 * @brief Creates a PostgreSQL VarBit or Bit. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param len the number of elements.
 * @param bytes the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumVarBit(int len, bits8 *bytes);

/**
 * @brief Creates a empty PostgreSQL range. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param rangetypid the Oid of the range.
 * @return Datum the empty datum object.
 */
extern Datum pldotnet_createEmptyDatumRange(Oid rangetypid);

/**
 * @brief Creates a PostgreSQL range according to the provided Oid. It is used
 * to convert from a .NET type to a PostgreSQL Datum.
 *
 * @param rt_oid
 * @param lower_datum
 * @param lower_infinite
 * @param lower_inclusive
 * @param upper_datum
 * @param upper_infinite
 * @param upper_inclusive
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumRange(Oid rt_oid, Datum lower_datum,
                                       bool lower_infinite,
                                       bool lower_inclusive, Datum upper_datum,
                                       bool upper_infinite,
                                       bool upper_inclusive);

/**
 * @brief Creates a PostgreSQL Array of the specified type. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param element_id the OID of the elements.
 * @param dimNumber the number of dimensions.
 * @param dimLengths the array with the lengths in each dimension.
 * @param datums the flat array with the datum objects.
 * @param nulls the array that maps the null values of the datums variable.
 * @return Datum the datum object.
 */
extern Datum pldotnet_createDatumArray(int element_id, int dimNumber,
                                       int *dimLengths, Datum *datums,
                                       bool *nulls);

#endif  // PLDOTNET_CONVERSIONS_H_
