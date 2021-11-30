using Iis.Interfaces.DirectQueries;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Iis.DbLayer.DirectQueries
{
    public class DirectQueryFactory: IDirectQueryFactory
    {
        private const string QueryFolder = "Queries";
        private string _baseDirectory;

        private Dictionary<string, string> _baseSqls = new Dictionary<string, string>();

        public DirectQueryFactory(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
        }

        public IDirectQuery GetDirectQuery(string queryFileName)
        {
            var sql = _baseSqls.GetValueOrDefault(queryFileName);
            if (sql == null)
            {
                var fileName = $"{_baseDirectory}\\{QueryFolder}\\{queryFileName}.sql";
                sql = File.ReadAllText(fileName);
            }
            return new DirectQuery(sql);
        }

    }
}
