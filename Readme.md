<!-- default file list -->
*Files to look at*:

* [ManageUserSettingsListViewController.cs](./CS/DatabaseUserSettings/Controllers/ManageUserSettingsListViewController.cs) (VB: [ManageUserSettingsListViewController.vb](./VB/DatabaseUserSettings/Controllers/ManageUserSettingsListViewController.vb))
* [ManageUserSettingsWindowController.cs](./CS/DatabaseUserSettings/Controllers/ManageUserSettingsWindowController.cs) (VB: [ManageUserSettingsWindowController.vb](./VB/DatabaseUserSettings/Controllers/ManageUserSettingsWindowController.vb))
* **[DatabaseUserSettingsStore.cs](./CS/DatabaseUserSettings/DatabaseUserSettingsStore.cs) (VB: [DatabaseUserSettingsStore.vb](./VB/DatabaseUserSettings/DatabaseUserSettingsStore.vb))**
* [Module.cs](./CS/DatabaseUserSettings/Module.cs) (VB: [Module.vb](./VB/DatabaseUserSettings/Module.vb))
* [Updater.cs](./CS/E968.Module/Updater.cs) (VB: [Updater.vb](./VB/E968.Module/Updater.vb))
<!-- default file list end -->
# OBSOLETE - How to: Store Model Differences in Database


<p><strong>========================================================================</strong><br /><strong>Starting with v14.2, the database storage is supported out-of-the box (see <a href="https://documentation.devexpress.com/#Xaf/CustomDocument3698">How to: Store the Application Model Differences in the Database</a>). You should not use this example in a new project.</strong><br /><strong>========================================================================</strong><br /><br />This example illustrates how to store user UI settings (so-called model differences) in the application database instead of the file system (in Windows Forms applications) and session (in ASP.NET appications).</p>
<p>By default, an <strong>XAF Windows Forms application</strong> stores user customizations of the <a href="https://documentation.devexpress.com/xaf/CustomDocument2596.aspx"><u>Application Model</u></a> in the <em>Model.User.xafml</em> file, located in the application's directory. The application's model difference storage is also file-based (<em>Model.xafml</em> file). These two layers are superimposed when running an application. All model-related changes made by the end-user (e.g., layout customizations) are saved to the <em>Model.User.xafml</em>. The potential disadvantages of this default approach are:</p>
<br />
<p>- The <em>Model.xafm</em>l file is not shared (except when the application is deployed to the terminal server). So even if this file is modified by an administrator, it must be re-deployed to all users to take effect;</p>
<p>- Since model differences are stored in a plain XML format, anyone who has access to XAFML files can easily influence the work of the application. So, it is possible to affect a critical setting that will cause the application to stop functioning;</p>
<p>- Several users sharing the same copy of the application cannot have individual customizations. This can be resolved by changing the user differences file location to the user profile folder (by setting the <strong>UserModelDiffsLocation </strong>key value to the <strong>CurrentUserApplicationDataFolder </strong>in the application configuration file). However, the problem persists if users are working under the same system account.</p>
<p>By default, an <strong>XAF ASP.NET application</strong> stores user customizations of the Application Model in the session. The global application model difference storage is file-based (<em>Model.xafml</em> file located in the web application folder). These two layers are superimposed when running an application. All model-related changes (e.g., columns order and visibility in List Views, dashboard layouts, etc.) made by end-users are saved to cookies. If a user accesses the ASP.NET application via several different devices (desktop, laptop or tablet), settings are not synchronized.</p>
<br />
<p>Generally, you can use the default storage in most scenarios. However, the disadvantages listed above become critical if you wish to give more power to application administrators and enhance application security. This example provides a solution that eliminates all these disadvantages. The main idea is to store all model differences in the application's database. Follow the instructions below to use the database model difference storage in your XAF application.</p>
<p> </p>
<p><strong>Build the DatabaseUserSettings.dll Assembly</strong></p>
<p>- Download the solution attached to this example.</p>
<p>- Upgrade the downloaded solution up to your currently installed version of DXperience. Use the <a href="http://help.devexpress.com/ProjectConverter/CustomDocument2529.aspx"><u>Project Converter</u></a> utility for this purpose.</p>
<p>- Open the converted solution in Visual Studio. Switch to the <strong>Release </strong>configuration and build the <strong>DatabaseUserSettings</strong> project (this project contains the module that we are going to use).</p>
<p>- Copy the <strong>DatabaseUserSettings.dll</strong> assembly to an appropriate location in your development workstation.</p>
<p><strong><br /> Add the DatabaseUserSettings Module to Your XAF Solution</strong><strong><br /> </strong></p>
<p>- Open your XAF solution that will use the DatabaseUserSettings module.</p>
<p>- Add the <em>DatabaseModelStrorage.dll</em> reference to the platform-agnostic module project.</p>
<p>- Right-click the <em>Module.cs</em> file and choose View Code. Modify the module's constructor in the following manner.</p>
<p>          </p>


