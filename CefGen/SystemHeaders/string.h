#ifndef CEFGEN_STRING_H
#define CEFGEN_STRING_H
#include <stddef.h>
void* memcpy(void* dest, const void* src, size_t count);
void* memset(void* dest, int value, size_t count);
size_t strlen(const char* str);
#endif
