using System;

using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;

namespace UserDiffsToDB.Module {
    public class Updater : ModuleUpdater {
        public Updater(ObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            SimpleUser adminUser = ObjectSpace.FindObject<SimpleUser>(new BinaryOperator("UserName", "Sam"));
            if (adminUser == null) {
                adminUser = ObjectSpace.CreateObject<SimpleUser>();
                adminUser.UserName = "Sam";
                adminUser.FullName = "Sam";
            }
            adminUser.IsAdministrator = true;
            adminUser.SetPassword("");
            adminUser.Save();

            SimpleUser user = ObjectSpace.FindObject<SimpleUser>(new BinaryOperator("UserName", "John"));
            if (user == null) {
                user = ObjectSpace.CreateObject<SimpleUser>();
                user.UserName = "John";
                user.FullName = "John";
            }
            user.IsAdministrator = false;
            user.SetPassword("");
            user.Save();
        }
    }
}