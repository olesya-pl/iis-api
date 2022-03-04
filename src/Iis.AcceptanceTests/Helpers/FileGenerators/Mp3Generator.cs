using System;
using System.IO;
using NAudio.Lame;
using NAudio.Utils;
using NAudio.Wave;

namespace AcceptanceTests.Helpers.FileGenerators
{
    public static class Mp3Generator
    {
        private const int SampleRate = 44100;
        private const int NumberOfChannels = 2;

        public static (long, byte[]) Generate()
        {
            using (var waveMemoryStream = new MemoryStream())
            {
                using (var retMs = new MemoryStream())
                {
                    using (var writer = new WaveFileWriter(new IgnoreDisposeStream(waveMemoryStream), new WaveFormat(SampleRate, 16, NumberOfChannels)))
                    {
                        for (var i = 0; i < SampleRate * 10; i++)
                        {
                            writer.WriteSample((float)(0.5 * new Random().NextDouble() * 2 - 1));
                        }
                        waveMemoryStream.Seek(9, SeekOrigin.Begin);
                        using (var wtr = new LameMP3FileWriter(retMs, new WaveFormat(SampleRate, NumberOfChannels), 128))
                        {
                            waveMemoryStream.CopyTo(retMs);
                            retMs.Seek(0, SeekOrigin.Begin);
                            return (retMs.Length, retMs.ToArray());
                        }
                    }
                }
            }
            
        }
    }
}
