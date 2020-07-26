using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IEntityRelationMeta : IRelationMetaBase
    {
        EntityOperation[] AcceptsEntityOperations { get; set; }
        string Type { get; set; }
        IInversedRelationMeta Inversed { get; }
        string[] TargetTypes { get; set; }
    }
}
