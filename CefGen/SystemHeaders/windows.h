#ifndef CEFGEN_WINDOWS_H
#define CEFGEN_WINDOWS_H
typedef unsigned long DWORD;
typedef int BOOL;
typedef unsigned int UINT;
typedef void* HANDLE;
typedef struct HWND__* HWND;
typedef struct HINSTANCE__* HINSTANCE;
typedef struct HMENU__* HMENU;
typedef struct HICON__* HICON;
typedef HICON HCURSOR;
typedef unsigned long long WPARAM;
typedef long long LPARAM;
typedef struct tagPOINT { long x; long y; } POINT;
typedef struct tagMSG {
  HWND hwnd;
  UINT message;
  WPARAM wParam;
  LPARAM lParam;
  DWORD time;
  POINT pt;
} MSG;
#define CW_USEDEFAULT ((int)0x80000000)
#endif
