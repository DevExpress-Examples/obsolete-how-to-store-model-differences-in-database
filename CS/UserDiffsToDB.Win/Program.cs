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

namespace UserDiffsToDB.Win {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached;
            UserDiffsToDBWindowsFormsApplication winApplication = new UserDiffsToDBWindowsFormsApplication();
            winApplication.CreateCustomUserModelDifferenceStore += new EventHandler<CreateCustomModelDifferenceStoreEventArgs>(winApplication_CreateCustomUserModelDifferenceStore);
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

        static void winApplication_CreateCustomUserModelDifferenceStore(object sender, CreateCustomModelDifferenceStoreEventArgs e) {
            DictionaryDifferenceStore userDiffs = new UserStore((WinApplication)sender);
            e.Store = userDiffs;
            e.Handled = true;
        }

        public class UserStore : DictionaryDifferenceStore {
            private static readonly string xmlHeader =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + System.Environment.NewLine;
            private WinApplication application;

            public override string Name {
                get {
                    return "UserStore";
                }
            }

            public UserStore(WinApplication winApplication) {
                application = winApplication;
            }

            private XmlStore FindStoreByAspect(XPCollection<XmlStore> stores, string aspect) {
                foreach (XmlStore store in stores) {
                    if (store.Aspect == aspect) return store;
                }
                return null;
            }

            private XmlUser FindXmlUserByCurrentUser() {
                ObjectSpace objectSpace = application.CreateObjectSpace();
                SimpleUser currentUser = objectSpace.GetObject((SimpleUser)SecuritySystem.CurrentUser);
                XmlUser user = objectSpace.FindObject<XmlUser>(
                    new BinaryOperator("User", currentUser, BinaryOperatorType.Equal));
                return user;
            }

            protected override Dictionary LoadDifferenceCore(Schema schema) {
                DictionaryXmlReader Reader = new DictionaryXmlReader();
                XmlUser user = FindXmlUserByCurrentUser();
                if (user != null) {
                    Dictionary dictionary = new Dictionary(schema);
                    foreach (XmlStore store in user.Aspects) {
                        dictionary.AddAspect(store.Aspect, Reader.ReadFromString(store.XmlData));
                    }
                    return dictionary;
                }
                return null;
            }

            public override void SaveDifference(Dictionary diffDictionary) {
                DictionaryXmlWriter Writer = new DictionaryXmlWriter();
                ObjectSpace objectSpace = application.CreateObjectSpace();
                XmlUser user = objectSpace.GetObject(FindXmlUserByCurrentUser());
                if (user == null) {
                    user = new XmlUser(objectSpace.Session);
                    user.User = objectSpace.GetObject((SimpleUser)SecuritySystem.CurrentUser);
                }
                foreach (string aspect in diffDictionary.Aspects) {                    
                    string xmlContent = Writer.GetAspectXml(diffDictionary.GetAspectIndex(aspect), diffDictionary.RootNode);
                    if (!string.IsNullOrEmpty(xmlContent)) {                    
                    XmlStore store = FindStoreByAspect(user.Aspects, aspect);
                    if (store == null)
                        store = new XmlStore(objectSpace.Session);
                    store.User = user;
                    store.Aspect = aspect;
                    store.XmlData = xmlHeader + xmlContent;
                    }
                }
                objectSpace.CommitChanges();
            }
        }
    }
}