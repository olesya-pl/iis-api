using System;

namespace Iis.Domain
{
    public interface IEntityTypeModel: INodeTypeModel
    {
        bool IsAbstract { get; }
    }
}