#include <windows.h>
#include <GL/GL.h>
#include <mmsystem.h>

#define PixGL _declspec(dllexport)

extern "C"
{
	float pw, ph, sw, sh, ww, wh;
	
	struct Pixel
	{
		unsigned char r, g, b, a;
	};

	PixGL void SetValues(float pw_, float ph_, float sw_, float sh_, float ww_, float wh_);

	PixGL void RenderUnitPixels(int width, int height, const Pixel* pixels);

	PixGL void RenderPixels(int width, int height, int pixW, int pixH, const Pixel* pixels);

	PixGL void RenderText(int width, int height, const Pixel* pixels);
}