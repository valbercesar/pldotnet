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

#include "pldotnet_main.h"
#include <utils/rangetypes.h>

////////////////////////////////////
/// Datum -> Npgsql or .NET type ///
////////////////////////////////////

/**
 * @brief Returns the int16_t value that corresponds to a PostgreSQL small
 * integer. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return int16_t value.
 */
extern int16_t pldotnet_GetInt16(void *datum);

/**
 * @brief Returns the int32_t value that corresponds to a PostgreSQL integer. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return int32_t value.
 */
extern int32_t pldotnet_GetInt32(void *datum);

/**
 * @brief Returns the int64_t value that corresponds to a PostgreSQL big
 * integer. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return int64_t value.
 */
extern int64_t pldotnet_GetInt64(void *datum);

/**
 * @brief Returns the float value that corresponds to a PostgreSQL float. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return float value.
 */
extern float pldotnet_GetFloat(void *datum);

/**
 * @brief Returns the float value that corresponds to a PostgreSQL double. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return double value.
 */
extern double pldotnet_GetDouble(void *datum);

/**
 * @brief Returns the boolean value that corresponds to a PostgreSQL boolean. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @return bool value.
 */
extern bool pldotnet_GetBoolean(void *datum);

/**
 * @brief Modifies the arguments with the coordinates of a PostgreSQL Point. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param x the x coordinate.
 * @param y the y coordinate.
 */
extern void pldotnet_GetDatumPointAttributes(void *datum, double *x, double *y);

/**
 * @brief Modifies the arguments with the parameters of a PostgreSQL Line. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param a the line parameter.
 * @param b the line parameter.
 * @param c the line parameter.
 */
extern void pldotnet_GetDatumLineAttributes(void *datum, double *a, double *b,
                                            double *c);

/**
 * @brief Modifies the arguments with the point coordinates that make up a
 * PostgreSQL Line Segment. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param x1 the x coordinate of the first point.
 * @param y1 the y coordinate of the first point.
 * @param x2 the x coordinate of the second point.
 * @param y2 the y coordinate of the second point.
 */
extern void pldotnet_GetDatumLineSegmentAttributes(void *datum, double *x1,
                                                   double *y1, double *x2,
                                                   double *y2);
/**
 * @brief Modifies the arguments with the point coordinates that make up a
 * PostgreSQL Box. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param x1 the x coordinate of the upper right corner.
 * @param y1 the y coordinate of the upper right corner.
 * @param x2 the x coordinate of the lower left corner.
 * @param y2 the y coordinate of the lower left corner.
 */
extern void pldotnet_GetDatumBoxAttributes(void *datum, double *x1, double *y1,
                                           double *x2, double *y2);

/**
 * @brief Modifies the arguments with the number of points and whether the
 * PostgreSQL Path is closed or not. It is used to convert from PostgreSQL type
 * to .NET type.
 *
 * @param datum the datum object.
 * @param pointNumber is the number of points of the PostgreSQL Path.
 * @param closed whether the Path is closed or not.
 */
extern void pldotnet_GetDatumPathAttributes(void *datum, int *pointNumber,
                                            int *closed);

/**
 * @brief Sets the coordinates of the points that make up the PostgreSQL Path to
 * two arrays. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param xCoordinates the array with the x coordinates.
 * @param yCoordinates the array with the y coordinates.
 */
extern void pldotnet_GetDatumPathCoordinates(void *datum, double *xCoordinates,
                                             double *yCoordinates);

/**
 * @brief Modifies the argument with the number of points that make up the
 * PostgreSQL Polygon. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param pointNumber is a pointer to the number of points.
 */
extern void pldotnet_GetDatumPolygonAttributes(void *datum, int *pointNumber);

/**
 * @brief Sets the coordinates of the points that make up the PostgreSQL Path to
 * two arrays. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param xCoordinates the array with the x coordinates.
 * @param yCoordinates the array with the y coordinates.
 */
