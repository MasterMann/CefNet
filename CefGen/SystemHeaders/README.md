Minimal C system headers for parsing the CEF C API.

CefGen parses with `-nostdinc` and these headers instead of the host's C library or Windows SDK, so
generation needs no compiler or SDK installation and gives the same result on every host. They
declare only the names the CEF headers use; the generator maps those names to .NET types, so the
underlying types only need to be valid C.
