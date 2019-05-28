namespace IIS.Core
{
    public interface ISchemaVisitor
    {
        void VisitConstraint(Constraint schema);
        void VisitAbstractClass(TypeEntity schema);
        void VisitClass(TypeEntity schema);
        void VisitAttributeClass(AttributeClass schema);
        void VisitUnionClass(UnionClass schema);
    }
}