extern void pldotnet_GetDatumPolygonCoordinates(void *datum,
                                                double *xCoordinates,
                                                double *yCoordinates);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Circle. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param x the x coordinate of the center point.
 * @param y the y coordinate of the center point.
 * @param r the radius of the circle.
 */
extern void pldotnet_GetDatumCircleAttributes(void *datum, double *x, double *y,
                                              double *r);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Text. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumTextAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Character
 * (n). It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumCharAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Character
 * Varying (n). It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumVarCharAttributes(void *datum, int *len,
                                               char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Bytea. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumByteaAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Xml. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumXmlAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL JSON. It is
 * used to convert from PostgreSQL type to .NET type.
 * @remark a PostgreSQL JSON is treated as text.
 *
 * @param datum the datum object.
 * @param len the number of characters.
 * @param buf a pointer that will point to the data content (char*).
 */
extern void pldotnet_GetDatumJsonAttributes(void *datum, int *len, char **buf);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Date. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param date the int32 value that represents a Date in PostgreSQL.
 */
extern void pldotnet_GetDatumDateAttributes(void *datum, int *date);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Time. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param time the int64 value that represents a Date in PostgreSQL.
 */
extern void pldotnet_GetDatumTimeAttributes(void *datum, long *time);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Time. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param time is the int64 value that represents a Time in PostgreSQL.
 */
extern void pldotnet_GetDatumTimeTzAttributes(void *datum, long *time,
                                              int *zone);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Timestamp
 * without time now. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param time is the int64 value that represents a Timestamp in PostgreSQL.
 */
extern void pldotnet_GetDatumTimestampAttributes(void *datum, long *timestamp);

/**
 * @brief Modifies the argument with the property of a PostgreSQL Timestamp wit
 * time now. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param time is the int64 value that represents a Timestamp in PostgreSQL.
 */
extern void pldotnet_GetDatumTimestampTzAttributes(void *datum,
                                                   long *timestamp);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Interval.
 * It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param time the ticks of time.
 * @param day the day.
 * @param month the month.
 */
extern void pldotnet_GetDatumIntervalAttributes(void *datum, long *time,
                                                int *day, int *month);

/**
 * @brief Modifies an argument with the properties of a PostgreSQL Mac Address.
 * The MAC address length must be 6 or 8. It is used to convert from PostgreSQL
 * type to .NET type.
 *
 * @param datum the datum object.
 * @param length the MAC address length.
 * @param bytes an array with the byte value that makes up the MAC address.
 */
extern void pldotnet_GetDatumMacAddressAttributes(void *datum, int length,
                                                  unsigned char *bytes);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL INET or
 * CIDR. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param nelem the number of elements.
 * @param bytes the array with byte values.
 * @param netmask the number of bits in the netmask.
 */
extern void pldotnet_GetDatumInetAttributes(void *datum, int *nelem,
                                            unsigned char *bytes, int *netmask);

/**
 * @brief Modifies the argument with the property of PostgreSQL Money. It is
 * used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param value is the long integer that represents the PostgreSQL Money.
 */
extern void pldotnet_GetDatumMoneyAttributes(void *datum, long *value);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Bit or
 * VarBit. It is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param len the length of the bit string.
 * @param dat a pointer that points to an array of unsigned char.
 */
extern void pldotnet_GetDatumVarBitAttributes(void *datum, int *len,
                                              bits8 **dat);

/**
 * @brief Modifies the array argument with the UUID data. It is used to convert
 * from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param data a pointer that points to an array of unsigned char.
 */
extern void pldotnet_GetDatumUuidAttributes(void *datum, unsigned char *data);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Array. It
 * is used to convert from PostgreSQL type to .NET type.
 *
 * @param datum the datum object.
 * @param typeId the OID of the elements.
 * @param nDims the number of dimensions.
 * @param dims the array with the lengths of each dimension.
 * @param nullmap is the pointer that points to an array with the nullmap of the
 * PostgreSQL Array.
 */
extern void pldotnet_GetArrayAttributes(void *datum, int *typeId,
                                        int *nDims, int *dims,
                                        uint8_t **nullmap);

/**
 * @brief Get a maximum number of dimensions of a PostgreSQL Array.
 *
 * @return the maximum number of dimensions of a PostgreSQL Array.
 */
extern int get_Maxdim(void);

/**
 * @brief Modifies the arguments with the properties of a PostgreSQL Array.
 *
 * @param arrayDatum the PostgreSQL Array.
 * @param results in the flat array with the datum objects.
 * @param nElems the number of elements.
 * @param typeId the OID of the elements.
 * @return int an integer to control errors: 0 on success, other on failure.
 */
extern int pldotnet_GetArrayDatum(Datum arrayDatum, Datum *results, int nElems,
                                  int typeId);

extern void pldotnet_GetDatumRangeBoundAttributes(RangeBound *inputRange,
                                                  Datum *rangeDatum,
                                                  bool *infinite,
                                                  bool *inclusive, bool *lower);

extern void pldotnet_GetDatumRangeAttributes(Datum inputDatum, bool *isEmpty,
                                             RangeBound **lowerRange,
                                             RangeBound **upperDange);

////////////////////////////////////
/// Npgsql or .NET type -> Datum ///
////////////////////////////////////

/**
 * @brief Creates a PostgreSQL small integer from an int16_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumInt16(int16_t value);

/**
 * @brief Creates a PostgreSQL integer from an int32_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumInt32(int32_t value);

/**
 * @brief Creates a PostgreSQL big integer from an int64_t value. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumInt64(int64_t value);

/**
 * @brief Creates a PostgreSQL float from a float value. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumFloat(float value);

/**
 * @brief Creates a PostgreSQL double from a double value. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumDouble(double value);

/**
 * @brief Creates a PostgreSQL double from a double. It is used to convert from
 * a .NET type to a PostgreSQL Datum.
 *
 * @param value
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumBoolean(bool value);

/**
 * @brief Creates a PostgreSQL Point from its coordinates. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param x
 * @param y
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumPoint(double x, double y);

/**
 * @brief Creates a PostgreSQL Line from its parameters. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param a
 * @param b
 * @param c
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumLine(double a, double b, double c);

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
extern Datum pldotnet_CreateDatumLineSegment(double x1, double y1, double x2,
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
extern Datum pldotnet_CreateDatumBox(double x1, double y1, double x2,
                                     double y2);

/**
 * @brief Creates a PostgreSQL Path from its point coordinates. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param npts the number of points.
 * @param closed whether the path is closed or not
 * @param xCoordinates the array with the x coordinate of the points.
 * @param yCoordinates the array with the y coordinate of the points.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumPath(int npts, int closed,
                                      double *xCoordinates,
                                      double *yCoordinates);

/**
 * @brief Creates a PostgreSQL Polygon from its point coordinates. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param npts the number of points.
 * @param xCoordinates the array with the x coordinate of the points.
 * @param yCoordinates the array with the y coordinate of the points.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumPolygon(int npts, double *xCoordinates,
                                         double *yCoordinates);

/**
 * @brief Creates a PostgreSQL Circle from its properties. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param x the x coordinate of the center point.
 * @param y the y coordinate of the center point.
 * @param r the radius of the circle.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumCircle(double x, double y, double r);

/**
 * @brief Creates a PostgreSQL Text. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumText(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Character(n). It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumChar(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Character Varying(n). It is used to convert from
 * a .NET type to a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumVarChar(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Bytea. It is used to convert from a .NET type to
 * a PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumBytea(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Xml. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumXml(int len, char *buf);

/**
 * @brief Creates a PostgreSQL JSON. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 * @remark a PostgreSQL JSON is treated as text.
 *
 * @param len the number of characters.
 * @param buf the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumJson(int len, char *buf);

/**
 * @brief Creates a PostgreSQL Date. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param date is the integer value that represents a Date in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumDate(int date);

/**
 * @brief Creates a PostgreSQL Time. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param time is the long integer value that represents a Time in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumTime(long time);

/**
 * @brief Creates a PostgreSQL Time with the time zone. It is used to convert from a
 * .NET type to a PostgreSQL Datum.
 *
 * @param time is the long integer value that represents a Time in PostgreSQL.
 * @param zone the zone value.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumTimeTz(long time, int zone);

/**
 * @brief Creates a PostgreSQL Timestamp without a time zone. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param time is the long integer value that represents a Timestamp in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumTimestamp(long timestamp);

/**
 * @brief Creates a PostgreSQL Timestamp with the time zone. It is used to convert
 * from a .NET type to a PostgreSQL Datum.
 *
 * @param time is the long integer value that represents a Timestamp in PostgreSQL.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumTimestampTz(long timestamp);

/**
 * @brief Creates a PostgreSQL Interval. It is used to convert from a .NET type
 * to a PostgreSQL Datum.
 *
 * @param time is the long integer value that represents a time in PostgreSQL.
 * @param day the number of days.
 * @param month the number of months.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumInterval(long time, int day, int month);

/**
 * @brief Creates a PostgreSQL MAC Address. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param length the length of the MAC address; it must be 6 or 8.
 * @param bytes the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumMacAddress(int length, unsigned char *bytes);

/**
 * @brief Creates a PostgreSQL INET. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param length the number of elements.
 * @param bytes the array with byte values.
 * @param netmask the number of bits in the netmask.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumInet(int length, unsigned char *bytes,
                                      int netmask);

/**
 * @brief Creates PostgreSQL Money. It is used to convert from a .NET type to
 * a PostgreSQL Datum.
 *
 * @param value the long integer that represents the money in PostgreSQL
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumMoney(long value);

/**
 * @brief Creates a PostgreSQL VarBit or Bit. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param len the number of elements.
 * @param dat the array with byte values.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumVarBit(int len, bits8 *dat);

/**
 * @brief Creates a PostgreSQL UUID. It is used to convert from a .NET type to a
 * PostgreSQL Datum.
 *
 * @param data the array with the UUID values.
 */
extern Datum pldotnet_CreateDatumUuid(unsigned char *data);

/**
 * @brief Creates an empty PostgreSQL range. It is used to convert from a .NET
 * type to a PostgreSQL Datum.
 *
 * @param rangeTypeId the Oid of the range.
 * @return Datum the empty datum object.
 */
extern Datum pldotnet_CreateEmptyDatumRange(Oid rangeTypeId);

/**
 * @brief Creates a PostgreSQL range according to the provided Oid. It is used
 * to convert from a .NET type to a PostgreSQL Datum.
 *
 * @param rtOid
 * @param lowerDatum
 * @param lowerInfinite
 * @param lowerInclusive
 * @param upperDatum
 * @param upperInfinite
 * @param upperInclusive
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumRange(Oid rtOid, Datum lowerDatum,
                                       bool lowerInfinite,
                                       bool lowerInclusive, Datum upperDatum,
                                       bool upperInfinite,
                                       bool upperInclusive);

/**
 * @brief Creates a PostgreSQL Array of the specified type. It is used to
 * convert from a .NET type to a PostgreSQL Datum.
 *
 * @param elementId is the OID of the elements.
 * @param dimNumber the number of dimensions.
 * @param dimLengths the array with the lengths in each dimension.
 * @param datums the flat array with the datum objects.
 * @param nulls the array that maps the null values of the datums variable.
 * @return Datum the datum object.
 */
extern Datum pldotnet_CreateDatumArray(int elementId, int dimNumber,
                                       int *dimLengths, Datum *datums,
                                       bool *nulls);

#endif  // PLDOTNET_CONVERSIONS_H_
