using System;
using DatabaseUserSettings;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DatabaseUserSettings.BusinessObjects;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;

namespace E968.Module {
    public class Updater : ModuleUpdater {
        public Updater(IObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            CreateAnonymousAccess();

            SecurityRole defaultRole = CreateDefaultRole();
            SecurityRole administratorRole = CreateAdministratorsRole();
            SecurityRole configuratorRole = CreateConfiguratorRole();

            SecurityUser userAdmin = ObjectSpace.FindObject<SecurityUser>(new BinaryOperator("UserName", "Sam"));
            if (userAdmin == null) {
                userAdmin = ObjectSpace.CreateObject<SecurityUser>();
                userAdmin.UserName = "Sam";
                userAdmin.IsActive = true;
                userAdmin.SetPassword("");
                userAdmin.Roles.Add(defaultRole);
                userAdmin.Roles.Add(administratorRole);
                userAdmin.Save();
            }
            SecurityUser userConfigurator = ObjectSpace.FindObject<SecurityUser>(new BinaryOperator("UserName", DatabaseUserSettingsModule.ConfiguratorUserName));
            if (userConfigurator == null) {
                userConfigurator = ObjectSpace.CreateObject<SecurityUser>();
                userConfigurator.UserName = DatabaseUserSettingsModule.ConfiguratorUserName;
                userConfigurator.IsActive = true;
                userConfigurator.SetPassword("");
                userConfigurator.Roles.Add(defaultRole);
                userConfigurator.Roles.Add(administratorRole);
                userConfigurator.Roles.Add(configuratorRole);
                userConfigurator.Save();
            }
            SecurityUser userJohn = ObjectSpace.FindObject<SecurityUser>(new BinaryOperator("UserName", "John"));
            if (userJohn == null) {
                userJohn = ObjectSpace.CreateObject<SecurityUser>();
                userJohn.UserName = "John";
                userJohn.IsActive = true;
                userJohn.Roles.Add(defaultRole);
                userJohn.Save();
            }
            ObjectSpace.CommitChanges();
        }
        private SecurityRole CreateAdministratorsRole() {
            SecurityRole administratorRole = ObjectSpace.FindObject<SecurityRole>(new BinaryOperator("Name", SecurityStrategy.AdministratorRoleName));
            if (administratorRole == null) {
                administratorRole = ObjectSpace.CreateObject<SecurityRole>();
                administratorRole.Name = SecurityStrategy.AdministratorRoleName;
            }
            ModelOperationPermissionData modelPermission = ObjectSpace.CreateObject<ModelOperationPermissionData>();
            modelPermission.Save();

            administratorRole.BeginUpdate();
            administratorRole.Permissions.GrantRecursive(typeof(object), SecurityOperations.Read);
            administratorRole.Permissions.GrantRecursive(typeof(object), SecurityOperations.Write);
            administratorRole.Permissions.GrantRecursive(typeof(object), SecurityOperations.Create);
            administratorRole.Permissions.GrantRecursive(typeof(object), SecurityOperations.Delete);
            administratorRole.Permissions.GrantRecursive(typeof(object), SecurityOperations.Navigate);
            GrantDefaultSecurityAccessToServiceModelClasses(administratorRole);
            administratorRole.EndUpdate();

            administratorRole.PersistentPermissions.Add(modelPermission);
            administratorRole.Save();
            return administratorRole;
        }
        private SecurityRole CreateConfiguratorRole() {
            SecurityRole configuratorRole = ObjectSpace.FindObject<SecurityRole>(new BinaryOperator("Name", DatabaseUserSettingsModule.ConfiguratorRoleName));
            if (configuratorRole == null) {
                configuratorRole = ObjectSpace.CreateObject<SecurityRole>();
                configuratorRole.Name = DatabaseUserSettingsModule.ConfiguratorRoleName;
                configuratorRole.Permissions[typeof(XPUserSettingsAspect)].Grant(SecurityOperations.FullAccess);
                configuratorRole.Permissions[typeof(XPUserSettings)].Grant(SecurityOperations.FullAccess);
            }
            return configuratorRole;
        }

