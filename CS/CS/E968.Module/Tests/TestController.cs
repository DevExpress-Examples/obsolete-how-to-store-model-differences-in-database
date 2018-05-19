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
            SimpleAction testAction = new SimpleAction(this, "TestAction", DevExpress.Persistent.Base.PredefinedCategory.View);
            testAction.Execute += new SimpleActionExecuteEventHandler(testAction_Execute);
        }
        void testAction_Execute(object sender, SimpleActionExecuteEventArgs e) {
            IModelRootNavigationItems navigationItems = ((IModelApplicationNavigationItems)Application.Model).NavigationItems;
            if (SecuritySystem.CurrentUserName == DatabaseUserSettingsModule.ConfiguratorUserName)
                navigationItems.StartupNavigationItem = navigationItems.AllItems[2];
            if (SecuritySystem.CurrentUserName == "Sam")
                navigationItems.StartupNavigationItem = navigationItems.AllItems[3];
            Application.SaveModelChanges();
        }
    }
}
#endif
