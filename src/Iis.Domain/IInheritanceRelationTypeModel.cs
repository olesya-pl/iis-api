namespace Iis.Domain
{
    public interface IInheritanceRelationTypeModel: IRelationTypeModel
    {
        EntityType ParentType { get; }
    }
}