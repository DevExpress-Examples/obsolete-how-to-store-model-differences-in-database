Imports Microsoft.VisualBasic
Imports System

Imports DevExpress.ExpressApp.Updating
Imports DevExpress.Xpo
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.BaseImpl

Namespace UserDiffsToDB.Module
    Public Class Updater
        Inherits ModuleUpdater
        Public Sub New(ByVal session As Session, ByVal currentDBVersion As Version)
            MyBase.New(session, currentDBVersion)
        End Sub
        Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
            MyBase.UpdateDatabaseAfterUpdateSchema()

            Dim adminUser As SimpleUser = Session.FindObject(Of SimpleUser)(New BinaryOperator("UserName", "Sam"))
            If adminUser Is Nothing Then
                adminUser = New SimpleUser(Session)
                adminUser.UserName = "Sam"
                adminUser.FullName = "Sam"
            End If
            adminUser.IsAdministrator = True
            adminUser.SetPassword("")
            adminUser.Save()

            Dim user As SimpleUser = Session.FindObject(Of SimpleUser)(New BinaryOperator("UserName", "John"))
            If user Is Nothing Then
                user = New SimpleUser(Session)
                user.UserName = "John"
                user.FullName = "John"
            End If
            user.IsAdministrator = False
            user.SetPassword("")
            user.Save()
        End Sub
    End Class
End Namespace