using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

using DevExpress.ExpressApp;

namespace UserDiffsToDB.Module.Win {
    [ToolboxItemFilter("Xaf.Platform.Win")]
    public sealed partial class UserDiffsToDBWindowsFormsModule : ModuleBase {
        public UserDiffsToDBWindowsFormsModule() {
            InitializeComponent();
        }
    }
}
