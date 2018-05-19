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

            winApplication.Security = New MySecuritySimple(Of SimpleUser)(isAdminMode, New AuthenticationStandard(Of SimpleUser)())

            AddHandler winApplication.CreateCustomUserModelDifferenceStore, _
            AddressOf winApplication_CreateCustomUserModelDifferenceStore
            AddHandler winApplication.CreateCustomModelDifferenceStore, _
            AddressOf winApplication_CreateCustomModelDifferenceStore
            AddHandler winApplication.LoggedOn, AddressOf winApplication_LoggedOn
            If ConfigurationManager.ConnectionStrings("ConnectionString") IsNot Nothing Then
                winApplication.ConnectionString = _
                ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString
            End If
            Try
                winApplication.Setup()
                winApplication.Start()
            Catch e As Exception
                winApplication.HandleException(e)
            End Try
        End Sub

        Private Shared Sub winApplication_LoggedOn(ByVal sender As Object, ByVal e As LogonEventArgs)
            If isAdminMode Then
                Dim result As DialogResult = WinApplication.Messaging.Show( _
                "The application is running in the administrator mode. In this mode, all changes" & Constants.vbLf & _
                "you make in the Model Editor are automatically propagated to all users, as well" & Constants.vbLf & _
                "as UI modifications (e.g. field layout modifications, columns order, etc.)." & Constants.vbLf & _
                "Do you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If result = DialogResult.No Then
                    CType(sender, WinApplication).Exit()
                End If
            End If
        End Sub

        Private Shared Sub winApplication_CreateCustomModelDifferenceStore( _
        ByVal sender As Object, ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            If isAdminMode Then
                e.Store = Nothing
            Else
                e.Store = New DatabaseModelStore(CType(sender, XafApplication))
            End If
            e.Handled = True
        End Sub
        Private Shared Sub winApplication_CreateCustomUserModelDifferenceStore( _
        ByVal sender As Object, ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            If isAdminMode Then
                e.Store = New DatabaseModelStore(CType(sender, XafApplication))
            Else
                e.Store = New DatabaseUserModelStore(CType(sender, XafApplication))
            End If
            e.Handled = True
        End Sub
    End Class
End Namespace