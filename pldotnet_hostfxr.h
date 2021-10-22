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
#ifndef PLNETHOST_H
#define PLNETHOST_H

#include <nethost.h>
/* Header files copied from https://github.com/dotnet/core-setup */
#include <coreclr_delegates.h>
#include <hostfxr.h>

typedef load_assembly_and_get_function_pointer_fn dotnet_loader;

int pldotnet_LoadHostfxr(void);
load_assembly_and_get_function_pointer_fn GetNetLoadAssembly(const char_t *assembly);
load_assembly_and_get_function_pointer_fn GetNetLoadAssemblySetup(const char_t *config_path, const char_t *host_base_path);
/* loaded host placeholder variable */
extern void *nethost_lib;

bool pldotnet_LoadHostFxrIfNeeded(void);

#endif  /* PLNETHOST_H */
