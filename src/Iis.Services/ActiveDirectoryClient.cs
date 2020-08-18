using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace Iis.Services
{
    public class ActiveDirectoryClient : IActiveDirectoryClient
    {
        private const string DistinguishedName = "DC=pogliad,DC=net";
        private static readonly string[] IncludedFields = new string[] { "name", "objectGUID" };
        private readonly string _server;
        private readonly string _login;
        private readonly string _password;

        public ActiveDirectoryClient(string server, string login, string password)
        {
            _server = server;
            _login = login;
            _password = password;
        }

        public List<ActiveDirectoryGroupDto> GetAllGroups()
        {
            var response = MakeRequest("(&(objectCategory=Group))");
            return ToDtos(response);
        }

        public List<ActiveDirectoryGroupDto> GetGroupsByIds(params Guid[] ids)
        {
            if(!ids.Any())
                return new List<ActiveDirectoryGroupDto>();
            
            var ldapQueries = ids.Select(BuildLdapQueryById);
            var response = MakeRequest($"(|{string.Join("", ldapQueries)})");
            
            return ToDtos(response);
        }

        private LdapConnection CreateConnection()
        {
            return new LdapConnection(
               new LdapDirectoryIdentifier(_server),
               new System.Net.NetworkCredential(_login, _password));
        }

        private SearchRequest CreateSearchRequest(string filter)
        {
            return new SearchRequest(
                DistinguishedName,
                filter,
                SearchScope.Subtree,
                IncludedFields);
        }

        private SearchResponse MakeRequest(string filter) 
        {
            using (var client = CreateConnection())
            {
                var request = CreateSearchRequest(filter);
                var result = (SearchResponse)client.SendRequest(request);

                return result;
            }
        }

        private List<ActiveDirectoryGroupDto> ToDtos(SearchResponse response)
        {
            return (
                from SearchResultEntry item in response.Entries
                select new ActiveDirectoryGroupDto()
                {
                    Id = new Guid((byte[])item.Attributes["objectGUID"][0]),
                    Name = item.Attributes["name"][0].ToString()
                }).ToList();
        }

        private string BuildLdapQueryById(Guid id)
        {
            var guidInHexadecimal = id.ToByteArray().Select(x => x.ToString("X"));
            var toActiveDirectoryFormat = string.Join(@"\", guidInHexadecimal);
            return $"(objectGUID=\\{toActiveDirectoryFormat})";
        }
    }
}
