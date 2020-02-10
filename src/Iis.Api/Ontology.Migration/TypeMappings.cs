using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class TypeMappings: ITypeMappings
    {
        private Dictionary<Guid, Guid> _mapping;
        public void InitFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"TypeMappings init file is not found: {fileName}");
            }
            _mapping = new Dictionary<Guid, Guid>();
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                var parts = line.Split("\t");
                var IdFrom = new Guid(parts[0]);
                var IdTo = new Guid(parts[1]);
                _mapping[IdFrom] = IdTo;
            }
        }

        public bool IsMapped(Guid nodeTypeId)
        {
            return _mapping.ContainsKey(nodeTypeId);
        }

        public Guid GetMapTypeId(Guid nodeTypeId)
        {
            return _mapping[nodeTypeId];
        }
    }
}
