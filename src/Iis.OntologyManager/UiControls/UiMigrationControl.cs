﻿using Iis.Interfaces.Ontology.Data;
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
    public class UiMigrationControl: UIBaseControl
    {
        Label _lblTitle;
        Button _btnOpen;
        Button _btnRun;
        CheckBox _cbMigrate;
        CheckBox _cbDelete;
        RichTextBox _txtLog;

        public event Func<IMigrationEntity, Dictionary<Guid, Guid>> OnRun;

        public IMigrationEntity _migration;
        protected override void CreateControls()
        {
            var panels = _uiControlsCreator.GetTopBottomPanels(MainPanel, 200);
            var container = new UiContainerManager("MigrateOptions", panels.panelTop);
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
                _migration = JsonConvert.DeserializeObject<MigrationEntity>(json);
                _lblTitle.Text = $"{_migration.SourceEntityName} => {_migration.TargetEntityName}";
                _btnRun.Enabled = true;
            }
        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            var idMapping = OnRun(_migration);
            var sb = new StringBuilder();
            sb.AppendLine($"Миграція з {_migration.SourceEntityName} у {_migration.TargetEntityName}");
            sb.AppendLine($"Всього {idMapping.Keys.Count} об'єктів");
            foreach (var key in idMapping.Keys)
            {
                sb.AppendLine();
                sb.AppendLine($"Entity{_migration.SourceEntityName}.{key}/view");
                sb.AppendLine($"Entity{_migration.TargetEntityName}.{idMapping[key]}/view");
            }
            _txtLog.Text = sb.ToString();
        }
    }
}
