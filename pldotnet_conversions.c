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
 * pldotnet_conversion.c
 *
 */

#include "pldotnet_conversions.h"

#include <utils/date.h>
#include <utils/inet.h>
#include <utils/timestamp.h>

#include "pldotnet_common.h"

////////////////////////////////////
//// Datum -> Npgsql or C# type ////
////////////////////////////////////

int16_t pldotnet_getInt16(void *datum) {
  int16_t value = DatumGetInt16((Datum)datum);
  return value;
}

int32_t pldotnet_getInt32(void *datum) {
  int32_t value = DatumGetInt32((Datum)datum);
  return value;
}

int64_t pldotnet_getInt64(void *datum) {
  int64_t value = DatumGetInt64((Datum)datum);
  return value;
}

float pldotnet_getFloat(void *datum) {
  float value = DatumGetFloat4((Datum)datum);
  return value;
}

double pldotnet_getDouble(void *datum) {
  double value = DatumGetFloat8((Datum)datum);
  return value;
}

bool pldotnet_getBoolean(void *datum) {
  bool value = DatumGetBool((Datum)datum);
  return value;
}

void pldotnet_getDatumPointAttributes(void *datum, double *x, double *y) {
  Point *orig_p = DatumGetPointP((Datum)datum);
  *x = orig_p->x;
  *y = orig_p->y;
}

void pldotnet_getDatumLineAttributes(void *datum, double *a, double *b,
                                     double *c) {
  LINE *orig_l = DatumGetLineP((Datum)datum);
  *a = orig_l->A;
  *b = orig_l->B;
  *c = orig_l->C;
}

void pldotnet_getDatumLineSegmentAttributes(void *datum, double *x1, double *y1,
                                            double *x2, double *y2) {
  LSEG *orig_l = DatumGetLsegP((Datum)datum);
  *x1 = orig_l->p[0].x;
  *y1 = orig_l->p[0].y;
  *x2 = orig_l->p[1].x;
  *y2 = orig_l->p[1].y;
}

void pldotnet_getDatumBoxAttributes(void *datum, double *x1, double *y1,
                                    double *x2, double *y2) {
  BOX *orig_b = DatumGetBoxP((Datum)datum);
  *x1 = orig_b->high.x;
  *y1 = orig_b->high.y;
  *x2 = orig_b->low.x;
  *y2 = orig_b->low.y;
}

void pldotnet_getDatumTextAttributes(void *datum, int *len, char **buf) {
  text *t = DatumGetTextPP((Datum)datum);
  const size_t datum_len = VARSIZE_ANY_EXHDR(t);

  *len = datum_len;
  *buf = VARDATA_ANY(t);
}

void pldotnet_getDatumPathAttributes(void *datum, int *pointNumber,
                                     int *closed) {
  PATH *orig_p = DatumGetPathP((Datum)datum);
  *pointNumber = orig_p->npts;
  *closed = orig_p->closed;
}

void pldotnet_getDatumPathCoordinates(void *datum, double *xCoordinates,
                                      double *yCoordinates) {
  PATH *orig_p = DatumGetPathP((Datum)datum);
  for (int i = 0, npts = orig_p->npts; i < npts; i++) {
    xCoordinates[i] = orig_p->p[i].x;
    yCoordinates[i] = orig_p->p[i].y;
  }
}

void pldotnet_getDatumPolygonAttributes(void *datum, int *pointNumber) {
  POLYGON *orig_p = DatumGetPolygonP((Datum)datum);
  *pointNumber = orig_p->npts;
}

void pldotnet_getDatumPolygonCoordinates(void *datum, double *xCoordinates,
                                         double *yCoordinates) {
  POLYGON *orig_p = DatumGetPolygonP((Datum)datum);
  for (int i = 0, npts = orig_p->npts; i < npts; i++) {
    xCoordinates[i] = orig_p->p[i].x;
    yCoordinates[i] = orig_p->p[i].y;
  }
}

void pldotnet_getDatumCircleAttributes(void *datum, double *x, double *y,
                                       double *r) {
  CIRCLE *orig_c = DatumGetCircleP((Datum)datum);
  *x = orig_c->center.x;
  *y = orig_c->center.y;
  *r = orig_c->radius;
}

void pldotnet_getDatumDateAttributes(void *datum, int *date) {
  DateADT orig_d = DatumGetDateADT((Datum)datum);
  *date = orig_d;
}

void pldotnet_getDatumTimeAttributes(void *datum, long *time) {
  TimeADT orig_t = DatumGetTimeADT((Datum)datum);
  *time = orig_t;
}

