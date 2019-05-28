namespace IIS.Core
{
    public abstract class SchemaVisitor : ISchemaVisitor
    {
        public abstract void VisitConstraint(Constraint schema);
        public abstract void VisitAbstractClass(TypeEntity schema);
        public abstract void VisitClass(TypeEntity schema);
        public abstract void VisitAttributeClass(AttributeClass schema);
        public abstract void VisitUnionClass(UnionClass schema);
        public void VisitNode(ISchemaNode node)
        {
            EnterNode(node);

            node.AcceptVisitor(this);

            if (IsInterested(node))
                foreach (var child in node.Nodes) VisitNode(child);

            ExitNode(node);
        }
        protected virtual void EnterNode(ISchemaNode node) { }
        protected virtual void ExitNode(ISchemaNode node) { }
        protected virtual bool IsInterested(ISchemaNode node) => true;
    }
}
