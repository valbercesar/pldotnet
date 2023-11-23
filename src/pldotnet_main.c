/*
 * PL/.NET (pldotnet) - PostgreSQL support for .NET C# and F# as
 *             procedural languages (PL)
 *
 *
 * Copyright (c) 2023 Brick Abode
 *
 * This code is subject to the terms of the PostgreSQL License.
 * The full text of the license can be found in the LICENSE file
 * at the top level of the pldotnet repository.
 *
 * pldotnet_main.c
 *
 */

#include "pldotnet_main.h"

#define ASSERT(x)                                                        \
    if (!(x)) {                                                          \
        elog(ERROR, "%s:%d: FAILED ASSERTION! " #x, __FILE__, __LINE__); \
    }
#define ERETURN(...)          \
    elog(ERROR, __VA_ARGS__); \
    return (Datum)0;

/*
 * Exported functions
 */
PG_FUNCTION_INFO_V1(plcsharp_call_handler);
PG_FUNCTION_INFO_V1(plcsharp_inline_handler);
PG_FUNCTION_INFO_V1(plcsharp_validator);
PG_FUNCTION_INFO_V1(plfsharp_call_handler);
PG_FUNCTION_INFO_V1(plfsharp_inline_handler);
PG_FUNCTION_INFO_V1(plfsharp_validator);

/*
 * START: declaring variables
 */

/*
 * Directories where C#/F# projects for user code are built when
 * USE_DOTNETBUILD is defined. Otherwise that is where our C#/F# compiler
 * projects are located. Default for Linux is /var/lib/PlDotNet/
 */
char *root_path = NULL;
char *dnldir = STR(PLDOTNET_ENGINE_DIR);

GHashTable *procedures = nullptr;

dotnet_loader assembly_loader = nullptr;

compile_user_fn compile_user_function = nullptr;
build_datum_list_fn build_datum_list = nullptr;
// Obsolete
add_datum_to_list_fn add_datum_to_list = nullptr;
free_generic_gchandle_fn free_generic_gchandle = nullptr;
unload_assemblies_fn unload_assemblies = nullptr;
run_user_fn run_user_function = nullptr;

pldotnet_PathConfig path_config;

// Give each SRF instance a distinct id
static unsigned long global_srf_id = 0;

/*
 * END: declaring variables
 */

#if 0  // disabled, used for debugging
/*
 * START: declaring static functions
 */

static void dumpUserFunctionDeclaration(
    const pldotnet_UserFunctionDeclaration *declaration);
#endif

/**
 * @brief The main handler function, which receives an additional bool argument
 * to deal with both normal and inline calls.
 *
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param is_inline Wether the function is inline or not.
 * @param language The .NET language (C# or F#).
 * @return The datum that can be stored in a PostgreSQL table.
 */
static Datum pldotnet_generic_handler(FunctionCallInfo fcinfo, bool is_inline,
                                      pldotnet_Language language);

/**
 * @brief The validator function will inspect the function body for syntactical
 * correctness, but it can also look at other properties of the function,
 * for example if the language cannot handle certain argument types.
 * This functions calls the C# function to create and compile the user function
 * into .NET Framework.
 *
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param language The .NET language (C# or F#).
 * @return a void datum.
 */
static Datum pldotnet_validator(FunctionCallInfo fcinfo,
                                pldotnet_Language language);

/**
 * @brief This function starts to building the output paths into a static
 * pldotnet_PathConfig, then it parses the procedure data into a valid source
 * code. This second step produces a valid pldotnet_UserFunctionDeclaration
 * which contains useful information regarding the current function. While
 * building pldotnet_UserFunctionDeclaration, it tries to find a previous cached
 * function aiming to avoid reloading stuff from .NET.
 *
 * Case 1: If there is no previous cached function, it loads .NET, gets the
 * function pointers and calls Engine.Compile() and Engine.Run() to retrieve the
 * desired results. After calling the user function, it saves the current
 * pldotnet_UserFunctionDeclaration into a global hash table called procedures
 * (see pldotnet_main.h).
 *
 *  Case 2: if there is a previous cached function, then it just calls
 * Engine.RunUserFunction()
 * @param fcinfo The standard parameter list for fmgr-compatible functions.
 * @param is_inline Wether the function is inline or not.
 * @param language The .NET language (C# or F#).
 * @return The datum that can be stored in a PostgreSQL table.
 */
static Datum pldotnet_CompileAndRunUserFunction(const FunctionCallInfo fcinfo,
                                                bool is_inline,
                                                pldotnet_Language language);

/**
 * @brief Returns the declared function or creates one.
 *
 * @param oid the function ID.
 * @param fcinfo
 * @param proc
 * @param is_inline whether it is a inline function or not
 * @param validation whether the function should be validated.
 * @param language The .NET language (C# or F#).
 * @return pldotnet_UserFunctionDeclaration* the function created by the user.
 */
static pldotnet_UserFunctionDeclaration *pldotnet_GetFunctionDecl(
    Oid oid, FunctionCallInfo fcinfo, HeapTuple proc, bool is_inline,
    bool validation, pldotnet_Language language);

/**
 * @brief This function tries to build a valid pldotnet_UserFunctionDeclaration.
 * It searchs for information on PG SysCache and it stores the required data
 * into the last argument (pldotnet_UserFunctionDeclaration *function_decl) this
 * structure is meant to be saved into a hash table.
 *
 * @param oid the function ID.
 * @param fcinfo
 * @param proc
 * @param is_inline whether it is a inline function or not.
 * @param validation whether the function should be validated.
 * @param function_decl the struct that contains all the function information
 * @param language the programming language (C# or F#)
 * @return true it it succeeds.
 * @return false otherwise.
 */
static bool pldotnet_BuildFunctionDecl(
    Oid oid, FunctionCallInfo fcinfo, HeapTuple proc, bool is_inline,
    bool validation, pldotnet_UserFunctionDeclaration *function_decl,
    pldotnet_Language language);

/**
 * @brief Set the function information to the pldotnet_GetSourceCode object
 * if the function is not null.
 *
 * @param fcinfo the function information
 * @param proc
 * @param procst
 * @param is_inline whether the functions is inline
 * @param validation whetter the function should be validated
 * @param user_function_decl the struct that stores the function information
 * @param language the programming language (C# or F#)
 * @return true if the process was successful
 * @return false if the source code could not be obtained
 */
static bool pldotnet_GetSourceCode(
    FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst,
    bool is_inline, bool validation,
    pldotnet_UserFunctionDeclaration *user_function_decl,
    pldotnet_Language language);

/**
 * @brief Find and return a specified .NET method.
 *
 * @param library_path The .NET project path.
 * @param dotnet_type The .NET type (the namespace and class).
 * @param dotnet_type_method The .NET function name.
 * @param delegate_type_name The .NET delegate function name.
 * @return void* the C function pointer that points to the specified .NET method
 */
static void *pldotnet_GetDotNetMethod(const char *library_path,
                                      const char *dotnet_type,
                                      const char *dotnet_type_method,
                                      const char *delegate_type_name);

/**
 * @brief Calls the CompileUserFunction function from the .NET environment.
 * Thus, this function will generate and dynamically compile the user code,
 * which is defined through the "declaration" argument.
 *
 * @param declaration The object that contains the user's function information.
 * @return true if the process succeeded.
 * @return false if the process failed.
 */
static bool pldotnet_CompileUserFunction(
    pldotnet_UserFunctionDeclaration *declaration);

/**
 * @brief Saves or replaces the user function in the hash table.
 *
 * @param function the object that contains the user function information
 * @param insert whether the function should be inserted. On the other hand,
 * it will be replaced.
 */
static void pldotnet_SaveFunction(pldotnet_UserFunctionDeclaration *function,
                                  bool insert);

/**
 * @brief Returns the function body as a char*.
 *
 * @param proc
 * @param procst
 * @return const char* the function body.
 */
static const char *pldotnet_GetFunctionBody(HeapTuple proc,
                                            Form_pg_proc procst);

/**
 * @brief Returns the SQL parameter names as a space-separated, nul-terminated
 * const char*.
 *
 * @param proc the HeapTuple object.
 * @param procst the Form_pg_proc object.
 * @return space-separated, nul-terminated `const char*` of the SQL parameters.
 */
static char *pldotnet_GetSqlParamsName(char **names, int input_names);

/**
 * @brief Creates a list of IntPtr using C# methods and then adds the user
 * arguments in this list and returns the created list.
 *
 * @param fcinfo the FunctionCallInfo object.
 * @param procst the Form_pg_proc object.
 * @return void* the List<IntPtr> that contains the Datums.
 */
static void *pldotnet_BuildArgumentList(FunctionCallInfo fcinfo,
                                        Form_pg_proc procst,
                                        int input_arguments, char *modes);

/**
 * @brief Return the Datum of an specific argument.
 *
 * @param fcinfo the function information
 * @param index the argument index
 * @return Datum of the provided index of the arguments list.
 */
static Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo, size_t index);

