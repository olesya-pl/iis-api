using System;
using Iis.DataModel;
using Microsoft.EntityFrameworkCore;

namespace IIS.Core.Files.EntityFramework
{
    public class FileServiceFactory
    {
        private readonly string _connectionString;

        public FileServiceFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IFileService CreateService()
        {
            var options = new DbContextOptionsBuilder<OntologyContext>()
                .UseNpgsql(_connectionString)
                .EnableSensitiveDataLogging()
                .Options;
            var context = new OntologyContext(options);
            var fileService = new FileService(context);

            return fileService;
        }
    }
}
