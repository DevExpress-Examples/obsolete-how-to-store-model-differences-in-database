Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.Persistent.Base
Imports System.Windows.Forms
Imports System.IO
Imports DevExpress.ExpressApp.Core
Imports DevExpress.ExpressApp.Model.Core
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.BaseImpl

Namespace UserDiffsToDB.Module
    Public Class ImportExportUserDifferencesController
        Inherits WindowController
        Private exportDifferencesAction As SimpleAction
        Private importDifferencesAction As SimpleAction
        Public Sub New()
            exportDifferencesAction = New SimpleAction(Me, "ExportUserDifferences", PredefinedCategory.Tools)
            exportDifferencesAction.ImageName = "Action_LocalizationExport"
            AddHandler exportDifferencesAction.Execute, AddressOf exportDifferencesAction_Execute
            importDifferencesAction = New SimpleAction(Me, "ImportUserDifferences", PredefinedCategory.Tools)
            importDifferencesAction.ImageName = "Action_LocalizationImport"
            AddHandler importDifferencesAction.Execute, AddressOf importDifferencesAction_Execute
            Me.TargetWindowType = WindowType.Main
        End Sub
        Public Sub UpdateActivity()
            Dim isAdministrator As Boolean = (CType(SecuritySystem.CurrentUser, SimpleUser)).IsAdministrator
            importDifferencesAction.Active("Security") = isAdministrator
            exportDifferencesAction.Active("Security") = isAdministrator
        End Sub
        Private Sub exportDifferencesAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs)
            Dim saveFileDialog As New SaveFileDialog()
            saveFileDialog.AddExtension = True
            saveFileDialog.Filter = "Model differences files (*.xafml)|*.xafml"
            saveFileDialog.FileName = ModelDifferenceStore.UserDiffDefaultName & ".xafml"
            If saveFileDialog.ShowDialog(Form.ActiveForm) = DialogResult.OK Then
                Dim file As String = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName)
                Dim path As String = System.IO.Path.GetDirectoryName(saveFileDialog.FileName)
                Dim fileModelStore As New FileModelStore(path, file)
                Frame.SaveModel()
                fileModelStore.SaveDifference((CType(Application.Model, ModelApplicationBase)).LastLayer)
            End If
        End Sub
        Private Sub importDifferencesAction_Execute(ByVal sender As Object, ByVal e As SimpleActionExecuteEventArgs)
            Dim openFileDialog As New OpenFileDialog()
            openFileDialog.AddExtension = True
            openFileDialog.Filter = "Model differences files (*.xafml)|*.xafml"
            openFileDialog.FileName = ModelDifferenceStore.UserDiffDefaultName & ".xafml"
            If openFileDialog.ShowDialog(Form.ActiveForm) = DialogResult.OK Then
                Dim file As String = System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName)
                Dim path As String = System.IO.Path.GetDirectoryName(openFileDialog.FileName)
                Dim fileModelStore As New FileModelStore(path, file)
                ApplicationModelsManager.RereadLastLayer(fileModelStore, Application.Model)
                Frame.View.LoadModel()
                UpdateActivity()
            End If
        End Sub
        Protected Overrides Sub OnWindowChanging(ByVal window As Window)
            MyBase.OnWindowChanging(window)
            UpdateActivity()
        End Sub
    End Class
End Namespace
