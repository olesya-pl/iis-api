using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Flee.PublicTypes;
using Iis.Domain;
using Iis.Domain.Meta;
using Iis.Interfaces.Meta;

namespace IIS.Core.Ontology.ComputedProperties
{
    public interface IComputedPropertyResolver
    {
        object Resolve(Guid nodeId, string formula);
    }

    public class ComputedPropertyResolver : IComputedPropertyResolver
    {
        IOntologyService _ontologyService;
        public ComputedPropertyResolver(IOntologyService ontologyService)
        {
            _ontologyService = ontologyService;
        }
        // Contour legacy formulas

        private string ReplaceVariables(Guid nodeId, string formula)
        {
            var regex = new Regex("[^{]*{([^}]+)}");
            var matches = regex.Matches(formula);
            var result = formula;
            foreach (Match match in matches)
            {
                var dotName = match.Groups[1].ToString();
                var value = _ontologyService.GetAttributeValueByDotName(nodeId, dotName);
                result = result.Replace("{" + dotName + "}", "\"" + value + "\"");
            }
            return result;
        }
        public object Resolve(Guid nodeId, string formula)
        {
            var singleFormulas = formula.Split(';').Select(s => s.Trim());
            string value = null;
            foreach (var singleFormula in singleFormulas)
            {
                value = ResolveSingleFormula(nodeId, singleFormula)?.ToString();
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
            return value;
        }
        private object ResolveSingleFormula(Guid nodeId, string formula)
        {
            var replaced = ReplaceVariables(nodeId, formula);

            var context = new ExpressionContext();
            context.Imports.AddType(typeof(ComputedPropertyFunctions));
            var eDynamic = context.CompileDynamic(replaced);
            var result = eDynamic.Evaluate();
            return result;
        }
    }
}
