#include "pldotnet_helpers.h"

char*
pldotnet_ToLowerCase(const char* str)
{
    size_t i;
    size_t length = strlen(str);
    char *copy = (char*) palloc(length);
    for (i = 0; i < length; ++i)
        copy[i] = (char) tolower(str[i]);
    return copy;
}

/*
 *
 * This function receives the function name and body.
 * If the function name is found inside body, then it returns true
 * meaning it's a recursive function. Otherwise, it returns false.
 * The main purpose of this function (although) is to fix the function name
 * inside body. This way we guarantee that recursion works after POSTGRES
 * changes in the function name: it is transformed to lowercase there.
 * 
 */
bool
pldotnet_FixFunctionName(const char *function_name, char *function_body)
{
    size_t i, pos;
    const size_t length = strlen(function_name);
    char *copy = pldotnet_ToLowerCase(function_body);
    char *cursor = copy;
    bool is_recursive = false;

    while (nullptr != (cursor = strstr(cursor, function_name)))
    {
        is_recursive = true;
        pos = cursor - copy;
        for (i = pos; i < pos + length; ++i)
            function_body[i] = copy[i];
        if (length < strlen(cursor))
            cursor += length;
    }

    return is_recursive;
}