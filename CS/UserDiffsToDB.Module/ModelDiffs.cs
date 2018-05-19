using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace UserDiffsToDB.Module {
    [Browsable(false)]
    public class ModelDiffs : BaseObject {
        public ModelDiffs(Session session) : base(session) { }

        [Association("ModelDiffs-Aspects"), Aggregated]
        public XPCollection<ModelAspect> Aspects {
            get { return GetCollection<ModelAspect>("Aspects"); }
        }
    }

    [Browsable(false)]
    public class ModelUserDiffs : ModelDiffs {
        public ModelUserDiffs(Session session) : base(session) { }

        public SimpleUser User {
            get { return GetPropertyValue<SimpleUser>("User"); }
            set { SetPropertyValue("User", value); }
        }
    }

    [Browsable(false)]
    public class ModelAspect : BaseObject {
        public ModelAspect(Session session) : base(session) { }

        [Association("ModelDiffs-Aspects")]
        public ModelDiffs ModelDifferences {
            get { return GetPropertyValue<ModelDiffs>("ModelDifferences"); }
            set { SetPropertyValue("ModelDifferences", value); }
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
