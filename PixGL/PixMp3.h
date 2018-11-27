#include <iostream>
#include <windows.h>
#include <GL/GL.h>

#define PixMp3 _declspec(dllexport)

extern "C"
{
	PixMp3 bool Convert(WCHAR *wszSourceFile, WCHAR *wszTargetFile);
}