        private SecurityRole CreateDefaultRole() {
            SecurityRole defaultRole = ObjectSpace.FindObject<SecurityRole>(new BinaryOperator("Name", "Default"));
            if (defaultRole == null) {
                defaultRole = ObjectSpace.CreateObject<SecurityRole>();
                defaultRole.Name = "Default";
            }
            GrantDefaultSecurityAccessToServiceModelClasses(defaultRole);
            ObjectOperationPermissionData myDetailsPermission = ObjectSpace.CreateObject<ObjectOperationPermissionData>();
            myDetailsPermission.TargetType = typeof(SecurityUser);
            myDetailsPermission.Criteria = "[Oid] = CurrentUserId()";
            myDetailsPermission.AllowNavigate = true;
            myDetailsPermission.AllowRead = true;
            myDetailsPermission.Save();
            defaultRole.PersistentPermissions.Add(myDetailsPermission);
            MemberOperationPermissionData ownPasswordPermission = ObjectSpace.CreateObject<MemberOperationPermissionData>();
            ownPasswordPermission.TargetType = typeof(SecurityUser);
            ownPasswordPermission.Members = "ChangePasswordOnFirstLogon, StoredPassword";
            ownPasswordPermission.AllowWrite = true;
            ownPasswordPermission.Save();
            defaultRole.PersistentPermissions.Add(ownPasswordPermission);
            ObjectOperationPermissionData defaultRolePermission = ObjectSpace.CreateObject<ObjectOperationPermissionData>();
            defaultRolePermission.TargetType = typeof(SecurityRole);
            defaultRolePermission.Criteria = "[Name] = 'Default'";
            defaultRolePermission.AllowNavigate = true;
            defaultRolePermission.AllowRead = true;
            defaultRolePermission.Save();
            defaultRole.PersistentPermissions.Add(defaultRolePermission);
            return defaultRole;
        }

        private void CreateAnonymousAccess() {
            SecurityRole anonymousRole = ObjectSpace.FindObject<SecurityRole>(new BinaryOperator("Name", SecurityStrategy.AnonymousUserName));
            if (anonymousRole == null) {
                anonymousRole = ObjectSpace.CreateObject<SecurityRole>();
                anonymousRole.Name = SecurityStrategy.AnonymousUserName;
                anonymousRole.BeginUpdate();
                anonymousRole.Permissions[typeof(SecurityUser)].Grant(SecurityOperations.Read);
                GrantDefaultSecurityAccessToServiceModelClasses(anonymousRole);
                anonymousRole.EndUpdate();
                anonymousRole.Save();
            }

            SecurityUser anonymousUser = ObjectSpace.FindObject<SecurityUser>(new BinaryOperator("UserName", SecurityStrategy.AnonymousUserName));
            if (anonymousUser == null) {
                anonymousUser = ObjectSpace.CreateObject<SecurityUser>();
                anonymousUser.UserName = SecurityStrategy.AnonymousUserName;
                anonymousUser.IsActive = true;
                anonymousUser.SetPassword(string.Empty);
                anonymousUser.Roles.Add(anonymousRole);
                anonymousUser.Save();
            }
        }
        private static void GrantDefaultSecurityAccessToServiceModelClasses(SecurityRole role) {
            role.Permissions[typeof(XPUserSettingsAspect)].Deny(SecurityOperations.FullAccess);
            role.Permissions[typeof(XPUserSettings)].Deny(SecurityOperations.FullAccess);
            role.Permissions[typeof(XPUserSettingsAspect)].Grant(SecurityOperations.Write);
            role.Permissions[typeof(XPUserSettings)].Grant(SecurityOperations.Write);
        }
    }
}