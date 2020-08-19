using Iis.Services.Contracts.Dtos;
using Iis.Services.Contracts.Interfaces;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Services
{
    public class ActiveDirectoryClient : IActiveDirectoryClient
    {
        private const string DistinguishedName = "DC=pogliad,DC=net";
        private const int DefaultPort = 389;
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
            using (var ldapConn = new LdapConnection())
            {
                SetUpConnection(ldapConn);

                var response = MakeRequest(ldapConn, "(&(objectCategory=Group))");

                return ToDtos(response);
            }
        }

        public List<ActiveDirectoryGroupDto> GetGroupsByIds(params Guid[] ids)
        {
            if (!ids.Any())
                return new List<ActiveDirectoryGroupDto>();

            return GetAllGroups().Where(g => ids.Contains(g.Id)).ToList();
        }

        private void SetUpConnection(LdapConnection connection)
        {
            connection.Connect(_server, DefaultPort);
            connection.Bind(_login, _password);
        }

        private ILdapSearchResults MakeRequest(LdapConnection connection, string filter)
        {
            return connection.Search(DistinguishedName, LdapConnection.ScopeSub, filter, IncludedFields, false);
        }

        private List<ActiveDirectoryGroupDto> ToDtos(ILdapSearchResults response)
        {
            var result = new List<ActiveDirectoryGroupDto>();
            while (response.HasMore())
            {
                try
                {
                    LdapEntry nextEntry = response.Next();
                    var attributes = nextEntry.GetAttributeSet();
                    result.Add(new ActiveDirectoryGroupDto 
                    {
                        Id = new Guid(attributes["objectGUID"].ByteValue),
                        Name = attributes["name"].StringValue
                    });
                }
                catch (LdapException)
                {
                    continue;
                }
            }

            return result;
        }
    }
}
