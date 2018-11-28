using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using static PixelEngine.Windows;

namespace PixelEngine
{
	internal static class DllHelper
	{
		private const string DllName = "PixGL.dll";

		private static IntPtr dllHandle;

		public static void LoadDll()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();

			string dllPath = Path.Combine(TempPath, DllName);

			using (Stream stream = assembly.GetManifestResourceStream($"{nameof(PixelEngine)}.Properties.{DllName}"))
			{
				try
				{
					using (Stream outFile = File.Create(dllPath))
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

			dllHandle = LoadLibrary(dllPath);
		}

		public static void DestroyDll() => FreeLibrary(dllHandle);
	}
}