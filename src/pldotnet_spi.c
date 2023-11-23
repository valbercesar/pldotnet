/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *             procedural languages (PL)
 *
 *
 * Copyright 2023 Brick Abode
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
 * pldotnet_spi.c
 *
 */

#include "pldotnet_spi.h"

bool is_spi_open = false;

const char *pldotnet_ErrorSeverity(int elevel) {
    const char *prefix;

    switch (elevel) {
    case DEBUG1:
    case DEBUG2:
    case DEBUG3:
    case DEBUG4:
    case DEBUG5:
        prefix = gettext_noop("DEBUG");
        break;
    case LOG:
    case LOG_SERVER_ONLY:
        prefix = gettext_noop("LOG");
        break;
    case INFO:
        prefix = gettext_noop("INFO");
        break;
    case NOTICE:
        prefix = gettext_noop("NOTICE");
        break;
    case WARNING:
    #if PG_VERSION_NUM >= 140000
    case WARNING_CLIENT_ONLY:
    #endif
        prefix = gettext_noop("WARNING");
        break;
    case ERROR:
        prefix = gettext_noop("ERROR");
        break;
    case FATAL:
        prefix = gettext_noop("FATAL");
        break;
    case PANIC:
        prefix = gettext_noop("PANIC");
        break;
    default:
        prefix = "???";
        break;
    }
    return prefix;
}

SPITupleTable *pldotnet_SPIExecute(char *cmd,
                                   bool read_only,
                                   long limit,
                                   ErrorData **errorData) {
    MemoryContextWrapper memory_context;
    int rv;

    elog(INFO,
         "Enter pldotnet_SPIExecute: %s, %s, %ld",
         cmd,
         read_only ? "true" : "false",
         limit);

    PG_TRY();
    {
        pldotnet_StartNewMemoryContext(&memory_context);

        rv = SPI_execute(cmd, read_only, limit);
        elog(INFO, "Exit pldotnet_SPIExecute rv: %d....", rv);
        elog(INFO, "Exit pldotnet_SPIExecute SPI_result: %d....", SPI_result);

        pldotnet_ResetMemoryContext(&memory_context);
        elog(INFO, "TUPTABLE INFO: %p", SPI_tuptable);
    }
    PG_CATCH();
    {
        elog(WARNING, "Exception: pldotnet_SPIExecute");
        pldotnet_ResetMemoryContext(&memory_context);
        *errorData = CopyErrorData();
        FlushErrorState();
    }
    PG_END_TRY();

    return SPI_tuptable;
}

SPITupleTable *pldotnet_SPIExecutePlan(SPIPlanPtr plan,
                                       Datum *paramValues,
                                       const char *nullmap,
                                       bool read_only,
                                       long limit,
                                       ErrorData **errorData) {
    MemoryContextWrapper memory_context;
    int rv;

    elog(INFO,
         "Enter pldotnet_SPIExecutePlan: %p, %s, %ld",
         plan,
         read_only ? "true" : "false",
         limit);

    PG_TRY();
    {
        pldotnet_StartNewMemoryContext(&memory_context);

        rv = SPI_execute_plan(plan, paramValues, nullmap, read_only, limit);
        elog(INFO, "Exit pldotnet_SPIExecute rv: %d....", rv);
        elog(INFO, "Exit pldotnet_SPIExecute SPI_result: %d....", SPI_result);

        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "Exception: pldotnet_SPIExecute");
        pldotnet_ResetMemoryContext(&memory_context);
        *errorData = CopyErrorData();
        FlushErrorState();
    }
    PG_END_TRY();

    return SPI_tuptable;
    return SPI_tuptable;
}

void pldotnet_SPICommit(ErrorData **errorData) {
    MemoryContextWrapper memory_context;

    elog(INFO, "Calling pldotnet_SPICommit");

    PG_TRY();
    {
        pldotnet_StartNewMemoryContext(&memory_context);
        SPI_commit();
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "Exception: pldotnet_SPICommit");
        pldotnet_ResetMemoryContext(&memory_context);
        *errorData = CopyErrorData();
        FlushErrorState();
    }
    PG_END_TRY();
}

void pldotnet_SPIRollback(ErrorData **errorData) {
    MemoryContextWrapper memory_context;

    elog(INFO, "Calling pldotnet_SPIRollback");

    PG_TRY();
    {
        pldotnet_StartNewMemoryContext(&memory_context);
        SPI_rollback();
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "Exception: pldotnet_SPIRollback");
        pldotnet_ResetMemoryContext(&memory_context);
        *errorData = CopyErrorData();
        FlushErrorState();
    }
    PG_END_TRY();
}

int pldotnet_GetTableColumnNumber(SPITupleTable *tupleTable) {
    return tupleTable != NULL ? (int)tupleTable->tupdesc->natts : 0;
}

void pldotnet_GetColProps(int *columnTypes,
                          char **columnNames,
                          SPITupleTable *tupleTable) {
    int ncols = (int)tupleTable->tupdesc->natts;
    Form_pg_attribute attr;

    elog(INFO, "Enter pldotnet_GetColProps: %p, %p", columnTypes, columnNames);

    for (int i = 0; i < ncols; i++) {
        attr = TupleDescAttr(tupleTable->tupdesc, i);
        columnTypes[i] = attr->atttypid;
        columnNames[i] = NameStr(attr->attname);
    }
}

void pldotnet_GetRow(int row,
                     Datum *datums,
                     bool *isNull,
                     SPITupleTable *tupleTable) {
    int ncols = (int)tupleTable->tupdesc->natts;
    Form_pg_attribute attr;

    elog(INFO, "Enter pldotnet_GetRow: %i | %p", row, datums);

    for (int i = 0; i < ncols; i++) {
        attr = TupleDescAttr(tupleTable->tupdesc, i);
        datums[i] = heap_getattr(tupleTable->vals[row],
                                 attr->attnum,
                                 tupleTable->tupdesc,
                                 &isNull[i]);
    }
}

void pldotnet_SPIPrepare(SPIPlanPtr *cmdPointer,
                         char *command,
                         int nargs,
                         Oid *argTypes,
                         ErrorData **errorData) {
    MemoryContextWrapper memory_context;
    elog(INFO, "Enter pldotnet_SPIPrepare: %s, %p", command, cmdPointer);
    elog(INFO, "Enter SPI_result: %d....", SPI_result);

    PG_TRY();
    {
        pldotnet_StartNewMemoryContext(&memory_context);
        *cmdPointer = SPI_prepare(command, nargs, argTypes);
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    {
        elog(WARNING, "Exception: pldotnet_SPIPrepare");
        pldotnet_ResetMemoryContext(&memory_context);
        *errorData = CopyErrorData();
        FlushErrorState();
    }
    PG_END_TRY();

    elog(INFO, "Exit SPI_result: %d....", SPI_result);
    elog(INFO, "Exit pldotnet_SPIPrepare: %s, %p", command, cmdPointer);
}

void pldotnet_FreeErrorData(ErrorData *errorData) {
    elog(INFO, "Calling pldotnet_Free_error_data");
    FreeErrorData(errorData);
    elog(INFO, "Exit pldotnet_Free_error_data");
}

uint64 pldotnet_GetProcessedRowsNumber(void) {
    return SPI_processed;
}
