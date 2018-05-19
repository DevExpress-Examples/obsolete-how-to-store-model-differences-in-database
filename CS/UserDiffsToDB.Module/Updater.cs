using System;

using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.Persistent.BaseImpl;

namespace UserDiffsToDB.Module {
    public class Updater : ModuleUpdater {
        public Updater(Session session, Version currentDBVersion) : base(session, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();

            SimpleUser adminUser = Session.FindObject<SimpleUser>(new BinaryOperator("UserName", "Sam"));
            if (adminUser == null) {
                adminUser = new SimpleUser(Session);
                adminUser.UserName = "Sam";
                adminUser.FullName = "Sam";
            }
            adminUser.IsAdministrator = true;
            adminUser.SetPassword("");
            adminUser.Save();

            SimpleUser user = Session.FindObject<SimpleUser>(new BinaryOperator("UserName", "John"));
            if (user == null) {
                user = new SimpleUser(Session);
                user.UserName = "John";
                user.FullName = "John";
            }
            user.IsAdministrator = false;
            user.SetPassword("");
            user.Save();
        }
    }
}