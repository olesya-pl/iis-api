using System;
using Iis.Api.Configuration;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IIS.Core.Files.EntityFramework
{
    public class FileServiceFactory
    {
        private readonly string _connectionString;
        private readonly FilesConfiguration _configuration;
        private readonly ILogger<FileService> _logger;

        public FileServiceFactory(string connectionString, FilesConfiguration configuration, ILogger<FileService> logger)
        {
            _connectionString = connectionString;
            _configuration = configuration;
            _logger = logger;
        }

        public IFileService CreateService()
        {
            var options = new DbContextOptionsBuilder<OntologyContext>()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;
            var context = new OntologyContext(options);
            var fileService = new FileService(context, _configuration, _logger);

            return fileService;
        }
    }
}