/**
 * @brief Maps the NULL Datums in the user argument list.
 *
 * @return bool* the pointer to the nullmap of user arguments.
 */
static bool *pldotnet_BuildNullArgumentList(FunctionCallInfo fcinfo,
                                            Form_pg_proc procst);

/**
 * @brief Check that the datum argument is null.
 *
 * @param fcinfo the function information
 * @param index the argument index
 * @return true if the referent argument is null.
 * @return false if the referent argument is non-null.
 */
static bool pldotnet_CheckNullArgument(FunctionCallInfo fcinfo, int index);

/**
 * @brief Retuns the Postgres Heap.
 *
 * @param oid the function ID.
 * @return HeapTuple
 */
static HeapTuple pldotnet_GetPostgresHeapTuple(Oid oid);

/**
 * @brief Calls the ReleaseSysCache function from utils/syscache.h.
 *
 * @param proc the HeapTuple object
 */
static void pldotnet_ReleasePostgresHeapTuple(HeapTuple proc);

/**
 * @brief Allocates <len> bytes in the TopMemoryContext
 *
 * @param len the number of bytes to be allocated
 */
static void *pldotnet_TopAlloc(size_t len);

/**
 * @brief Allocates <len> bytes in the TopMemoryContext returns a copy of the
 * data in it
 *
 * @param data original data to be copied
 * @param len the number of bytes to be allocated
 */
