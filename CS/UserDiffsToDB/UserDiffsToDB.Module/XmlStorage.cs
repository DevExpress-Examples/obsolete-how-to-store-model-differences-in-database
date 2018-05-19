using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace UserDiffsToDB.Module {
    [Browsable(false)]
    public class XmlUser : BaseObject {
        public XmlUser(Session session) : base(session) { }

        public SimpleUser User {
            get { return GetPropertyValue<SimpleUser>("User"); }
            set { SetPropertyValue("User", value); }
        }

        [Association("XmlUser-Aspects"), Aggregated]
        public XPCollection<XmlStore> Aspects {
            get { return GetCollection<XmlStore>("Aspects"); }
        }
    }

    [Browsable(false)]
    public class XmlStore : BaseObject {
        public XmlStore(Session session) : base(session) { }

        [Association("XmlUser-Aspects")]
        public XmlUser User {
            get { return GetPropertyValue<XmlUser>("User"); }
            set { SetPropertyValue("User", value); }
        }

        public string Aspect {
            get { return GetPropertyValue<string>("Aspect"); }
            set { SetPropertyValue("Aspect", value); }
        }

        [Size(SizeAttribute.Unlimited)]
        public string XmlData {
            get { return GetPropertyValue<string>("XmlData"); }
            set { SetPropertyValue("XmlData", value); }
        }
    }
}
