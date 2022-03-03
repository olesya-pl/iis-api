using System;
using System.IO;
using NAudio.Lame;
using NAudio.Wave;

namespace AcceptanceTests.Helpers.FileGenerators
{
    public static class Mp3Generator
    {
        public static (long, byte[]) Generate()
        {
            using (var retMs = new MemoryStream())
            {
                using (var wtr = new LameMP3FileWriter(retMs, new WaveFormat(44100,1), 128))
                {
                    retMs.Seek(0, SeekOrigin.Begin);
                    return (retMs.Length, retMs.ToArray());
                }
                    
            }
        }
    }
}
