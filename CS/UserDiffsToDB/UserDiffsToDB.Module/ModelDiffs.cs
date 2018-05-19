using System;
using System.ComponentModel;

using DevExpress.Xpo;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace UserDiffsToDB.Module {
    [Browsable(false)]
    public abstract class ModelDiffsBase : BaseObject {
        public ModelDiffsBase(Session session) : base(session) { }

        [Association("ModelDiffs-Aspects"), Aggregated]
        public XPCollection<ModelAspect> Aspects {
            get { return GetCollection<ModelAspect>("Aspects"); }
        }
    }

    [Browsable(false)]
    public class ModelDiffs : ModelDiffsBase {
        public ModelDiffs(Session session) : base(session) { }
    }

    [Browsable(false)]
    public class ModelUserDiffs : ModelDiffsBase {
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
        public ModelDiffsBase ModelDifferences {
            get { return GetPropertyValue<ModelDiffsBase>("ModelDifferences"); }
            set { SetPropertyValue("ModelDifferences", value); }
        }
        public string Aspect {
            get { return GetPropertyValue<string>("Aspect"); }
            set { SetPropertyValue<string>("Aspect", value); }
        }

        [Size(SizeAttribute.Unlimited)]
        public string XmlData {
            get { return GetPropertyValue<string>("XmlData"); }
            set { SetPropertyValue("XmlData", value); }
        }
    }
}