static void *pldotnet_TopAllocCopy(void *data, size_t len);

/**
 * @brief Allocate to the memory a pldotnet_UserFunctionDeclaration
 * struct and return it.
 *
 * @return pldotnet_UserFunctionDeclaration*
 */
static pldotnet_UserFunctionDeclaration *pldotnet_CreateFunctionDecl(void);

/**
 * @brief Finds the pldotnet function declared by the user through its ID.
 *
 * @param function_id the function ID.
 * @return pldotnet_UserFunctionDeclaration*
 */
static pldotnet_UserFunctionDeclaration *pldotnet_FindFunctionDecl(
    int function_id);

/**
 * @brief Resize_result allocates new arrays for values and nulls,
 * discarding the old arrays.
 *
 * @param r the pldotnet_Result object.
 * @param length the number of Datums to return.
 */
void resize_result(struct pldotnet_Result *r, size_t length);

/**
 * @brief Creates an array of pldotnet_Result.
 *
 * @param length the number of datums.
 * @return a pointer to an array of pldotnet_Result.
 */
struct pldotnet_Result *create_result(size_t length);

/*
 * START: implementing functions
 */

void pldotnet_SetResult(pldotnet_Result *output, int offset, Datum value,
                        bool is_null, Oid oid) {
    // elog(INFO, "Called into void pldotnet_SetResult(pldotnet_Result
    // *output=%p, int offset=%d, Datum value=%p, bool is_null=%d, Oid oid=%d)",
    // output, offset, value, is_null, oid);
    // elog(INFO, "Result: length is %d, values is %p, nulls is %p, oids is %p",
    // output->length, output->values, output->nulls, output->oids);
    assert(offset < output->length);
    output->values[offset] = value;
    output->nulls[offset] = is_null;
    output->oids[offset] = oid;
    // elog(INFO, "Done with void pldotnet_SetResult()");
}

void resize_result(struct pldotnet_Result *r, size_t length) {
    // elog(INFO, "Before Result resize: length is %d, values is %p, nulls is
    // %p, oids is %p", r->length, r->values, r->nulls, r->oids);
    if (r->values) pfree(r->values);
    if (r->nulls) pfree(r->nulls);

    r->length = length;
    r->values = palloc((sizeof(Datum) * length));
    r->nulls = palloc((sizeof(bool) * length));
    r->oids = palloc((sizeof(Oid) * length));

    memset(r->values, 0, (sizeof(Datum) * length));
    memset(r->nulls, 0, (sizeof(bool) * length));
    memset(r->oids, 0, (sizeof(Oid) * length));
    // elog(INFO, "After Result resize: length is %d, values is %p, nulls is %p,
    // oids is %p", r->length, r->values, r->nulls, r->oids);
}

struct pldotnet_Result *create_result(size_t length) {
    pldotnet_Result *output = palloc(sizeof(pldotnet_Result));
    memset(output, 0, sizeof(*output));  // safety requirement; `values` and
                                         // `nulls` must be NULL before resizing
    resize_result(output, length);
    // elog(INFO, "New Result: length is %d, values is %p, nulls is %p, oids is
    // %p", output->length, output->values, output->nulls, output->oids);
    return output;
}

Datum plcsharp_call_handler(PG_FUNCTION_ARGS) {
    return pldotnet_generic_handler(fcinfo, false, csharp);
}

Datum plcsharp_inline_handler(PG_FUNCTION_ARGS) {
    Datum result = pldotnet_generic_handler(fcinfo, true, csharp);
    unload_assemblies(fcinfo->flinfo->fn_oid);
    return result;
}

Datum plcsharp_validator(PG_FUNCTION_ARGS) {
    return pldotnet_validator(fcinfo, csharp);
}

Datum plfsharp_call_handler(PG_FUNCTION_ARGS) {
    return pldotnet_generic_handler(fcinfo, false, fsharp);
}

Datum plfsharp_inline_handler(PG_FUNCTION_ARGS) {
    Datum result = pldotnet_generic_handler(fcinfo, true, fsharp);
    unload_assemblies(fcinfo->flinfo->fn_oid);
    return result;
}

Datum plfsharp_validator(PG_FUNCTION_ARGS) {
    return pldotnet_validator(fcinfo, fsharp);
}

bool pldotnet_BuildPaths(void) {
    const char json_path_suffix[] =
        "/bin/Release/net6.0/PlDotNET."
        "runtimeconfig.json";
    const char dll_path_suffix[] = "/bin/Release/net6.0/PlDotNET.dll";

    SNPRINTF(path_config.prefix, MAXPGPATH, "%s", root_path);
    SNPRINTF(path_config.config_path, MAXPGPATH, "%s%s", root_path,
             json_path_suffix);
    SNPRINTF(path_config.library_path, MAXPGPATH, "%s%s", root_path,
             dll_path_suffix);
    return true;
}

bool pldotnet_SetNetLoader(void) {
    if (assembly_loader != nullptr) return true;

    assembly_loader =
        GetNetLoadAssemblySetup(path_config.config_path, path_config.prefix);

    return nullptr != assembly_loader;
}

