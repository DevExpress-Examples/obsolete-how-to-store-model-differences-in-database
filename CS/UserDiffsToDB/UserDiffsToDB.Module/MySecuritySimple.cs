using System;
using System.Collections.Generic;
using System.Text;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;
using DevExpress.Persistent.Base.Security;


namespace UserDiffsToDB.Module {
    public class MySecuritySimple<UserType> : SecuritySimple<UserType> where UserType : ISimpleUser {
        public MySecuritySimple(bool isAdminMode, AuthenticationBase authentication)
            : base(authentication) {
            IsAdminMode = isAdminMode;
        }
        public override void Logon(object user) {
            if(IsAdminMode) {
                if(!((SimpleUser)user).IsAdministrator) {
                    throw (new UserFriendlyException(
                        "Only administrators can run the application " +
                        "with the '-admin' command line parameter."));
                }
            }
            base.Logon(user);
        }

        bool isAdminMode;
        public bool IsAdminMode { get{return isAdminMode;} set{isAdminMode = value;} }
    }
}
