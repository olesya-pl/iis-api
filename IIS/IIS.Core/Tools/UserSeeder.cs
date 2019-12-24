using System;
using System.Linq;
using System.Threading.Tasks;
using IIS.Core.Ontology.EntityFramework.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IIS.Core.Tools
{
    internal sealed class UserSeeder
    {
        private readonly ILogger<UserSeeder> _logger;
        private readonly IConfiguration _configuration;
        private readonly OntologyContext _context;

        public UserSeeder(ILogger<UserSeeder> logger, IConfiguration configuration,  OntologyContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }

        public async Task SeedDataAsync()
        {
            var defaultUserName = _configuration.GetValue<string>("defaultUserName");
            var defaultPassword = _configuration.GetValue<string>("defaultPassword");

            if (!string.IsNullOrWhiteSpace(defaultUserName) && !string.IsNullOrWhiteSpace(defaultPassword))
            {
                var admin = _context.Users.SingleOrDefault(x => x.Username == defaultUserName);
                if (admin == null)
                {
                    _context.Users.Add(new Core.Users.EntityFramework.User
                    {
                        Id = Guid.NewGuid(),
                        IsBlocked = false,
                        Name = defaultUserName,
                        Username = defaultUserName,
                        PasswordHash = _configuration.GetPasswordHashAsBase64String(defaultPassword)
                    });

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Added default user {user}", defaultUserName);
                }
            }
        }
    }
}
