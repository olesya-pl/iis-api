using System;
using System.Text;
using System.Text.Json;
using Iis.RabbitMq.Helpers;
using Xunit;
using FluentAssertions;

namespace Iis.UnitTests.Iis.RabbitMq
{
    public class MessageType
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }
    public class SerializationExtensionTest
    {
        [Fact]
        public void ToMessage_NullInstanceReturnsDefaultByteArray()
        {
            (null as object).ToByteArray()
                .Should()
                .BeEquivalentTo(ReadOnlyMemory<byte>.Empty);
        }

        [Fact]
        public void ToObject_EmptyByteArrayReturnsDefaultOfType()
        {
            ReadOnlyMemory<byte>.Empty.
            ToObject<object>()
                .Should()
                .BeNull();
        }

        [Fact]
        public void ToMessage_InstanceReturnsSuccess()
        {
            var actual = new MessageType { Name = "user", Type = "userType" };
            var expected = Encoding.UTF8.GetBytes("{\"Name\":\"user\",\"Type\":\"userType\"}");

            actual.ToByteArray().ToArray()
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ToMessage_InstanceWithJsonWebOptionsReturnsSuccess()
        {
            var actual = new MessageType { Name = "user", Type = "userType" };
            var expected = Encoding.UTF8.GetBytes("{\"name\":\"user\",\"type\":\"userType\"}");
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            actual.ToByteArray(options).ToArray().
                Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ToObject_ByteArrayReturnsSuccess()
        {
            var actual = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"Name\":\"user\",\"Type\":\"userType\"}"));

            var expected = new MessageType { Name = "user", Type = "userType" };

            actual.ToObject<MessageType>()
                .Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void ToObject_ByteArrayJsonWebOptionsReturnsSuccess()
        {
            var actual = new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes("{\"name\":\"user\",\"type\":\"userType\"}"));

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var expected = new MessageType { Name = "user", Type = "userType" };

            actual.ToObject<MessageType>(options)
                .Should().BeEquivalentTo(expected);
        }
    }
}