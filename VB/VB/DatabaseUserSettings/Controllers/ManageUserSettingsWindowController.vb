Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.ExpressApp.Editors
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.Security

Namespace DatabaseUserSettings.Controllers
    Public Class ManageUserSettingsWindowController
        Inherits WindowController

        Public Const CriteriaKeyAllUsersExceptConfigurator As String = "AllUsersExceptConfigurator"
        Public Const ActiveKeyConfiguratorOnly As String = "ConfiguratorOnly"
        Private Const ActiveKeyAlwaysInactive As String = "AlwaysInactive"
        Private ReadOnly _manageUserSettingsAction As PopupWindowShowAction
        Private [module] As DatabaseUserSettingsModule
        Public Sub New()
            TargetWindowType = WindowType.Main
            _manageUserSettingsAction = New PopupWindowShowAction(Me, "ManageUserSettings", PredefinedCategory.Tools)
            AddHandler _manageUserSettingsAction.CustomizePopupWindowParams, AddressOf ManageUserSettingsAction_CustomizePopupWindowParams
            _manageUserSettingsAction.ImageName = "BO_Department"
            _manageUserSettingsAction.ToolTip = "Allows you to manage user settings for all application users."
        End Sub
        Protected ReadOnly Property DbModelStoreModule() As DatabaseUserSettingsModule
            Get
                Return [module]
            End Get
        End Property
        Protected Overrides Sub OnActivated()
            MyBase.OnActivated()
            Guard.ArgumentNotNull(Application, "application")
            [module] = CType(Application.Modules.FirstOrDefault(Function(m) m.GetType() Is GetType(DatabaseUserSettingsModule)), DatabaseUserSettingsModule)
            Guard.ArgumentNotNull([module], "module")
            UpdateManageSettingsActions()
            UpdateUserSettings()
        End Sub
        Protected Overridable Sub UpdateManageSettingsActions()
            Using objectSpace As IObjectSpace = Application.CreateObjectSpace()
                _manageUserSettingsAction.Active(ActiveKeyConfiguratorOnly) = SecuritySystem.CurrentUserName = DatabaseUserSettingsModule.ConfiguratorUserName OrElse (SecuritySystem.CurrentUser IsNot Nothing AndAlso objectSpace.IsObjectFitForCriteria(SecuritySystem.CurrentUser, New FunctionOperator(IsCurrentUserInRoleOperator.OperatorName, New OperandValue(DatabaseUserSettingsModule.ConfiguratorRoleName))).GetValueOrDefault(False))
            End Using
        End Sub
        Protected Overridable Sub UpdateUserSettings()
            Using objectSpace As IObjectSpace = Application.CreateObjectSpace()
                If SecuritySystem.UserType IsNot Nothing Then
                    For Each user As Object In objectSpace.GetObjects(SecuritySystem.UserType)
                        DatabaseUserSettingsStore.GetUserSettings(objectSpace, DbModelStoreModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(objectSpace.GetKeyValue(user)))
                    Next user
                End If
                objectSpace.CommitChanges()
            End Using
        End Sub
        Private Sub ManageUserSettingsAction_CustomizePopupWindowParams(ByVal sender As Object, ByVal e As CustomizePopupWindowParamsEventArgs)
            SetupPopupWindow(e)
        End Sub
        Protected Overridable Sub SetupPopupWindow(ByVal e As CustomizePopupWindowParamsEventArgs)
            Dim objectSpace As IObjectSpace = e.Application.CreateObjectSpace()
            Dim detailView As DetailView = e.Application.CreateDetailView(objectSpace, objectSpace.CreateObject(DbModelStoreModule.ManageUserSettingsParameterType), False)
            detailView.ViewEditMode = ViewEditMode.Edit
            e.DialogController.AcceptAction.Active(ActiveKeyAlwaysInactive) = False
            e.View = detailView
        End Sub
        Public ReadOnly Property ManageUserSettingsAction() As PopupWindowShowAction
            Get
                Return _manageUserSettingsAction
            End Get
        End Property
    End Class
End Namespace