```cs
public sealed partial class MySolutionModule : ModuleBase {
    public MySolutionModule() {
        InitializeComponent();
        this.RequiredModuleTypes.Add(typeof(DatabaseUserSettings.DatabaseUserSettingsModule));
    }
}

```


<p><strong><em>Note:</em></strong> <em>Alternatively, you can add the </em><strong><em>DatabaseUserSettings </em></strong><em>in the </em><a href="https://documentation.devexpress.com/xaf/CustomDocument2828.aspx"><em><u>Module Designer</u></em></a><em>.</em> <em>But you should register the </em><strong><em>DatabaseUserSettingsModule</em></strong><em> toolbox item first (see </em><a href="http://msdn.microsoft.com/en-us/library/ms165355"><em><u>How to: Add Items to the Toolbox</u></em></a><em>).</em><em><br /> </em></p>
<p>Now you can logon using different credentials and apply different customizations for each user. For instance, you can change the active skin for each user. Different users will have different skins selected after the application is restarted. Users should have read/write access to the <strong>XPUserSettings</strong> and <strong>XPUserSettingsAspect</strong> persistent types (these types are provided by the <strong>DatabaseUserSettings</strong> module and are used to store model differences).</p>
<p><img src="https://raw.githubusercontent.com/DevExpress-Examples/obsolete-how-to-store-model-differences-in-database-e968/12.2.4+/media/c6fc6922-c9a8-4f37-bfe2-8febb1604152.png"></p>
<p><strong>The Configurator Account</strong><strong><br /> </strong></p>
<p>To manage default model settings applied to users, a special <strong>Configurator </strong>account can be used. All customizations made by <strong>Configurator </strong>in the Model Editor or directly in the UI will be stored to the shared Application Model layer. To create such an account, do the following.</p>
<br />
<p>- Add a Role named "Configurator" (this name is declared by the <strong>DatabaseUserSettingsModule.ConfiguratorRoleName</strong> constant).</p>
<p>- For the added Role, grant full access to the <strong>XPUserSettings </strong>and <strong>XPUserSettingsAspect </strong>persistent types and read access to the <strong>UserName </strong>member of the type that represent users in your application (<a href="https://documentation.devexpress.com/xaf/clsDevExpressExpressAppSecurityStrategySecuritySystemUsertopic.aspx"><u>SecuritySystemUser</u></a> by default). Alternatively, you can simply mark this role as administrative (see <a href="https://documentation.devexpress.com/xaf/DevExpressExpressAppSecurityStrategySecuritySystemRoleBase_IsAdministrativetopic.aspx"><u>SecuritySystemRoleBase.IsAdministrative</u></a>).</p>
<p>- Add a User named "Configurator" (this name is declared by the <strong>DatabaseUserSettingsModule.ConfiguratorUserName</strong> constant), and associate this user with the "Configurator" role.</p>
<p>Refer to the <em>Updater</em><em>.cs</em> file to see the code.</p>
<p><strong>The ManageUserSettings Action</strong><strong><br /> </strong></p>
<p>The <strong>Configurator </strong>user has access to the <strong>ManageUserSettings </strong>Action. This Action of the <a href="https://documentation.devexpress.com/xaf/clsDevExpressExpressAppActionsPopupWindowShowActiontopic.aspx"><u>PopupWindowShowAction</u></a> type is provided by the <strong>ManageUserSettingsWindowController </strong>Controller implemented in the <strong>DatabaseUserSettings </strong>module. This Action is intended to import a user setting from one user to another.</p>
<p><img src="https://raw.githubusercontent.com/DevExpress-Examples/obsolete-how-to-store-model-differences-in-database-e968/12.2.4+/media/7d995422-4fe2-4b06-bbb5-62d8d207cd83.png"></p>
<p><strong>See </strong><strong>A</strong><strong>lso</strong><strong><br /> </strong><a href="http://www.devexpress.com/issue=S32444"><u>Core - Provide an easy way to store administrator and user model differences in a custom store (e.g., in a database)</u></a><u><br /> A reusable XAF module for storing model settings in the database (security system type insensitive!)</u></p>
<p><strong>Import</strong><strong>a</strong><strong>nt</strong> <strong>Notes</strong><strong><br /> 1. </strong>Be aware of the following issue with this example: <a href="http://www.devexpress.com/issue=Q470416"><u>User settings may be duplicated/overridden under certain circumstances after merging configurator and user settings applied to the same element</u></a><u><br /> </u><strong>2.</strong> This example solution is not yet tested in the middle-tier and SecuredObjectSpaceProvider scenario and most likely, it will have to be modified to support its specifics.<br /> <strong>3.</strong> This example solution is not yet tested with <a href="https://documentation.devexpress.com/#Xaf/CustomDocument3583"><u>custom fields</u></a>.</p>

<br/>


