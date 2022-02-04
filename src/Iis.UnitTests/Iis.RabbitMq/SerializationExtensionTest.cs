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
                .BeNull();
        }

        [Fact]
        public void ToObject_NullByteArrayReturnsDefaultOfType()
        {
            (null as byte[]).ToObject<object>()
                .Should()
                .BeNull();
        }

        [Fact]
        public void ToObject_EmptyByteArrayReturnsDefaultOfType()
        {
            Array.Empty<byte>().ToObject<object>()
                .Should()
                .BeNull();
        }

        [Fact]
        public void ToMessage_InstanceReturnsSuccess()
        {
            var actual = new MessageType { Name = "user", Type = "userType" };
            var expected = Encoding.UTF8.GetBytes("{\"Name\":\"user\",\"Type\":\"userType\"}");

            actual.ToByteArray()
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ToMessage_InstanceWithJsonWebOptionsReturnsSuccess()
        {
            var actual = new MessageType { Name = "user", Type = "userType" };
            var expected = Encoding.UTF8.GetBytes("{\"name\":\"user\",\"type\":\"userType\"}");
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            actual.ToByteArray(options)
                .Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void ToObject_ByteArrayReturnsSuccess()
        {
            var actual = Encoding.UTF8.GetBytes("{\"Name\":\"user\",\"Type\":\"userType\"}");

            var expected = new MessageType { Name = "user", Type = "userType" };

            actual.ToObject<MessageType>()
                .Should().BeEquivalentTo(expected);
        }
        [Fact]
        public void ToObject_ByteArrayJsonWebOptionsReturnsSuccess()
        {
            var actual = Encoding.UTF8.GetBytes("{\"name\":\"user\",\"type\":\"userType\"}");

            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var expected = new MessageType { Name = "user", Type = "userType" };

            actual.ToObject<MessageType>(options)
                .Should().BeEquivalentTo(expected);
        }
    }
}