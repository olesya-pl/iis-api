using System;
using System.Collections.Generic;
using System.Linq;
using Flee.PublicTypes;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.Ontology.ComputedProperties
{
    public interface IComputedPropertyResolver
    {
        object Resolve(EmbeddingRelationType relationType, Node node);
        List<string> GetRequiredFields(EmbeddingRelationType relationType);
    }

    public class ComputedPropertyResolver : IComputedPropertyResolver
    {
        // Contour legacy formulas
        private string CleanFormula(string formula)
        {
            formula = formula.Replace("entity.", ""); // remove prefix from existing formulas, remove later
            formula = formula.Replace("h.", ""); // remove prefix from existing formulas, remove later
            return formula;
        }

        private string GetFormula(EmbeddingRelationType relationType)
        {
            var formula = (relationType.Meta as AttributeRelationMeta)?.Formula;
            if (formula == null)
                throw new ArgumentException($"No formula found in computed property {relationType.Name}");
            return CleanFormula(formula);
        }

        public object Resolve(EmbeddingRelationType relationType, Node node)
        {
            var formula = GetFormula(relationType);
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(ComputedPropertyFunctions));
            context.Variables.ResolveVariableType += (sender, args)
                => { args.VariableType = node.Type.GetProperty(args.VariableName).TargetType.ClrType; };
            context.Variables.ResolveVariableValue += (sender, args)
                => { args.VariableValue = node.GetAttributeValue(args.VariableName); };
            var eDynamic = context.CompileDynamic(formula);
            var result = eDynamic.Evaluate();
            return result;
        }

        public List<string> GetRequiredFields(EmbeddingRelationType relationType)
        {
            var formula = GetFormula(relationType);
            var list = new List<string>();
            var context = new ExpressionContext();
            context.Imports.AddType(typeof(ComputedPropertyFunctions));
            context.Variables.ResolveVariableType += (sender, args) =>
            {
                list.Add(args.VariableName);
                args.VariableType = typeof(object);
            };
            var eDynamic = context.CompileDynamic(formula);
            return list;
        }
    }
}
