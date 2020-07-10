using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Interfaces.Meta
{
    public interface IInversedRelationMeta: IRelationMetaBase
    {
        string Code { get; set; }
        bool Editable { get; set; }
    }
}