bool pldotnet_SetDotNetMethods(void) {
    const char *library_path = path_config.library_path;

    compile_user_function = (compile_user_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "CompileUserFunction",
        "PlDotNET.Engine+DelCompileUserFunction, PlDotNET");

    run_user_function = (run_user_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "RunUserFunction",
        "PlDotNET.Engine+DelRunUserFunction, PlDotNET");

    build_datum_list = (build_datum_list_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "BuildDatumList",
        "PlDotNET.Engine+DelBuildDatumList, PlDotNET");

    add_datum_to_list = (add_datum_to_list_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "AddDatumToList",
        "PlDotNET.Engine+DelAddDatumToList, PlDotNET");

    free_generic_gchandle = (free_generic_gchandle_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "FreeGenericGCHandle",
        "PlDotNET.Engine+DelFreeGenericGCHandle, PlDotNET");

    unload_assemblies = (unload_assemblies_fn)pldotnet_GetDotNetMethod(
        library_path, "PlDotNET.Engine, PlDotNET", "UnloadAssemblies",
        "PlDotNET.Engine+DelUnloadAssemblies, PlDotNET");

    return nullptr != compile_user_function && nullptr != run_user_function &&
           nullptr != build_datum_list && nullptr != add_datum_to_list &&
           nullptr != free_generic_gchandle && nullptr != unload_assemblies;
}

void pldotnet_Elog(int level, char *message) { elog(level, "%s", message); }

static Datum pldotnet_generic_handler(FunctionCallInfo fcinfo, bool is_inline,
                                      pldotnet_Language language) {
    MemoryContextWrapper memory_context;
    Datum retval = 0;

    bool nonatomic = fcinfo->context && IsA(fcinfo->context, CallContext) &&
                     !castNode(CallContext, fcinfo->context)->atomic;

    /// Transactions don't work when using SPI_connect()
    /// So we replicated plpython implementation
    if (SPI_connect_ext(nonatomic ? SPI_OPT_NONATOMIC : 0) != SPI_OK_CONNECT)
        elog(ERROR, "SPI_connect failed");

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        retval =
            pldotnet_CompileAndRunUserFunction(fcinfo, is_inline, language);

        /* REVERT PREV MEM CONTEXT */
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    { PG_RE_THROW(); }

    PG_END_TRY();

    if (SPI_finish() != SPI_OK_FINISH) elog(ERROR, "SPI_finish failed");

    return retval;
}

static Datum pldotnet_validator(FunctionCallInfo fcinfo,
                                pldotnet_Language language) {
    MemoryContextWrapper memory_context;
    HeapTuple proc;
    Oid funcoid = PG_GETARG_OID(0);

    if (!check_function_bodies) return (Datum)0;

    PG_TRY();
    {
        /* START NEW MEM CONTEXT */
        pldotnet_StartNewMemoryContext(&memory_context);

        proc = pldotnet_GetPostgresHeapTuple(funcoid);

        pldotnet_GetFunctionDecl(funcoid, fcinfo, proc, false, true, language);

        pldotnet_ReleasePostgresHeapTuple(proc);

        /* REVERT PREV MEM CONTEXT */
        pldotnet_ResetMemoryContext(&memory_context);
    }
    PG_CATCH();
    { PG_RE_THROW(); }
    PG_END_TRY();

    PG_RETURN_VOID();
}

#if 0  // Disabled; used for debugging
static void dumpUserFunctionDeclaration(
    const pldotnet_UserFunctionDeclaration *declaration) {
    // debugging function
    elog(INFO, "UFD_DUMP language: '%s' (%p)", declaration->language,
         (void *)declaration->language);
    elog(INFO, "UFD_DUMP func_name: '%s' (%p)", declaration->func_name,
         (void *)declaration->func_name);
    elog(INFO, "UFD_DUMP func_ret_type: %u", declaration->func_ret_type);
    elog(INFO, "UFD_DUMP func_param_names: '%s' (%p)",
         declaration->func_param_names, (void *)declaration->func_param_names);

    elog(INFO, "UFD_DUMP func_param_types (at %p):",
         (void *)declaration->func_param_types);
    for (int i = 0; i < declaration->num_args; i++) {
        elog(INFO, "UFD_DUMP   - %u", declaration->func_param_types[i]);
    }

    elog(INFO, "UFD_DUMP num_args: %d", declaration->num_args);
    elog(INFO, "UFD_DUMP func_param_modes: %.*s", declaration->num_args,
         declaration->func_param_modes);
    elog(INFO, "UFD_DUMP num_input_args: %d", declaration->num_input_args);
    elog(INFO, "UFD_DUMP num_output_values: %d",
         declaration->num_output_values);
    elog(INFO, "UFD_DUMP func_body: %s", declaration->func_body);
    elog(INFO, "UFD_DUMP func_oid: %u", declaration->func_oid);
    elog(INFO, "UFD_DUMP support_null_input: %s",
         declaration->support_null_input ? "true" : "false");
}
#endif

static void srf_MemoryContextCallback(void *cbdp) {
    // tells dotnet to free the IEnumerator for the SRF from its cache
    int res;
    cb_data *cbd = (cb_data *)cbdp;
    elog(INFO, "Now freeing call %lu on function %u", cbd->call_id,
         cbd->functionId);
    res = run_user_function(cbd->functionId, cbd->call_id, CALL_SRF_CLEANUP,
                            NULL, 0, NULL, NULL);
    if (res != RETURN_SRF_DONE) {
        elog(WARNING,
             "BAD: Got result (%d) from freeing call %u on function %lu", res,
             cbd->functionId, cbd->call_id);
    }
}

