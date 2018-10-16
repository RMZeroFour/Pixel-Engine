using System;

namespace PixelEngine
{
	public static class Noise
	{
		public static float[] Calculate(float[] seed, int octaves, float bias)
		{
			float[] output = new float[seed.Length];

			for (int x = 0; x < seed.Length; x++)
			{
				float noise = 0.0f;
				float scaleAcc = 0.0f;
				float scale = 1.0f;

				for (int o = 0; o < octaves; o++)
				{
					int pitch = seed.Length >> o;
					int sampleA = x / pitch * pitch;
					int sampleB = (sampleA + pitch) % seed.Length;

					float blend = (x - sampleA) / (float)pitch;

					float sample = (1.0f - blend) * seed[sampleA] + blend * seed[sampleB];

					scaleAcc += scale;
					noise += sample * scale;
					scale = scale / bias;
				}

				output[x] = noise / scaleAcc;
			}

			return output;
		}

		public static float[,] Calculate(float[,] seed, int octaves, float bias)
		{
			int width = seed.GetLength(0);
			int height = seed.GetLength(1);

			float[,] output = new float[width, height];

			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					float noise = 0.0f;
					float scaleAcc = 0.0f;
					float scale = 1.0f;

					for (int o = 0; o < octaves; o++)
					{
						int pitch = width >> o;
						int sampleX1 = (x / pitch) * pitch;
						int sampleY1 = (y / pitch) * pitch;

						int sampleX2 = (sampleX1 + pitch) % width;
						int sampleY2 = (sampleY1 + pitch) % width;

						float blendX = (float)(x - sampleX1) / pitch;
						float blendY = (float)(y - sampleY1) / pitch;

						float sampleT = (1.0f - blendX) * seed[sampleX1, sampleY1] + blendX * seed[sampleX2, sampleY1];
						float sampleB = (1.0f - blendX) * seed[sampleX1, sampleY2] + blendX * seed[sampleX2, sampleY2];

						scaleAcc += scale;
						noise += (blendY * (sampleB - sampleT) + sampleT) * scale;
						scale = scale / bias;
					}

					output[x, y] = noise / scaleAcc;
				}
			}

			return output;
		}
	}
}