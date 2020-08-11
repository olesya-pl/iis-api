using Iis.Services.Contracts;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;

namespace Iis.Services
{
    public class ActiveDirectoryClient : IActiveDirectoryClient
    {
        private readonly DirectoryEntry _directoryEntry;


        public ActiveDirectoryClient(string activeDirectoryDomain, string login, string password)
        {
            _directoryEntry = new DirectoryEntry(activeDirectoryDomain, login, password);
        }

        public List<ActiveDirectoryGroupDto> GetAllGroups()
        {
            var searcher = new DirectorySearcher(_directoryEntry)
            {
                Filter = "(&(objectCategory=Group))"
            };

            IncludeProperties(searcher);
            return ToDtos(searcher.FindAll());
        }

        public List<ActiveDirectoryGroupDto> GetGroupsByIds(params Guid[] ids)
        {
            var ldapQueries = ids.Select(BuildLdapQueryById);
            var searcher = new DirectorySearcher(_directoryEntry)
            {
                Filter = $"(|{string.Join("", ldapQueries)})"
            };

            IncludeProperties(searcher);
            return ToDtos(searcher.FindAll());
        }

        private List<ActiveDirectoryGroupDto> ToDtos(SearchResultCollection result)
        {
            return (
                from SearchResult searchResult in result
                select new ActiveDirectoryGroupDto()
                {
                    Id = new Guid((byte[])searchResult.Properties["objectGUID"][0]),
                    Name = searchResult.Properties["name"][0].ToString()
                }).ToList();
        }

        private void IncludeProperties(DirectorySearcher searcher)
        {
            searcher.PropertiesToLoad.Add("name");
            searcher.PropertiesToLoad.Add("objectGUID");
        }

        private string BuildLdapQueryById(Guid id)
        {
            var guidInHexadecimal = id.ToByteArray().Select(x => x.ToString("X"));
            var toActiveDirectoryFormat = string.Join(@"\", guidInHexadecimal);
            return $"(objectGUID=\\{toActiveDirectoryFormat})";
        }
    }
}
