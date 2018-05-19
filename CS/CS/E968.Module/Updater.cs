using System;
using DatabaseUserSettings;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Updating;
using DatabaseUserSettings.BusinessObjects;
using DevExpress.ExpressApp.Security.Strategy;

namespace E968.Module {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            SecuritySystemRole adminRole = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", SecurityStrategy.AdministratorRoleName));
            if (adminRole == null) {
                adminRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                adminRole.Name = SecurityStrategy.AdministratorRoleName;
                adminRole.IsAdministrative = true;
                adminRole.Save();
            }
            SecuritySystemUser adminUser = ObjectSpace.FindObject<SecuritySystemUser>(new BinaryOperator("UserName", "Sam"));
            if (adminUser == null) {
                adminUser = ObjectSpace.CreateObject<SecuritySystemUser>();
                adminUser.UserName = "Sam";
                adminUser.SetPassword("");
                adminUser.Roles.Add(adminRole);
            }
            SecuritySystemUser userJohn = ObjectSpace.FindObject<SecuritySystemUser>(new BinaryOperator("UserName", "John"));
            if (userJohn == null) {
                userJohn = ObjectSpace.CreateObject<SecuritySystemUser>();
                userJohn.UserName = "John";
                userJohn.IsActive = true;
                userJohn.Roles.Add(GetDefaultRole());
                userJohn.Save();
            }
            SecuritySystemUser configuratorUser = ObjectSpace.FindObject<SecuritySystemUser>(new BinaryOperator("UserName", DatabaseUserSettingsModule.ConfiguratorUserName));
            if (configuratorUser == null) {
                configuratorUser = ObjectSpace.CreateObject<SecuritySystemUser>();
                configuratorUser.UserName = DatabaseUserSettingsModule.ConfiguratorUserName;
                configuratorUser.IsActive = true;
                configuratorUser.Roles.Add(GetConfiguratorRole());
                configuratorUser.Roles.Add(adminRole);
                configuratorUser.Save();
            }
            ObjectSpace.CommitChanges();
        }
        private SecuritySystemRole GetDefaultRole() {
            SecuritySystemRole defaultRole = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", "Default"));
            if (defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                defaultRole.Name = "Default";

                SecuritySystemTypePermissionObject userPermissions = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                userPermissions.TargetType = typeof(SecuritySystemUser);
                userPermissions.AllowNavigate = true;
                defaultRole.TypePermissions.Add(userPermissions);
                SecuritySystemObjectPermissionsObject myDetailsPermission = ObjectSpace.CreateObject<SecuritySystemObjectPermissionsObject>();
                myDetailsPermission.Criteria = "[Oid] = CurrentUserId()";
                myDetailsPermission.AllowNavigate = true;
                myDetailsPermission.AllowRead = true;
                userPermissions.ObjectPermissions.Add(myDetailsPermission);
                SecuritySystemMemberPermissionsObject ownPasswordPermission = ObjectSpace.CreateObject<SecuritySystemMemberPermissionsObject>();
                ownPasswordPermission.Members = "ChangePasswordOnFirstLogon; StoredPassword";
                ownPasswordPermission.AllowWrite = true;
                userPermissions.MemberPermissions.Add(ownPasswordPermission);

                SecuritySystemTypePermissionObject rolePermissions = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                rolePermissions.TargetType = typeof(SecuritySystemRole);
                rolePermissions.AllowNavigate = true;
                defaultRole.TypePermissions.Add(rolePermissions);

                SecuritySystemObjectPermissionsObject defaultRolePermission = ObjectSpace.CreateObject<SecuritySystemObjectPermissionsObject>();
                defaultRolePermission.Criteria = "[Name] = 'Default'";
                defaultRolePermission.AllowRead = true;
                rolePermissions.ObjectPermissions.Add(defaultRolePermission);

                defaultRole.TypePermissions.Add(DatabaseUserSettings.DatabaseUserSettingsModule.CreateUserSettingsAspectPermissions(ObjectSpace));
                defaultRole.TypePermissions.Add(DatabaseUserSettings.DatabaseUserSettingsModule.CreateUserSettingsPermissions(ObjectSpace));
                defaultRole.Save();
            }
            return defaultRole;
        }
        private SecuritySystemRole GetConfiguratorRole() {
            SecuritySystemRole configuratorRole = ObjectSpace.FindObject<SecuritySystemRole>(new BinaryOperator("Name", DatabaseUserSettingsModule.ConfiguratorRoleName));
            if (configuratorRole == null) {
                configuratorRole = ObjectSpace.CreateObject<SecuritySystemRole>();
                configuratorRole.Name = DatabaseUserSettingsModule.ConfiguratorRoleName;
                configuratorRole.ChildRoles.Add(GetDefaultRole());

                SecuritySystemTypePermissionObject userSettingsAspectPermissions = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                userSettingsAspectPermissions.TargetType = typeof(XPUserSettingsAspect);
                userSettingsAspectPermissions.AllowCreate = true;
                userSettingsAspectPermissions.AllowDelete = true;
                userSettingsAspectPermissions.AllowNavigate = true;
                userSettingsAspectPermissions.AllowRead = true;
                userSettingsAspectPermissions.AllowWrite = true;
                configuratorRole.TypePermissions.Add(userSettingsAspectPermissions);

                SecuritySystemTypePermissionObject userSettingsPermissions = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                userSettingsPermissions.TargetType = typeof(XPUserSettings);
                userSettingsPermissions.AllowCreate = true;
                userSettingsPermissions.AllowDelete = true;
                userSettingsPermissions.AllowNavigate = true;
                userSettingsPermissions.AllowRead = true;
                userSettingsPermissions.AllowWrite = true;
                configuratorRole.TypePermissions.Add(userSettingsPermissions);

                SecuritySystemTypePermissionObject userPermissions = ObjectSpace.CreateObject<SecuritySystemTypePermissionObject>();
                userPermissions.TargetType = typeof(SecuritySystemUser);
                configuratorRole.TypePermissions.Add(userPermissions);
                SecuritySystemMemberPermissionsObject userNamePermission = ObjectSpace.CreateObject<SecuritySystemMemberPermissionsObject>();
                userNamePermission.Members = "UserName";
                userNamePermission.AllowRead = true;
                userPermissions.MemberPermissions.Add(userNamePermission);
                configuratorRole.Save();
            }
            return configuratorRole;
        }
    }
}