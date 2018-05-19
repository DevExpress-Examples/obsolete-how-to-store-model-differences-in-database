Imports DevExpress.ExpressApp.Model
Imports System.IO
Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports System.ComponentModel
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.Validation

Namespace DatabaseUserSettings.BusinessObjects
    <DefaultClassOptions, ImageName("ModelEditor_ModelMerge"), ModelDefault("Caption", "User Settings")> _
    Public Class XPUserSettings
        Inherits XPObject
        Implements IDatabaseUserSettings

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        <Association, Aggregated> _
        Public ReadOnly Property Aspects() As XPCollection(Of XPUserSettingsAspect)
            Get
                Return GetCollection(Of XPUserSettingsAspect)("Aspects")
            End Get
        End Property
        <Browsable(False), RuleUniqueValue(Nothing, DefaultContexts.Save), ModelDefault("AllowEdit", "False")> _
        Public Property UserId() As String Implements IDatabaseUserSettings.UserId
            Get
                Return GetPropertyValue(Of String)(DatabaseUserSettingsModule.UserIdProperty)
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)(DatabaseUserSettingsModule.UserIdProperty, value)
            End Set
        End Property
        Private userNameCore As String
        <RuleUniqueValue(Nothing, DefaultContexts.Save)> _
        Public ReadOnly Property UserName() As String Implements IDatabaseUserSettings.UserName
            Get
                If String.IsNullOrEmpty(UserId) Then
                    userNameCore = DatabaseUserSettingsModule.ConfiguratorUserName
                ElseIf String.IsNullOrEmpty(userNameCore) Then
                    userNameCore = Convert.ToString(Session.Evaluate(DatabaseUserSettingsModule.UserTypeInfo.Type, New OperandProperty(DatabaseUserSettingsModule.UserNameProperty), New OperandProperty(DatabaseUserSettingsModule.UserTypeInfo.KeyMember.Name) = New OperandValue(DatabaseUserSettingsModule.UserIdTypeConverter.ConvertFromInvariantString(UserId))))
                End If
                Return userNameCore
            End Get
        End Property
        <Action(Caption := "Reset", ImageName := "ModelEditor_Action_ResetDifferences_Xml", ConfirmationMessage := "You are about to reset the selected user settings record(s). Do you want to proceed?", ToolTip := "Resets selected user settings. Users must reopen their applications to see the changes.")> _
        Public Sub Reset() Implements IDatabaseUserSettings.Reset
            For Each aspect As IDatabaseUserSettingsAspect In Aspects
                aspect.Xml = DatabaseUserSettingsStore.EmptyXafml
            Next aspect
        End Sub
        <Action(Caption := "Export", ImageName := "Action_Export_ToXML", ToolTip := "Exports selected user settings to an XAFML file located in the ExportedUserSettings folder near the application.")> _
        Public Sub Export() Implements IDatabaseUserSettings.Export
            Dim path As String = DatabaseUserSettingsModule.ExportedUserSettingsPath & UserName
            If Not Directory.Exists(path) Then
                Directory.CreateDirectory(path)
            End If
            For Each aspect As IDatabaseUserSettingsAspect In Aspects
                File.WriteAllText(String.Format("{0}\{1}{2}.xafml", path, ModelDifferenceStore.UserDiffDefaultName,If(String.IsNullOrEmpty(aspect.Name), String.Empty, "." & aspect.Name)), aspect.Xml)
            Next aspect
        End Sub
        Public Sub ImportFrom(ByVal source As IDatabaseUserSettings, ByVal importBehavior As ImportUserSettingsBehavior) Implements IDatabaseUserSettings.ImportFrom
            If source IsNot Nothing Then
                Select Case importBehavior
                    Case ImportUserSettingsBehavior.Merge
                        Throw New NotSupportedException("Merge will be supported in the future.")
                    Case ImportUserSettingsBehavior.Overwrite
                        For Each aspect As IDatabaseUserSettingsAspect In Aspects
                            Dim sourceAspect As IDatabaseUserSettingsAspect = DatabaseUserSettingsStore.FindAspectByName(source, aspect.Name)
                            If sourceAspect IsNot Nothing Then
                                aspect.Xml = sourceAspect.Xml
                            End If
                        Next aspect
                End Select
            End If
        End Sub
        Private ReadOnly Property IDatabaseUserSettings_Aspects() As IList(Of IDatabaseUserSettingsAspect) Implements IDatabaseUserSettings.Aspects
            Get
                Return New ListConverter(Of IDatabaseUserSettingsAspect, XPUserSettingsAspect)(Aspects)
            End Get
        End Property
    End Class
End Namespace