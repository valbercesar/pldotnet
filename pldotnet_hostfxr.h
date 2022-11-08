/*
 * Work in this file comes from:
 * (https://github.com/dotnet/samples/tree/master/core/hosting/HostWithHostFxr)
 *
 * Copyright and LICENSE is accodring to what is found there:
 *
 * Licensed to the .NET Foundation under one or more agreements.
 * The .NET Foundation licenses this file to you under the MIT license.
 * See the LICENSE file of (https://github.com/dotnet/samples)
 * for more information.
 *
 * pldotnet_hostfxr.h
 *
 */
#ifndef PLDOTNET_HOSTFXR_H_
#define PLDOTNET_HOSTFXR_H_

#include <nethost.h>
#include <stdbool.h>

#if defined(_WIN32)
#define CORECLR_DELEGATE_CALLTYPE __stdcall
#ifdef _WCHAR_T_DEFINED
typedef wchar_t char_t;
#else
typedef unsigned short char_t;
#endif
#else
#define CORECLR_DELEGATE_CALLTYPE
typedef char char_t;
#endif

// Header files copied from https://github.com/dotnet/core-setup
#include "coreclr_delegates.h"
// #include <experimental_coreclr_delegates.h>
#include "hostfxr.h"

typedef load_assembly_and_get_function_pointer_fn dotnet_loader;

typedef int(CORECLR_DELEGATE_CALLTYPE *compile_user_fn)(
    uint32_t functionId, char *func_name, int func_rettype,
    char *func_paramsName, int *func_paramsType, char *func_body,
    bool support_null_input);

typedef int(CORECLR_DELEGATE_CALLTYPE *user_method_delegate)(
    uint32_t functionId, void *arguments, bool *nullmap, void *output);

typedef void *(CORECLR_DELEGATE_CALLTYPE *build_datum_list_t)(void);

typedef void(CORECLR_DELEGATE_CALLTYPE *add_datum_to_list_t)(void *list,
                                                             void *datum);

typedef void(CORECLR_DELEGATE_CALLTYPE *free_generic_gchandle_t)(
    void *gchandle);

/** @brief Loads dotnet using the HostFXR.  HostFXR "finds and resolves
 * the runtime and all the frameworks the app needs", which in our
 * case is via `nethost`, "which is used by native apps (any app
 * which is not .NET Core) to load .NET Core code dynamically"
 *
 * See these URLs for more background:
 * https://github.com/dotnet/runtime/blob/main/docs/design/features/host-components.md
 * https://github.com/dotnet/runtime/blob/main/docs/design/features/sharedfx-lookup.md
 *
 * @return 1 on success, 0 on failure
 */
int pldotnet_LoadHostfxr(void);

/**
 * @brief Load and initialize .NET Core and get desired function pointer for
 * scenario.
 *
 * @param config_path the config path.
 * @param host_base_path the host base path.
 *
 * @return pointer to .NET's function to load an assembly and get the function
 * pointer, or NULL on error.
 */
load_assembly_and_get_function_pointer_fn GetNetLoadAssemblySetup(
    const char_t *config_path, const char_t *host_base_path);

/**
 * @brief Using the nethost library, this function discovers the location of
 * hostfxr and get exports IF the hostfxr was not loaded yet.
 *
 * @return Returns whether the hostfxr was loaded.
 */
bool pldotnet_LoadHostFxrIfNeeded(void);

#endif  // PLDOTNET_HOSTFXR_H_
