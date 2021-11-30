using Iis.Interfaces.DirectQueries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.DbLayer.DirectQueries
{
    public class DirectQuery: IDirectQuery
    {
        private string _baseSql;
        private Dictionary<string, string> _parameters = new Dictionary<string, string>();
        public DirectQuery(string baseSql)
        {
            _baseSql = baseSql;
        }
        public IDirectQuery SetParameter(string name, string value)
        {
            _parameters[name] = value;
            return this;
        }
        public string GetFinalSql()
        {
            var result = _baseSql;
            foreach (var parameterName in _parameters.Keys)
            {
                result = result.Replace("{" + parameterName + "}", _parameters[parameterName]);
            }
            return result;
        }
    }
}