void pldotnet_getDatumTimeTzAttributes(void *datum, long *time, int *zone) {
  TimeTzADT *orig_tz = DatumGetTimeTzADTP((Datum)datum);
  *time = orig_tz->time;
  *zone = orig_tz->zone;
}

void pldotnet_getDatumTimestampAttributes(void *datum, long *timestamp) {
  Timestamp orig_ts = DatumGetTimestamp((Datum)datum);
  *timestamp = orig_ts;
}

void pldotnet_getDatumTimestampTzAttributes(void *datum, long *timestamp) {
  TimestampTz orig_ts = DatumGetTimestampTz((Datum)datum);
  *timestamp = orig_ts;
}

void pldotnet_getDatumIntervalAttributes(void *datum, long *time, int *day,
                                         int *month) {
  Interval *orig_i = DatumGetIntervalP((Datum)datum);
  *time = orig_i->time;
  *day = orig_i->day;
  *month = orig_i->month;
}

void pldotnet_getDatumMacAddressAttributes(void *datum, int length,
                                           unsigned char *bytes) {
  if (length == 6) {
    macaddr *orig_ma = DatumGetMacaddrP((Datum)datum);
    bytes[0] = orig_ma->a;
    bytes[1] = orig_ma->b;
    bytes[2] = orig_ma->c;
    bytes[3] = orig_ma->d;
    bytes[4] = orig_ma->e;
    bytes[5] = orig_ma->f;
    return;
  }
  macaddr8 *orig_ma8 = DatumGetMacaddr8P((Datum)datum);
  bytes[0] = orig_ma8->a;
  bytes[1] = orig_ma8->b;
  bytes[2] = orig_ma8->c;
  bytes[3] = orig_ma8->d;
  bytes[4] = orig_ma8->e;
  bytes[5] = orig_ma8->f;
  bytes[6] = orig_ma8->g;
  bytes[7] = orig_ma8->h;
}

void pldotnet_getDatumInetAttributes(void *datum, int *nelem,
                                     unsigned char *bytes, int *netmask) {
  inet *orig_i = DatumGetInetP((Datum)datum);
  if (orig_i->inet_data.family == PGSQL_AF_INET)
    *nelem = 4;
  else if (orig_i->inet_data.family == PGSQL_AF_INET6)
    *nelem = 16;
  else
    elog(ERROR, "Unrecognized Inet family: %u", orig_i->inet_data.family);

  *netmask = orig_i->inet_data.bits;

  for (int i = 0; i < *nelem; i++) {
    bytes[i] = orig_i->inet_data.ipaddr[i];
  }
}

////////////////////////////////////
//// Npgsql or C# type -> Datum ////
////////////////////////////////////

