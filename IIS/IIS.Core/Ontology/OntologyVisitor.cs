namespace IIS.Core
{
    public abstract class OntologyVisitor : IOntologyVisitor
    {
        public abstract void VisitAttribute(Attribute attribute);
        public abstract void VisitObject(Entity entity);
        public abstract void VisitUnion(Union union);
        public abstract void VisitArrayRelation(ArrayRelation relation);
        public abstract void VisitRelation(Relation relation);
        protected void VisitNode(IOntologyNode node)
        {
            EnterNode(node);

            foreach (var child in node.Nodes) VisitNode(child);

            ExitNode(node);
        }
        protected virtual void EnterNode(IOntologyNode node) { }
        protected virtual void ExitNode(IOntologyNode node) { }
    }
}
