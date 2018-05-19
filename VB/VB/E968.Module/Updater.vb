Imports DatabaseUserSettings
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Updating
Imports DatabaseUserSettings.BusinessObjects
Imports DevExpress.ExpressApp.Security.Strategy

Namespace E968.Module
    Public Class Updater
        Inherits ModuleUpdater

        Public Sub New(ByVal objectSpace As IObjectSpace, ByVal currentDBVersion As Version)
            MyBase.New(objectSpace, currentDBVersion)
        End Sub
        Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
            MyBase.UpdateDatabaseAfterUpdateSchema()
            Dim adminRole As SecuritySystemRole = ObjectSpace.FindObject(Of SecuritySystemRole)(New BinaryOperator("Name", SecurityStrategy.AdministratorRoleName))
            If adminRole Is Nothing Then
                adminRole = ObjectSpace.CreateObject(Of SecuritySystemRole)()
                adminRole.Name = SecurityStrategy.AdministratorRoleName
                adminRole.IsAdministrative = True
                adminRole.Save()
            End If
            Dim adminUser As SecuritySystemUser = ObjectSpace.FindObject(Of SecuritySystemUser)(New BinaryOperator("UserName", "Sam"))
            If adminUser Is Nothing Then
                adminUser = ObjectSpace.CreateObject(Of SecuritySystemUser)()
                adminUser.UserName = "Sam"
                adminUser.SetPassword("")
                adminUser.Roles.Add(adminRole)
            End If
            Dim userJohn As SecuritySystemUser = ObjectSpace.FindObject(Of SecuritySystemUser)(New BinaryOperator("UserName", "John"))
            If userJohn Is Nothing Then
                userJohn = ObjectSpace.CreateObject(Of SecuritySystemUser)()
                userJohn.UserName = "John"
                userJohn.IsActive = True
                userJohn.Roles.Add(GetDefaultRole())
                userJohn.Save()
            End If
            Dim configuratorUser As SecuritySystemUser = ObjectSpace.FindObject(Of SecuritySystemUser)(New BinaryOperator("UserName", DatabaseUserSettingsModule.ConfiguratorUserName))
            If configuratorUser Is Nothing Then
                configuratorUser = ObjectSpace.CreateObject(Of SecuritySystemUser)()
                configuratorUser.UserName = DatabaseUserSettingsModule.ConfiguratorUserName
                configuratorUser.IsActive = True
                configuratorUser.Roles.Add(GetConfiguratorRole())
                configuratorUser.Roles.Add(adminRole)
                configuratorUser.Save()
            End If
            ObjectSpace.CommitChanges()
        End Sub
        Private Function GetDefaultRole() As SecuritySystemRole
            Dim defaultRole As SecuritySystemRole = ObjectSpace.FindObject(Of SecuritySystemRole)(New BinaryOperator("Name", "Default"))
            If defaultRole Is Nothing Then
                defaultRole = ObjectSpace.CreateObject(Of SecuritySystemRole)()
                defaultRole.Name = "Default"

                Dim userPermissions As SecuritySystemTypePermissionObject = ObjectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
                userPermissions.TargetType = GetType(SecuritySystemUser)
                userPermissions.AllowNavigate = True
                defaultRole.TypePermissions.Add(userPermissions)
                Dim myDetailsPermission As SecuritySystemObjectPermissionsObject = ObjectSpace.CreateObject(Of SecuritySystemObjectPermissionsObject)()
                myDetailsPermission.Criteria = "[Oid] = CurrentUserId()"
                myDetailsPermission.AllowNavigate = True
                myDetailsPermission.AllowRead = True
                userPermissions.ObjectPermissions.Add(myDetailsPermission)
                Dim ownPasswordPermission As SecuritySystemMemberPermissionsObject = ObjectSpace.CreateObject(Of SecuritySystemMemberPermissionsObject)()
                ownPasswordPermission.Members = "ChangePasswordOnFirstLogon; StoredPassword"
                ownPasswordPermission.AllowWrite = True
                userPermissions.MemberPermissions.Add(ownPasswordPermission)

                Dim rolePermissions As SecuritySystemTypePermissionObject = ObjectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
                rolePermissions.TargetType = GetType(SecuritySystemRole)
                rolePermissions.AllowNavigate = True
                defaultRole.TypePermissions.Add(rolePermissions)

                Dim defaultRolePermission As SecuritySystemObjectPermissionsObject = ObjectSpace.CreateObject(Of SecuritySystemObjectPermissionsObject)()
                defaultRolePermission.Criteria = "[Name] = 'Default'"
                defaultRolePermission.AllowRead = True
                rolePermissions.ObjectPermissions.Add(defaultRolePermission)

                defaultRole.TypePermissions.Add(DatabaseUserSettings.DatabaseUserSettingsModule.CreateUserSettingsAspectPermissions(ObjectSpace))
                defaultRole.TypePermissions.Add(DatabaseUserSettings.DatabaseUserSettingsModule.CreateUserSettingsPermissions(ObjectSpace))
                defaultRole.Save()
            End If
            Return defaultRole
        End Function
        Private Function GetConfiguratorRole() As SecuritySystemRole
            Dim configuratorRole As SecuritySystemRole = ObjectSpace.FindObject(Of SecuritySystemRole)(New BinaryOperator("Name", DatabaseUserSettingsModule.ConfiguratorRoleName))
            If configuratorRole Is Nothing Then
                configuratorRole = ObjectSpace.CreateObject(Of SecuritySystemRole)()
                configuratorRole.Name = DatabaseUserSettingsModule.ConfiguratorRoleName
                configuratorRole.ChildRoles.Add(GetDefaultRole())

                Dim userSettingsAspectPermissions As SecuritySystemTypePermissionObject = ObjectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
                userSettingsAspectPermissions.TargetType = GetType(XPUserSettingsAspect)
                userSettingsAspectPermissions.AllowCreate = True
                userSettingsAspectPermissions.AllowDelete = True
                userSettingsAspectPermissions.AllowNavigate = True
                userSettingsAspectPermissions.AllowRead = True
                userSettingsAspectPermissions.AllowWrite = True
                configuratorRole.TypePermissions.Add(userSettingsAspectPermissions)

                Dim userSettingsPermissions As SecuritySystemTypePermissionObject = ObjectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
                userSettingsPermissions.TargetType = GetType(XPUserSettings)
                userSettingsPermissions.AllowCreate = True
                userSettingsPermissions.AllowDelete = True
                userSettingsPermissions.AllowNavigate = True
                userSettingsPermissions.AllowRead = True
                userSettingsPermissions.AllowWrite = True
                configuratorRole.TypePermissions.Add(userSettingsPermissions)

                Dim userPermissions As SecuritySystemTypePermissionObject = ObjectSpace.CreateObject(Of SecuritySystemTypePermissionObject)()
                userPermissions.TargetType = GetType(SecuritySystemUser)
                configuratorRole.TypePermissions.Add(userPermissions)
                Dim userNamePermission As SecuritySystemMemberPermissionsObject = ObjectSpace.CreateObject(Of SecuritySystemMemberPermissionsObject)()
                userNamePermission.Members = "UserName"
                userNamePermission.AllowRead = True
                userPermissions.MemberPermissions.Add(userNamePermission)
                configuratorRole.Save()
            End If
            Return configuratorRole
        End Function
    End Class
End Namespace