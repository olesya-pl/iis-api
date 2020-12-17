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
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelRight = new System.Windows.Forms.Panel();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.gridTypes = new System.Windows.Forms.DataGridView();
            this.panelTypeHeader = new System.Windows.Forms.Panel();
            this.panelTypeMain = new System.Windows.Forms.Panel();
            this.panelMeta = new System.Windows.Forms.Panel();
            this.panelMain.SuspendLayout();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridTypes)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panelTop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1000, 177);
            this.panelTop.TabIndex = 0;
            // 
            // panelMain
            // 
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.panelRight);
            this.panelMain.Controls.Add(this.panelLeft);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.Location = new System.Drawing.Point(0, 177);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(1000, 273);
            this.panelMain.TabIndex = 1;
            // 
            // panelRight
            // 
            this.panelRight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRight.Location = new System.Drawing.Point(259, 0);
            this.panelRight.Name = "panelRight";
            this.panelRight.Size = new System.Drawing.Size(739, 271);
            this.panelRight.TabIndex = 1;
            // 
            // panelLeft
            // 
            this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLeft.Controls.Add(this.gridTypes);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(259, 271);
            this.panelLeft.TabIndex = 0;
            // 
            // gridTypes
            // 
            this.gridTypes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridTypes.ColumnHeadersVisible = false;
            this.gridTypes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridTypes.Location = new System.Drawing.Point(0, 0);
            this.gridTypes.Name = "gridTypes";
            this.gridTypes.ReadOnly = true;
            this.gridTypes.RowHeadersVisible = false;
            this.gridTypes.Size = new System.Drawing.Size(257, 269);
            this.gridTypes.TabIndex = 0;
            this.gridTypes.SelectionChanged += new System.EventHandler(this.gridTypes_SelectionChanged);
            // 
            // panelTypeHeader
            // 
            this.panelTypeHeader.Location = new System.Drawing.Point(0, 0);
            this.panelTypeHeader.Name = "panelTypeHeader";
            this.panelTypeHeader.Size = new System.Drawing.Size(200, 100);
            this.panelTypeHeader.TabIndex = 0;
            // 
            // panelTypeMain
            // 
            this.panelTypeMain.Location = new System.Drawing.Point(0, 0);
            this.panelTypeMain.Name = "panelTypeMain";
            this.panelTypeMain.Size = new System.Drawing.Size(200, 100);
            this.panelTypeMain.TabIndex = 0;
            // 
            // panelMeta
            // 
            this.panelMeta.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMeta.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMeta.Location = new System.Drawing.Point(0, 0);
            this.panelMeta.Name = "panelMeta";
            this.panelMeta.Size = new System.Drawing.Size(200, 100);
            this.panelMeta.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 450);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTop);
            this.Name = "MainForm";
            this.Text = "Ontology Manager";
            this.panelMain.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridTypes)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelRight;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.DataGridView gridTypes;
        private System.Windows.Forms.Button btnCompare;
        private System.Windows.Forms.ComboBox cmbSchemaSources;
        private System.Windows.Forms.Panel panelMeta;
        private TextBox txtId;
        private TextBox txtName;
        private TextBox txtTitle;
        
        private Panel panelTypeHeader;
        private Panel panelTypeMain;
        private Button btnTypeBack;
        private Button btnSwitch;
        private Label lblTypeHeaderName;
        
        private Panel panelComparison;
        private RichTextBox txtComparison;
        private Button btnSaveSchema;
        
    }
}