static Datum result_to_record(TupleDesc desc, pldotnet_Result *result) {
    HeapTuple tuple;
    Datum output_datum;

    elog(INFO, "Returning COMPOSITE with %d members", desc->natts);

    // confirm that we produced the correct kind of objects
    for (int i = 0; i < desc->natts; i++) {
        Form_pg_attribute attr = TupleDescAttr(desc, i);
        if (result->oids[i] != attr->atttypid) {
            elog(ERROR,
                 "BAD: Pldotnet slot %d: psql OID != %d, pldotnet OID = %d", i,
                 result->oids[i], attr->atttypid);
            return (Datum)0;
        } else {
            elog(INFO,
                 "GOOD: Pldotnet slot %d: psql OID == %d, pldotnet OID = %d", i,
                 result->oids[i], attr->atttypid);
        }
    }

    // create and return the Datum
    tuple = heap_form_tuple(desc, result->values, result->nulls);
    output_datum = heap_copy_tuple_as_datum(tuple, desc);
    heap_freetuple(tuple);
    return output_datum;
}

static Datum set_null_and_return_datum(const FunctionCallInfo fcinfo,
                                       pldotnet_Result *result, int offset) {
    if (result->nulls[offset]) fcinfo->isnull = true;
    return result->values[offset];
}

static Datum pldotnet_CompileAndRunUserFunction(const FunctionCallInfo fcinfo,
                                                bool is_inline,
                                                pldotnet_Language language) {
    HeapTuple proc;
    TupleDesc desc;
    Form_pg_proc procst;
    pldotnet_UserFunctionDeclaration *function_decl = nullptr;
    void *arglist = nullptr;
    bool *nullmap = nullptr;
    pldotnet_Result *output = create_result(0);
    int num_output_values = -1, retval;
    bool retset, is_trigger;
    bool is_null = false;
    Datum result_datum = (Datum)0;
    int result_type = get_call_result_type(fcinfo, NULL, &desc);

    // set up initial data
    proc = pldotnet_GetPostgresHeapTuple(fcinfo->flinfo->fn_oid);
    procst = (Form_pg_proc)GETSTRUCT(proc);
    pldotnet_ReleasePostgresHeapTuple(proc);

    retset = procst->proretset;
    is_trigger = (procst->prorettype == TRIGGEROID);

    function_decl = pldotnet_GetFunctionDecl(fcinfo->flinfo->fn_oid, fcinfo,
                                             proc, is_inline, false, language);

    // Juggle normal versus OUT/INOUT versus RECORD argument counts.
    // Reminder: (num_output_values==0) means it's a normal return;
    // we still need one entry in the pldotnet_Result in that case.
    num_output_values = function_decl->num_output_values;
    if (num_output_values < 0) {
        ERETURN("[pldotnet]: Got negative num_output_values");
    } else if (num_output_values ==
               0) {  // special encoding for normal return type
        num_output_values = 1;
    }

    // size pldotnet_Result properly
    elog(INFO, "Resizing output to %d", num_output_values);
    resize_result(output, num_output_values);

    // Sanity checks
    if (retset && (function_decl->num_output_values > 0)) {
        ERETURN("SRF cannot be combined with OUT/INOUT");
    }
    if (retset && is_trigger) {
        ERETURN("Function cannot be a trigger and return a set.");
    }
    if (is_trigger) {
        ERETURN("Triggers are not supported.");
    }
    if (nullptr == function_decl || nullptr == run_user_function) {
        ERETURN("[pldotnet]: Could not load function_decl");
    }

    // Build prerequisites for calling function: arglist, nullmap, output
    arglist = pldotnet_BuildArgumentList(fcinfo, procst,
                                         function_decl->num_input_args,
                                         function_decl->func_param_modes);
    nullmap = function_decl->support_null_input
                  ? pldotnet_BuildNullArgumentList(fcinfo, procst)
                  : nullptr;

    // Handle Set Returning Function
    if (retset) {
        // This function returns a set, so give it special handling here.
        FuncCallContext *funcctx;

        // only simple functions are allowed to be SRF
        ASSERT(function_decl->num_output_values == 0);

        // The first time we're called, we need to perform setup here and in
        // dotnet
        if (SRF_IS_FIRSTCALL()) {
            cb_data *cbd;

            funcctx = SRF_FIRSTCALL_INIT();
            funcctx->user_fctx =
                (void *)++global_srf_id;  // This tells the UserHandler where to
                                          // find the enumerator in the cache

            // create and register the callback for garbage collection
            cbd = (cb_data *)funcctx->multi_call_memory_ctx->methods->alloc(
                funcctx->multi_call_memory_ctx, sizeof(cb_data));

            cbd->cb_record.arg = cbd;
            cbd->cb_record.func = srf_MemoryContextCallback;
            cbd->cb_record.next = funcctx->multi_call_memory_ctx->reset_cbs;
            cbd->functionId = function_decl->func_oid;
            cbd->call_id = (uint64_t)funcctx->user_fctx;

            funcctx->multi_call_memory_ctx->reset_cbs = &(cbd->cb_record);

            // call the user function with CALL_SRF_FIRST, which performs setup
            retval = run_user_function(
                function_decl->func_oid, (unsigned long)funcctx->user_fctx,
                CALL_SRF_FIRST, arglist, function_decl->num_input_args,
                &nullmap[0], (void *)output);
            if (nullmap) pfree(nullmap);  // no longer needed
            if (arglist) pfree(arglist);  // no longer needed

            if (retval == RETURN_SRF_DONE) {  // that was fast
                elog(WARNING, "SRF finished before it started");
                SRF_RETURN_DONE(funcctx);
            }
        }

        funcctx = SRF_PERCALL_SETUP();

        // call the user function with CALL_SRF_NEXT to get the next value
        retval =
            run_user_function(function_decl->func_oid,
                              (unsigned long)funcctx->user_fctx, CALL_SRF_NEXT,
                              NULL,  // only needed on first call
                              0,     // only needed on first call
                              NULL,  // only needed on first call
                              (void *)output);

        // handle RECORD versus normal
        if (result_type == TYPEFUNC_COMPOSITE) {  // RECORD
            is_null = false;
            result_datum = result_to_record(desc, output);
        } else {
            is_null = output->nulls[0];
            result_datum = output->values[0];
        }

        // Do the correct kind of SRF return
        if (retval == RETURN_SRF_NEXT) {
            if (is_null) SRF_RETURN_NEXT_NULL(funcctx);
            SRF_RETURN_NEXT(funcctx, result_datum);
        } else if (retval == RETURN_SRF_DONE) {
            SRF_RETURN_DONE(funcctx);
        }

        // error handler
        elog(WARNING, "Unrecognized return value from user function: %d",
             retval);
        SRF_RETURN_DONE(funcctx);
    }

    // Not a `retset`, so do a normal call/return
    //
    // the user function knows from compile time how many entries are
    // in `output`, so we don't need to pass it here.
    retval = run_user_function(function_decl->func_oid, 0, CALL_NORMAL, arglist,
                               function_decl->num_input_args, &nullmap[0],
                               (void *)output);
    if (nullmap) pfree(nullmap);  // no longer needed
    if (arglist) pfree(arglist);  // no longer needed

    if (retval != RETURN_NORMAL) {
        ERETURN("Unknown error(return=%d): PL.NET function \"%s\".", retval,
                function_decl->func_name);
    }

    // handle RECORD versus normal
    if (result_type == TYPEFUNC_COMPOSITE) {  // RECORD, or (INOUT/OUT)>1
        // this cannot be null, so no need to check
        // (the error case does `longjmp` via `elog(ERROR, ...)`)
        return result_to_record(desc, output);
    } else if ((function_decl->num_output_values == 0) ||
               (function_decl->num_output_values == 1)) {
        // 0: normal return value; 1: single output-argument value
        return set_null_and_return_datum(fcinfo, output, 0);
    }

    // error handler
    ERETURN("Unrecognized return value type from PL.NET function \"%s\".",
            function_decl->func_name);
}

