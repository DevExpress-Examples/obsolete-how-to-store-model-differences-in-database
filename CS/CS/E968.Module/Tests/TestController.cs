#if EasyTest
using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;

namespace DatabaseUserSettings.Tests {
    public class TestController : ViewController {
        public TestController() {
            SimpleAction testAction = new SimpleAction(this, "TestAction", DevExpress.Persistent.Base.PredefinedCategory.Tools);
            testAction.Execute += new SimpleActionExecuteEventHandler(testAction_Execute);
        }
        void testAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            IModelRootNavigationItems navigationItems = ((IModelApplicationNavigationItems)Application.Model).NavigationItems;
            if (SecuritySystem.CurrentUserName == DatabaseUserSettingsModule.ConfiguratorUserName)
                navigationItems.StartupNavigationItem = navigationItems.AllItems["SecuritySystemRole_ListView"];
            if (SecuritySystem.CurrentUserName == "Sam")
                navigationItems.StartupNavigationItem = navigationItems.AllItems["SecuritySystemUser_ListView"];
            Application.SaveModelChanges();
        }
    }
}
#endif
