﻿using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiRelationAttributeControl: UiTypeViewControl, IUiNodeTypeControl
    {
        private TextBox txtName;
        private TextBox txtTitle;
        private RichTextBox txtMeta;
        private RichTextBox txtAliases;
        private ComboBox cmbEmbedding;
        private ComboBox cmbScalarType;
        private Button btnSave;
        private UiControlsCreator _uiControlsCreator;

        public event Action<INodeTypeUpdateParameter> OnSave;
        public UiRelationAttributeControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
            
            _container.Add(cmbEmbedding = _uiControlsCreator.GetEnumComboBox(typeof(EmbeddingOptions)), "Embedding Options");
            _container.Add(cmbScalarType = _uiControlsCreator.GetEnumComboBox(typeof(ScalarType)), "ScalarType");

            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); };
            _container.Add(btnSave);

            _container.GoToNewColumn();
            _container.Add(txtMeta = new RichTextBox(), "Meta", true);
            _container.GoToNewColumn();
            _container.Add(txtAliases = new RichTextBox(), "Aliases", true);
        }
        public void CreateNew()
        {
            txtId.Clear();
            txtName.Clear();
            txtTitle.Clear();
            txtMeta.Clear();
            txtAliases.Clear();
            cmbEmbedding.SelectedIndex = 0;
            cmbScalarType.SelectedIndex = 0;
        }
        public void SetUiValues(INodeTypeLinked nodeType)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            txtMeta.Text = nodeType.Meta;
            txtAliases.Lines = nodeType.Aliases?.Split(',');
            _uiControlsCreator.SetSelectedValue(cmbEmbedding, nodeType.RelationType.EmbeddingOptions.ToString());
            _uiControlsCreator.SetSelectedValue(cmbScalarType, nodeType.RelationType.TargetType.AttributeType.ScalarType.ToString());
        }
        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            var isNew = string.IsNullOrEmpty(txtId.Text);
            var aliases = string.IsNullOrWhiteSpace(txtAliases.Text) ?
                null :
                string.Join(',', txtAliases.Lines.Where(l => !string.IsNullOrWhiteSpace(l)));

            return new NodeTypeUpdateParameter
            {
                Id = isNew ? (Guid?)null : new Guid(txtId.Text),
                Name = isNew ? txtName.Text : null,
                Title = txtTitle.Text,
                Meta = txtMeta.Text,
                Aliases = aliases,
                EmbeddingOptions = (EmbeddingOptions)cmbEmbedding.SelectedItem,
                ScalarType = (ScalarType)cmbScalarType.SelectedItem,
                ParentTypeId = _parentTypeId
            };
        }
    }
}
