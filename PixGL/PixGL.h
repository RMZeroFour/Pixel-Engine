#include <iostream>
#include <windows.h>
#include <GL/GL.h>

#define PixGL _declspec(dllexport)

extern "C"
{
	float pw, ph, ww, wh;

	struct Pixel
	{
		unsigned char r, g, b, a;
	};

	PixGL void SetValues(float pw_, float ph_, float ww_, float wh_);

	PixGL void RenderUnitPixels(int width, int height, const Pixel* pixels);

	PixGL void RenderPixels(int width, int height, const Pixel* pixels);

	PixGL void RenderText(int width, int height, const Pixel* pixels);
}