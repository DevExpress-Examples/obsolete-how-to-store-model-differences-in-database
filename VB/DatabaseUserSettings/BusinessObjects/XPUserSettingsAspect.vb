Imports DevExpress.ExpressApp.Model
Imports DevExpress.Xpo
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.Validation

Namespace DatabaseUserSettings.BusinessObjects
    <ImageName("ModelEditor_ModelMerge"), ModelDefault("Caption", "User Settings Aspect")> _
    Public Class XPUserSettingsAspect
        Inherits XPObject
        Implements IDatabaseUserSettingsAspect

        Public Sub New(ByVal session As Session)
            MyBase.New(session)
        End Sub
        <Association, RuleRequiredField(Nothing, DefaultContexts.Save)> _
        Public Property Owner() As XPUserSettings
            Get
                Return GetPropertyValue(Of XPUserSettings)("Owner")
            End Get
            Set(ByVal value As XPUserSettings)
                SetPropertyValue(Of XPUserSettings)("Owner", value)
            End Set
        End Property
        Public Property Name() As String Implements IDatabaseUserSettingsAspect.Name
            Get
                Return GetPropertyValue(Of String)("Name")
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)("Name", value)
            End Set
        End Property
        <Size(SizeAttribute.Unlimited)> _
        Public Property Xml() As String Implements IDatabaseUserSettingsAspect.Xml
            Get
                Return GetPropertyValue(Of String)("Xml")
            End Get
            Set(ByVal value As String)
                SetPropertyValue(Of String)("Xml", value)
            End Set
        End Property
        Private Property IDatabaseUserSettingsAspect_Owner() As IDatabaseUserSettings Implements IDatabaseUserSettingsAspect.Owner
            Get
                Return Owner
            End Get
            Set(ByVal value As IDatabaseUserSettings)
                Owner = TryCast(value, XPUserSettings)
            End Set
        End Property
    End Class
End Namespace
