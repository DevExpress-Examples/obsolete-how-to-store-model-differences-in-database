Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp.Xpo
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Model

Namespace DatabaseUserSettings.BusinessObjects
    <NonPersistent, ModelDefault("Caption", "Select User Settings To Manage"), ImageName("ModelEditor_ModelMerge")> _
    Public Class XPManageUserSettingsParameter
        Inherits XPObject
        Implements IManageDatabaseUserSettingsParameter

        Private targetsCore As XPCollection(Of XPUserSettings)
        Public Sub New(ByVal session As Session)
            MyBase.New(session)
            ImportBehavior = ImportUserSettingsBehavior.Overwrite
            If SecuritySystem.UserType IsNot Nothing Then
                Source = CType(DatabaseUserSettingsStore.GetUserSettings(XPObjectSpace.FindObjectSpaceByObject(Me), GetType(XPUserSettings), Convert.ToString(SecuritySystem.CurrentUserId)), XPUserSettings)
            End If
        End Sub
        <ImmediatePostData> _
        Public Property Source() As XPUserSettings
            Get
                Return GetPropertyValue(Of XPUserSettings)("Source")
            End Get
            Set(ByVal value As XPUserSettings)
                SetPropertyValue(Of XPUserSettings)("Source", value)
                If Source IsNot Nothing Then
                    Targets.Criteria = New OperandProperty("Oid") <> New OperandValue(Source.Oid)
                Else
                    Targets.Criteria = Nothing
                End If
                OnChanged("Targets")
            End Set
        End Property
        Public Property ImportBehavior() As ImportUserSettingsBehavior Implements IManageDatabaseUserSettingsParameter.ImportBehavior
            Get
                Return GetPropertyValue(Of ImportUserSettingsBehavior)("ImportBehavior")
            End Get
            Set(ByVal value As ImportUserSettingsBehavior)
                SetPropertyValue(Of ImportUserSettingsBehavior)("ImportBehavior", value)
            End Set
        End Property
        <ModelDefault("AllowEdit", "False"), ModelDefault("AllowDelete", "False")> _
        Public ReadOnly Property Targets() As XPCollection(Of XPUserSettings)
            Get
                If targetsCore Is Nothing Then
                    targetsCore = New XPCollection(Of XPUserSettings)(Session)
                End If
                Return targetsCore
            End Get
        End Property
        #Region "IManageUserSettingsParameter Members"
        Private Property IManageDatabaseUserSettingsParameter_Source() As IDatabaseUserSettings Implements IManageDatabaseUserSettingsParameter.Source
            Get
                Return Source
            End Get
            Set(ByVal value As IDatabaseUserSettings)
                Source = TryCast(value, XPUserSettings)
            End Set
        End Property
        Private ReadOnly Property IManageDatabaseUserSettingsParameter_Targets() As IList(Of IDatabaseUserSettings) Implements IManageDatabaseUserSettingsParameter.Targets
            Get
                Return New ListConverter(Of IDatabaseUserSettings, XPUserSettings)(Targets)
            End Get
        End Property
        #End Region
    End Class
End Namespace