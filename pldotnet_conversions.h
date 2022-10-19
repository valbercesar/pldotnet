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

////////////////////////////////////
//// Datum -> Npgsql or C# type ////
////////////////////////////////////

extern int16_t pldotnet_getInt16(void *datum);

extern int32_t pldotnet_getInt32(void *datum);

extern int64_t pldotnet_getInt64(void *datum);

extern float pldotnet_getFloat(void *datum);

extern double pldotnet_getDouble(void *datum);

extern bool pldotnet_getBoolean(void *datum);

extern void pldotnet_getDatumPointAttributes(void *datum, double *x, double *y);

extern void pldotnet_getDatumLineAttributes(void *datum, double *a, double *b,
                                            double *c);

extern void pldotnet_getDatumLineSegmentAttributes(void *datum, double *x1,
                                                   double *y1, double *x2,
                                                   double *y2);

extern void pldotnet_getDatumBoxAttributes(void *datum, double *x1, double *y1,
                                           double *x2, double *y2);

extern void pldotnet_getDatumTextAttributes(void *datum, int *len, char **buf);

extern void pldotnet_getDatumPathAttributes(void *datum, int *pointNumber,
                                            int *closed);

extern void pldotnet_getDatumPathCoordinates(void *datum, double *xCoordinates,
                                             double *yCoordinates);

extern void pldotnet_getDatumPolygonAttributes(void *datum, int *pointNumber);

extern void pldotnet_getDatumPolygonCoordinates(void *datum,
                                                double *xCoordinates,
                                                double *yCoordinates);

extern void pldotnet_getDatumCircleAttributes(void *datum, double *x, double *y,
                                              double *r);

extern void pldotnet_getDatumDateAttributes(void *datum, int *date);

extern void pldotnet_getDatumTimeAttributes(void *datum, long *time);

extern void pldotnet_getDatumTimeTzAttributes(void *datum, long *time,
                                              int *zone);

extern void pldotnet_getDatumTimestampAttributes(void *datum, long *timestamp);

extern void pldotnet_getDatumTimestampTzAttributes(void *datum,
                                                   long *timestamp);

extern void pldotnet_getDatumIntervalAttributes(void *datum, long *time,
                                                int *day, int *month);

extern void pldotnet_getDatumMacAddressAttributes(void *datum, int length,
                                                  unsigned char *bytes);

extern void pldotnet_getDatumInetAttributes(void *datum, int *nelem,
                                            unsigned char *bytes, int *netmask);

////////////////////////////////////
//// Npgsql or C# type -> Datum ////
////////////////////////////////////

extern Datum pldotnet_createDatumInt16(int16_t dotnetValue);

extern Datum pldotnet_createDatumInt32(int32_t dotnetValue);

extern Datum pldotnet_createDatumInt64(int64_t dotnetValue);

extern Datum pldotnet_createDatumFloat(float dotnetValue);

extern Datum pldotnet_createDatumDouble(double dotnetValue);

extern Datum pldotnet_createDatumBoolean(bool dotnetValue);

extern Datum pldotnet_createDatumPoint(double x, double y);

extern Datum pldotnet_createDatumLine(double a, double b, double c);

extern Datum pldotnet_createDatumLineSegment(double x1, double y1, double x2,
                                             double y2);

extern Datum pldotnet_createDatumBox(double x1, double y1, double x2,
                                     double y2);

extern Datum pldotnet_createDatumText(int len, char *buf);

extern Datum pldotnet_createDatumPath(int pointNumber, int closed,
                                      double *xCoordinates,
                                      double *yCoordinates);

extern Datum pldotnet_createDatumPolygon(int pointNumber, double *xCoordinates,
                                         double *yCoordinates);

extern Datum pldotnet_createDatumCircle(double x, double y, double r);

extern Datum pldotnet_createDatumDate(int date);

extern Datum pldotnet_createDatumTime(long time);

extern Datum pldotnet_createDatumTimeTz(long time, int zone);

extern Datum pldotnet_createDatumTimestamp(long timestamp);

extern Datum pldotnet_createDatumTimestampTz(long timestamp);

extern Datum pldotnet_createDatumInterval(long time, int day, int month);

extern Datum pldotnet_createDatumMacAddress(int length, unsigned char *bytes);

extern Datum pldotnet_createDatumInet(int length, unsigned char *bytes,
                                      int netmask);

#endif  // PLDOTNET_CONVERSIONS_H_
