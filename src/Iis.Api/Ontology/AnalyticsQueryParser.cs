using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology {
    public class AnalyticsQueryParser {
        private readonly IOntologyModel _ontology;

        public AnalyticsQueryParser(IOntologyModel ontology) {
            _ontology = ontology;
        }

        public Ast Parse(string query)
        {
            return ParseUsing(new Ast(), query);
        }

        public Ast ParseUsing(Ast ast, string query)
        {
            MatchCollection matches = Regex.Matches(query, "@?\\(\\w*:\\w+(\\s+\\{[^}]+\\})?\\)|\\(\\w*\\)|-\\[\\w*:\\w+\\]->|<-\\[\\w*:\\w+\\]-");
            var length = matches.Aggregate(0, (acc, match) => acc + match.Value.Length);

            if (length != query.Length)
            {
                throw new InvalidOperationException($"Unable to parse query '{query}'");
            }

            foreach (Match chunk in matches) {
                var value = chunk.Value.Trim();

                switch (value[0])
                {
                    case '@':
                        _tryToAddAttribute(ast, value.Substring(2, value.Length - 3));
                        break;
                    case '(':
                        _tryToAddNode(ast, value.Substring(1, value.Length - 2));
                        break;
                    case '<':
                        _tryToAddRelation(ast, value.Substring(3, value.Length - 5), isDirect: false);
                        break;
                    case '-':
                        _tryToAddRelation(ast, value.Substring(2, value.Length - 5), isDirect: true);
                        break;
                    default:
                        throw new ArgumentException($"Unexpected symbol '{value[0]}' at first position of the chunk '{value}'");
                }
            }

            return ast;
        }

        private void _tryToAddAttribute(Ast ast, string value)
        {
            var (name, types, conditions) = _parseChunk<INodeTypeModel>(value);
            ast.Add(new AstAttribute(types.FirstOrDefault(), conditions) { Name = name });
        }

        private void _tryToAddNode(Ast ast, string value)
        {
            var (name, types, conditions) = _parseChunk<INodeTypeModel>(value);
            var type = types == null ? null : types.FirstOrDefault();

            if (conditions != null)
            {
                throw new InvalidOperationException($"Unexpected conditions for node '{value}'. Conditions may be used only with Attributes.");
            }

            if (ast.HasRef(name) && type == null)
            {
                ast.Add(new AstRef(ast) { Name = name });
            }
            else
            {
                ast.Add(new AstNode(type) { Name = name });
            }
        }

        private void _tryToAddRelation(Ast ast, string value, bool isDirect)
        {
            var (name, relationTypes, _) = _parseChunk<INodeTypeModel>(value);

            ast.Add(new AstRelation(relationTypes, isDirect) {
                Name = name
            });
        }

        private (string, IEnumerable<T>, string) _parseChunk<T>(string rawValue) where T: INodeTypeModel
        {
            var value = rawValue;
            var conditionsIndex = value.IndexOf('{');
            var conditions = null as string;

            if (conditionsIndex != -1)
            {
                var conditionsEndIndex = value.IndexOf('}', conditionsIndex);
                conditions = value.Substring(conditionsIndex, conditionsEndIndex - conditionsIndex + 1);
                value = value.Substring(0, conditionsIndex).Trim();
            }

            var chunks = value.Contains(':') ? value.Split(':', 2) : new string[] { value, null };
            var name = string.IsNullOrEmpty(chunks[0]) ? null : chunks[0];

            if (chunks[1] == null)
            {
                return (name, null, conditions);
            }

            // TODO: this should be changed to GetType<T>.
            //       Cannot use it right because relation cannot be uniquely identified by its name
            var types = _ontology.GetTypes<T>(chunks[1]);

            if (!types.Any())
            {
                throw new InvalidOperationException($"Unknown entity type {chunks[1]}");
            }

            return (name, types, conditions);
        }

        public class Ast {
            public AstNode Root;
            public AstNode Tail;
            private Dictionary<string, AstNode> _identifiers = new Dictionary<string, AstNode>();

            public void Add(AstNode node)
            {
                if (!node.IsVirtual && node.Name != null)
                {
                    _identifiers.Add(node.Name, node);
                }

                if (Root == null)
                {
                    Root = node;
                    Tail = node;
                }
                else
                {
                    node.Prev = Tail;
                    Tail.Next = node;
                    Tail = node;
                }
            }

            public bool HasRef(string name)
            {
                return !string.IsNullOrEmpty(name) && _identifiers.ContainsKey(name);
            }

            public AstNode nodeByRef(string refName)
            {
                return _identifiers[refName];
            }
        }

        public class AstNode {
            public string Name;
            public virtual INodeTypeModel Type { get; private set; }
            public AstNode Next;
            public AstNode Prev;
            public virtual bool IsVirtual
            {
                get { return false; }
            }

            public virtual string INodeTypeModel
            {
                get { return GetType().Name; }
            }

            public AstNode(INodeTypeModel type)
            {
                Type = type;
            }

            public virtual Guid[] TypeIds
            {
                get { return Type == null ? null : new Guid[] { Type.Id }; }
            }
        }

        public class AstRelation : AstNode {
            public readonly bool IsDirect;

            public readonly IEnumerable<INodeTypeModel> Types;

            public override Guid[] TypeIds
            {
                get { return Types.Select(type => type.Id).ToArray(); }
            }

            public AstRelation(IEnumerable<INodeTypeModel> types, bool isDirect): base(null) {
                Types = types;
                IsDirect = isDirect;
            }
        }

        public class AstRef: AstNode
        {
            public override string INodeTypeModel
            {
                get { return this._ast.nodeByRef(Name).INodeTypeModel; }
            }

            public override INodeTypeModel Type
            {
                get { return this._ast.nodeByRef(Name).Type; }
            }

            private readonly Ast _ast;

            public override bool IsVirtual
            {
                get { return true; }
            }

            public AstRef(Ast ast): base(null) {
                _ast = ast;
            }
        }

        public class AstAttribute : AstNode
        {
            public readonly JObject Conditions;

            public AstAttribute(INodeTypeModel type): base(type)
            {
            }

            public AstAttribute(INodeTypeModel type, string conditions): base(type)
            {
                try {
                    Conditions = conditions == null ? null : JObject.Parse(conditions);
                }
                catch
                {
                    throw new InvalidCastException($"Unable to parse conditions JSON '{conditions}'");
                }
            }
        }
    }
}