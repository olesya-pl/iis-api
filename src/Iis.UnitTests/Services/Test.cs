using System;
using System.DirectoryServices;
using System.Linq;
using Iis.Services;
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

        [Fact]
        public void test()
        {
            var domain = "LDAP://192.168.88.31";
            SearchResultCollection results;
            DirectorySearcher ds = null;
            DirectoryEntry de = new DirectoryEntry(domain);
            de.Username = "tmykhats";
            de.Password = "S2bd&QRP";

            ds = new DirectorySearcher(de);
            var guid = new Guid("d6e91b97-a252-489a-9132-596d48b1af7f");
            var guid2 = new Guid("a5490dde-8248-4476-bb4c-ace52b4f8f50");
            //97\1B\E9\D6\52\A2\9A\48\91\32\59\6D\48\B1\AF\7F
            //151\27\233\214\82\162\154\72\145\50\89\109\72\177\175\127
            var byteArray = string.Join(@"\", guid.ToByteArray().Select(x => x.ToString("X")));
            var byteArray2 = string.Join(@"\", guid2.ToByteArray().Select(x => x.ToString("X")));

            //ds.Filter = "(&(objectCategory=Group))";
            //ds.Filter = $"(objectGUID=\\97\\1B\\E9\\D6\\52\\A2\\9A\\48\\91\\32\\59\\6D\\48\\B1\\AF\\7F)";
            ds.Filter = $"(|(objectGUID=\\{byteArray})(objectGUID=\\{byteArray2}))";
            //ds.Filter = "(groupType:1.2.840.113556.1.4.803:=2147483648)";

            ds.PropertiesToLoad.Add("name");
            ds.PropertiesToLoad.Add("memberof");
            ds.PropertiesToLoad.Add("member");
            ds.PropertiesToLoad.Add("objectGUID");

            results = ds.FindAll();

            _testOutputHelper.WriteLine($"Count: {results.Count}");
            foreach (SearchResult sr in results)
            {
                // Using the index zero (0) is required!
                var s = (byte[])sr.Properties["objectGUID"][0];
                _testOutputHelper.WriteLine($"{sr.Properties["name"][0]}: {new Guid(s)}");
            }
        }

        [Fact]
        public void GetAllGroups()
        {
            var client = new ActiveDirectoryClient("LDAP://192.168.88.31", "tmykhats", "S2bd&QRP");
            var groups = client.GetAllGroups();

            _testOutputHelper.WriteLine($"Count: {groups.Count}");
            foreach (var sr in groups)
            {
                _testOutputHelper.WriteLine($"{sr.Name}: {sr.Id}");
            }
        }
    }
}