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
 * coreclr_delegates.h
 *
 */
#ifndef CORECLR_DELEGATES_H_
#define CORECLR_DELEGATES_H_

#include <stdint.h>

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

/* Signature of delegate returned
 * by coreclr_delegate_type::load_assembly_and_get_function_pointer
 *
 * assembly_path:      Fully qualified path to assembly
 * type_name:          Assembly qualified type name
 * method_name:        Public static method name compatible with delegateType
 * delegate_type_name: Assembly qualified delegate type name or null
 * reserved:           Extensibility parameter (currently unused and must be 0
 * delegate:           Pointer where to store the function pointer result
 */
typedef int
(CORECLR_DELEGATE_CALLTYPE *load_assembly_and_get_function_pointer_fn)(
    const char_t *assembly_path,
    const char_t *type_name,
    const char_t *method_name,
    const char_t *delegate_type_name,
    void         *reserved,
    /*out*/ void **delegate);

/* Signature of delegate returned
 * by load_assembly_and_get_function_pointer_fn
 * when delegate_type_name == null (default)
 */
typedef int
(CORECLR_DELEGATE_CALLTYPE *component_entry_point_fn)(
    void *arg,
    int32_t arg_size_in_bytes);

#endif /* CORECLR_DELEGATES_H_ */
