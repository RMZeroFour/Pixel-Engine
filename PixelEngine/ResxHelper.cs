using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static PixelEngine.Windows;

namespace PixelEngine
{
	internal static class ResxHelper
	{
		private const string DllName = "PixGL.dll";

		private static IntPtr dllHandle;

		public static void LoadDll() => dllHandle = LoadLibrary(LoadFile(DllName));

		public static void LoadFonts()
		{
			LoadFile("Retro.png");
			LoadFile("Modern.png");
			LoadFile("Formal.png");
			LoadFile("Handwritten.png");

			LoadFile("Modern.dat");
			LoadFile("Formal.dat");
			LoadFile("Handwritten.dat");
		}

		private static string LoadFile(string file)
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			string path = Path.Combine(TempPath, file);

			using (Stream stream = assembly.GetManifestResourceStream($"{nameof(PixelEngine)}.Properties.{file}"))
			{
				try
				{
					using (Stream outFile = File.Create(path))
					{
						const int Size = 4096;

						byte[] buffer = new byte[Size];

						while (true)
						{
							int nRead = stream.Read(buffer, 0, Size);

							if (nRead < 1)
								break;

							outFile.Write(buffer, 0, nRead);
						}
					}
				}
				catch { }
			}

			return path;
		}

		public static void DestroyDll() => FreeLibrary(dllHandle);
	}
}