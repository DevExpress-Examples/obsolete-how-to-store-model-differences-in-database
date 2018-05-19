Imports Microsoft.VisualBasic
Imports System
Imports System.Configuration
Imports System.Windows.Forms

Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Security
Imports DevExpress.ExpressApp.Win
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl

Imports UserDiffsToDB.Module
Imports DevExpress.ExpressApp.Model.Core
Imports DevExpress.ExpressApp.Model
Imports System.Collections.Generic
Imports DevExpress.Persistent.Base.Security

Namespace UserDiffsToDB.Win
    Public Class SecuritySimpleAdminOnly
        Inherits SecuritySimple
        Public Sub New(ByVal securitySimple As SecuritySimple)
            Me.Authentication = securitySimple.Authentication
            Me.IsGrantedForNonExistentPermission = securitySimple.IsGrantedForNonExistentPermission
            Me.UserType = securitySimple.UserType
        End Sub
        Public Overrides Sub Logon(ByVal user As Object)
            MyBase.Logon(user)
            If Not(CType(user, ISimpleUser)).IsAdministrator Then
                Throw New AuthenticationException((CType(user, ISimpleUser)).UserName)
            End If
        End Sub
    End Class
    Friend NotInheritable Class Program
        Private Shared isAdminMode As Boolean
        ''' <summary>
        ''' The main entry point for the application.
        ''' </summary>
        Private Sub New()
        End Sub
        <STAThread> _
        Shared Sub Main(ByVal args() As String)
            isAdminMode = args.Length > 0 AndAlso args(0) = "-admin"
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)
            EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached
            Dim winApplication As New UserDiffsToDBWindowsFormsApplication()
            If isAdminMode Then
                winApplication.Security = New SecuritySimpleAdminOnly(CType(winApplication.Security, SecuritySimple))
            End If
            AddHandler winApplication.CreateCustomUserModelDifferenceStore, AddressOf winApplication_CreateCustomUserModelDifferenceStore
            AddHandler winApplication.CreateCustomModelDifferenceStore, AddressOf winApplication_CreateCustomModelDifferenceStore
            If ConfigurationManager.ConnectionStrings("ConnectionString") IsNot Nothing Then
                winApplication.ConnectionString = ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
            End If
            Try
                winApplication.Setup()
                winApplication.Start()
            Catch e As Exception
                winApplication.HandleException(e)
            End Try
        End Sub

        Private Shared Sub winApplication_CreateCustomModelDifferenceStore(ByVal sender As Object, ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            If isAdminMode Then
                e.Store = Nothing
            Else
                e.Store = New DatabaseModelStore(CType(sender, XafApplication))
            End If
            e.Handled = True
        End Sub
        Private Shared Sub winApplication_CreateCustomUserModelDifferenceStore(ByVal sender As Object, ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            If isAdminMode Then
                e.Store = New DatabaseModelStore(CType(sender, XafApplication))
            Else
                e.Store = New DatabaseUserModelStore(CType(sender, XafApplication))
            End If
            e.Handled = True
        End Sub
    End Class
End Namespace