using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.Comparison
{
    public class CompareResultForGrid: ISchemaSaveParameters
    {
        public List<CompareResultForGridItem> Items = new List<CompareResultForGridItem>();

        public CompareResultForGrid(ISchemaCompareResult compareResult)
        {
            AddNodeTypeItems(compareResult.ItemsToAdd, CompareResultOperation.Insert);
            AddDifferenceItems(compareResult.ItemsToUpdate);
            AddNodeTypeItems(compareResult.ItemsToDelete, CompareResultOperation.Delete);
            AddAliasItems(compareResult.AliasesToAdd, CompareResultOperation.Insert);
            AddAliasItems(compareResult.AliasesToUpdate, CompareResultOperation.Update);
            AddAliasItems(compareResult.AliasesToDelete, CompareResultOperation.Delete);
        }

        private void AddNodeTypeItems(IReadOnlyList<INodeTypeLinked> nodeTypes, CompareResultOperation operation)
        {
            foreach (var nodeType in nodeTypes)
            {
                var item = new CompareResultForGridItem
                {
                    Checked = true,
                    Title = nodeType.GetStringCode(),
                    NewValue = nodeType.Title,
                    OldValue = null,
                    Operation = operation,
                    ItemType = CompareResultItemType.NodeType,
                    LinkedObject = nodeType
                };

                Items.Add(item);
            }
        }

        private void AddDifferenceItems(IReadOnlyList<ISchemaCompareDiffItem> updateItems)
        {
            foreach (var updateItem in updateItems)
            {
                var differences = updateItem.NodeTypeFrom.GetDifference(updateItem.NodeTypeTo);
                var item = new CompareResultForGridItem
                {
                    Checked = true,
                    Title = $"{updateItem.NodeTypeFrom.Name}",
                    NewValue = GetDiffText(differences, diffInfo => diffInfo.NewValue),
                    OldValue = GetDiffText(differences, diffInfo => diffInfo.OldValue),
                    Operation = CompareResultOperation.Update,
                    ItemType = CompareResultItemType.NodeType,
                    LinkedObject = updateItem
                };

                Items.Add(item);
            }
        }

        private string GetDiffText(IReadOnlyList<ISchemaCompareDiffInfo> differences, Func<ISchemaCompareDiffInfo, string> func)
        {
            var sb = new StringBuilder();
            foreach (var diff in differences)
            {
                sb.AppendLine($"[{diff.PropertyName}]");
                
                var value = func(diff);
                if (value?.Length > 100)
                    value = value.Substring(0, 100) + "...";

                sb.AppendLine(value);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private void AddAliasItems(IReadOnlyList<IAlias> aliases, CompareResultOperation operation)
        {
            foreach (var alias in aliases)
            {
                var item = new CompareResultForGridItem
                {
                    Checked = true,
                    Title = alias.DotName,
                    NewValue = alias.Value,
                    OldValue = null,
                    Operation = operation,
                    ItemType = CompareResultItemType.Alias,
                    LinkedObject = alias
                };

                Items.Add(item);
            }
        }

        public bool IsChecked(object obj) => Items.SingleOrDefault(item => item.LinkedObject == obj)?.Checked ?? false;
    }
}
