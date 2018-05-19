Imports System.ComponentModel
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.DC
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.ExpressApp.Design
Imports DatabaseUserSettings.BusinessObjects
Imports DevExpress.ExpressApp.Security.Strategy

Namespace DatabaseUserSettings
    <Description("Provides the capability to store user settings in the database instead of the file system by default. If a security system is enabled, then it is possible to manage user settings for all application users via a special 'Configurator' user.")> _
    Public NotInheritable Partial Class DatabaseUserSettingsModule
        Inherits ModuleBase

        Public Shared ConfiguratorUserName As String = "Configurator"
        Public Shared ConfiguratorRoleName As String = ConfiguratorUserName
        Public Shared ExportedUserSettingsPath As String = String.Format("{0}\ExportedUserSettings\", PathHelper.GetApplicationFolder())
        Public Const UserIdProperty As String = "UserId"
        Public Const UserNameProperty As String = "UserName"
        Private _userSettingsType As Type = GetType(XPUserSettings)
        Private _userSettingsAspectType As Type = GetType(XPUserSettingsAspect)
        Public _manageUserSettingsParameterType As Type = GetType(XPManageUserSettingsParameter)
        Public Sub New()
            InitializeComponent()
        End Sub
        Public Overrides Sub Setup(ByVal application As XafApplication)
            MyBase.Setup(application)
            Guard.ArgumentNotNullOrEmpty(ConfiguratorUserName, "ConfiguratorUserName")
            AddHandler application.CreateCustomUserModelDifferenceStore, AddressOf OnCreateDatabaseModelDifferenceStore
            AddHandler application.CreateCustomModelDifferenceStore, AddressOf OnCreateDatabaseModelDifferenceStore
            AddHandler application.Disposed, AddressOf application_Disposed
        End Sub
        Private Sub application_Disposed(ByVal sender As Object, ByVal e As EventArgs)
            Dim _application As XafApplication = CType(sender, XafApplication)
            RemoveHandler _application.CreateCustomUserModelDifferenceStore, AddressOf OnCreateDatabaseModelDifferenceStore
            RemoveHandler _application.CreateCustomModelDifferenceStore, AddressOf OnCreateDatabaseModelDifferenceStore
            RemoveHandler _application.Disposed, AddressOf application_Disposed
        End Sub
        Private Sub OnCreateDatabaseModelDifferenceStore(ByVal sender As Object, ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            e.Store = New DatabaseUserSettingsStore(CType(sender, XafApplication))
            e.Handled = True
        End Sub
        Private Shared _userIdTypeConverter As TypeConverter
        Friend Shared ReadOnly Property UserIdTypeConverter() As TypeConverter
            Get
                If _userIdTypeConverter Is Nothing Then
                    _userIdTypeConverter = TypeDescriptor.GetConverter(UserTypeInfo.KeyMember.MemberType)
                End If
                Return _userIdTypeConverter
            End Get
        End Property
        Private Shared _userTypeInfo As ITypeInfo
        Friend Shared ReadOnly Property UserTypeInfo() As ITypeInfo
            Get
                If _userTypeInfo Is Nothing Then
                    _userTypeInfo = XafTypesInfo.Instance.FindTypeInfo(SecuritySystem.UserType)
                End If
                Return _userTypeInfo
            End Get
        End Property
        <DefaultValue(GetType(XPUserSettings)), Description("Gets or sets the type used to store user settings in the database"), TypeConverter(GetType(BusinessClassTypeConverter(Of IDatabaseUserSettings))), Category("Data")> _
        Public Property UserSettingsType() As Type
            Get
                Return _userSettingsType
            End Get
            Set(ByVal value As Type)
                If value IsNot Nothing Then
                    If Not ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(GetType(IDatabaseUserSettings)), XafTypesInfo.Instance.FindTypeInfo(value)) Then
                        ReflectionHelper.ThrowInvalidCastException(GetType(IDatabaseUserSettings), value)
                    Else
                        _userSettingsType = value
                    End If
                End If
            End Set
        End Property
        <DefaultValue(GetType(XPUserSettingsAspect)), Description("Gets or sets the type used to store user settings aspects in the database"), TypeConverter(GetType(BusinessClassTypeConverter(Of IDatabaseUserSettingsAspect))), Category("Data")> _
        Public Property UserSettingsAspectType() As Type
            Get
                Return _userSettingsAspectType
            End Get
            Set(ByVal value As Type)
                If value IsNot Nothing Then
                    If Not ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(GetType(IDatabaseUserSettingsAspect)), XafTypesInfo.Instance.FindTypeInfo(value)) Then
                        ReflectionHelper.ThrowInvalidCastException(GetType(IDatabaseUserSettingsAspect), value)
                    Else
                        _userSettingsAspectType = value
                    End If
                End If
            End Set
        End Property
        <DefaultValue(GetType(XPManageUserSettingsParameter)), Description("Gets or sets the type used to manage user settings via UI"), TypeConverter(GetType(BusinessClassTypeConverter(Of IManageDatabaseUserSettingsParameter))), Category("Data")> _
        Public Property ManageUserSettingsParameterType() As Type
            Get
                Return _manageUserSettingsParameterType
            End Get
            Set(ByVal value As Type)
                If value IsNot Nothing Then
                    If Not ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(GetType(IManageDatabaseUserSettingsParameter)), XafTypesInfo.Instance.FindTypeInfo(value)) Then
                        ReflectionHelper.ThrowInvalidCastException(GetType(IManageDatabaseUserSettingsParameter), value)
                    Else
                        _manageUserSettingsParameterType = value
                    End If
                End If
            End Set
        End Property
        Protected Overrides Function GetDeclaredControllerTypes() As IEnumerable(Of Type)
            Return New Type() { GetType(DatabaseUserSettings.Controllers.ManageUserSettingsListViewController), GetType(DatabaseUserSettings.Controllers.ManageUserSettingsWindowController) }
        End Function
        Protected Overrides Function GetDeclaredExportedTypes() As IEnumerable(Of Type)
            Return New Type() { GetType(DatabaseUserSettings.BusinessObjects.XPManageUserSettingsParameter), GetType(DatabaseUserSettings.BusinessObjects.XPUserSettings), GetType(DatabaseUserSettings.BusinessObjects.XPUserSettingsAspect) }
        End Function
        Public Shared Function CreateUserSettingsAspectPermissions(ByVal objectSpace As IObjectSpace) As SecuritySystemTypePermissionObject
            Dim userSettingsAspectPermissions As SecuritySystemTypePermissionObject = objectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
            userSettingsAspectPermissions.TargetType = GetType(XPUserSettingsAspect)
            userSettingsAspectPermissions.AllowWrite = True
            userSettingsAspectPermissions.AllowRead = True
            Return userSettingsAspectPermissions
        End Function
        Public Shared Function CreateUserSettingsPermissions(ByVal objectSpace As IObjectSpace) As SecuritySystemTypePermissionObject
            Dim userSettingsPermissions As SecuritySystemTypePermissionObject = objectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
            userSettingsPermissions.TargetType = GetType(XPUserSettings)
            userSettingsPermissions.AllowWrite = True
            userSettingsPermissions.AllowRead = True
            Return userSettingsPermissions
        End Function
    End Class
End Namespace