static pldotnet_UserFunctionDeclaration *pldotnet_GetFunctionDecl(
    Oid oid, FunctionCallInfo fcinfo, HeapTuple proc, bool is_inline,
    bool validation, pldotnet_Language language) {
    pldotnet_UserFunctionDeclaration *decl = pldotnet_FindFunctionDecl(oid);
    bool found = nullptr != decl;

    if (found && !validation) return decl;
    decl = pldotnet_CreateFunctionDecl();
    if (!pldotnet_BuildFunctionDecl(oid, fcinfo, proc, is_inline, validation,
                                    decl, language)) {
        return nullptr;
    }
    if (!is_inline) pldotnet_SaveFunction(decl, !found);
    return decl;
}

static bool pldotnet_BuildFunctionDecl(
    Oid oid, FunctionCallInfo fcinfo, HeapTuple proc, bool is_inline,
    bool validation, pldotnet_UserFunctionDeclaration *function_decl,
    pldotnet_Language language) {
    Form_pg_proc procst = (Form_pg_proc)GETSTRUCT(proc);

    if (nullptr == function_decl)
        elog(ERROR, "[pldotnet]: Invalid argument, function_decl is null");

    /* save some basic data */
    function_decl->func_oid = (uint32_t)oid;
    function_decl->support_null_input = procst->proisstrict ? false : true;
    function_decl->func_ret_type = procst->prorettype;

    if (!pldotnet_GetSourceCode(fcinfo, proc, procst, is_inline, validation,
                                function_decl, language)) {
        elog(ERROR, "[pldotnet]: Could not obtain the source code");
        return false;
    }

    if (!pldotnet_CompileUserFunction(function_decl)) {
        elog(ERROR, "PL.NET function \"%s\".", function_decl->func_name);
        return false;
    }

    return true;
}

