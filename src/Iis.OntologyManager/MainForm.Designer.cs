using System.Windows.Forms;

namespace Iis.OntologyManager
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.button1 = new System.Windows.Forms.Button();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelTypeHeader = new System.Windows.Forms.Panel();
            this.panelTypeMain = new System.Windows.Forms.Panel();
            this.nodeTypeTabControl = new System.Windows.Forms.TabControl();
            this.tabNodeTypeMain = new System.Windows.Forms.TabPage();
            this.tabNodeTypeInheritance = new System.Windows.Forms.TabPage();
            this.tabNodeTypeMeta = new System.Windows.Forms.TabPage();
            this.panelMeta = new System.Windows.Forms.Panel();
            this.tabNodeTypeMeta.Controls.Add(panelMeta);
            this.panelLeft = new System.Windows.Forms.Panel();
            this.gridTypes = new System.Windows.Forms.DataGridView();
            this.panelTop.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelRight.SuspendLayout();
            this.nodeTypeTabControl.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelMeta.SuspendLayout();
            this.panelTypeMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTypes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Controls.Add(this.button1);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(800, 100);
            this.panelTop.TabIndex = 0;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(400, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 25);
            this.button1.TabIndex = 0;
            this.button1.Text = "Load Data";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // panelMain
            // 
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Controls.Add(this.panelLeft);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 100);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(800, 350);
            this.panelMain.TabIndex = 1;
            // 
            // panelTypeHeader
            // 
            //this.panelTypeHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //this.panelTypeHeader.Location = new System.Drawing.Point(0, 0);
            //this.panelTypeHeader.Name = "panelTypeHeader";
            //this.panelTypeHeader.TabIndex = 1;
            //// 
            //// panelTypeMain
            //// 
            //this.panelTypeMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //this.panelTypeMain.Dock = System.Windows.Forms.DockStyle.Fill;
            //this.panelTypeMain.Name = "panelTypeHeader";
            //this.panelTypeMain.TabIndex = 2;
            //this.panelTypeMain.Controls.Add(this.nodeTypeTabControl);
            // 
            // panelRight
            // 
            this.panelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //this.panelRight.Controls.Add(panelTypeHeader);
            //this.panelRight.Controls.Add(panelTypeMain);
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(259, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(539, 348);
            this.panelRight.TabIndex = 1;
            //
            // panelMeta
            //
            this.panelMeta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMeta.Name = "panelMeta";
            this.panelMeta.TabIndex = 1;
            // 
            // nodeTypeTabControl
            // 
            this.nodeTypeTabControl.Controls.Add(this.tabNodeTypeMain);
            this.nodeTypeTabControl.Controls.Add(this.tabNodeTypeInheritance);
            this.nodeTypeTabControl.Controls.Add(this.tabNodeTypeMeta);
            this.nodeTypeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nodeTypeTabControl.Location = new System.Drawing.Point(0, 0);
            this.nodeTypeTabControl.Name = "nodeTypeTabControl";
            this.nodeTypeTabControl.SelectedIndex = 0;
            //this.nodeTypeTabControl.Size = new System.Drawing.Size(537, 346);
            this.nodeTypeTabControl.TabIndex = 0;
            // 
            // tabNodeTypeMain
            // 
            this.tabNodeTypeMain.Location = new System.Drawing.Point(4, 24);
            this.tabNodeTypeMain.Name = "tabNodeTypeMain";
            this.tabNodeTypeMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabNodeTypeMain.Size = new System.Drawing.Size(529, 318);
            this.tabNodeTypeMain.TabIndex = 0;
            this.tabNodeTypeMain.Text = "Main";
            this.tabNodeTypeMain.UseVisualStyleBackColor = true;
            // 
            // tabNodeTypeInheritance
            // 
            this.tabNodeTypeInheritance.Location = new System.Drawing.Point(4, 24);
            this.tabNodeTypeInheritance.Name = "tabNodeTypeInheritance";
            this.tabNodeTypeInheritance.Padding = new System.Windows.Forms.Padding(3);
            this.tabNodeTypeInheritance.Size = new System.Drawing.Size(529, 318);
            this.tabNodeTypeInheritance.TabIndex = 1;
            this.tabNodeTypeInheritance.Text = "Inheritance";
            this.tabNodeTypeInheritance.UseVisualStyleBackColor = true;
            // 
            // tabNodeTypeMeta
            // 
            this.tabNodeTypeMeta.Location = new System.Drawing.Point(4, 24);
            this.tabNodeTypeMeta.Name = "tabNodeTypeMeta";
            this.tabNodeTypeMeta.Size = new System.Drawing.Size(529, 318);
            this.tabNodeTypeMeta.TabIndex = 2;
            this.tabNodeTypeMeta.Text = "MetaData";
            // 
            // panelLeft
            // 
            this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLeft.Controls.Add(this.gridTypes);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(259, 348);
            this.panelLeft.TabIndex = 0;
            // 
            // gridTypes
            // 
            this.gridTypes.AutoGenerateColumns = false;
            this.gridTypes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTypes.ColumnHeadersVisible = false;
            this.gridTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTypes.Location = new System.Drawing.Point(0, 0);
            this.gridTypes.Name = "gridTypes";
            this.gridTypes.RowHeadersVisible = false;
            this.gridTypes.Size = new System.Drawing.Size(257, 346);
            this.gridTypes.TabIndex = 0;
            this.gridTypes.ReadOnly = true;
            this.gridTypes.SelectionChanged += new System.EventHandler(this.gridTypes_SelectionChanged);
            this.gridTypes.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.gridTypes_CellFormatting);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "Ontology Manager";
            this.panelTop.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelRight.ResumeLayout(false);
            this.nodeTypeTabControl.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelMeta.ResumeLayout();
            this.panelTypeMain.ResumeLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTypes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.DataGridView gridTypes;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panelMeta;
        private TabControl nodeTypeTabControl;
        private TabPage tabNodeTypeMain;
        private TabPage tabNodeTypeInheritance;
        private TabPage tabNodeTypeMeta;
        private Label lblId;
        private TextBox txtId;
        private Label lblName;
        private TextBox txtName;
        private Label lblTitle;
        private TextBox txtTitle;
        private CheckBox cbFilterEntities;
        private CheckBox cbFilterAttributes;
        private CheckBox cbFilterRelations;
        private Label lblFilterName;
        private TextBox txtFilterName;
        private DataGridView gridChildren;
        private Panel panelTypeHeader;
        private Panel panelTypeMain;
    }
}

