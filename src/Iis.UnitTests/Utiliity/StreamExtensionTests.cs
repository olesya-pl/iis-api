using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using AutoFixture;
using AutoFixture.Xunit2;
using FluentAssertions;
using Iis.Utility;
using Iis.UnitTests.TestHelpers;

namespace Iis.UnitTests.Utiliity
{
    public class StreamExtensionsTests : IDisposable
    {
        public const int RepeatCount = 30;
        private const int OriginalArraySize = 32768;
        private const int MinArraySize = 2048;
        private Fixture _fixture;
        private Random _random;

        private int GetRandomArraySize()
        {
            return _random.Next(MinArraySize, OriginalArraySize - MinArraySize);
        }
        public StreamExtensionsTests()
        {
            _fixture = new Fixture();
            _random = new Random();
        }

        public void Dispose()
        {
            _fixture = null;
            _random = null;
        }

        [Theory, AutoData]
        public void ToByteArray_ReturnsExpected(byte[] byteArray)
        {
            var actual = new MemoryStream(byteArray).ToByteArray();

            actual.Should().BeEquivalentTo(byteArray);
        }

        [Fact]
        public void ToByteArray_EmptyStreamReturnsEmptyArray()
        {
            var actual = new MemoryStream().ToByteArray();

            actual.Should().BeEmpty();
        }

        [Fact]
        public void ToByteArray_NullStreamReturnsEmptyArray()
        {
            var actual = (null as MemoryStream).ToByteArray();

            actual.Should().BeEmpty();
        }

        [Theory, AutoData]
        public async Task ToByteArray_ReturnsExpectedAsync(byte[] byteArray)
        {
            var actual = await new MemoryStream(byteArray).ToByteArrayAsync();

            actual.Should().BeEquivalentTo(byteArray);
        }

        [Fact]
        public async Task ToByteArray_EmptyStreamReturnsEmptyArrayAsync()
        {
            var actual = await new MemoryStream().ToByteArrayAsync();

            actual.Should().BeEmpty();
        }

        [Fact]
        public async Task ToByteArray_NullStreamReturnsEmptyArrayAsync()
        {
            var actual = await (null as MemoryStream).ToByteArrayAsync();

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Stream2StreamIsEqual_DifferenceInLengthReturnsFalse()
        {
            ReadOnlySpan<byte> byteSpan = _fixture.CreateMany<byte>(10).ToArray();

            var stream1 = new MemoryStream(byteSpan.Slice(0, 5).ToArray());

            var stream2 = new MemoryStream(byteSpan.Slice(0, 6).ToArray());

            stream1.IsEqual(stream2)
                .Should()
                .BeFalse();
        }

        [Theory, Repeat(RepeatCount)]
        public void Stream2StreamIsEqual_SameLengthButDifferenceInContentReturnsFalse(int index)
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var streamSize = GetRandomArraySize();

            var stream1 = new MemoryStream(original.Slice(0, streamSize).ToArray());
            var stream2 = new MemoryStream(original.Slice(144, streamSize).ToArray());

            stream1.IsEqual(stream2)
                .Should()
                .BeFalse();
        }

        [Theory, Repeat(RepeatCount)]
        public void Stream2StreamIsEqual_SameContentReturnsTrue(int index)
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var streamSize = GetRandomArraySize();

            var stream1 = new MemoryStream(original.Slice(0, streamSize).ToArray());
            var stream2 = new MemoryStream(original.Slice(0, streamSize).ToArray());

            stream1.IsEqual(stream2)
                .Should()
                .BeTrue();
        }

        [Fact]
        public void Stream2StreamIsEqual_EmptyStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream2 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            new MemoryStream().IsEqual(stream2)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2StreamIsEqual_NullStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream2 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            (null as MemoryStream).IsEqual(stream2)
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2StreamIsEqual_EmptyOtherStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream1 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            stream1.IsEqual(new MemoryStream())
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2StreamIsEqual_NullOtherStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream1 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            stream1.IsEqual((null as MemoryStream))
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2ArrayIsEqual_NullStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            (null as MemoryStream).IsEqual(original.Slice(0, MinArraySize).ToArray())
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2ArrayIsEqual_EmptyStreamReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            new MemoryStream().IsEqual(original.Slice(0, MinArraySize).ToArray())
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2ArrayIsEqual_NullArrayReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream1 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            stream1.IsEqual((null as byte[]))
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2ArrayIsEqual_EmptyArrayReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream1 = new MemoryStream(original.Slice(0, MinArraySize).ToArray());

            stream1.IsEqual(Array.Empty<byte>())
                .Should()
                .BeFalse();
        }

        [Fact]
        public void Stream2ArrayIsEqual_DifferenceInLengthReturnsFalse()
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var stream = new MemoryStream(original.Slice(0, MinArraySize).ToArray());
            var array = original.Slice(0, 2 * MinArraySize).ToArray();

            stream.IsEqual(array)
                .Should()
                .BeFalse();
        }

        [Theory, Repeat(RepeatCount)]
        public void Stream2ArrayIsEqual_SameLengthButDifferenceInContentReturnsFalse(int index)
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var streamSize = GetRandomArraySize();

            ReadOnlySpan<byte> s = original.Slice(0, 12);

            var stream = new MemoryStream(original.Slice(0, streamSize).ToArray());
            var array = original.Slice(144, streamSize).ToArray();

            stream.IsEqual(array)
                .Should()
                .BeFalse();
        }

        [Theory, Repeat(RepeatCount)]
        public void Stream2ArrayIsEqual_SameContentReturnsTrue(int index)
        {
            ReadOnlySpan<byte> original = _fixture.CreateMany<byte>(OriginalArraySize).ToArray();

            var streamSize = GetRandomArraySize();

            var stream = new MemoryStream(original.Slice(0, streamSize).ToArray());
            var array = original.Slice(0, streamSize).ToArray();

            stream.IsEqual(array)
                .Should()
                .BeTrue();
        }
    }
}