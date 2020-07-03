namespace Iis.Domain
{
    public interface IInheritanceRelationTypeModel: IRelationTypeModel
    {
        IEntityTypeModel ParentType { get; }
    }
}