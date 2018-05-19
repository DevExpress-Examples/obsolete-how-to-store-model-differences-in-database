using System;
using System.IO;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using System.Collections.Generic;
using DevExpress.Persistent.Validation;

namespace DatabaseUserSettings.BusinessObjects {
    [DefaultClassOptions]
    [ImageName("ModelEditor_ModelMerge")]
    [Custom("Caption", "User Settings")]
    public class XPUserSettings : XPObject, IDatabaseUserSettings {
        public XPUserSettings(Session session) : base(session) { }
        [Association, Aggregated]
        public XPCollection<XPUserSettingsAspect> Aspects {
            get { return GetCollection<XPUserSettingsAspect>("Aspects"); }
        }
        [Browsable(false)]
        [RuleUniqueValue(null, DefaultContexts.Save)]
        [Custom("AllowEdit", "False")]
        public string UserId {
            get { return GetPropertyValue<string>(DatabaseUserSettingsModule.UserIdProperty); }
            set { SetPropertyValue<string>(DatabaseUserSettingsModule.UserIdProperty, value); }
        }
        private string userNameCore;
        [RuleUniqueValue(null, DefaultContexts.Save)]
        public string UserName {
            get {
                if (string.IsNullOrEmpty(UserId))
                    userNameCore = DatabaseUserSettingsModule.ConfiguratorUserName;
                else if (SecuritySystem.UserType != null && string.IsNullOrEmpty(userNameCore)) {
                    userNameCore = Convert.ToString(Session.Evaluate(
                        SecuritySystem.UserType,
                        new OperandProperty(DatabaseUserSettingsModule.UserNameProperty),
                        new OperandProperty(Session.GetClassInfo(SecuritySystem.UserType).KeyProperty.Name) == new OperandValue(UserId))
                    );
                }
                return userNameCore;
            }
        }
        [Action(Caption = "Reset", ImageName = "ModelEditor_Action_ResetDifferences_Xml", ConfirmationMessage = "You are about to reset the selected user settings record(s). Do you want to proceed?", ToolTip = "Resets selected user settings. Users must reopen their applications to see the changes.")]
        public void Reset() {
            foreach (IDatabaseUserSettingsAspect aspect in Aspects)
                aspect.Xml = DatabaseUserSettingsStore.EmptyXafml;
        }
        [Action(Caption = "Export", ImageName = "Action_Export_ToXML", ToolTip = "Exports selected user settings to an XAFML file located in the ExportedUserSettings folder near the application.")]
        public void Export() {
            string path = DatabaseUserSettingsModule.ExportedUserSettingsPath + UserName;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            foreach (IDatabaseUserSettingsAspect aspect in Aspects)
                File.WriteAllText(String.Format("{0}\\{1}{2}.xafml", path, ModelDifferenceStore.UserDiffDefaultName, string.IsNullOrEmpty(aspect.Name) ? string.Empty : "." + aspect.Name), aspect.Xml);
        }
        public void ImportFrom(IDatabaseUserSettings source, ImportUserSettingsBehavior importBehavior) {
            if (source != null)
                switch (importBehavior) {
                    case ImportUserSettingsBehavior.Merge:
                        throw new NotSupportedException("Merge will be supported in the future.");
                        break;
                    case ImportUserSettingsBehavior.Overwrite:
                        foreach (IDatabaseUserSettingsAspect aspect in Aspects) {
                            IDatabaseUserSettingsAspect sourceAspect = DatabaseUserSettingsStore.FindAspectByName(source, aspect.Name);
                            if (sourceAspect != null)
                                aspect.Xml = sourceAspect.Xml;
                        }
                        break;
                }
        }
        IList<IDatabaseUserSettingsAspect> IDatabaseUserSettings.Aspects {
            get { return new ListConverter<IDatabaseUserSettingsAspect, XPUserSettingsAspect>(Aspects); }
        }
    }
}