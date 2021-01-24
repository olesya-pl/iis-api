using System.Text;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.Ontology {
    public class AnalyticsQueryBuilder
    {
        private AnalyticsQueryParser _queryParser;
        private AnalyticsQueryParser.Ast _matchQuery;
        private List<Expr> _groups;
        private Expr _aggregation;

        private Expr[] _select;

        private List<ConditionExpr> _conditions;

        private static string[] _possibleOperators = new string[] { ">", ">=", "<", "<=", "=" };

        public AnalyticsQueryBuilder(IOntologySchema ontologySchema)
        {
            _queryParser = new AnalyticsQueryParser(ontologySchema);
        }

        public static AnalyticsQueryBuilder From(IOntologySchema ontologySchema)
        {
            return new AnalyticsQueryBuilder(ontologySchema);
        }

        public AnalyticsQueryBuilder Load(AnalyticsQueryBuilderConfig config)
        {
            if (config.Match != null)
            {
                foreach (var condition in config.Match)
                    Match(condition);
            }

            if (config.Select != null)
                Select(config.Select);

            if (config.Sum != null)
                Sum(config.Sum);

            if (config.Count != null)
                Count(config.Count);

            if (config.GroupBy != null)
                GroupBy(config.GroupBy);

            if (config.Conditions != null)
            {
                foreach (var condition in config.Conditions)
                    Where(condition[0], condition[1], condition[2]);
            }

            return this;
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

        public AnalyticsQueryBuilder Select(params string[] exprs)
        {
            _select = new Expr[exprs.Length];

            for (var i = 0; i< exprs.Length; i++)
                _select[i] = _parseExpr(exprs[i]);

            return this;
        }

        public AnalyticsQueryBuilder Where(string rawExpr, string op, object value)
        {
            if (!_possibleOperators.Contains(op))
                throw new ArgumentException($"Unknown conditional operator {op}. Supported operators {string.Join(',', _possibleOperators)}");

            if (_conditions == null)
                _conditions = new List<ConditionExpr>();

            var expr = _parseExpr(rawExpr);
            _conditions.Add(new ConditionExpr(expr, op, value));

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
            var conditions = _conditions != null ? _conditions.ToArray() : null;
            var generator = new SQLGenerator(_matchQuery, _groups, _aggregation, _select, conditions);
            return generator.Generate();
        }

        class Expr
        {
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

        class ConditionExpr
        {
            public readonly Expr Expr;
            public readonly string Op;
            public readonly object Value;

            public ConditionExpr(Expr expr, string op, object value)
            {
                Expr = expr;
                Op = op;
                Value = value;
            }
        }

        class SQLGenerator {
            private readonly AnalyticsQueryParser.Ast _ast;
            private readonly List<Expr> _groups;
            private readonly Expr _agg;
            private readonly Expr[] _select;
            private Dictionary<string, string> _refToAlias = new Dictionary<string, string>();
            private Dictionary<string, string> _tables = new Dictionary<string, string>() {
                { "AstNode", "Nodes" },
                { "AstRelation", "Relations" },
                { "AstAttribute", "Attributes" },
            };
            private Dictionary<string, object> _sqlParams = new Dictionary<string, object>();
            private ConditionExpr[] _conditions;

            private Dictionary<ScalarType, string> _dataTypesToSqlTypes = new Dictionary<ScalarType, string>() {
                { ScalarType.Int, "integer" },
                { ScalarType.Decimal, "double" },
                { ScalarType.Boolean, "boolean" },
                { ScalarType.Date, "timestamp" }
            };

            public SQLGenerator(AnalyticsQueryParser.Ast ast, List<Expr> groups, Expr agg, Expr[] select, ConditionExpr[] conditions)
            {
                _ast = ast;
                _groups = groups;
                _agg = agg;
                _select = select;
                _conditions = conditions;
            }

            public (string, Dictionary<string, object>) Generate() {
                var matches = _genMatches();
                var groups = _genGroups();
                var selectFields = new List<string> { _genSelect(), groups, _genAgg() };
                var select = string.Join(',', selectFields.Where(part => part != null));
                var sql = $"SELECT {select} FROM {matches}";

                if (_conditions != null)
                    sql += $"\nAND {_genWhere()}";

                if (groups != null)
                    sql += $"\nGROUP BY {groups}";

                return (sql, _sqlParams);
            }

            private string _genSelect()
            {
                if (_select == null)
                    return null;

                var fields = new string[_select.Length];

                for (var i = 0; i < fields.Length; i++)
                {
                    var fieldExpr = _select[i];
                    fields[i] = _stringifyExpr(fieldExpr);
                }

                return string.Join(',', fields);
            }

            private string _stringifyExpr(Expr expr, bool castType = false)
            {
                var field = $"\"{expr.Field}\"";

                if (expr.Ref == null)
                    return field;

                var alias = _refToAlias[expr.Ref];
                var node = _ast.nodeByRef(expr.Ref);

                if (castType && node is AnalyticsQueryParser.AstAttribute attr && attr.Type != null)
                {
                    var type = attr.Type;

                    if (_dataTypesToSqlTypes.ContainsKey(type.ScalarTypeEnum))
                        field += $"::{_dataTypesToSqlTypes[type.ScalarTypeEnum]}";
                }

                return $"\"{alias}\".{field}";
            }

            private string _genAgg()
            {
                if (_agg == null)
                    return null;

                return $"{_agg.Op}({_stringifyExpr(_agg, castType: true)})";
            }

            private string _genGroups()
            {
                if (_groups == null)
                    return null;

                var groups = new string[_groups.Count];

                for (var i = 0; i < _groups.Count; i++)
                    groups[i] = _stringifyExpr(_groups[i]);

                return string.Join(',', groups);
            }

            private string _genMatches() {
                var sql = new StringBuilder();
                var node = _ast.Root.Next;
                var i = 1;

                while (node != null)
                {
                    if (!(node is AnalyticsQueryParser.AstRef))
                    {
                        sql.Append(_genNode(node, i++));
                        sql.Append('\n');
                    }

                    node = node.Next;
                }

                return _genRootMatches(_ast.Root, sql.ToString());
            }

            private string _genNode(AnalyticsQueryParser.AstNode node, int index, string joinType = "INNER JOIN")
            {
                var (conditionsAlias, alias, sql) = _genFullNodeJoin(node, index, joinType);
                return sql + " AND " + _genNodeConditions(node, conditionsAlias, alias);
            }

            private (string, string, string) _genFullNodeJoin(AnalyticsQueryParser.AstNode node, int index, string joinType)
            {
                var (table, alias) = _tableAndAliasFor(node, index);
                var sql = $"{joinType} \"{table}\" AS \"{alias}\"";

                if (node.Prev != null)
                {
                    var (_, prevAlias) = _tableAndAliasFor(node.Prev, index - 1);

                    if (node is AnalyticsQueryParser.AstRelation rel)
                    {
                        var fieldName = rel.IsDirect ? "SourceNodeId" : "TargetNodeId";
                        sql += $" ON \"{alias}\".\"{fieldName}\" = \"{prevAlias}\".\"Id\"";
                    }
                    else
                    {
                        var prevRel = (AnalyticsQueryParser.AstRelation)node.Prev;
                        var fieldName = prevRel.IsDirect ? "TargetNodeId" : "SourceNodeId";
                        sql += $" ON \"{alias}\".\"Id\" = \"{prevAlias}\".\"{fieldName}\"";
                    }
                }

                var conditionsAlias = alias;

                if (_isRequireNodesTable(node))
                {
                    conditionsAlias = $"{alias}_node";
                    sql += $"\nINNER JOIN \"{_tables["AstNode"]}\" \"{conditionsAlias}\" ON \"{conditionsAlias}\".\"Id\" = \"{alias}\".\"Id\"";
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
                    var paramName = $"@{conditionsAlias}_typeIds";
                    var paramNames = new string[node.TypeIds.Length];

                    for (var i = 0; i < node.TypeIds.Length; i++)
                    {
                        paramNames[i] = $"{paramName}{i}";
                        _sqlParams.Add(paramNames[i], node.TypeIds[i]);
                    }
                    conditions.Add($"\"{conditionsAlias}\".\"NodeTypeId\" IN ({string.Join(',', paramNames)})");
                }

                conditions.Add($"\"{conditionsAlias}\".\"IsArchived\" = false");

                if (node is AnalyticsQueryParser.AstAttribute attr && attr.Conditions != null)
                {
                    var paramName = $"@{alias}_value";
                    _sqlParams.Add(paramName, (string)attr.Conditions["Value"]);
                    conditions.Add($"\"{alias}\".\"Value\" = {paramName}");
                }

                return string.Join(" AND ", conditions);
            }

            private (string, string) _tableAndAliasFor(AnalyticsQueryParser.AstNode node, int index)
            {
                var table = _tables[node.INodeTypeLinked];
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

            private string _genWhere()
            {

                var conditions = new string[_conditions.Length];

                for (var i = 0; i < _conditions.Length; i++)
                {
                    var condition = _conditions[i];
                    var paramName = $"@condition{i}";
                    conditions[i] = _stringifyExpr(condition.Expr, castType: true) + condition.Op + $" {paramName}";
                    _sqlParams.Add(paramName, _castValue(condition));
                }

                return string.Join(" AND ", conditions);
            }

            private object _castValue(ConditionExpr condition)
            {
                var node = _ast.nodeByRef(condition.Expr.Ref);

                if (!(node is AnalyticsQueryParser.AstAttribute) || node.Type == null)
                    return condition.Value;

                var attrType = node.Type;

                return AttributeType.ParseValue(condition.Value.ToString(), attrType.ScalarTypeEnum);
            }
        }
    }

    public class AnalyticsQueryBuilderConfig
    {
        public string[] Match { get; set; }
        public string[] Select { get; set; }
        public string[] GroupBy { get; set; }
        public string Sum { get; set; }
        public string Count { get; set; }
        public string[][] Conditions { get; set; }
        public string StartDateField { get; set; }
        public string EndDateField { get; set; }
    }
}