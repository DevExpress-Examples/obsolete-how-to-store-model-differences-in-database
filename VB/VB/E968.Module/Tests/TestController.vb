#If EasyTest Then
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.SystemModule

Namespace DatabaseUserSettings.Tests
    Public Class TestController
        Inherits ViewController

        Public Sub New()
            Dim testAction As New SimpleAction(Me, "TestAction", DevExpress.Persistent.Base.PredefinedCategory.Tools)
            AddHandler testAction.Execute, AddressOf testAction_Execute
        End Sub
        Private Sub testAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs)
            Dim navigationItems As IModelRootNavigationItems = CType(Application.Model, IModelApplicationNavigationItems).NavigationItems
            If SecuritySystem.CurrentUserName = DatabaseUserSettingsModule.ConfiguratorUserName Then
                navigationItems.StartupNavigationItem = navigationItems.AllItems("SecuritySystemRole_ListView")
            End If
            If SecuritySystem.CurrentUserName = "Sam" Then
                navigationItems.StartupNavigationItem = navigationItems.AllItems("SecuritySystemUser_ListView")
            End If
            Application.SaveModelChanges()
        End Sub
    End Class
End Namespace
#End If
