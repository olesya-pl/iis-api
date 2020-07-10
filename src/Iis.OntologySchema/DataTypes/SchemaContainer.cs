using Iis.Interfaces.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaContainer: IContainerMeta
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
    }
}
