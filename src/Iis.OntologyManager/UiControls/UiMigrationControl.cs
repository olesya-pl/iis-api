using Iis.Desktop.Common.Controls;
using Iis.Interfaces.Ontology.Data;
using Iis.OntologyData.Migration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Iis.OntologyManager.UiControls
{
    public class UiMigrationControl: UiBaseControl
    {
        Label _lblTitle;
        Button _btnOpen;
        Button _btnRun;
        CheckBox _cbMigrate;
        CheckBox _cbDelete;
        RichTextBox _txtLog;

        public event Func<IMigration, IMigrationOptions, IMigrationResult> OnRun;

        public IMigration _migration;
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("MigrateOptions", panels.panelTop, _style);
            container.Add(_lblTitle = new Label { Text = "Відкрийте Файл Міграции (*.omg"});
            _btnOpen = new Button { Text = "Відкрити Міграцію", MinimumSize = new Size { Height = _style.ButtonHeightDefault } };
            container.Add(_btnOpen);
            container.Add(_cbMigrate = new CheckBox { Text = "Створити нові об'єкти", Checked = true });
            container.Add(_cbDelete = new CheckBox { Text = "Видалити старі об'єкти", Checked = true });

            _btnRun = new Button { 
                Text = "Поїхали", 
                Enabled = false, 
                MinimumSize = new Size { Height = _style.ButtonHeightDefault } 
            };
            _btnRun.Click += btnRun_Click;
            container.Add(_btnRun);

            _txtLog = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true };
            panels.panelBottom.Controls.Add(_txtLog);

            _btnOpen.Click += btnOpen_Click;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Ontology Migration files (*.omg)|*.omg|All files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var json = File.ReadAllText(dialog.FileName);
                _migration = JsonConvert.DeserializeObject<Migration>(json);
                _lblTitle.Text = $"{_migration.Title}";
                _btnRun.Enabled = true;
            }
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            var migrationResult = OnRun(_migration, GetMigrationOptions());
            _txtLog.Text = migrationResult.Log;
        }
        private IMigrationOptions GetMigrationOptions() =>
            new MigrationOptions
            {
                SaveNewObjects = _cbMigrate.Checked,
                DeleteOldObjects = _cbDelete.Checked
            };
    }
}