Datum pldotnet_createDatumInt16(int16_t dotnetValue) {
  Datum value = Int16GetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumInt32(int32_t dotnetValue) {
  Datum value = Int32GetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumInt64(int64_t dotnetValue) {
  Datum value = Int64GetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumFloat(float dotnetValue) {
  Datum value = Float4GetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumDouble(double dotnetValue) {
  Datum value = Float8GetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumBoolean(bool dotnetValue) {
  Datum value = BoolGetDatum(dotnetValue);
  return value;
}

Datum pldotnet_createDatumPoint(double x, double y) {
  Point *new_p = (Point *)palloc(sizeof(Point));
  new_p->x = x;
  new_p->y = y;
  return PointerGetDatum(new_p);
}

Datum pldotnet_createDatumLine(double a, double b, double c) {
  LINE *new_l = (LINE *)palloc(sizeof(LINE));
  new_l->A = a;
  new_l->B = b;
  new_l->C = c;
  return LinePGetDatum(new_l);
}

Datum pldotnet_createDatumLineSegment(double x1, double y1, double x2,
                                      double y2) {
  LSEG *new_l = (LSEG *)palloc(sizeof(LSEG));
  new_l->p[0].x = x1;
  new_l->p[0].y = y1;
  new_l->p[1].x = x2;
  new_l->p[1].y = y2;
  return LsegPGetDatum(new_l);
}

Datum pldotnet_createDatumBox(double x1, double y1, double x2, double y2) {
  BOX *new_b = (BOX *)palloc(sizeof(BOX));
  new_b->high.x = x1;
  new_b->high.y = y1;
  new_b->low.x = x2;
  new_b->low.y = y2;
  return BoxPGetDatum(new_b);
}

Datum pldotnet_createDatumText(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  text *new_t = (text *)palloc(new_size);

  SET_VARSIZE(new_t, new_size);
  memcpy((void *)VARDATA(new_t), buf, len);
  PG_RETURN_TEXT_P(new_t);
}

Datum pldotnet_createDatumPath(int npts, int closed, double *xCoordinates,
                               double *yCoordinates) {
  size_t path_size = sizeof(PATH) + ((size_t)npts * sizeof(Point));
  PATH *new_p = (PATH *)palloc(path_size);
  SET_VARSIZE(new_p, path_size);
  new_p->npts = npts;
  new_p->closed = closed;
  new_p->dummy = 0;
  for (int i = 0; i < npts; i++) {
    new_p->p[i].x = xCoordinates[i];
    new_p->p[i].y = yCoordinates[i];
  }
  return PathPGetDatum(new_p);
}

Datum pldotnet_createDatumPolygon(int npts, double *xCoordinates,
                                  double *yCoordinates) {
  size_t poly_size = sizeof(POLYGON) + ((size_t)npts * sizeof(Point));
  POLYGON *new_p = (POLYGON *)palloc(poly_size);
  SET_VARSIZE(new_p, poly_size);
  new_p->npts = npts;
  for (int i = 0; i < npts; i++) {
    new_p->p[i].x = xCoordinates[i];
    new_p->p[i].y = yCoordinates[i];
  }
  return PolygonPGetDatum(new_p);
}

Datum pldotnet_createDatumCircle(double x, double y, double r) {
  CIRCLE *new_c = (CIRCLE *)palloc(sizeof(CIRCLE));
  new_c->center.x = x;
  new_c->center.y = y;
  new_c->radius = r;
  return CirclePGetDatum(new_c);
}

Datum pldotnet_createDatumDate(int date) { return DateADTGetDatum(date); }

Datum pldotnet_createDatumTime(long time) { return TimeADTGetDatum(time); }

Datum pldotnet_createDatumTimeTz(long time, int zone) {
  TimeTzADT *new_tz = (TimeTzADT *)palloc(sizeof(TimeTzADT));
  new_tz->time = time;
  new_tz->zone = zone;
  return TimeTzADTPGetDatum(new_tz);
}

Datum pldotnet_createDatumTimestamp(long timestamp) {
  return TimestampGetDatum(timestamp);
}

Datum pldotnet_createDatumTimestampTz(long timestamp) {
  return TimestampTzGetDatum(timestamp);
}

Datum pldotnet_createDatumInterval(long time, int day, int month) {
  Interval *new_i = (Interval *)palloc(sizeof(Interval));
  new_i->time = time;
  new_i->day = day;
  new_i->month = month;
  return IntervalPGetDatum(new_i);
}

Datum pldotnet_createDatumMacAddress(int length, unsigned char *bytes) {
  if (length == 6) {
    macaddr *new_ma = (macaddr *)palloc(sizeof(macaddr));
    new_ma->a = bytes[0];
    new_ma->b = bytes[1];
    new_ma->c = bytes[2];
    new_ma->d = bytes[3];
    new_ma->e = bytes[4];
    new_ma->f = bytes[5];
    return MacaddrPGetDatum(new_ma);
  }
  macaddr8 *new_ma8 = (macaddr8 *)palloc(sizeof(macaddr8));
  new_ma8->a = bytes[0];
  new_ma8->b = bytes[1];
  new_ma8->c = bytes[2];
  new_ma8->d = bytes[3];
  new_ma8->e = bytes[4];
  new_ma8->f = bytes[5];
  new_ma8->g = bytes[6];
  new_ma8->h = bytes[7];
  return Macaddr8PGetDatum(new_ma8);
}

// TODO(rosicley) - Add an argument to check if the Datum is a `CIDR`.
// If so, check if the address has nonzero bits to the right of the netmask.
Datum pldotnet_createDatumInet(int length, unsigned char *bytes, int netmask) {
  inet *new_i = (inet *)palloc(sizeof(inet));
  SET_VARSIZE(new_i, sizeof(inet));
  if (length == 4)
    new_i->inet_data.family = PGSQL_AF_INET;
  else if (length == 16)
    new_i->inet_data.family = PGSQL_AF_INET6;
  else
    elog(ERROR, "Unrecognized Inet family with %d items.", length);

  new_i->inet_data.bits = netmask;
  for (int i = 0; i < length; i++) {
    new_i->inet_data.ipaddr[i] = bytes[i];
  }
  return InetPGetDatum(new_i);
}
