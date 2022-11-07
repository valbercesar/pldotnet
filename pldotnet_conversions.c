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

#include <utils/cash.h>
#include <utils/date.h>
#include <utils/inet.h>
#include <utils/rangetypes.h>
#include <utils/timestamp.h>
#include <utils/varbit.h>
#include <utils/xml.h>

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

void pldotnet_getDatumTextAttributes(void *datum, int *len, char **buf) {
  text *t = DatumGetTextPP((Datum)datum);
  const size_t datum_len = VARSIZE_ANY_EXHDR(t);
  *len = datum_len;
  *buf = VARDATA_ANY(t);
}

void pldotnet_getDatumCharAttributes(void *datum, int *len, char **buf) {
  BpChar *orig_bpc = DatumGetBpCharPP((Datum)datum);
  *len = VARSIZE_ANY_EXHDR(orig_bpc);
  *buf = VARDATA_ANY(orig_bpc);
}

void pldotnet_getDatumVarCharAttributes(void *datum, int *len, char **buf) {
  VarChar *orig_bpc = DatumGetVarCharPP((Datum)datum);
  *len = VARSIZE_ANY_EXHDR(orig_bpc);
  *buf = VARDATA_ANY(orig_bpc);
}

void pldotnet_getDatumByteaAttributes(void *datum, int *len, char **buf) {
  bytea *orig_b = DatumGetByteaPP((Datum)datum);
  *len = VARSIZE_ANY_EXHDR(orig_b);
  *buf = VARDATA_ANY(orig_b);
}

