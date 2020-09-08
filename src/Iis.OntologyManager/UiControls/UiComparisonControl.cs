using Iis.DataModel;
using Iis.DbLayer.OntologySchema;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Parameters;
using Iis.OntologySchema.Saver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiComparisonControl: UIBaseControl
    {
        List<IOntologySchemaSource> _schemaSources;
        OntologySchemaService _schemaService;
        IOntologySchema _schema;
        ISchemaCompareResult _compareResult;

        ComboBox cmbSchemaSourcesCompare;
        RichTextBox txtComparison;
        CheckBox cbComparisonCreate;
        CheckBox cbComparisonUpdate;
        CheckBox cbComparisonDelete;
        CheckBox cbComparisonAliases;

        public UiComparisonControl(List<IOntologySchemaSource> schemaSources,
            OntologySchemaService schemaService,
            IOntologySchema schema) 
        {
            _schemaSources = schemaSources;
            _schemaService = schemaService;
            _schema = schema;
        }
        
        protected override void CreateControls()
        {
            MainPanel.SuspendLayout();
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 100, 10);
            var container = new UiContainerManager("Comparison", panels.panelTop);

            cmbSchemaSourcesCompare = new ComboBox
            {
                Name = "cmbSchemaSourcesCompare",
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Title",
                BackColor = panels.panelTop.BackColor
            };
            var src = new List<IOntologySchemaSource>(_schemaSources);
            cmbSchemaSourcesCompare.DataSource = src;
            cmbSchemaSourcesCompare.SelectedIndexChanged += (sender, e) => { CompareSchemas(); };
            container.Add(cmbSchemaSourcesCompare);

            var btnComparisonUpdate = new Button { Text = "Update database", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            btnComparisonUpdate.Click += (sender, e) => { UpdateComparedDatabase(); };
            container.Add(btnComparisonUpdate);

            container.GoToNewColumn();
            container.Add(cbComparisonCreate = new CheckBox { Text = "Create", Checked = true });
            container.Add(cbComparisonUpdate = new CheckBox { Text = "Update", Checked = true });
            container.Add(cbComparisonDelete = new CheckBox { Text = "Delete" });
            container.Add(cbComparisonAliases = new CheckBox { Text = "Aliases" });

            txtComparison = new RichTextBox
            {
                ReadOnly = true,
                Dock = DockStyle.Fill,
                BackColor = MainPanel.BackColor
            };
            panels.panelBottom.Controls.Add(txtComparison);

            MainPanel.ResumeLayout();
        }
        private string GetCompareText(ISchemaCompareResult compareResult)
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetCompareText("NODES TO ADD", compareResult.ItemsToAdd.Select(item => item.GetStringCode())));
            sb.AppendLine(GetCompareText("NODES TO DELETE", compareResult.ItemsToDelete.Select(item => item.GetStringCode())));
            sb.AppendLine("===============");
            sb.AppendLine("NODES TO UPDATE");
            sb.AppendLine("===============");
            foreach (var item in compareResult.ItemsToUpdate)
            {
                sb.AppendLine(item.NodeTypeFrom.GetStringCode());
                var differences = item.NodeTypeFrom.GetDifference(item.NodeTypeTo);
                foreach (var diff in differences)
                {
                    sb.AppendLine($"{diff.PropertyName}:\n{diff.OldValue}\n{diff.NewValue}");
                }
                sb.AppendLine();
            }
            sb.AppendLine(GetCompareText("ALIASES TO ADD", compareResult.AliasesToAdd.Select(item => item.ToString())));
            sb.AppendLine(GetCompareText("ALIASES TO DELETE", compareResult.AliasesToDelete.Select(item => item.ToString())));
            sb.AppendLine(GetCompareText("ALIASES TO UPDATE", compareResult.AliasesToUpdate.Select(item => item.ToString())));
            return sb.ToString();
        }
        private string GetCompareText(string title, IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            sb.AppendLine("===============");
            sb.AppendLine(title);
            sb.AppendLine("===============");
            foreach (var line in lines)
            {
                sb.AppendLine(line);
            }

            return sb.ToString();
        }
        private void CompareSchemas()
        {
            var selectedSource = (IOntologySchemaSource)cmbSchemaSourcesCompare.SelectedItem;
            if (selectedSource == null) return;

            var schema = _schemaService.GetOntologySchema(selectedSource);
            _compareResult = _schema.CompareTo(schema);
            txtComparison.Text = GetCompareText(_compareResult);
        }
        private void UpdateComparedDatabase()
        {
            if (_compareResult == null) return;
            if (MessageBox.Show($"Are you sure you want to update database {_compareResult.SchemaSource.Title}?", "Update Database", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                return;
            }
            using var context = OntologyContext.GetContext(_compareResult.SchemaSource.Data);
            var schema = _schemaService.GetOntologySchema(_compareResult.SchemaSource);
            var schemaSaver = new OntologySchemaSaver(context);
            var parameters = new SchemaSaveParameters
            {
                Create = cbComparisonCreate.Checked,
                Update = cbComparisonUpdate.Checked,
                Delete = cbComparisonDelete.Checked,
                Aliases = cbComparisonAliases.Checked
            };

            schemaSaver.SaveToDatabase(_compareResult, schema, parameters);
            CompareSchemas();
        }
    }
}
