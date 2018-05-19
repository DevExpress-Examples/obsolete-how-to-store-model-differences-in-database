using System;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Design;
using DatabaseUserSettings.BusinessObjects;
using DevExpress.ExpressApp.Security.Strategy;

namespace DatabaseUserSettings {
    [Description("Provides the capability to store user settings in the database instead of the file system by default. If a security system is enabled, then it is possible to manage user settings for all application users via a special 'Configurator' user.")]
    public sealed partial class DatabaseUserSettingsModule : ModuleBase {
        public static string ConfiguratorUserName = "Configurator";
        public static string ConfiguratorRoleName = ConfiguratorUserName;
        public static string ExportedUserSettingsPath = String.Format("{0}\\ExportedUserSettings\\", PathHelper.GetApplicationFolder());
        public const string UserIdProperty = "UserId";
        public const string UserNameProperty = "UserName";
        private Type userSettingsType = typeof(XPUserSettings);
        private Type userSettingsAspectType = typeof(XPUserSettingsAspect);
        public Type manageUserSettingsParameterType = typeof(XPManageUserSettingsParameter);
        public DatabaseUserSettingsModule() {
            InitializeComponent();
        }
        public override void Setup(XafApplication application) {
            base.Setup(application);
            Guard.ArgumentNotNullOrEmpty(ConfiguratorUserName, "ConfiguratorUserName");
            application.CreateCustomUserModelDifferenceStore += OnCreateDatabaseModelDifferenceStore;
            application.CreateCustomModelDifferenceStore += OnCreateDatabaseModelDifferenceStore;
            application.Disposed += application_Disposed;
        }
        private void application_Disposed(object sender, EventArgs e) {
            XafApplication application = (XafApplication)sender;
            application.CreateCustomUserModelDifferenceStore -= OnCreateDatabaseModelDifferenceStore;
            application.CreateCustomModelDifferenceStore -= OnCreateDatabaseModelDifferenceStore;
            application.Disposed -= application_Disposed;
        }
        private void OnCreateDatabaseModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            e.Store = new DatabaseUserSettingsStore((XafApplication)sender);
            e.Handled = true;
        }
        private static TypeConverter userIdTypeConverter;
        internal static TypeConverter UserIdTypeConverter {
            get {
                if (userIdTypeConverter == null) {
                    userIdTypeConverter = TypeDescriptor.GetConverter(UserTypeInfo.KeyMember.MemberType);
                }
                return userIdTypeConverter;
            }
        }
        private static ITypeInfo userTypeInfo;
        internal static ITypeInfo UserTypeInfo {
            get {
                if (userTypeInfo == null)
                    userTypeInfo = XafTypesInfo.Instance.FindTypeInfo(SecuritySystem.UserType);
                return userTypeInfo;
            }
        }
        [DefaultValue(typeof(XPUserSettings))]
        [Description("Gets or sets the type used to store user settings in the database")]
        [TypeConverter(typeof(BusinessClassTypeConverter<IDatabaseUserSettings>))]
        [Category("Data")]
        public Type UserSettingsType {
            get { return userSettingsType; }
            set {
                if (value != null) {
                    if (!ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(typeof(IDatabaseUserSettings)),
                        XafTypesInfo.Instance.FindTypeInfo(value))) {
                        ReflectionHelper.ThrowInvalidCastException(typeof(IDatabaseUserSettings), value);
                    }
                    else {
                        userSettingsType = value;
                    }
                }
            }
        }
        [DefaultValue(typeof(XPUserSettingsAspect))]
        [Description("Gets or sets the type used to store user settings aspects in the database")]
        [TypeConverter(typeof(BusinessClassTypeConverter<IDatabaseUserSettingsAspect>))]
        [Category("Data")]
        public Type UserSettingsAspectType {
            get { return userSettingsAspectType; }
            set {
                if (value != null) {
                    if (!ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(typeof(IDatabaseUserSettingsAspect)),
                        XafTypesInfo.Instance.FindTypeInfo(value))) {
                        ReflectionHelper.ThrowInvalidCastException(typeof(IDatabaseUserSettingsAspect), value);
                    }
                    else {
                        userSettingsAspectType = value;
                    }
                }
            }
        }
        [DefaultValue(typeof(XPManageUserSettingsParameter))]
        [Description("Gets or sets the type used to manage user settings via UI")]
        [TypeConverter(typeof(BusinessClassTypeConverter<IManageDatabaseUserSettingsParameter>))]
        [Category("Data")]
        public Type ManageUserSettingsParameterType {
            get { return manageUserSettingsParameterType; }
            set {
                if (value != null) {
                    if (!ReflectionHelper.IsTypeAssignableFrom(XafTypesInfo.Instance.FindTypeInfo(typeof(IManageDatabaseUserSettingsParameter)),
                        XafTypesInfo.Instance.FindTypeInfo(value))) {
                        ReflectionHelper.ThrowInvalidCastException(typeof(IManageDatabaseUserSettingsParameter), value);
                    }
                    else {
                        manageUserSettingsParameterType = value;
                    }
                }
            }
        }
        protected override IEnumerable<Type> GetDeclaredControllerTypes() {
            return new Type[] { 
                typeof(DatabaseUserSettings.Controllers.ManageUserSettingsListViewController),
                typeof(DatabaseUserSettings.Controllers.ManageUserSettingsWindowController)
            };
        }
        protected override IEnumerable<Type> GetDeclaredExportedTypes() {
            return new Type[] { 
                typeof(DatabaseUserSettings.BusinessObjects.XPManageUserSettingsParameter),
                typeof(DatabaseUserSettings.BusinessObjects.XPUserSettings),
                typeof(DatabaseUserSettings.BusinessObjects.XPUserSettingsAspect)
            };
        }
        public static SecuritySystemTypePermissionObject CreateUserSettingsAspectPermissions(IObjectSpace objectSpace) {
            SecuritySystemTypePermissionObject userSettingsAspectPermissions = objectSpace.CreateObject<SecuritySystemTypePermissionObject>();
            userSettingsAspectPermissions.TargetType = typeof(XPUserSettingsAspect);
            userSettingsAspectPermissions.AllowWrite = true;
            userSettingsAspectPermissions.AllowRead = true;
            return userSettingsAspectPermissions;
        }
        public static SecuritySystemTypePermissionObject CreateUserSettingsPermissions(IObjectSpace objectSpace) {
            SecuritySystemTypePermissionObject userSettingsPermissions = objectSpace.CreateObject<SecuritySystemTypePermissionObject>();
            userSettingsPermissions.TargetType = typeof(XPUserSettings);
            userSettingsPermissions.AllowWrite = true;
            userSettingsPermissions.AllowRead = true;
            return userSettingsPermissions;
        }
    }
}