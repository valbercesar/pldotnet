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

int16_t pldotnet_getInt16(void* datum) {
  int16_t value = DatumGetInt16((Datum)datum);
  return value;
}

int32_t pldotnet_getInt32(void* datum) {
  int32_t value = DatumGetInt32((Datum)datum);
  return value;
}

int64_t pldotnet_getInt64(void* datum) {
  int64_t value = DatumGetInt64((Datum)datum);
  return value;
}

float pldotnet_getFloat(void* datum) {
  float value = DatumGetFloat4((Datum)datum);
  return value;
}

double pldotnet_getDouble(void* datum) {
  double value = DatumGetFloat8((Datum)datum);
  return value;
}

bool pldotnet_getBoolean(void* datum) {
  bool value = DatumGetBool(datum);
  return value;
}

void pldotnet_getDatumPointAttributes(void* datum, double *x,  double *y) {
  Point* orig_p = DatumGetPointP((Datum)datum);
  *x = orig_p->x;
  *y = orig_p->y;
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
  Point  *new_p = (Point *) palloc(sizeof(Point));
  new_p->x = x;
  new_p->y = y;
  return PointerGetDatum(new_p);
}
