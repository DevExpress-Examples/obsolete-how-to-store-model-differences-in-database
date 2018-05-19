Imports Microsoft.VisualBasic
Imports System
Imports System.ComponentModel

Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.Validation

Namespace UserDiffsToDB.Module
	<Browsable(False)> _
	Public Class XmlUser
		Inherits BaseObject
		Public Sub New(ByVal session As Session)
			MyBase.New(session)
		End Sub

		Public Property User() As SimpleUser
			Get
				Return GetPropertyValue(Of SimpleUser)("User")
			End Get
			Set(ByVal value As SimpleUser)
				SetPropertyValue("User", value)
			End Set
		End Property

		<Association("XmlUser-Aspects"), Aggregated> _
		Public ReadOnly Property Aspects() As XPCollection(Of XmlStore)
			Get
				Return GetCollection(Of XmlStore)("Aspects")
			End Get
		End Property
	End Class

	<Browsable(False)> _
	Public Class XmlStore
		Inherits BaseObject
		Public Sub New(ByVal session As Session)
			MyBase.New(session)
		End Sub

		<Association("XmlUser-Aspects")> _
		Public Property User() As XmlUser
			Get
				Return GetPropertyValue(Of XmlUser)("User")
			End Get
			Set(ByVal value As XmlUser)
				SetPropertyValue("User", value)
			End Set
		End Property

		Public Property Aspect() As String
			Get
				Return GetPropertyValue(Of String)("Aspect")
			End Get
			Set(ByVal value As String)
				SetPropertyValue("Aspect", value)
			End Set
		End Property

		<Size(SizeAttribute.Unlimited)> _
		Public Property XmlData() As String
			Get
				Return GetPropertyValue(Of String)("XmlData")
			End Get
			Set(ByVal value As String)
				SetPropertyValue("XmlData", value)
			End Set
		End Property
	End Class
End Namespace
