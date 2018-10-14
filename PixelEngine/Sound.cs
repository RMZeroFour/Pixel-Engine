using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static PixelEngine.Windows;

namespace PixelEngine
{
	public class Sound
	{
		public bool Loop { get; set; }

		public bool Playing { get; private set; }
		public string FileName { get; private set; }

		private int volume = 500;
		public int Volume
		{
			get => volume / 100;
			set => volume = Math.Min(Math.Max(value, 0), 100) * 100;
		}

		private int id;
		private static int idGen;

		private string Alias => "Sound" + id;

		public Sound(string fileName)
		{
			id = idGen++;
			FileName = new FileInfo(fileName).FullName;
		}

		private void PlayWorker()
		{
			StringBuilder sb = new StringBuilder();
			int result = MciSendString("open \"" + FileName + "\" type waveaudio  alias " + Alias, sb, 0, IntPtr.Zero);
			MciSendString("play Sound" + this.id, sb, 0, IntPtr.Zero);
			Playing = true;

			sb = new StringBuilder();
			MciSendString("status " + Alias + " length", sb, 255, IntPtr.Zero);
			int length = Convert.ToInt32(sb.ToString());

			int pos = 0;
			int oldvol = volume;

			while (Playing)
			{
				sb = new StringBuilder();
				MciSendString("status " + Alias + " position", sb, 255, IntPtr.Zero);
				pos = Convert.ToInt32(sb.ToString());
				if (pos >= length)
				{
					if (!Loop)
					{
						Playing = false;
						break;
					}
					else
					{
						MciSendString("play " + Alias + " from 0", sb, 0, IntPtr.Zero);
					}
				}

				if (oldvol != volume)
				{
					sb = new StringBuilder("................................................................................................................................");
					string cmd = "setaudio " + Alias + " volume to " + volume.ToString();
					long err = MciSendString(cmd, sb, sb.Length, IntPtr.Zero);
					oldvol = volume;
				}
			}
			MciSendString("stop " + Alias, sb, 0, IntPtr.Zero);
			MciSendString("close " + Alias, sb, 0, IntPtr.Zero);
		}

		public void Play()
		{
			try
			{
				if (Playing || !File.Exists(FileName))
					return;

				new Thread(this.PlayWorker).Start();
			}
			catch (Exception) { }
		}
		public void Stop() => Playing = false;
	}
}