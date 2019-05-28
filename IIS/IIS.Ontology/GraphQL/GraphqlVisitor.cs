//using System.Collections.Generic;
//using GraphQL.Types;
//using IIS.Core;

//namespace IIS.Ontology.GraphQL
//{
//    public class GraphqlVisitor : SchemaVisitor
//    {
//        private static readonly Dictionary<string, IComplexGraphType> ComplexTypes = new Dictionary<string, IComplexGraphType>();
//        private static readonly Dictionary<ScalarType, ScalarGraphType> ScalarTypes = new Dictionary<ScalarType, ScalarGraphType>
//        {
//            [ScalarType.String] = new StringGraphType(),
//            [ScalarType.Int] = new IntGraphType(),
//            [ScalarType.Decimal] = new DecimalGraphType(),
//            [ScalarType.Date] = new DateGraphType(),
//            [ScalarType.Boolean] = new BooleanGraphType(),
//            [ScalarType.Geo] = new StringGraphType(), // todo: custom scalar
//            [ScalarType.File] = new StringGraphType(),// todo: custom object
//            [ScalarType.Json] = new StringGraphType()
//        };

//        static GraphqlVisitor()
//        {
//            foreach (var type in ScalarTypes.Keys)
//            {
//                var value = new ObjectGraphType { Name = type.Camelize() + "Value" };
//                value.AddField(new FieldType { Name = "Id", ResolvedType = new NonNullGraphType(ScalarTypes[ScalarType.Int]) });
//                value.AddField(new FieldType { Name = "Value", ResolvedType = new NonNullGraphType(ScalarTypes[type]) });

//                ComplexTypes[(string)type] = value;
//            }
//        }
//        //private IComplexGraphType _currentComplex;
//        //private UnionGraphType _currentUnion;
//        //private FieldType _currentField;
//        //private IGraphType _currentType;

//        private Stack<object> _nodes = new Stack<object>();

//        public ObjectGraphType GetSchema() => _nodes.Pop() as ObjectGraphType;

//        protected override bool IsInterested(ISchemaNode node)
//        {
//            if (!(node is IType)) return true;
//            var type = node as IType;
//            return !ComplexTypes.ContainsKey(type.Name);
//        }

//        protected override void ExitNode(ISchemaNode node)
//        {
//            if (node is Constraint)
//            {
//                var constraint = node as Constraint;
//                if (constraint.Target.Kind == Kind.Attribute)
//                {
//                    var attributeType = ((AttributeClass)constraint.Target).Type;
//                    var currentAttrType = constraint.IsArray
//                        ? (IGraphType)ComplexTypes[(string)attributeType]
//                        : ScalarTypes[attributeType];
//                    _nodes.Push(currentAttrType);
//                }
//                else
//                {
//                    var currentComplexType = (IGraphType)_nodes.Pop();
//                    var isArray = constraint.IsArray;
//                    var isRequired = constraint.IsRequired;
//                    if (isRequired && !isArray) currentComplexType = new NonNullGraphType(currentComplexType);
//                    if (isArray) currentComplexType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(currentComplexType)));
//                    _nodes.Push(currentComplexType);
//                }
//                var currentType = (IGraphType)_nodes.Pop();
//                var currentField = (FieldType)_nodes.Pop();
//                currentField.ResolvedType = currentType;
//                _nodes.Push(currentField);

//                //var currentComplex = (IComplexGraphType)_nodes.Pop();
//                //currentComplex.AddField(currentField);
//                //_nodes.Push(currentComplex);
//            }
//            else if (node is TypeEntity)
//            {
//                var fields = new List<FieldType>();
//                var complexType = default(IComplexGraphType);
//                while (complexType == null)
//                {
//                    var item = _nodes.Pop();
//                    if (item is IComplexGraphType) complexType = item as IComplexGraphType;
//                    else fields.Add(item as FieldType);
//                }
//                foreach (var field in fields) complexType.AddField(field);
//                _nodes.Push(complexType);
//                ComplexTypes[complexType.Name] = complexType;
//            }
//            else if (node is UnionClass)
//            {
//                var types = new List<IObjectGraphType>();
//                var union = default(UnionGraphType);
//                while (union == null)
//                {
//                    var item = _nodes.Pop();
//                    if (item is UnionGraphType) union = item as UnionGraphType;
//                    else types.Add(item as IObjectGraphType);
//                }
//                foreach (var type in types) union.AddPossibleType(type);
//                _nodes.Push(union);
//            }
//        }

//        public override void VisitConstraint(Constraint schema)
//        {
//            var resolver = schema.Resolver;
//            var currentField = new FieldType
//            {
//                Name = schema.Name,
//                Resolver = resolver == null ? null : new GenericAsyncResolver(resolver)
//            };
//            _nodes.Push(currentField);
//        }

//        public override void VisitAbstractClass(TypeEntity schema)
//        {
//            var currentComplex = CreateComplexType(schema, out _);
//            _nodes.Push(currentComplex);
//        }

//        public override void VisitClass(TypeEntity schema)
//        {
//            var currentComplex = CreateComplexType(schema, out _);
//            _nodes.Push(currentComplex);
//            //if (exists) return;
//            //if (schema.Parent != null) VisitInheritanceConstraint(schema.Parent);
//        }

//        //protected void VisitInheritanceConstraint(TypeEntity type)
//        //{
//        //    var derived = (IObjectGraphType)_nodes.Pop();
//        //    type.AcceptVisitor(this);
//        //    var abstractType = (IInterfaceGraphType)_currentComplex;
//        //    derived.AddResolvedInterface(abstractType);
//        //    derived.IsTypeOf = relation => ((Entity)((Relation)relation).Target).IsTypeOf(type);
//        //    _currentComplex = derived;
//        //}

//        public override void VisitAttributeClass(AttributeClass schema)
//        {

//        }

//        public override void VisitUnionClass(UnionClass schema)
//        {
//            var currentUnion = new UnionGraphType { Name = schema.Name };
//            _nodes.Push(currentUnion);
//        }

//        private IComplexGraphType CreateComplexType(TypeEntity type, out bool exists)
//        {
//            if (exists = ComplexTypes.ContainsKey(type.Name)) return ComplexTypes[type.Name];
//            if (type.IsAbstract) return new InterfaceGraphType { Name = type.Name };
//            return new ObjectGraphType { Name = type.Name };
//        }

//        //private static void CreateFieldType(Constraint constraint, ref FieldType fieldType, IGraphType resolvedType)
//        //{
//        //    var isArray = constraint.IsArray;
//        //    var isRequired = constraint.IsRequired;
//        //    if (isRequired && !isArray) resolvedType = new NonNullGraphType(resolvedType);
//        //    if (isArray) resolvedType = new NonNullGraphType(new ListGraphType(new NonNullGraphType(resolvedType)));
//        //    var resolver = constraint.Resolver;
//        //    fieldType.Resolver = resolver == null ? null : new GenericAsyncResolver(resolver);
//        //    fieldType.ResolvedType = resolvedType;
//        //}
//    }
//}
