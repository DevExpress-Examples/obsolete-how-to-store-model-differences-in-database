using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using System.Windows.Forms;
using System.IO;
using DevExpress.ExpressApp.Core;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;

namespace UserDiffsToDB.Module {
    public class ImportExportUserDifferencesController : WindowController {
        private SimpleAction exportDifferencesAction;
        private SimpleAction importDifferencesAction;
        public ImportExportUserDifferencesController() {
            exportDifferencesAction = new SimpleAction(this, "ExportUserDifferences", PredefinedCategory.Tools);
            exportDifferencesAction.ImageName = "Action_LocalizationExport";
            exportDifferencesAction.Execute += exportDifferencesAction_Execute;
            importDifferencesAction = new SimpleAction(this, "ImportUserDifferences", PredefinedCategory.Tools);
            importDifferencesAction.ImageName = "Action_LocalizationImport";
            importDifferencesAction.Execute += importDifferencesAction_Execute;
            this.TargetWindowType = WindowType.Main;
        }
        public void UpdateActivity() {
            bool isAdministrator = ((SimpleUser)SecuritySystem.CurrentUser).IsAdministrator;
            importDifferencesAction.Active["Security"] = isAdministrator;
            exportDifferencesAction.Active["Security"] = isAdministrator;
        }
        private void exportDifferencesAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.AddExtension = true;
            saveFileDialog.Filter = "Model differences files (*.xafml)|*.xafml";
            saveFileDialog.FileName = ModelDifferenceStore.UserDiffDefaultName + ".xafml";
            if(saveFileDialog.ShowDialog(Form.ActiveForm) == DialogResult.OK) {
                string file = Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                string path = Path.GetDirectoryName(saveFileDialog.FileName);
                FileModelStore fileModelStore = new FileModelStore(path, file);
                Frame.SynchronizeInfo();
                fileModelStore.SaveDifference(((ModelApplicationBase)Application.Model).LastLayer);
            }
        }
        private void importDifferencesAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.AddExtension = true;
            openFileDialog.Filter = "Model differences files (*.xafml)|*.xafml";
            openFileDialog.FileName = ModelDifferenceStore.UserDiffDefaultName + ".xafml";
            if(openFileDialog.ShowDialog(Form.ActiveForm) == DialogResult.OK) {
                string file = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                string path = Path.GetDirectoryName(openFileDialog.FileName);
                FileModelStore fileModelStore = new FileModelStore(path, file);
                ApplicationModelsManager.RereadLastLayer(fileModelStore, Application.Model);
                Frame.View.SynchronizeWithInfo();
                UpdateActivity();
            }
        }
        protected override void OnWindowChanging(Window window) {
            base.OnWindowChanging(window);
            UpdateActivity();
        }
    }
}
