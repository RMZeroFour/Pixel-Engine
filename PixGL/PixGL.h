#include <execution>
#include <vector>
#include <iterator>
#include <algorithm>
#include <windows.h>
#include <GL/GL.h>

#define PixGL _declspec(dllexport)

extern "C"
{
	struct Pixel { unsigned char r, g, b, a; };
	struct Point { float x, y; int index; };

	std::vector<std::pair<Point, Point>> coords;
	std::vector<Point> unitCoords;

	float pw, ph, ww, wh;

	PixGL void SetValues(float pw_, float ph_, float ww_, float wh_);
	
	PixGL void CreateCoords(int pixW, int pixH, int scrW, int scrH);

	PixGL void DestroyCoords();

	PixGL void RenderUnitPixels(int width, int height, const Pixel* pixels);

	PixGL void RenderPixels(int width, int height, const Pixel* pixels);

	PixGL void RenderText(int scrW, int scrH, int width, int height, const Pixel* pixels);
}