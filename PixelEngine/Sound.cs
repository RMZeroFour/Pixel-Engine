using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static PixelEngine.Windows;

namespace PixelEngine
{
	public class Sound
	{
		internal Sound(string file)
		{
			using (BinaryReader reader = new BinaryReader(File.OpenRead(file)))
			{
				const string Riff = "RIFF";
				const string Wave = "WAVE";
				const string Fmt = "fmt ";

				char[] dump;

				dump = reader.ReadChars(4); // RIFF"
				if (string.Compare(string.Concat(dump), Riff) != 0)
					return;

				dump = reader.ReadChars(4); // Ignore

				dump = reader.ReadChars(4); // "WAVE"
				if (string.Compare(string.Concat(dump), Wave) != 0)
					return;

				dump = reader.ReadChars(4); // "fmt "
				if (string.Compare(string.Concat(dump), Fmt) != 0)
					return;

				dump = reader.ReadChars(4); // Ignore

				WavHeader = new WaveFormatEx()
				{
					FormatTag = reader.ReadInt16(),
					Channels = reader.ReadInt16(),
					SamplesPerSec = reader.ReadInt32(),
					AvgBytesPerSec = reader.ReadInt32(),
					BlockAlign = reader.ReadInt16(),
					BitsPerSample = reader.ReadInt16()
				};
				WavHeader.Size = (short)Marshal.SizeOf(WavHeader);
				
				if (WavHeader.SamplesPerSec != 44100)
					return;

				const string Data = "data";

				dump = reader.ReadChars(4); // Chunk header
				long chunkSize = reader.ReadUInt32();
				while (string.Compare(string.Concat(dump), Data) != 0)
				{
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					dump = reader.ReadChars(4);
					chunkSize = reader.ReadUInt32();
				}
				
				SampleCount = chunkSize / (WavHeader.Channels * (WavHeader.BitsPerSample >> 3));
				Channels = WavHeader.Channels;

				Samples = new float[SampleCount * Channels];

				int index = 0;
				for (long l = 0; l < SampleCount; l++)
				{
					for (int c = 0; c < Channels; c++)
					{
						// Divide by 8 to convert to bits to bytes 
						// as sizeof returns bytes

						switch (WavHeader.BitsPerSample / 8)
						{
							case sizeof(byte): // 8-bits
								byte b = reader.ReadByte();
								Samples[index] = (float)b / byte.MaxValue;
								index++;
								break;

							case sizeof(short): // 16-bits
								short s = reader.ReadInt16();
								Samples[index] = (float)s / short.MaxValue;
								index++;
								break;

							case sizeof(int): // 32-bits
								int i = reader.ReadInt32();
								Samples[index] = (float)i / int.MaxValue;
								index++;
								break;
						}
					}
				}
			}

			Valid = true;
		}

		public bool Loop { get; set; }

		internal WaveFormatEx WavHeader;
		internal float[] Samples = null;
		internal long SampleCount = 0;
		internal int Channels = 0;
		internal bool Valid = false;
	}

	internal struct PlayingSample
	{
		public Sound AudioSample { get; set; }
		public long SamplePosition { get; set; }
		public bool Finished { get; set; }
		public bool Loop { get; set; }
	}

	internal class AudioEngine
	{
		public Func<int, float, float, float> OnSoundCreate { get; set; }
		public Func<int, float, float, float> OnSoundFilter { get; set; }

		public bool Active { get; private set; }

		public float GlobalTime { get; private set; }

		internal float Volume = 1;

		private List<Sound> samples;
		private List<PlayingSample> playingSamples;

		private static WaveDelegate waveProc;

		private uint sampleRate;
		private uint channels;
		private uint blockCount;
		private uint blockSamples;
		private uint blockCurrent;

		private short[] blockMemory = null;
		private WaveHdr[] waveHeaders = null;
		private IntPtr device = IntPtr.Zero;
		private Thread audioThread;
		private uint blockFree = 0;

		private const int SoundInterval = 10;

		public Sound LoadSound(string file)
		{
			if (samples == null)
				samples = new List<Sound>();

			Sound s = new Sound(file);
			if (s.Valid)
			{
				samples.Add(s);
				return s;
			}
			else
			{
				return null;
			}
		}

		public void PlaySound(Sound s)
		{
			if (s == null)
				return;

			if (playingSamples == null)
				playingSamples = new List<PlayingSample>();

			PlayingSample ps = new PlayingSample
			{
				AudioSample = s,
				SamplePosition = 0,
				Finished = false,
				Loop = s.Loop
			};

			playingSamples.Add(ps);
		}

