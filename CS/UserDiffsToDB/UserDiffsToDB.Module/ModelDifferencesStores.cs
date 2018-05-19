using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;

namespace UserDiffsToDB.Module {
    public class DatabaseModelStore : ModelDifferenceStore {
        private static readonly string xmlHeader =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine;
        private XafApplication application;
        public DatabaseModelStore(XafApplication winApplication) {
            application = winApplication;
        }

        public override string Name {
            get {
                return "DatabaseModelStore";
            }
        }

        protected ModelAspect FindModelAspect(IEnumerable<ModelAspect> stores, string aspect) {
            foreach (ModelAspect store in stores) {
                if (store.Aspect == aspect) return store;
            }
            return null;
        }
        protected virtual ModelDiffs GetModelDiffs(ObjectSpace objectSpace) {
            IList<ModelDiffs> modelDiffsList = objectSpace.GetObjects<ModelDiffs>();
            return modelDiffsList.Count == 0 ? objectSpace.CreateObject<ModelDiffs>() : modelDiffsList[0];
        }

        public override void Load(ModelApplicationBase model) {
            ModelXmlReader xmlReader = new ModelXmlReader();
            ObjectSpace objectSpace = application.CreateObjectSpace();
            ModelDiffs modelDiffs = GetModelDiffs(objectSpace);
            foreach (ModelAspect store in modelDiffs.Aspects) {
                xmlReader.ReadFromString(model, store.Aspect, store.XmlData);
            }
            objectSpace.CommitChanges();
        }

        public override void SaveDifference(ModelApplicationBase model) {
            ObjectSpace objectSpace = application.CreateObjectSpace();
            ModelDiffs modelDiffs = GetModelDiffs(objectSpace);
            for (int i = 0; i < model.AspectCount; ++i) {
                ModelXmlWriter xmlWriter = new ModelXmlWriter();
                string aspect = model.GetAspect(i);
                string xmlContent = xmlWriter.WriteToString(model, i);
                if (!string.IsNullOrEmpty(xmlContent)) {
                    ModelAspect modelAspect = FindModelAspect(modelDiffs.Aspects, aspect);
                    if (modelAspect == null) {
                        modelAspect = objectSpace.CreateObject<ModelAspect>();
                    }
                    modelAspect.ModelDifferences = modelDiffs;
                    modelAspect.Aspect = aspect;
                    modelAspect.XmlData = xmlHeader + xmlContent;
                }
            }
            objectSpace.CommitChanges();
        }
    }

    public class DatabaseUserModelStore : DatabaseModelStore {
        public override string Name {
            get {
                return "DatabaseUserModelStore";
            }
        }
        public DatabaseUserModelStore(XafApplication application)
            : base(application) {
        }
        protected override ModelDiffs GetModelDiffs(ObjectSpace objectSpace) {
            SimpleUser currentUser = objectSpace.GetObject<SimpleUser>((SimpleUser)SecuritySystem.CurrentUser);
            CriteriaOperator criteriaOperator = new BinaryOperator("User", currentUser, BinaryOperatorType.Equal);
            ModelUserDiffs modelUserDiffs = objectSpace.FindObject<ModelUserDiffs>(criteriaOperator);
            if (modelUserDiffs == null) {
                modelUserDiffs = objectSpace.CreateObject<ModelUserDiffs>();
                modelUserDiffs.User = currentUser;
            }
            return modelUserDiffs;
        }
    }
}
