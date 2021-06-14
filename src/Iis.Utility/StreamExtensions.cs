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
            using var ms = new MemoryStream();
                stream.CopyTo(ms);
                return ms.ToArray();
        }

        public static async Task<byte[]> ToByteArrayAsync(this Stream stream, CancellationToken ct = default)
        {
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
            if (otherStream.Length != stream.Length)
            {
                return false;
            }

            byte[] buffer = new byte[bufferSize];
            byte[] otherBuffer = new byte[bufferSize];

            while ((_ = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var _ = otherStream.Read(otherBuffer, 0, otherBuffer.Length);

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
            var i = 0;
            if (otherBytes.Length != stream.Length)
            {
                return false;
            }

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var contentsBuffer = otherBytes
                    .Skip(i * bufferSize)
                    .Take(bytesRead)
                    .ToArray();

                if (!contentsBuffer.SequenceEqual(buffer))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    return false;
                }
            }
            stream.Seek(0, SeekOrigin.Begin);
            return true;
        }
    }
}
