using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    // TODO: remove this file together with Legacy folder when all applications migrated to .NET Core
    //       builders should be replaced with OntologyProviders completely
    public class OntologyBuildContext
    {
        private bool _isBuilt;
        private readonly Dictionary<string, OntologyBuilder> _builders = new Dictionary<string, OntologyBuilder>();

        public ITypeBuilder CreateBuilder()
        {
            var builder = new OntologyBuilder(_builders);
            return builder;
        }

        public IEnumerable<Type> BuildOntology()
        {
            if (_isBuilt) throw new Exception("Current ontology context has already been built.");
            _isBuilt = true;
            return _builders.Values.Select(b => b.Build()).ToArray();
        }
    }
}