void pldotnet_getDatumXmlAttributes(void *datum, int *len, char **buf) {
  xmltype *orig_x = DatumGetXmlP((Datum)datum);
  *len = VARSIZE_ANY_EXHDR(orig_x);
  *buf = VARDATA_ANY(orig_x);
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

void pldotnet_getDatumMoneyAttributes(void *datum, long *value) {
  Cash orig_c = DatumGetCash((Datum)datum);
  *value = orig_c;
}

void pldotnet_getDatumVarBitAttributes(void *datum, int *len, bits8 **dat) {
  VarBit *orig_vb = DatumGetVarBitP((Datum)datum);
  *len = orig_vb->bit_len;
  *dat = &orig_vb->bit_dat[0];
}

void pldotnet_getDatumRangeAttributes(Datum input_datum, bool *is_empty,
                                      RangeBound **lower_range,
                                      RangeBound **upper_range) {
  RangeType *orig_r = DatumGetRangeTypeP(input_datum);
  Oid rt_oid = RangeTypeGetOid(orig_r);
  // Oid             ul_oid = range_underlying(rt_oid);
  TypeCacheEntry *typcache;

  *lower_range = palloc(sizeof(RangeBound));
  *upper_range = palloc(sizeof(RangeBound));

  elog(INFO, "# DEBUG(C): lower_range is %p, upper_range is %p.\n", lower_range,
       upper_range);

  typcache = lookup_type_cache(rt_oid, TYPECACHE_RANGE_INFO);
  range_deserialize(typcache, orig_r, *lower_range, *upper_range, is_empty);

  elog(INFO,
       "# DEBUG(C): after range_deserialize, lower_range is %p, upper_range is "
       "%p, is_empty is %d.\n",
       *lower_range, *upper_range, is_empty);
}

void pldotnet_getDatumRangeBoundAttributes(RangeBound *input_range,
                                           Datum *range_datum, bool *infinite,
                                           bool *inclusive, bool *lower) {
  fprintf(stderr,
          "# START DEBUG[before](C:pldotnet_getDatumRangeBoundAttributes)\n");
  fprintf(stderr, "# *range_datum = %p\n", range_datum);
  fprintf(stderr, "# *infinite = %d\n", *infinite);
  fprintf(stderr, "# *inclusive = %d\n", *inclusive);
  fprintf(stderr, "# *lower = %d\n", *lower);
  fprintf(stderr, "# END DEBUG(C:pldotnet_getDatumRangeBoundAttributes)\n");

  *range_datum = input_range->val;
  *infinite = input_range->infinite;
  *inclusive = input_range->inclusive;
  *lower = input_range->lower;

  fprintf(stderr,
          "# START DEBUG[after](C:pldotnet_getDatumRangeBoundAttributes)\n");
  fprintf(stderr, "# *range_datum = %p\n", range_datum);
  fprintf(stderr, "# *infinite = %d\n", *infinite);
  fprintf(stderr, "# *inclusive = %d\n", *inclusive);
  fprintf(stderr, "# *lower = %d\n", *lower);
  fprintf(stderr, "# END DEBUG(C:pldotnet_getDatumRangeBoundAttributes)\n");
}

int get_maxdim(void) { return MAXDIM; }

void pldotnet_getArrayAttributes(void *datum, int *element_typeid, int *ndims,
                                 int *dims, uint8_t **nullmap) {
  /* the size of dims needs to be MAXDIM */
  ArrayType *array = DatumGetArrayTypeP((Datum)datum);
  int *dims_in;
  int i;

  *ndims = ARR_NDIM(array);
  Assert(ndim <= MAXDIM);
  *nullmap = ARR_NULLBITMAP(array);
  *element_typeid = ARR_ELEMTYPE(array);

  dims_in = ARR_DIMS(array);
  for (i = 0; i < *ndims; i++) {
    dims[i] = dims_in[i];
  }
}

int pldotnet_getArrayDatum(Datum array_datum, Datum *results, int nelems,
                           int element_typeid) {
  // converts the PostgreSQL array to a linear array of `Datum`
  // returns 0 on success, other on failure
  ArrayType *array = DatumGetArrayTypeP(array_datum);
  int ndim = ARR_NDIM(array);
  int *dims = ARR_DIMS(array);
  int computed_nelems = 1;
  int i;
  char *dataptr;
  int16 typlen;
  bool typbyval;
  char typalign;

  if (ndim > MAXDIM) {
    elog(22, "# C ERROR: ndimcomputed_nelems(%d) > MAXDIM(%d))\n", ndim,
         MAXDIM);
    return -1;
  }
  for (i = 0; i < ndim; i++) {
    computed_nelems *= dims[i];
  }
  if (computed_nelems != nelems) {
    elog(22, "# C ERROR: (computed_nelems(%d)!=nelems(%d))\n", computed_nelems,
         nelems);
    return -2;
  }

  // this will be cleaned up later into parameters
  get_typlenbyvalalign((Oid)element_typeid, &typlen, &typbyval, &typalign);

  dataptr = ARR_DATA_PTR(array);

  for (i = 0; i < nelems; i++) {
    results[i] = fetch_att(dataptr, typbyval, typlen);
    dataptr = att_addlength_pointer(dataptr, typlen, dataptr);
    dataptr = (char *)att_align_nominal(dataptr, typalign);
  }
  return 0;
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

Datum pldotnet_createDatumText(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  text *new_t = (text *)palloc(new_size);

  SET_VARSIZE(new_t, new_size);
  memcpy((void *)VARDATA(new_t), buf, len);
  PG_RETURN_TEXT_P(new_t);
}

Datum pldotnet_createDatumChar(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  BpChar *new_bpc = (BpChar *)palloc(new_size);

  SET_VARSIZE(new_bpc, new_size);
  memcpy((void *)VARDATA(new_bpc), buf, len);
  PG_RETURN_BPCHAR_P(new_bpc);
}

Datum pldotnet_createDatumVarChar(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  VarChar *new_bpc = (VarChar *)palloc(new_size);

  SET_VARSIZE(new_bpc, new_size);
  memcpy((void *)VARDATA(new_bpc), buf, len);
  PG_RETURN_VARCHAR_P(new_bpc);
}

Datum pldotnet_createDatumBytea(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  bytea *new_b = (bytea *)palloc(new_size);

  SET_VARSIZE(new_b, new_size);
  memcpy((void *)VARDATA(new_b), buf, len);
  PG_RETURN_BYTEA_P(new_b);
}

Datum pldotnet_createDatumXml(int len, char *buf) {
  const size_t new_size = VARHDRSZ + len;
  xmltype *new_x = (xmltype *)palloc(new_size);

  SET_VARSIZE(new_x, new_size);
  memcpy((void *)VARDATA(new_x), buf, len);
  PG_RETURN_XML_P(new_x);
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

Datum pldotnet_createDatumMoney(long value) {
  Datum new_m = CashGetDatum(value);
  return CashGetDatum(new_m);
}

Datum pldotnet_createDatumVarBit(int len, bits8 *bytes) {
  size_t size = VARBITTOTALLEN(len);
  VarBit *new_vb = (VarBit *)palloc(size);
  SET_VARSIZE(new_vb, size);
  new_vb->bit_len = len;
  for (int i = 0; i < len; i++) new_vb->bit_dat[i] = bytes[i];
  return VarBitPGetDatum(new_vb);
}

void pldotnet_typlenbyvalalign(int oid, int16 *typlen, bool *typbyval,
                               char *typalign) {
  // not sure if dotnet can call pg functions through this lib,
  // so faking it for now

  get_typlenbyvalalign(oid, &typlen, &typbyval, &typalign);
}

Datum pldotnet_createDatumArray(int element_id, int dimNumber, int *dimLengths,
                                Datum *datums, bool *nulls) {
  Oid element_type = (Oid)element_id;
  ArrayType *at;
  int16 typlen;
  bool typbyval;
  char typalign;
  get_typlenbyvalalign(element_type, &typlen, &typbyval, &typalign);

  int *lsb = (int *)palloc(sizeof(int) * dimNumber);

  for (int i = 0; i < dimNumber; i++) {
    lsb[i] = 1;
  }

  at = construct_md_array(datums, nulls, dimNumber, dimLengths, lsb,
                          element_type, typlen, typbyval, typalign);

  PG_RETURN_ARRAYTYPE_P(at);
}