static bool pldotnet_GetSourceCode(
    FunctionCallInfo fcinfo, HeapTuple proc, Form_pg_proc procst,
    bool is_inline, bool validation,
    pldotnet_UserFunctionDeclaration *user_function_decl,
    pldotnet_Language language) {
    // Populates the pldotnet_UserFunctionDeclaration
    int i, args_total, args_in = 0, args_out = 0;
    Oid *types, *orig_types;
    char **names;
    char *modes, *orig_modes;
    char *input_names;

    args_total = get_func_arg_info(proc, &orig_types, &names, &orig_modes);
    types =
        pldotnet_TopAllocCopy(orig_types, sizeof(orig_types[0]) * args_total);

    if (!args_total) {
        modes = (char *)nullptr;
        input_names = (char *)nullptr;
    } else {
        // `modes` is already set
        input_names = pldotnet_GetSqlParamsName(names, args_total);
    }

    if (orig_modes == nullptr) {
        // It's annoying to deal with an empty `modes` array, so we make one
        // filled with `IN`
        size_t modes_size;

        modes_size = args_total * sizeof(modes[0]);
        modes = (char *)pldotnet_TopAlloc(modes_size);
        for (i = 0; i < args_total; i++) {
            modes[i] = PROARGMODE_IN;
        }
        args_in = args_total;
        args_out = 0;
    } else {
        modes = pldotnet_TopAllocCopy(orig_modes,
                                      sizeof(orig_modes[0]) * args_total);
        for (i = 0; i < args_total; i++) {
            // First, we check for illegal modes
            if ((modes[i] != PROARGMODE_IN) && (modes[i] != PROARGMODE_INOUT) &&
                (modes[i] != PROARGMODE_OUT)) {
                elog(ERROR, "Illegal argument number %d, '%s', found: type %c",
                     i, names[i], modes[i]);
                return -2;
            }

            // Second, we count the input and output arguments
            if ((modes[i] == PROARGMODE_IN) || (modes[i] == PROARGMODE_INOUT)) {
                args_in += 1;
            }
            if ((modes[i] == PROARGMODE_OUT) ||
                (modes[i] == PROARGMODE_INOUT)) {
                args_out += 1;
            }
        }
    }

    if (user_function_decl == nullptr) {
        elog(ERROR, "[pldotnet]: Invalid argument: user_function_decl is null");
        return false;
    }

    user_function_decl->language = language == csharp ? "csharp" : "fsharp";
    if (is_inline) {
        user_function_decl->func_name = "plcsharp_inline_block";
        user_function_decl->func_ret_type = VOIDOID;
        user_function_decl->func_body =
            ((InlineCodeBlock *)DatumGetPointer(PG_GETARG_DATUM(0)))
                ->source_text;
    } else {
        user_function_decl->func_name = NameStr(procst->proname);
        user_function_decl->func_ret_type = procst->prorettype;
        user_function_decl->func_param_names = input_names;
        user_function_decl->func_param_types = types;
        user_function_decl->func_param_modes = modes;
        // This will be 0 for a normal function; we handle that in
        // pldotnet_CompileUserFunction
        user_function_decl->num_args = args_total;
        user_function_decl->num_input_args = args_in;
        user_function_decl->num_output_values = args_out;
        user_function_decl->func_body = pldotnet_GetFunctionBody(proc, procst);
        user_function_decl->retset = procst->proretset;
    }

    return true;
}

static void *pldotnet_GetDotNetMethod(const char *library_path,
                                      const char *dotnet_type,
                                      const char *dotnet_type_method,
                                      const char *delegate_type_name) {
    int rc;
    void *dotnet_method = nullptr;

    rc = assembly_loader(library_path, dotnet_type, dotnet_type_method,
                         delegate_type_name, nullptr, (void **)&dotnet_method);

    if (nullptr == dotnet_method)
        elog(ERROR, "[pldotnet]: Could not get_function_pointer(%s)",
             delegate_type_name);

    if (0 != rc)
        elog(ERROR,
             "[pldotnet]: Could not "
             "load_assembly_and_get_function_pointer(%s)",
             delegate_type_name);

    return dotnet_method;
}

static HeapTuple pldotnet_GetPostgresHeapTuple(Oid oid) {
    HeapTuple proc = SearchSysCache1(PROCOID, ObjectIdGetDatum(oid));
    if (!HeapTupleIsValid(proc))
        elog(ERROR, "[pldotnet]: Cache lookup failed for function %u", oid);
    return proc;
}

static inline void pldotnet_ReleasePostgresHeapTuple(HeapTuple proc) {
    ReleaseSysCache(proc);
}

static void *pldotnet_TopAlloc(size_t len) {
    void *retval;
    MemoryContext mem = CurrentMemoryContext;

    /* change to top mem context */
    MemoryContextSwitchTo(TopMemoryContext);

    retval = palloc(len);
    if (retval == NULL) elog(ERROR, "Could not allocate %lu bytes.", len);
    memset(retval, 0, len);

    /* revert to previous mem context */
    MemoryContextSwitchTo(mem);

    return retval;
}

static void *pldotnet_TopAllocCopy(void *data, size_t len) {
    void *retval = pldotnet_TopAlloc(len);
    memcpy(retval, data, len);
    return retval;
}

static pldotnet_UserFunctionDeclaration *pldotnet_CreateFunctionDecl(void) {
    return pldotnet_TopAlloc(sizeof(pldotnet_UserFunctionDeclaration));
}

