using System;

namespace Iis.Interfaces.Elastic
{
    public interface IElasticFieldEntity
    {
        Guid Id { get; set; }
        decimal Boost { get; set; }
        string Name { get; set; }
        int Fuzziness { get; set; }
        bool IsExcluded { get; set; }
        ElasticObjectType ObjectType { get; set; }
        string TypeName { get; set; }
    }
}