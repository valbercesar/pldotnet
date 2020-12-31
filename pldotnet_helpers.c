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