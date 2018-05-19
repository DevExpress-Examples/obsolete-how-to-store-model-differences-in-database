#DropDB E968
#Application E968Win
#Application E968Web

*FillForm
 User Name = Configurator
*Action Log On
*CheckActionValue Navigation(Default.My Details)
*Action Test Action
*ReopenApplication
*FillForm
 User Name = John
*Action Log On
;Test whether settings from Model.XAFML are loaded.
!FieldVisible Change Password On First Logon
;Test whether this Action is not available by default.
!Action Manage User Settings
;Test whether the Configurator's user setting are applied
*CheckActionValue Navigation(Default.Role)
;*Action Test Action
*Action Log Off
*FillForm
 User Name = Sam
*Action Log On
;Test whether settings from Model.XAFML are loaded.
!FieldVisible Change Password On First Logon
;Test whether this Action is not available by default.
!Action Manage User Settings
;Test whether the Configurator's user setting are applied
*CheckActionValue Navigation(Default.Role)
*Action Test Action

*Action Log Off
*FillForm
 User Name = Configurator
*Action Log On


#IfDef E968Win
*CheckActionValue Navigation(Default.Role)
#EndIf
#IfDef E968Web
;Because on the Web model changes are applied to all users.
*CheckActionValue Navigation(Default.User)
#EndIf
*Action Navigation(Default.User Settings)
*CheckTable User Settings
 Columns = User Name
 Row = John
 Row = Sam
 Row = Configurator
*Action Manage User Settings
*CheckFieldValues
 Source = Configurator
*CheckTable Targets
 Columns = User Name
 Row = Sam
 Row = John
*SelectRecords Targets
 Columns = User Name
 Row = John
*ActionAvailable Targets.Import From Source
*Action Targets.Export
*Action Targets.Reset
*HandleDialog
 Respond = Yes
*ExecuteEditorAction Source(Clear)
!ActionAvailable Targets.Import From Source
*CheckTable Targets
 Columns = User Name
 Row = Sam
 Row = John
 Row = Configurator
*FillForm
 Source = Sam
*CheckTable Targets
 Columns = User Name
 Row = John
 Row = Configurator
*ClearSelection Targets
!Action Targets.Import From Source
*SelectRecords Targets
 Columns = User Name
 Row = John
*Action Targets.Import From Source
*HandleDialog
 Respond = Yes
*Action Cancel
*Action Navigation(Default.User)
*ProcessRecord
 User Name = Sam
*OptionalAction Edit
*Action Roles.Link
*ProcessRecord
 Name = Configurator
*OptionalAction Save and Close



*Action Log Off
*FillForm
 User Name = John
*Action Log On
;Test whether the imported Sam's user settings are applied.
*CheckActionValue Navigation(Default.User)
!ActionAvailable Manage User Settings



*Action Log Off
*FillForm
 User Name = Sam
*Action Log On
*CheckActionValue Navigation(Default.User)
*Action Manage User Settings
*CheckFieldValues
 Source = Sam
*SelectRecords Targets
 Columns = User Name
 Row = John
*Action Targets.Reset
*HandleDialog
 Respond = Yes
*Action Cancel



*Action Log Off
*FillForm
 User Name = John
*Action Log On
;Test whether the user settings are resetted.
*CheckActionValue Navigation(Default.Role)
