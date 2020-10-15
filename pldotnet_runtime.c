#include "pldotnet_runtime.h"

static Datum
pldotnet_Run(
    dotnet_loader loader,
    const char *dotnet_type, 
    const char *dotnet_type_method, 
    const pldotnet_PathConfig *paths,
    int8_t *libargs,
    size_t args_length)
{
    Datum retval;
    int rc;
    component_entry_point_fn dotnet_method = nullptr;

    /* Function pointer to managed delegate */
    rc = loader(
        paths->library_path,
        dotnet_type,
        dotnet_type_method,
        nullptr,
        nullptr,
        (void**) &dotnet_method
    );

    assert(rc == 0 && dotnet_method != nullptr && \
        "Failure: load_assembly_and_get_function_pointer()");

    retval = (Datum) dotnet_method(libargs, args_length);

    return  retval;
}

Datum 
pldotnet_CompileUserFunction(
    dotnet_loader loader,
    const FunctionCallInfo fcinfo,
    const pldotnet_PathConfig *paths,
    pldotnet_ArgsSource *source
)
{
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Compile";

    if (nullptr == loader) 
        return (Datum) 0;

    if (!pldotnet_ValidPaths(paths))
        return (Datum) 0;

    return pldotnet_Run(
        loader,
        dotnet_type,
        dotnet_type_method,
        paths,
        (int8_t*) source,
        sizeof(pldotnet_ArgsSource)
    );
}

Datum 
pldotnet_RunUserFunction(
    dotnet_loader loader,
    const pldotnet_PathConfig *paths,
    int8_t *libargs, 
    size_t args_length)
{
    char dotnet_type[] = "PlDotNET.Engine, PlDotNET";
    char dotnet_type_method[64] = "Run";

    if (nullptr == loader) 
        return (Datum) 0;
    
    if (!pldotnet_ValidPaths(paths))
        return (Datum) 0;

    if (nullptr != libargs)
    {
        return pldotnet_Run(
            loader,
            dotnet_type, 
            dotnet_type_method, 
            paths,
            libargs,
            args_length
        );
    }
    
    return pldotnet_Run(
        loader,
        dotnet_type, 
        dotnet_type_method, 
        paths,
        nullptr,
        0
    );
}