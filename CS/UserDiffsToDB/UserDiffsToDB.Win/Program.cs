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

            winApplication.Security = new MySecuritySimple<SimpleUser>(isAdminMode, new AuthenticationStandard<SimpleUser>());
            
            winApplication.CreateCustomUserModelDifferenceStore += winApplication_CreateCustomUserModelDifferenceStore;
            winApplication.CreateCustomModelDifferenceStore += winApplication_CreateCustomModelDifferenceStore;
            winApplication.LoggedOn += winApplication_LoggedOn;
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

        static void winApplication_LoggedOn(object sender, LogonEventArgs e) {
            if (isAdminMode) {
                DialogResult result = WinApplication.Messaging.Show(
                    "The application is running in the administrator mode. In this mode, all changes\n" +
                    "you make in the Model Editor are automatically propagated to all users, as well\n" +
                    "as UI modifications (e.g. field layout modifications, columns order, etc.).\n" +
                    "Do you want to continue?",
                    "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if(result == DialogResult.No) ((WinApplication)sender).Exit();
            }
        }

        static void winApplication_CreateCustomModelDifferenceStore(
            object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            e.Store = isAdminMode ? null : new DatabaseModelStore((XafApplication)sender);
            e.Handled = true;
        }
        static void winApplication_CreateCustomUserModelDifferenceStore(
            object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            e.Store = isAdminMode ? new DatabaseModelStore((XafApplication)sender) : 
                new DatabaseUserModelStore((XafApplication)sender);
            e.Handled = true;
        }
    }
}