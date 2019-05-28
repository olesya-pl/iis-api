namespace IIS.Core
{
    public interface IOntologyVisitor
    {
        void VisitAttribute(Attribute attribute);
        void VisitObject(Entity entity);
        void VisitUnion(Union union);
        void VisitArrayRelation(ArrayRelation relation);
        void VisitRelation(Relation relation);
    }
}