pldotnet_UserFunctionDeclaration *pldotnet_FindFunctionDecl(int function_id) {
    gpointer value =
        g_hash_table_lookup(procedures, GUINT_TO_POINTER(function_id));

    if (nullptr != value) return (pldotnet_UserFunctionDeclaration *)value;

    return nullptr;
}

void pldotnet_StartNewMemoryContext(MemoryContextWrapper *config) {
    config->prev = CurrentMemoryContext;
    config->curr = AllocSetContextCreate(
        TopMemoryContext, "PL/NET func_exec_ctx", ALLOCSET_SMALL_SIZES);

    if (nullptr == config->curr)
        elog(ERROR, "Could not create a new memory context");

    MemoryContextSwitchTo(config->curr);
}

void pldotnet_ResetMemoryContext(MemoryContextWrapper *config) {
    if (nullptr == config) return;

    if (config->prev) MemoryContextSwitchTo(config->prev);

    if (config->curr) MemoryContextDelete(config->curr);
}

static bool pldotnet_CompileUserFunction(
    pldotnet_UserFunctionDeclaration *declaration) {
    // Our C# code understands "num_output_values == 0"
    int num_output_values = (int)declaration->num_output_values;
    int should_be_zero;

    should_be_zero = compile_user_function(
        (Oid)declaration->func_oid, (void *)declaration->func_name,
        (Oid)declaration->func_ret_type, declaration->retset,
        (void *)declaration->func_param_names,
        (Oid *)declaration->func_param_types,
        (char *)declaration->func_param_modes, num_output_values,
        (void *)declaration->func_body, declaration->support_null_input,
        (void *)declaration->language);

    return 0 == should_be_zero;
}

static void pldotnet_SaveFunction(pldotnet_UserFunctionDeclaration *function,
                                  bool insert) {
    if (insert)
        g_hash_table_insert(procedures, GUINT_TO_POINTER(function->func_oid),
                            (gpointer)function);
    else
        g_hash_table_replace(procedures, GUINT_TO_POINTER(function->func_oid),
                             (gpointer)function);
}

static const char *pldotnet_GetFunctionBody(HeapTuple proc,
                                            Form_pg_proc procst) {
    bool isnull = false;
    Datum prosrc = SysCacheGetAttr(PROCOID, proc, Anum_pg_proc_prosrc, &isnull);
    if (isnull) elog(ERROR, "null prosrc");
    return TextDatumGetCString(prosrc);
}

// Makes a pldotnet-style array of IN/INOUT parameter names
static char *pldotnet_GetSqlParamsName(char **names, int input_names) {
    char *sql_params = nullptr;
    const char *space = " ";
    size_t buffer_size = 0;

    if (!names) return "";
    if (*names == 0) return "";

    // elog(INFO, "pldotnet_GetSqlParamsName, got names %p, num %d", names,
    // input_names);

    for (int i = 0; i < input_names; i++) {
        buffer_size += strlen(names[i]);
    }

    buffer_size += ((strlen(space) * input_names) + 1);
    sql_params = (char *)pldotnet_TopAlloc(buffer_size);

    for (int i = 0, pos = 0; i < input_names; i++) {
        /* copy the arg name */
        strcat(sql_params, names[i]);
        if (pos < input_names - 1) strcat(sql_params, space);
        pos++;
    }
    sql_params[buffer_size] = 0;  // NUL-terminate the string as a courtesy

    return sql_params;
}

static void *pldotnet_BuildArgumentList(FunctionCallInfo fcinfo,
                                        Form_pg_proc procst, int num_input_args,
                                        char *modes) {
    // Returns an array of Datum, with the values of the input arguments
    // We could just pass `fcinfo->args` to pldotnet, but then it'd have to
    // handle the `PG_VERSION_NUM >= 120000` change
    Datum *arglist;
    int i;

    arglist = palloc(sizeof(Datum) * num_input_args);
    for (i = 0; i < num_input_args; ++i)
        arglist[i] = pldotnet_GetArgDatum(fcinfo, i);

    return (void *)arglist;
}

static inline Datum pldotnet_GetArgDatum(FunctionCallInfo fcinfo,
                                         size_t index) {
#if PG_VERSION_NUM >= 120000
    Datum retval = fcinfo->args[index].value;
#else
    Datum retval = fcinfo->arg[index];
#endif
    return retval;
}

static bool *pldotnet_BuildNullArgumentList(FunctionCallInfo fcinfo,
                                            Form_pg_proc procst) {
    // procst->pronargs actually seems right here, per plpythong
    int nargums = procst->pronargs;
    bool *isnull = (bool *)palloc(sizeof(bool) * nargums);

    for (int i = 0; i < nargums; ++i) {
        isnull[i] = pldotnet_CheckNullArgument(fcinfo, i);
    }
    return isnull;
}

static inline bool pldotnet_CheckNullArgument(FunctionCallInfo fcinfo,
                                              int index) {
#if PG_VERSION_NUM >= 120000
    return fcinfo->args[index].isnull;
#else
    return fcinfo->argnull[index];
#endif
}

char *pldotnet_GetPostgreSqlVersion() { return PACKAGE_VERSION; }

/*
 * END: implementing functions
 */
