using System;
using System.Configuration;
using System.Windows.Forms;

using DevExpress.Xpo;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;

using UserDiffsToDB.Module;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model;
using System.Collections.Generic;
using DevExpress.Persistent.Base.Security;

namespace UserDiffsToDB.Win {
    public class SecuritySimpleAdminOnly : SecuritySimple {
        public SecuritySimpleAdminOnly(SecuritySimple securitySimple) {
            this.Authentication = securitySimple.Authentication;
            this.IsGrantedForNonExistentPermission = securitySimple.IsGrantedForNonExistentPermission;
            this.UserType = securitySimple.UserType;
        }
        public override void Logon(object user) {
            base.Logon(user);
            if (!((ISimpleUser)user).IsAdministrator) {
                throw new AuthenticationException(((ISimpleUser)user).UserName);
            }
        }
    }
    static class Program {
        static bool isAdminMode;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args) {
            isAdminMode = args.Length > 0 && args[0] == "-admin";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            UserDiffsToDBWindowsFormsApplication winApplication = new UserDiffsToDBWindowsFormsApplication();
            if (isAdminMode) {
                winApplication.Security = new SecuritySimpleAdminOnly((SecuritySimple)winApplication.Security);
            }
            winApplication.CreateCustomUserModelDifferenceStore += new EventHandler<CreateCustomModelDifferenceStoreEventArgs>(winApplication_CreateCustomUserModelDifferenceStore);
            winApplication.CreateCustomModelDifferenceStore += new EventHandler<CreateCustomModelDifferenceStoreEventArgs>(winApplication_CreateCustomModelDifferenceStore);
            if (ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            }
            try {
                winApplication.Setup();
                winApplication.Start();
            }
            catch (Exception e) {
                winApplication.HandleException(e);
            }
        }

        static void winApplication_CreateCustomModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            if (isAdminMode) {
                e.Store = null;
            }
            else {
                e.Store = new DatabaseModelStore((XafApplication)sender);
            }
            e.Handled = true;
        }
        static void winApplication_CreateCustomUserModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            if (isAdminMode) {
                e.Store = new DatabaseModelStore((XafApplication)sender);
            }
            else {
                e.Store =  new DatabaseUserModelStore((XafApplication)sender);
            }
            e.Handled = true;
        }
    }
}