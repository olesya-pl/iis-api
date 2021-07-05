using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Iis.Utility
{
    public static class StreamExtensions
    {
        private const int bufferSize = 2048;

        public static byte[] ToByteArray(this Stream stream)
        {
            if(stream is null || stream.Length == 0) return Array.Empty<byte>();

            using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
        }

        public static async Task<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken ct = default)
        {
            if(stream is null || stream.Length == 0) return Array.Empty<byte>();

            using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, ct);
                return ms.ToArray();
        }

        public static Guid ComputeHashAsGuid(this Stream stream)
        {
            using HashAlgorithm algorithm = MD5.Create();
            byte[] byteArray = algorithm.ComputeHash(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return new Guid(byteArray);
        }

        public static bool IsEqual(this Stream stream, Stream otherStream)
        {
            if(stream is null || stream.Length == 0) return false;

            if(otherStream is null || otherStream.Length == 0) return false;

            if (stream.Length != otherStream.Length) return false;

            byte[] buffer = new byte[bufferSize];
            byte[] otherBuffer = new byte[bufferSize];

            while (stream.Read(buffer, 0, buffer.Length) > 0)
            {
                otherStream.Read(otherBuffer, 0, otherBuffer.Length);

                if (!otherBuffer.SequenceEqual(buffer))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    otherStream.Seek(0, SeekOrigin.Begin);
                    return false;
                }
            }

            stream.Seek(0, SeekOrigin.Begin);
            otherStream.Seek(0, SeekOrigin.Begin);

            return true;
        }

        public static bool IsEqual(this Stream stream, byte[] otherBytes)
        {
            if(stream is null || stream.Length == 0) return false;

            if(otherBytes is null || otherBytes.Length == 0) return false;

            if (otherBytes.Length != stream.Length) return false;

            byte[] buffer = new byte[bufferSize];
            ReadOnlySpan<byte> otherSpan = otherBytes;

            int bytesRead;
            int slicePosition = 0;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var otherBuffer = otherSpan.Slice(slicePosition, bytesRead);

                var sequencesEqual = false;
                if(otherBuffer.Length == buffer.Length)
                {
                    sequencesEqual = otherBuffer.SequenceEqual(buffer);
                }
                else
                {
                    var newBuffer = new ReadOnlySpan<byte>(buffer).Slice(0, otherBuffer.Length);
                    sequencesEqual = otherBuffer.SequenceEqual(newBuffer);
                }

                if(!sequencesEqual)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return false;

                }

                slicePosition += bytesRead;
            }

            stream.Seek(0, SeekOrigin.Begin);
            return true;
        }
    }
}
