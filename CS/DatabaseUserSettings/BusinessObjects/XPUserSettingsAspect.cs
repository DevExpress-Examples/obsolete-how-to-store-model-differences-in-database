using DevExpress.ExpressApp.Model;
using System;
using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;

namespace DatabaseUserSettings.BusinessObjects {
    [ImageName("ModelEditor_ModelMerge")]
    [ModelDefault("Caption", "User Settings Aspect")]
    public class XPUserSettingsAspect : XPObject, IDatabaseUserSettingsAspect {
        public XPUserSettingsAspect(Session session) : base(session) { }
        [Association]
        [RuleRequiredField(null, DefaultContexts.Save)]
        public XPUserSettings Owner {
            get { return GetPropertyValue<XPUserSettings>("Owner"); }
            set { SetPropertyValue<XPUserSettings>("Owner", value); }
        }
        public string Name {
            get { return GetPropertyValue<string>("Name"); }
            set { SetPropertyValue<string>("Name", value); }
        }
        [Size(SizeAttribute.Unlimited)]
        public string Xml {
            get { return GetPropertyValue<string>("Xml"); }
            set { SetPropertyValue<string>("Xml", value); }
        }
        IDatabaseUserSettings IDatabaseUserSettingsAspect.Owner {
            get { return Owner; }
            set { Owner = value as XPUserSettings; }
        }
    }
}