		public void StopSound(Sound s)
		{
			if (s == null)
				return;

			bool Match(PlayingSample p) => !p.Finished && p.AudioSample == s;

			if (playingSamples != null && playingSamples.Exists(Match))
			{
				int index = playingSamples.FindIndex(Match);
				PlayingSample ps = playingSamples[index];
				ps.Finished = true;
				playingSamples[index] = ps;
			}
		}

		public void CreateAudio(uint sampleRate = 44100, uint channels = 1, uint blocks = 8, uint blockSamples = 512)
		{
			Active = false;
			this.sampleRate = sampleRate;
			this.channels = channels;
			blockCount = blocks;
			this.blockSamples = blockSamples;
			blockFree = blockCount;
			blockCurrent = 0;
			blockMemory = null;
			waveHeaders = null;

			WaveFormatEx waveFormat = new WaveFormatEx
			{
				FormatTag = WaveFormatPcm,
				SamplesPerSec = (int)sampleRate,
				BitsPerSample = sizeof(short) * 8,
				Channels = (short)channels,
			};
			waveFormat.BlockAlign = (short)((waveFormat.BitsPerSample / 8) * waveFormat.Channels);
			waveFormat.AvgBytesPerSec = waveFormat.SamplesPerSec * waveFormat.BlockAlign;
			waveFormat.Size = (short)Marshal.SizeOf(waveFormat);

			waveProc = WaveOutProc;

			if (WaveOutOpen(out device, WaveMapper, waveFormat, waveProc, 0, CallbackFunction) != 0)
				DestroyAudio();

			blockMemory = new short[blockCount * blockSamples];
			waveHeaders = new WaveHdr[blockCount];

			unsafe
			{
				fixed (short* mem = blockMemory)
				{
					for (uint n = 0; n < blockCount; n++)
					{
						waveHeaders[n].BufferLength = (int)(blockSamples * sizeof(short));
						waveHeaders[n].Data = (IntPtr)(mem + (n * blockSamples));
					}
				}
			}

			Active = true;
			audioThread = new Thread(AudioThread);
			audioThread.Start();
		}

		public void DestroyAudio() => Active = false;

		private void WaveOutProc(IntPtr hWaveOut, int uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2)
		{
			if (uMsg != WomDone)
				return;

			blockFree++;
		}

		private float GetMixerOutput(int channel, float globalTime, float timeStep)
		{
			float mixerSample = 0.0f;

			if (playingSamples != null)
			{
				for (int i = 0; i < playingSamples.Count; i++)
				{
					PlayingSample ps = playingSamples[i];

					float increment = ps.AudioSample.WavHeader.SamplesPerSec * timeStep;
					ps.SamplePosition += (long)Math.Ceiling(increment);

					if (ps.SamplePosition < ps.AudioSample.SampleCount)
					{
						mixerSample += ps.AudioSample.Samples[(ps.SamplePosition * ps.AudioSample.Channels) + channel];
					}
					else
					{
						if (ps.Loop)
							ps.SamplePosition = 0;
						else
							ps.Finished = true;
					}

					playingSamples[i] = ps;
					playingSamples.RemoveAll(s => s.Finished);
				}
			}

			mixerSample += OnSoundCreate(channel, globalTime, timeStep);
			mixerSample = OnSoundFilter(channel, globalTime, mixerSample);
			mixerSample *= Volume;

			return mixerSample;
		}

		private void AudioThread()
		{
			float Clip(float sample, float max)
			{
				if (sample >= 0)
					return Math.Min(sample, max);
				else
					return Math.Max(sample, -max);
			}

			int whdrSize = Marshal.SizeOf(waveHeaders[blockCurrent]);

			GlobalTime = 0.0f;
			float timeStep = 1.0f / sampleRate;

			float maxSample = (float)(Math.Pow(2, (sizeof(short) * 8) - 1) - 1);

			while (Active)
			{
				if (blockFree == 0)
					while (blockFree == 0)
						Thread.Sleep(SoundInterval);

				blockFree--;

				short newSample = 0;
				int currentBlock = (int)(blockCurrent * blockSamples);

				if ((waveHeaders[blockCurrent].Flags & WHdrPrepared) != 0)
					WaveOutUnprepareHeader(device, ref waveHeaders[blockCurrent], whdrSize);

				for (uint n = 0; n < blockSamples; n += channels)
				{
					for (int c = 0; c < channels; c++)
					{
						newSample = (short)(Clip(GetMixerOutput(c, GlobalTime, timeStep), 1.0f) * maxSample);
						blockMemory[currentBlock + n + c] = newSample;
					}

					GlobalTime += timeStep;
				}

				WaveOutPrepareHeader(device, ref waveHeaders[blockCurrent], whdrSize);
				WaveOutWrite(device, ref waveHeaders[blockCurrent], whdrSize);

				blockCurrent++;
				blockCurrent %= blockCount;
			}
		}
	}
}