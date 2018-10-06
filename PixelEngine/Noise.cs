using System;

namespace PixelEngine
{
	public static class Noise
	{
		public static float[] Perlin1D(int count, float[] seed, int octaves, float bias)
		{
			float[] output = new float[count];

			for (int x = 0; x < count; x++)
			{
				float noise = 0.0f;
				float scaleAcc = 0.0f;
				float scale = 1.0f;

				for (int o = 0; o < octaves; o++)
				{
					int pitch = count >> o;
					int sampleA = x / pitch * pitch;
					int sampleB = (sampleA + pitch) % count;

					float fBlend = (x - sampleA) / (float)pitch;

					float fSample = (1.0f - fBlend) * seed[sampleA] + fBlend * seed[sampleB];

					scaleAcc += scale;
					noise += fSample * scale;
					scale = scale / bias;
				}

				output[x] = noise / scaleAcc;
			}

			return output;
		}

		public static float[,] Perlin2D(int width, int height, float[] seed, int octaves, float bias)
		{
			float[,] output = new float[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float fNoise = 0.0f;
					float fScaleAcc = 0.0f;
					float fScale = 1.0f;

					for (int o = 0; o < octaves; o++)
					{
						int nPitch = width >> o;
						int nSampleX1 = (x / nPitch) * nPitch;
						int nSampleY1 = (y / nPitch) * nPitch;

						int nSampleX2 = (nSampleX1 + nPitch) % width;
						int nSampleY2 = (nSampleY1 + nPitch) % width;

						float fBlendX = (float)(x - nSampleX1) / nPitch;
						float fBlendY = (float)(y - nSampleY1) / nPitch;

						float fSampleT = (1.0f - fBlendX) * seed[nSampleY1 * width + nSampleX1] + fBlendX * seed[nSampleY1 * width + nSampleX2];
						float fSampleB = (1.0f - fBlendX) * seed[nSampleY2 * width + nSampleX1] + fBlendX * seed[nSampleY2 * width + nSampleX2];

						fScaleAcc += fScale;
						fNoise += (fBlendY * (fSampleB - fSampleT) + fSampleT) * fScale;
						fScale = fScale / bias;
					}

					output[x, y] = fNoise / fScaleAcc;
				}
			}

			return output;
		}
	}
}