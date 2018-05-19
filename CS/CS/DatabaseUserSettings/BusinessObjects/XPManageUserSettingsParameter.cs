using System;
using System.Linq;
using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Xpo;
using System.Collections.Generic;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Model;

namespace DatabaseUserSettings.BusinessObjects {
    [NonPersistent]
    [ModelDefault("Caption", "Select User Settings To Manage")]
    [ImageName("ModelEditor_ModelMerge")]
    public class XPManageUserSettingsParameter : XPObject, IManageDatabaseUserSettingsParameter {
        private XPCollection<XPUserSettings> targetsCore;
        public XPManageUserSettingsParameter(Session session)
            : base(session) {
            ImportBehavior = ImportUserSettingsBehavior.Overwrite;
            if (SecuritySystem.UserType != null)
                Source = (XPUserSettings)DatabaseUserSettingsStore.GetUserSettings(XPObjectSpace.FindObjectSpaceByObject(this), typeof(XPUserSettings), Convert.ToString(SecuritySystem.CurrentUserId));
        }
        [ImmediatePostData]
        public XPUserSettings Source {
            get { return GetPropertyValue<XPUserSettings>("Source"); }
            set {
                SetPropertyValue<XPUserSettings>("Source", value);
                if (Source != null)
                    Targets.Criteria = new OperandProperty("Oid") != new OperandValue(Source.Oid);
                else
                    Targets.Criteria = null;
                OnChanged("Targets");
            }
        }
        public ImportUserSettingsBehavior ImportBehavior {
            get { return GetPropertyValue<ImportUserSettingsBehavior>("ImportBehavior"); }
            set { SetPropertyValue<ImportUserSettingsBehavior>("ImportBehavior", value); }
        }
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("AllowDelete", "False")]
        public XPCollection<XPUserSettings> Targets {
            get {
                if (targetsCore == null)
                    targetsCore = new XPCollection<XPUserSettings>(Session);
                return targetsCore;
            }
        }
        #region IManageUserSettingsParameter Members
        IDatabaseUserSettings IManageDatabaseUserSettingsParameter.Source {
            get { return Source; }
            set { Source = value as XPUserSettings; }
        }
        IList<IDatabaseUserSettings> IManageDatabaseUserSettingsParameter.Targets {
            get { return new ListConverter<IDatabaseUserSettings, XPUserSettings>(Targets); }
        }
        #endregion
    }
}