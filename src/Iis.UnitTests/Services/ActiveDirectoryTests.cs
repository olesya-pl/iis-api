using Iis.Services;
using Newtonsoft.Json.Linq;
using System;
using Xunit;
using Xunit.Abstractions;

namespace Iis.UnitTests.Services
{
    public class ActiveDirectoryTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ActiveDirectoryTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact(Skip = "only for debug")]
        public void GetAllGroups()
        {
            var client = new ActiveDirectoryClient("192.168.88.31", "iis", "VPkqGPv!");
            var groups = client.GetAllGroups();

            _testOutputHelper.WriteLine($"Count: {groups.Count}");
            foreach (var sr in groups)
            {
                _testOutputHelper.WriteLine($"{sr.Name}: {sr.Id}");
            }
        }

        [Fact(Skip = "only for debug")]
        public void GetGroupsByIds()
        {
            var client = new ActiveDirectoryClient("192.168.88.31", "iis", "VPkqGPv!");
            var groups = client.GetGroupsByIds(
                Guid.Parse("d6e91b97-a252-489a-9132-596d48b1af7f"),
                Guid.Parse("a5490dde-8248-4476-bb4c-ace52b4f8f50"));

            _testOutputHelper.WriteLine($"Count: {groups.Count}");
            foreach (var sr in groups)
            {
                _testOutputHelper.WriteLine($"{sr.Name}: {sr.Id}");
            }
        }
    }
}