using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiNodeTypeSearch: UIBaseControl
    {
        RichTextBox txtLog;
        IOntologySchema _schema;
        
        public UiNodeTypeSearch(IOntologySchema schema)
        {
            _schema = schema;
        }
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("Filters", panels.panelTop);

            AddSearchButton(container, "IsAggregated", GetWithAggregation);
            AddSearchButton(container, "Format", GetWithFormat);
            AddSearchButton(container, "IsImportantRelation", GetWithImportantRelation);
            AddSearchButton(container, "TargetTypes", GetWithTargetTypes);
            AddSearchButton(container, "AcceptsOperations", GetWithAcceptOperations);

            container.GoToNewColumn();

            AddSearchButton(container, "FormField", GetWithFormFieldType);
            AddSearchButton(container, "FormFieldLines", GetWithFormFieldLines);
            AddSearchButton(container, "FormFieldHint", GetWithFormFieldHint);
            AddSearchButton(container, "FormFieldIcon", GetWithFormFieldIcon);



            txtLog = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
            panels.panelBottom.Controls.Add(txtLog);
        }

        private void AddSearchButton(UiContainerManager container, string text, Func<string> func)
        {
            var btn = new Button { Text = text, MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btn.Click += (sender, e) => Run(func);
            container.Add(btn);
        }

        private IEnumerable<INodeTypeLinked> GetRelationNodeTypes() =>
            _schema.GetAllNodeTypes().Where(nt => nt.Kind == Kind.Relation);

        public string GetWithAggregation()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.MetaObject.IsAggregated == true);

            return ConvertToString(nodeTypes, nt => nt.MetaObject.IsAggregated.ToString());
        }

        public string GetWithImportantRelation()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.MetaObject.IsImportantRelation == true);

            return ConvertToString(nodeTypes, nt => nt.MetaObject.IsImportantRelation.ToString());
        }

        public string GetWithFormat()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => !string.IsNullOrEmpty(nt.MetaObject.Format));

            return ConvertToString(nodeTypes, nt => nt.MetaObject.Format);
        }

        public string GetWithTargetTypes()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.MetaObject.TargetTypes != null && nt.MetaObject.TargetTypes.Length > 0);

            return ConvertToString(nodeTypes, nt => ConvertToString(nt.MetaObject.TargetTypes));
        }

        public string GetWithAcceptOperations()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.MetaObject.AcceptsEntityOperations != null);

            return ConvertToString(nodeTypes, nt => $"{nt.MetaObject.AcceptsEntityOperations.Length}");
        }

        public string GetWithFormFieldLines()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.MetaObject.FormField?.Lines != null);

            return ConvertToString(nodeTypes, nt => nt.MetaObject.FormField.Lines.ToString());
        }

        public string GetWithFormFieldHint()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => !string.IsNullOrEmpty(nt.MetaObject.FormField?.Hint));

            return ConvertToString(nodeTypes, nt => nt.MetaObject.FormField.Hint);
        }

        public string GetWithFormFieldType()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => !string.IsNullOrEmpty(nt.MetaObject.FormField?.Type));

            return ConvertToString(nodeTypes, nt => $"{nt.TargetType.Name} => {nt.MetaObject.FormField.Type}");
        }

        public string GetWithFormFieldIcon()
        {
            var nodeTypes = GetRelationNodeTypes()
                .Where(nt => nt.TargetType.Name == "Country" && string.IsNullOrEmpty(nt.MetaObject.FormField?.Icon));

            return ConvertToString(nodeTypes, nt => $"{nt.TargetType.Name} => {nt.MetaObject.FormField?.Icon}");
        }

        private string ConvertToString(
            IEnumerable<INodeTypeLinked> nodeTypes, 
            Func<INodeTypeLinked, string> infoFunc)
        {
            var sb = new StringBuilder();

            foreach (var nodeType in nodeTypes)
            {
                var sourceType = nodeType.RelationType.SourceType;
                var targetLabel = nodeType.RelationType.TargetType.Kind == Kind.Entity ? "E" : "R";
                sb.Append($"({targetLabel}) {sourceType.Name}.{nodeType.Name}: \t\t");
                if (infoFunc != null) sb.Append(infoFunc(nodeType));
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string ConvertToString(IEnumerable<string> strings)
        {
            if (strings == null) return string.Empty;

            var sb = new StringBuilder();

            foreach (var str in strings)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(str);
            }

            return sb.ToString();
        }

        private void Run(Func<string> func)
        {
            txtLog.Text = func();
        }

    }
}
