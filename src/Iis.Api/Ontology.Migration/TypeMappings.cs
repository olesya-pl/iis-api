using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Iis.Api.Ontology.Migration
{
    public class TypeMappings: ITypeMappings
    {
        public List<TypeMapping> MappingList { get; set; }
        private Dictionary<Guid, Guid> _mapping;
        private Dictionary<Guid, Guid> Mapping { 
            get 
            {
                if (_mapping == null)
                {
                    _mapping = new Dictionary<Guid, Guid>();
                    foreach (var tm in MappingList)
                    {
                        _mapping[tm.IdFrom] = tm.IdTo;
                    }
                }
                return _mapping;
            } 
        }
        public void InitFromFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException($"TypeMappings init file is not found: {fileName}");
            }
            MappingList = new List<TypeMapping>();
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                var parts = line.Split("\t");
                var IdFrom = new Guid(parts[0]);
                var IdTo = new Guid(parts[1]);
                MappingList.Add(new TypeMapping { IdFrom = IdFrom, IdTo = IdTo });
            }
        }

        public bool IsMapped(Guid nodeTypeId)
        {
            return Mapping.ContainsKey(nodeTypeId);
        }

        public Guid GetMapTypeId(Guid nodeTypeId)
        {
            return Mapping[nodeTypeId];
        }
    }
}
