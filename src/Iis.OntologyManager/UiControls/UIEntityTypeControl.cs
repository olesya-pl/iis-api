using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyManager.Style;
using Iis.OntologySchema.ChangeParameters;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace Iis.OntologyManager.UiControls
{
    public class UiEntityTypeControl: UiTypeViewControl, IUiNodeTypeControl
    {
        private TextBox txtName;
        private TextBox txtTitle;
        private RichTextBox txtAliases;
        private ComboBox cmbUniqueValueFieldName;
        private DataGridView gridInheritedFrom;
        private DataGridView gridInheritedBy;
        private DataGridView gridEmbeddence;
        private ContextMenuStrip menuChildren;
        private DataGridView gridChildren;
        private Button btnSave;
        private CheckBox cbAbstract;
        private PictureBox iconBox;
        private string IconBase64Body;

        public event Action<IChildNodeType> OnShowRelationType;
        public event Action<IChildNodeType> OnShowTargetType;
        public event Action<INodeTypeLinked> OnShowEntityType;
        public event Action<INodeTypeUpdateParameter> OnSave;
        public event Action<Guid> OnCreateAttribute;
        public event Action<Guid> OnCreateRelationEntity;
        public event Action<IChildNodeType> OnDeleteRelationEntity;
        public event Action OnSetInheritance;
        public event Action<Guid> OnRemoveInheritance;

        public IChildNodeType SelectedChild
        {
            get
            {
                var selectedRow = gridChildren.SelectedRows.Count > 0 ? gridChildren.SelectedRows[0] : null;
                return selectedRow == null ? null : (IChildNodeType)selectedRow.DataBoundItem;
            }
        }
        private INodeTypeLinked SelectedAncestor
        {
            get
            {
                var selectedRow = gridInheritedFrom.SelectedRows.Count > 0 ? gridInheritedFrom.SelectedRows[0] : null;
                return (INodeTypeLinked)selectedRow?.DataBoundItem;
            }
        }

        public UiEntityTypeControl(UiControlsCreator uiControlsCreator)
        {
            _uiControlsCreator = uiControlsCreator;
        }
        public void SetUiValues(INodeTypeLinked nodeType, List<string> aliases)
        {
            txtId.Text = nodeType.Id.ToString("N");
            txtName.Text = nodeType.Name;
            txtTitle.Text = nodeType.Title;
            cbAbstract.Checked = nodeType.IsAbstract;
            txtAliases.Lines = aliases.ToArray();

            var children = nodeType.GetAllChildren()
                .OrderBy(ch => ch.InheritedFrom)
                .ThenBy(ch => ch.RelationName)
                .ToList();
            gridChildren.DataSource = children;
            var attributesList = children
                .Where(ch => ch.Kind == Kind.Attribute)
                .OrderBy(ch => ch.Name)
                .Select(ch => ch.Name)
                .ToList();
            attributesList.Insert(0, string.Empty);
            cmbUniqueValueFieldName.DataSource = attributesList;

            var ancestors = nodeType.GetAllAncestors();
            gridInheritedFrom.DataSource = ancestors;
            gridInheritedBy.DataSource = nodeType.GetDirectDescendants();
            gridEmbeddence.DataSource = nodeType.GetNodeTypesThatEmbedded();

            IconBase64Body = nodeType.IconBase64Body;
            if (!string.IsNullOrEmpty(IconBase64Body))
            {
                var bytes = Convert.FromBase64String(IconBase64Body);
                SetIcon(bytes);
            }
            else
            {
                iconBox.Image = null;
            }
        }
        private bool SetIcon(byte[] bytes) => SetIcon(GetImage(bytes));
        private bool SetIcon(Image image)
        {
            iconBox.Image = image;
            if (image == null) return false;
            Padding p = new Padding
            {
                Left = (iconBox.Width - image.Width) / 2,
                Top = (iconBox.Height - image.Height) / 2
            };
            iconBox.Padding = p;
            return true;
        }
        private Image GetImage(byte[] bytes)
        {

            try
            {
                using (var magickImage = new MagickImage(bytes))
                {
                    magickImage.Format = MagickFormat.Bmp;
                    var bmpBytes = magickImage.ToByteArray();
                    using (var ms = new MemoryStream(bmpBytes))
                    {
                        return new Bitmap(ms);
                    }
                }
            }
            catch
            {
            }
            return null;
        }
        private void RemoveIcon()
        {
            if (MessageBox.Show("Ви дійсно хочете видалити іконку?", "Питання", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                IconBase64Body = null;
                iconBox.Image = null;
            }
        }
        public void CreateNew()
        {
            txtId.Clear();
            txtName.Clear();
            txtTitle.Clear();
            cbAbstract.Checked = false;
            gridChildren.DataSource = null;
            gridInheritedFrom.DataSource = null;
            gridInheritedBy.DataSource = null;
            gridEmbeddence.DataSource = null;
            SetGridsEnabled(false);
        }
        private void SetGridsEnabled(bool enabled)
        {
            gridChildren.Enabled = enabled;
            gridInheritedFrom.Enabled = enabled;
            gridInheritedBy.Enabled = enabled;
            gridEmbeddence.Enabled = enabled;
        }
        protected override void CreateControls()
        {
            _container.Add(txtId = new TextBox { ReadOnly = true }, "Id");
            _container.Add(txtName = new TextBox(), "Name");
            _container.Add(txtTitle = new TextBox(), "Title");
            _container.Add(cbAbstract = new CheckBox { Text = "Abstract", Checked = true });

            cmbUniqueValueFieldName = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "",
                ValueMember = "",
                BackColor = _style.Common.BackgroundColor
            };
            _container.Add(cmbUniqueValueFieldName, "Unique Value Field Name");

            btnSave = new Button { Text = "Save" };
            btnSave.Click += (sender, e) => { OnSave?.Invoke(GetUpdateParameter()); SetGridsEnabled(true); };
            _container.Add(btnSave);
            _container.GoToNewColumn();

            _container.Add(txtAliases = new RichTextBox(), "Aliases", true);
            _container.GoToNewColumn();

            gridInheritedFrom = GetRelationsGrid(nameof(gridInheritedFrom));
            _container.Add(gridInheritedFrom, "Inherited From:", true);
            _container.GoToNewColumn();

            gridInheritedBy = GetRelationsGrid(nameof(gridInheritedBy));
            _container.Add(gridInheritedBy, "Ancestor To:", true);
            _container.GoToNewColumn();

            gridEmbeddence = GetRelationsGrid(nameof(gridEmbeddence));
            _container.Add(gridEmbeddence, "Embedded By:", true);
            _container.GoToNewColumn();

            var btnChooseIcon = new Button { Text = "Вибрати іконку" };
            btnChooseIcon.Click += (sender, e) => ChooseIcon();
            _container.Add(btnChooseIcon);
            var btnRemoveIcon = new Button { Text = "Видалити іконку" };
            btnRemoveIcon.Click += (sender, e) => RemoveIcon();
            _container.Add(btnRemoveIcon);
            iconBox = new PictureBox { Height = 100 };
            _container.Add(iconBox);

            _container.GoToBottom();
            _container.StepDown();
            _container.SetFullWidthColumn();

            var menuInheritance = new ContextMenuStrip();
            menuInheritance.Items.Add("Set Inheritance");
            menuInheritance.Items[0].Click += (sender, e) => { OnSetInheritance?.Invoke(); };
            menuInheritance.Items.Add("Remove Inheritance");
            menuInheritance.Items[1].Click += removeInheritance_Click;
            gridInheritedFrom.ContextMenuStrip = menuInheritance;

            menuChildren = new ContextMenuStrip();
            menuChildren.Items.Add("Show Target Type");
            menuChildren.Items[0].Click += (sender, e) => { gridChildrenEvent(OnShowTargetType); };
            menuChildren.Items.Add("Create Attribute");
            menuChildren.Items[1].Click += (sender, e) => { if (Id != null) OnCreateAttribute((Guid)Id); };
            menuChildren.Items.Add("Create Relation to Entity");
            menuChildren.Items[2].Click += (sender, e) => { if (Id != null) OnCreateRelationEntity((Guid)Id); };
            menuChildren.Items.Add("Delete");
            menuChildren.Items[3].Click += (sender, e) => { if (Id != null) gridChildrenEvent(OnDeleteRelationEntity); };

            menuChildren.Opening += (sender, e) => { menuChildren.Items[0].Visible = SelectedChild?.TargetType?.Kind == Kind.Entity; };

            gridChildren = _uiControlsCreator.GetDataGridView("gridChildren", null,
                new List<string> { "RelationName", "RelationTitle", "Name", "InheritedFrom", "EmbeddingOptions", "ScalarType" });
            gridChildren.Columns[0].Width *= 2;
            gridChildren.Columns[1].Width *= 5;
            gridChildren.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridChildren.ColumnHeadersVisible = true;
            gridChildren.AutoGenerateColumns = false;
            gridChildren.DoubleClick += gridChildren_DoubleClick;
            gridChildren.CellFormatting += gridChildren_CellFormatting;
            gridChildren.ContextMenuStrip = menuChildren;

            _container.Add(gridChildren, null, true);
        }
        private INodeTypeUpdateParameter GetUpdateParameter()
        {
            var isNew = string.IsNullOrEmpty(txtId.Text);
            return new NodeTypeUpdateParameter
            {
                Id = isNew ? (Guid?)null : new Guid(txtId.Text),
                Name = txtName.Text,
                Title = txtTitle.Text,
                IsAbstract = cbAbstract.Checked,
                ParentTypeId = null,
                Aliases = txtAliases.Lines,
                UniqueValueFieldName = string.IsNullOrEmpty(cmbUniqueValueFieldName.Text) ? null : cmbUniqueValueFieldName.Text,
                IconBase64Body = this.IconBase64Body
            };
        }
        private DataGridView GetRelationsGrid(string name)
        {
            var grid = _uiControlsCreator.GetDataGridView(name, null,
                new List<string> { "Name" });
            grid.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.CellFormatting += _style.GridTypes_CellFormatting;
            grid.DoubleClick += gridInheritance_DoubleClick;
            return grid;
        }
        private void gridInheritance_DoubleClick(object sender, EventArgs e)
        {
            if (OnShowEntityType == null) return;
            var grid = (DataGridView)sender;
            var selectedRow = grid.SelectedRows.Count > 0 ? grid.SelectedRows[0] : null;
            var selectedNodeType = (INodeTypeLinked)selectedRow?.DataBoundItem;
            if (selectedNodeType != null)
            {
                OnShowEntityType(selectedNodeType);
            }
        }
        private void removeInheritance_Click(object sender, EventArgs e)
        {
            if (SelectedAncestor != null)
            {
                OnRemoveInheritance?.Invoke(SelectedAncestor.Id);
            }
        }
        private void gridChildrenEvent(Action<IChildNodeType> action)
        {
            if (action == null) return;
            if (SelectedChild != null)
            {
                action(SelectedChild);
            }
        }
        private void gridChildren_DoubleClick(object sender, EventArgs e)
        {
            gridChildrenEvent(OnShowRelationType);
        }
        private void gridChildren_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var grid = (DataGridView)sender;
            var nodeType = (IChildNodeType)grid.Rows[e.RowIndex].DataBoundItem;
            if (nodeType == null) return;
            var row = (DataGridViewRow)grid.Rows[e.RowIndex];
            var style = row.DefaultCellStyle;

            style.BackColor =
                nodeType.Kind == Kind.Entity ?
                    _style.GetColorByAncestor(nodeType.TargetType) :
                    _style.GetColorByNodeTypeKind(nodeType.Kind);

            style.SelectionBackColor = style.BackColor;
            style.SelectionForeColor = grid.DefaultCellStyle.ForeColor;
            style.Font = row.Selected ? _style.SelectedFont : _style.DefaultFont;
        }
        private void ChooseIcon()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var bytes = File.ReadAllBytes(dialog.FileName);
                IconBase64Body = Convert.ToBase64String(bytes);
                if (!SetIcon(bytes))
                {
                    MessageBox.Show("Здається це не іконка");
                }
            }
        }
    }
}
