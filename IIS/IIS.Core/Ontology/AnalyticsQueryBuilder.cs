using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology {
    class AnalyticsQueryBuilder
    {
        private AnalyticsQueryParser _queryParser;
        private AnalyticsQueryParser.Ast _matchQuery;
        private List<Expr> _groups;
        private Expr _aggregation;

        private List<Expr> _conditions;

        public AnalyticsQueryBuilder(Ontology ontology)
        {
            _queryParser = new AnalyticsQueryParser(ontology);
        }

        public static AnalyticsQueryBuilder From(Ontology ontology)
        {
            return new AnalyticsQueryBuilder(ontology);
        }

        public AnalyticsQueryBuilder Match(string query)
        {
            _matchQuery = _matchQuery == null
                ? _queryParser.Parse(query)
                : _queryParser.ParseUsing(_matchQuery, query);
            return this;
        }

        public AnalyticsQueryBuilder GroupBy(params string[] groups)
        {
            _groups = new List<Expr>();
            foreach (var group in groups)
            {
                _groups.Add(_parseExpr(group));
            }

            return this;
        }

        public AnalyticsQueryBuilder Sum(string expr)
        {
            _aggregation = _parseExpr(expr, "SUM");
            return this;
        }


        public AnalyticsQueryBuilder Count(string expr)
        {
            _aggregation = _parseExpr(expr, "COUNT");
            return this;
        }

        public AnalyticsQueryBuilder Where(string expr, params object[] value)
        {
            var parsedExpr = _parseExpr(expr);


            return this;
        }

        private Expr _parseExpr(string expr, string operation = null)
        {
            var chunks = expr.Split('.', 2);

            if (!_matchQuery.HasRef(chunks[0]))
            {
                throw new InvalidOperationException($"Unknown reference {chunks[0]} while parsing expression '{expr}'");
            }

            return new Expr(chunks[0], chunks[1], operation: operation);
        }

        public (string, Dictionary<string, object>) ToSQL()
        {
            var generator = new SQLGenerator(_matchQuery, _groups, _aggregation);
            return generator.Generate();
        }

        class Expr {
            public readonly string Ref;
            public readonly string Field;
            public readonly string Op;

            public Expr(string refName, string fieldName, string operation)
            {
                Ref = refName;
                Field = fieldName;
                Op = operation;
            }
        }

        class SQLGenerator {
            private readonly AnalyticsQueryParser.Ast _ast;
            private readonly List<Expr> _groups;
            private readonly Expr _agg;
            private Dictionary<string, string> _refToAlias = new Dictionary<string, string>();
            private Dictionary<string, string> _tables = new Dictionary<string, string>() {
                { "AstNode", "Nodes" },
                { "AstRelation", "Relations" },
                { "AstAttribute", "Attributes" },
            };
            private Dictionary<string, object> _sqlParams = new Dictionary<string, object>();

            private Dictionary<ScalarType, string> _dataTypesToSqlTypes = new Dictionary<ScalarType, string>() {
                { ScalarType.Integer, "integer" },
                { ScalarType.Decimal, "double" },
                { ScalarType.Boolean, "boolean" },
                { ScalarType.DateTime, "datetime" }
            };

            public SQLGenerator(AnalyticsQueryParser.Ast ast, List<Expr> groups, Expr agg)
            {
                _ast = ast;
                _groups = groups;
                _agg = agg;
            }

            public (string, Dictionary<string, object>) Generate() {
                var matches = _genMatches();
                var groups = _genGroups();
                var sql = $@"
                  SELECT {_genAgg()}, {groups}
                  FROM {matches}
                  GROUP BY {groups}
                ";

                return (sql, _sqlParams);
            }

            private string _genAgg()
            {
                var alias = _refToAlias[_agg.Ref];
                var node = _ast.nodeByRef(_agg.Ref);
                var field = $"\"{_agg.Field}\"";

                if (node is AnalyticsQueryParser.AstAttribute attr && attr.Type != null)
                {
                    var type = (AttributeType)attr.Type;

                    if (_dataTypesToSqlTypes.ContainsKey(type.ScalarTypeEnum))
                    {
                        field += $"::{_dataTypesToSqlTypes[type.ScalarTypeEnum]}";
                    }
                }

                return $"{_agg.Op}({alias}.{field})";
            }

            private string _genGroups()
            {
                var groups = new string[_groups.Count];

                for (var i = 0; i < _groups.Count; i++)
                {
                    var expr = _groups[i];
                    var exprAlias = _refToAlias[expr.Ref];
                    groups[i] = $"{exprAlias}.\"{expr.Field}\"";
                }

                return string.Join(',', groups);
            }

            private string _genMatches() {
                var sql = "";
                var node = _ast.Root.Next;
                var i = 1;

                while (node != null)
                {
                    if (!(node is AnalyticsQueryParser.AstRef))
                    {
                        sql += _genNode(node, i++) + '\n';
                    }

                    node = node.Next;
                }

                return _genRootMatches(_ast.Root, sql);
            }

            private string _genNode(AnalyticsQueryParser.AstNode node, int index, string joinType = "INNER JOIN")
            {
                var (conditionsAlias, alias, sql) = _genFullNodeJoin(node, index, joinType);
                return sql + " AND " + _genNodeConditions(node, conditionsAlias, alias);
            }

            private (string, string, string) _genFullNodeJoin(AnalyticsQueryParser.AstNode node, int index, string joinType)
            {
                var (table, alias) = _tableAndAliasFor(node, index);
                var sql = $"{joinType} \"{table}\" AS {alias}";

                if (node.Prev != null)
                {
                    var (_, prevAlias) = _tableAndAliasFor(node.Prev, index - 1);

                    if (node is AnalyticsQueryParser.AstRelation rel)
                    {
                        var fieldName = rel.IsDirect ? "SourceNodeId" : "TargetNodeId";
                        sql += $" ON {alias}.\"{fieldName}\" = {prevAlias}.\"Id\"";
                    }
                    else
                    {
                        var prevRel = (AnalyticsQueryParser.AstRelation)node.Prev;
                        var fieldName = prevRel.IsDirect ? "TargetNodeId" : "SourceNodeId";
                        sql += $" ON {alias}.\"Id\" = {prevAlias}.\"{fieldName}\"";
                    }
                }

                var conditionsAlias = alias;

                if (_isRequireNodesTable(node))
                {
                    conditionsAlias = $"{alias}_node";
                    sql += $"\nINNER JOIN \"{_tables["AstNode"]}\" {conditionsAlias} ON {conditionsAlias}.\"Id\" = {alias}.\"Id\"";
                }

                if (node.Name != null)
                {
                    _refToAlias.Add(node.Name, alias);
                }

                return (conditionsAlias, alias, sql);
            }

            private string _genNodeConditions(AnalyticsQueryParser.AstNode node, string conditionsAlias, string alias)
            {
                var conditions = new List<string>();

                if (node.TypeIds != null)
                {
                    var paramName = $"{conditionsAlias}_typeIds";
                    _sqlParams.Add(paramName, node.TypeIds);
                    conditions.Add($"{conditionsAlias}.\"TypeId\" IN (@{paramName})");
                }

                conditions.Add($"{conditionsAlias}.\"IsArchived\" = false");

                if (node is AnalyticsQueryParser.AstAttribute attr && attr.Conditions != null)
                {
                    var paramName = $"{alias}_value";
                    _sqlParams.Add(paramName, (string)attr.Conditions["Value"]);
                    conditions.Add($"{alias}.\"Value\" = @{paramName}");
                }

                return string.Join(" AND ", conditions);
            }

            private (string, string) _tableAndAliasFor(AnalyticsQueryParser.AstNode node, int index)
            {
                var table = _tables[node.NodeType];
                var alias = node.Name ?? $"{table[0].ToString().ToLower()}_{index}";
                return (table, alias);
            }

            private bool _isRequireNodesTable(AnalyticsQueryParser.AstNode node)
            {
                return node is AnalyticsQueryParser.AstRelation || node is AnalyticsQueryParser.AstAttribute;
            }

            private string _genRootMatches(AnalyticsQueryParser.AstNode node, string sql)
            {
                var (conditionsAlias, alias, rootSql) = _genFullNodeJoin(node, 0, "");

                return rootSql + '\n' + sql + $"\nWHERE {_genNodeConditions(node, conditionsAlias, alias)}";
            }
        }
    }
}