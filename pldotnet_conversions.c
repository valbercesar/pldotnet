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
  bool value = DatumGetBool(datum);
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
  LSEG *orig_l = DatumGetLsegP(datum);
  *x1 = orig_l->p[0].x;
  *y1 = orig_l->p[0].y;
  *x2 = orig_l->p[1].x;
  *y2 = orig_l->p[1].y;
}

void pldotnet_getDatumBoxAttributes(void *datum, double *x1, double *y1,
                                    double *x2, double *y2) {
  BOX *orig_b = DatumGetBoxP(datum);
  *x1 = orig_b->high.x;
  *y1 = orig_b->high.y;
  *x2 = orig_b->low.x;
  *y2 = orig_b->low.y;
}

void pldotnet_getDatumTextAttributes(void *datum, int* len, char** buf) {
  text     *t = DatumGetTextPP(datum);
  const size_t datum_len = VARSIZE_ANY_EXHDR(t);

  *len = datum_len;
  *buf = VARDATA_ANY(t);
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

Datum pldotnet_createDatumText(int len, char* buf) {
  const size_t new_size = VARHDRSZ + len;
  text* new_t = (text*)palloc(new_size);

  SET_VARSIZE(new_t, new_size);
  memcpy((void *) VARDATA(new_t), buf, len);
  PG_RETURN_TEXT_P(new_t);
}
