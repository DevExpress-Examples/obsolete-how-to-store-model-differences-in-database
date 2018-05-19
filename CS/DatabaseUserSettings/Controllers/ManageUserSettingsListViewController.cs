using System;
using System.Linq;
using DevExpress.ExpressApp;
using System.Collections.Generic;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Xpo;

namespace DatabaseUserSettings.Controllers {
    public class ManageUserSettingsListViewController : ViewController<ListView> {
        private const string EnabledKeySourceShouldNotBeEmpty = "SourceShouldNotBeEmpty";
        private const string ActiveKeyTargetsNestedListViewOnly = "TargetsNestedListViewOnly";
        private readonly SimpleAction importUserSettingsAction;
        private IManageDatabaseUserSettingsParameter parameter;
        public ManageUserSettingsListViewController() {
            TargetObjectType = typeof(IDatabaseUserSettings);
            TargetViewNesting = Nesting.Nested;
            importUserSettingsAction = new SimpleAction(this, "ImportUserSettings", PredefinedCategory.RecordEdit);
            importUserSettingsAction.Caption = "Import From Source";
            importUserSettingsAction.Execute += importUserSettingsAction_Execute;
            importUserSettingsAction.ImageName = "ModelEditor_ModelMerge";
            importUserSettingsAction.ConfirmationMessage = "You are about to import user settings from the source to selected target record(s). Do you want to proceed?";
            importUserSettingsAction.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects;
            importUserSettingsAction.ToolTip = "Imports all user settings from the source to selected target record(s). Users must reopen their applications to see the changes.";
        }
        protected override void OnViewChanging(View view) {
            base.OnViewChanging(view);
            Active[ActiveKeyTargetsNestedListViewOnly] = view.Id.Contains("_Targets_ListView");
        }
        protected override void OnViewControlsCreated() {
            base.OnViewControlsCreated();
            if (parameter == null) {
                parameter = ((PropertyCollectionSource)View.CollectionSource).MasterObject as IManageDatabaseUserSettingsParameter;
                IObjectSpace parameterObjectSpace = XPObjectSpace.FindObjectSpaceByObject(parameter);
                parameterObjectSpace.Disposed += parameterObjectSpace_Disposed;
                parameterObjectSpace.ObjectChanged += parameterObjectSpace_ObjectChanged;
            }
        }
        private void parameterObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e) {
            UpdateImportUserSettingsFromSourceAction();
        }
        private void parameterObjectSpace_Disposed(object sender, EventArgs e) {
            IObjectSpace parameterObjectSpace = (IObjectSpace)sender;
            parameterObjectSpace.Disposed -= parameterObjectSpace_Disposed;
            parameterObjectSpace.ObjectChanged -= parameterObjectSpace_ObjectChanged;
        }
        protected virtual void UpdateImportUserSettingsFromSourceAction() {
            importUserSettingsAction.Enabled[EnabledKeySourceShouldNotBeEmpty] = parameter.Source != null;
        }
        private void importUserSettingsAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            ImportUserSettings(e);
        }
        protected virtual void ImportUserSettings(SimpleActionExecuteEventArgs e) {
            //Dennis: In WinForms, you can use the following code to enforce immediate saving of the user settings in all opened windows.
            //foreach (DevExpress.ExpressApp.Win.WinWindow item in ((DevExpress.ExpressApp.Win.WinShowViewStrategyBase)Application.ShowViewStrategy).Windows) {
            //    item.SaveModel();    
            //}
            Application.SaveModelChanges();//Dennis: There is a problem with this method when user settings are imported from the current user. Need to improve this method.
            if (parameter.Source != null) {
                foreach (IDatabaseUserSettings target in e.SelectedObjects) {
                    target.ImportFrom(parameter.Source, parameter.ImportBehavior);
                    View.ObjectSpace.SetModified(target);
                }
                View.ObjectSpace.CommitChanges();
            }
        }
        public SimpleAction ImportUserSettingsAction { get { return importUserSettingsAction; } }
    }
}