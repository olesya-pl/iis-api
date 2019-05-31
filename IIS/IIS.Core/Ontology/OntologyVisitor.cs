namespace IIS.Core
{
    public abstract class OntologyVisitor : IOntologyVisitor
    {
        public virtual void EnterAttribute(Attribute attribute) { }
        public virtual void EnterObject(Entity entity) { }
        public virtual void EnterUnion(Union union) { }
        public virtual void EnterArrayRelation(ArrayRelation relation) { }
        public virtual void EnterRelation(Relation relation) { }

        public virtual void LeaveAttribute(Attribute attribute) { }
        public virtual void LeaveObject(Entity entity) { }
        public virtual void LeaveUnion(Union union) { }
        public virtual void LeaveArrayRelation(ArrayRelation relation) { }
        public virtual void LeaveRelation(Relation relation) { }

        void IOntologyVisitor.VisitObject(Entity entity)
        {
            EnterObject(entity);
            VisitChildNodes(entity);
            LeaveObject(entity);
        }
        void IOntologyVisitor.VisitAttribute(Attribute attribute)
        {
            EnterAttribute(attribute);
            VisitChildNodes(attribute);
            LeaveAttribute(attribute);
        }
        void IOntologyVisitor.VisitUnion(Union union)
        {
            EnterUnion(union);
            VisitChildNodes(union);
            LeaveUnion(union);
        }
        void IOntologyVisitor.VisitArrayRelation(ArrayRelation relation)
        {
            EnterArrayRelation(relation);
            VisitChildNodes(relation);
            LeaveArrayRelation(relation);
        }
        void IOntologyVisitor.VisitRelation(Relation relation)
        {
            EnterRelation(relation);
            VisitChildNodes(relation);
            LeaveRelation(relation);
        }

        private void VisitChildNodes(IOntologyNode node)
        {
            foreach (var child in node.Nodes) child.AcceptVisitor(this);
        }
    }
}
