Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.ExpressApp.Xpo

Namespace DatabaseUserSettings.Controllers
    Public Class ManageUserSettingsListViewController
        Inherits ViewController(Of ListView)

        Private Const EnabledKeySourceShouldNotBeEmpty As String = "SourceShouldNotBeEmpty"
        Private Const ActiveKeyTargetsNestedListViewOnly As String = "TargetsNestedListViewOnly"
        Private ReadOnly _importUserSettingsAction As SimpleAction
        Private parameter As IManageDatabaseUserSettingsParameter
        Public Sub New()
            TargetObjectType = GetType(IDatabaseUserSettings)
            TargetViewNesting = Nesting.Nested
            _importUserSettingsAction = New SimpleAction(Me, "ImportUserSettings", PredefinedCategory.RecordEdit)
            _importUserSettingsAction.Caption = "Import From Source"
            AddHandler _importUserSettingsAction.Execute, AddressOf importUserSettingsAction_Execute
            _importUserSettingsAction.ImageName = "ModelEditor_ModelMerge"
            _importUserSettingsAction.ConfirmationMessage = "You are about to import user settings from the source to selected target record(s). Do you want to proceed?"
            _importUserSettingsAction.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            _importUserSettingsAction.ToolTip = "Imports all user settings from the source to selected target record(s). Users must reopen their applications to see the changes."
        End Sub
        Protected Overrides Sub OnViewChanging(ByVal view As View)
            MyBase.OnViewChanging(view)
            Active(ActiveKeyTargetsNestedListViewOnly) = view.Id.Contains("_Targets_ListView")
        End Sub
        Protected Overrides Sub OnViewControlsCreated()
            MyBase.OnViewControlsCreated()
            If parameter Is Nothing Then
                parameter = TryCast(CType(View.CollectionSource, PropertyCollectionSource).MasterObject, IManageDatabaseUserSettingsParameter)
                Dim parameterObjectSpace As IObjectSpace = XPObjectSpace.FindObjectSpaceByObject(parameter)
                AddHandler parameterObjectSpace.Disposed, AddressOf parameterObjectSpace_Disposed
                AddHandler parameterObjectSpace.ObjectChanged, AddressOf parameterObjectSpace_ObjectChanged
            End If
        End Sub
        Private Sub parameterObjectSpace_ObjectChanged(ByVal sender As Object, ByVal e As ObjectChangedEventArgs)
            UpdateImportUserSettingsFromSourceAction()
        End Sub
        Private Sub parameterObjectSpace_Disposed(ByVal sender As Object, ByVal e As EventArgs)
            Dim parameterObjectSpace As IObjectSpace = CType(sender, IObjectSpace)
            RemoveHandler parameterObjectSpace.Disposed, AddressOf parameterObjectSpace_Disposed
            RemoveHandler parameterObjectSpace.ObjectChanged, AddressOf parameterObjectSpace_ObjectChanged
        End Sub
        Protected Overridable Sub UpdateImportUserSettingsFromSourceAction()
            _importUserSettingsAction.Enabled(EnabledKeySourceShouldNotBeEmpty) = parameter.Source IsNot Nothing
        End Sub
        Private Sub importUserSettingsAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs)
            ImportUserSettings(e)
        End Sub
        Protected Overridable Sub ImportUserSettings(ByVal e As SimpleActionExecuteEventArgs)
            Application.SaveModelChanges() 'Dennis: There is a problem with this method when user settings are imported from the current user. Need to improve this method.
            If parameter.Source IsNot Nothing Then
                For Each target As IDatabaseUserSettings In e.SelectedObjects
                    target.ImportFrom(parameter.Source, parameter.ImportBehavior)
                    View.ObjectSpace.SetModified(target)
                Next target
                View.ObjectSpace.CommitChanges()
            End If
        End Sub
        Public ReadOnly Property ImportUserSettingsAction() As SimpleAction
            Get
                Return _importUserSettingsAction
            End Get
        End Property
    End Class
End Namespace