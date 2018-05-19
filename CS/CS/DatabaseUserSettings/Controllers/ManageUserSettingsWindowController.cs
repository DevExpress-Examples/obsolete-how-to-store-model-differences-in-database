using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Security;

namespace DatabaseUserSettings.Controllers {
    public class ManageUserSettingsWindowController : WindowController {
        public const string CriteriaKeyAllUsersExceptConfigurator = "AllUsersExceptConfigurator";
        public const string ActiveKeyConfiguratorOnly = "ConfiguratorOnly";
        private const string ActiveKeyAlwaysInactive = "AlwaysInactive";
        private readonly PopupWindowShowAction manageUserSettingsAction;
        private DatabaseUserSettingsModule module;
        public ManageUserSettingsWindowController() {
            TargetWindowType = WindowType.Main;
            manageUserSettingsAction = new PopupWindowShowAction(this, "ManageUserSettings", PredefinedCategory.Tools);
            manageUserSettingsAction.CustomizePopupWindowParams += ManageUserSettingsAction_CustomizePopupWindowParams;
            manageUserSettingsAction.ImageName = "BO_Department";
            manageUserSettingsAction.ToolTip = "Allows you to manage user settings for all application users.";
        }
        protected DatabaseUserSettingsModule DbModelStoreModule { get { return module; } }
        protected override void OnActivated() {
            base.OnActivated();
            Guard.ArgumentNotNull(Application, "application");
            module = (DatabaseUserSettingsModule)Application.Modules.FirstOrDefault<ModuleBase>(
                m => m.GetType() == typeof(DatabaseUserSettingsModule)
            );
            Guard.ArgumentNotNull(module, "module");
            UpdateManageSettingsActions();
            UpdateUserSettings();
        }
        protected virtual void UpdateManageSettingsActions() {
            using (IObjectSpace objectSpace = Application.CreateObjectSpace()) {
                manageUserSettingsAction.Active[ActiveKeyConfiguratorOnly] = SecuritySystem.CurrentUserName == DatabaseUserSettingsModule.ConfiguratorUserName
                    || (SecuritySystem.CurrentUser != null && objectSpace.IsObjectFitForCriteria(
                        SecuritySystem.CurrentUser,
                        new FunctionOperator(IsCurrentUserInRoleOperator.OperatorName, new OperandValue(DatabaseUserSettingsModule.ConfiguratorRoleName))
                    ).GetValueOrDefault(false)
                );
            }
        }
        protected virtual void UpdateUserSettings() {
            using (IObjectSpace objectSpace = Application.CreateObjectSpace()) {
                if (SecuritySystem.UserType != null)
                    foreach (object user in objectSpace.GetObjects(SecuritySystem.UserType))
                        DatabaseUserSettingsStore.GetUserSettings(objectSpace, DbModelStoreModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(objectSpace.GetKeyValue(user)));
                objectSpace.CommitChanges();
            }
        }
        private void ManageUserSettingsAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
            SetupPopupWindow(e);
        }
        protected virtual void SetupPopupWindow(CustomizePopupWindowParamsEventArgs e) {
            IObjectSpace objectSpace = e.Application.CreateObjectSpace();
            DetailView detailView = e.Application.CreateDetailView(objectSpace, objectSpace.CreateObject(DbModelStoreModule.ManageUserSettingsParameterType), false);
            detailView.ViewEditMode = ViewEditMode.Edit;
            e.DialogController.AcceptAction.Active[ActiveKeyAlwaysInactive] = false;
            e.View = detailView;
        }
        public PopupWindowShowAction ManageUserSettingsAction { get { return manageUserSettingsAction; } }
    }
}