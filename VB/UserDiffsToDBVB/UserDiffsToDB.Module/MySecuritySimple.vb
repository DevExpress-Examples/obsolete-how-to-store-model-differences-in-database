Imports System
Imports System.Collections.Generic
Imports System.Text
Imports DevExpress.ExpressApp.Security
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.Base.Security

Namespace UserDiffsToDB.Module

    Public Class MySecuritySimple(Of UserType As ISimpleUser)
        Inherits SecuritySimple(Of UserType)

        Public Sub New(ByVal isAdminMode As Boolean, ByVal authentication As AuthenticationBase)
            MyBase.new(authentication)
            Me.IsAdminMode = isAdminMode
        End Sub
        Public Overrides Sub Logon(ByVal user As Object)
            If IsAdminMode Then
                If Not (CType(User, SimpleUser)).IsAdministrator Then
                    Throw (New UserFriendlyException( _
                    "Only administrators can run the application " & _
                    "with the '-admin' command line parameter."))
                End If
            End If
            MyBase.Logon(User)
        End Sub

        Private _IsAdminMode As Boolean
        Public Property IsAdminMode() As Boolean
            Get
                Return _IsAdminMode
            End Get
            Set(ByVal value As Boolean)
                _IsAdminMode = value
            End Set
        End Property


    End Class
End Namespace