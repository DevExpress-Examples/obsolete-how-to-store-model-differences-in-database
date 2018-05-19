using System;
using System.IO;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;

namespace DatabaseUserSettings {
    public enum ImportUserSettingsBehavior { Merge, Overwrite };
    public interface IDatabaseUserSettings {
        IList<IDatabaseUserSettingsAspect> Aspects { get; }
        string UserId { get; set; }
        string UserName { get; }
        void Reset();
        void Export();
        void ImportFrom(IDatabaseUserSettings source, ImportUserSettingsBehavior importBehavior);
    }
    public interface IDatabaseUserSettingsAspect {
        string Name { get; set; }
        IDatabaseUserSettings Owner { get; set; }
        string Xml { get; set; }
    }
    public interface IManageDatabaseUserSettingsParameter {
        IDatabaseUserSettings Source { get; set; }
        IList<IDatabaseUserSettings> Targets { get; }
        ImportUserSettingsBehavior ImportBehavior { get; set; }
    }
    public class DatabaseUserSettingsStore : ModelDifferenceStore {
        private XafApplication application;
        private DatabaseUserSettingsModule module;
        public const string XafmlHeader = "<?xml version=\"1.0\" encoding=\"utf-8\"?>";
        public const string XafmlRootElement = "<Application/>";
        public static readonly string EmptyXafml = String.Format("{0}{1}{2}", DatabaseUserSettingsStore.XafmlHeader, Environment.NewLine, DatabaseUserSettingsStore.XafmlRootElement);
        public DatabaseUserSettingsStore(XafApplication application) {
            this.application = application;
            Guard.ArgumentNotNull(application, "application");
            module = (DatabaseUserSettingsModule)Application.Modules.FirstOrDefault<ModuleBase>(
                m => m.GetType() == typeof(DatabaseUserSettingsModule)
            );
            Guard.ArgumentNotNull(module, "module");
        }
        public override string Name { get { return GetType().Name; } }
        protected XafApplication Application { get { return application; } }
        protected DatabaseUserSettingsModule DatabaseUserSettingsModule { get { return module; } }
        public static IDatabaseUserSettings GetUserSettings(IObjectSpace objectSpace, Type userSettingsType, string userId) {
            IDatabaseUserSettings userSettings = null;
            if (string.IsNullOrEmpty(userId))
                userSettings = GetConfiguratorUserSettings(objectSpace, userSettingsType);
            else {
                object user = objectSpace.FindObject(DatabaseUserSettingsModule.UserTypeInfo.Type, new OperandProperty(DatabaseUserSettingsModule.UserTypeInfo.KeyMember.Name) == new OperandValue(DatabaseUserSettingsModule.UserIdTypeConverter.ConvertFromInvariantString(userId)));
                if (user != null)
                    if (Convert.ToString(DatabaseUserSettingsModule.UserTypeInfo.FindMember(DatabaseUserSettingsModule.UserNameProperty).GetValue(user)) == DatabaseUserSettingsModule.ConfiguratorUserName)
                        userSettings = GetConfiguratorUserSettings(objectSpace, userSettingsType);
                    else
                        userSettings = GetUserUserSettings(objectSpace, userSettingsType, userId);
            }
            return userSettings;
        }
        private static IDatabaseUserSettings GetUserUserSettings(IObjectSpace objectSpace, Type userSettingsType, string userId) {
            IDatabaseUserSettings userSettings = (IDatabaseUserSettings)objectSpace.FindObject(
                userSettingsType,
                new OperandProperty(DatabaseUserSettingsModule.UserIdProperty) == new OperandValue(userId)
            );
            if (userSettings == null) {
                userSettings = CreateUserSettings(objectSpace, userSettingsType);
                userSettings.UserId = userId;
            }
            return userSettings;
        }
        public static IDatabaseUserSettings GetConfiguratorUserSettings(IObjectSpace objectSpace, Type userSettingsType) {
            IDatabaseUserSettings userSettings = (IDatabaseUserSettings)objectSpace.FindObject(
                userSettingsType,
                new NullOperator(DatabaseUserSettingsModule.UserIdProperty)
            );
            if (userSettings == null) {
                userSettings = CreateUserSettings(objectSpace, userSettingsType);
                FileModelStore fileUserSettingsStore = new FileModelStore(PathHelper.GetApplicationFolder(), AppDiffDefaultName);
                IEnumerable<string> aspects = fileUserSettingsStore.GetAspects().Concat(new string[] { string.Empty });
                foreach (string aspectName in aspects)
                    try {
                        GetUserSettingsAspect(objectSpace, userSettings, aspectName).Xml = File.ReadAllText(PathHelper.GetApplicationFolder() + fileUserSettingsStore.GetFileNameForAspect(aspectName));
                    } catch (Exception e) {
                        Tracing.Tracer.LogError(e);
                        throw new UserFriendlyException(Localization.CannotLoadUserSettingsFromFile + Localization.UserSettingsFailureSuggestion + Environment.NewLine + e.Message);
                    }
            }
            return userSettings;
        }
        public override void Load(ModelApplicationBase model) {
            try {
                using (IObjectSpace objectSpace = Application.CreateObjectSpace()) {
                    ModelXmlReader reader = new ModelXmlReader();
                    IDatabaseUserSettings userSettings = GetUserSettings(objectSpace, DatabaseUserSettingsModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(SecuritySystem.CurrentUserId));
                    foreach (IDatabaseUserSettingsAspect aspect in userSettings.Aspects) {
                        if (string.IsNullOrEmpty(aspect.Xml))
                            aspect.Xml = EmptyXafml;
                        reader.ReadFromString(model, aspect.Name, aspect.Xml);
                    }
                    objectSpace.CommitChanges();
                }
            } catch (Exception e) {
                if (!(e is InvalidOperationException)) {
                    Tracing.Tracer.LogError(e);
                    throw new UserFriendlyException(Localization.CannotLoadUserSettingsFromTheDatabase + Localization.UserSettingsFailureSuggestion + Environment.NewLine + e.Message);
                }
            }
        }
        public override void SaveDifference(ModelApplicationBase model) {
            try {
                using (IObjectSpace objectSpace = Application.CreateObjectSpace()) {
                    IDatabaseUserSettings userSettings = GetUserSettings(objectSpace, DatabaseUserSettingsModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(SecuritySystem.CurrentUserId));
                    for (int i = 0; i < model.AspectCount; i++) {
                        ModelXmlWriter writer = new ModelXmlWriter();
                        string xml = writer.WriteToString(model, i);
                        if (!string.IsNullOrEmpty(xml)) {
                            string aspectName = model.GetAspect(i);
                            IDatabaseUserSettingsAspect aspect = GetUserSettingsAspect(objectSpace, userSettings, aspectName);
                            aspect.Xml = String.Format("{0}{1}{2}", XafmlHeader, Environment.NewLine, xml);
                        }
                    }
                    objectSpace.CommitChanges();
                }
            } catch (Exception e) {
                if (!(e is InvalidOperationException)) {
                    Tracing.Tracer.LogError(e);
                    throw new UserFriendlyException(Localization.CannotSaveUserSettingsToTheDatabase + Localization.UserSettingsFailureSuggestion + Environment.NewLine + e.Message);
                }
            }
        }
        public static IDatabaseUserSettingsAspect FindAspectByName(IDatabaseUserSettings userSettings, string aspectName) {
            if (userSettings == null || userSettings.Aspects == null)
                return null;
            IDatabaseUserSettingsAspect userSettingsAspect = userSettings.Aspects.FirstOrDefault<IDatabaseUserSettingsAspect>(
                a => a.Name == aspectName
            );
            return userSettingsAspect;
        }
        public static IDatabaseUserSettingsAspect GetUserSettingsAspect(IObjectSpace objectSpace, IDatabaseUserSettings userSettings, string aspectName) {
            if (userSettings == null)
                return null;
            IDatabaseUserSettingsAspect userSettingsAspect = FindAspectByName(userSettings, aspectName);
            if (userSettingsAspect == null) {
                userSettingsAspect = CreateUserSettingsAspect(objectSpace, objectSpace.TypesInfo.FindTypeInfo(userSettings.GetType()).FindMember("Aspects").ListElementType);
                userSettingsAspect.Owner = userSettings;
                userSettingsAspect.Name = aspectName;
            }
            return userSettingsAspect;
        }
        private static IDatabaseUserSettings CreateUserSettings(IObjectSpace objectSpace, Type userSettingsType) {
            return (IDatabaseUserSettings)objectSpace.CreateObject(userSettingsType);
        }
        private static IDatabaseUserSettingsAspect CreateUserSettingsAspect(IObjectSpace objectSpace, Type modelAspectType) {
            return (IDatabaseUserSettingsAspect)objectSpace.CreateObject(modelAspectType);
        }
    }
}