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
        Public Sub New(ByVal objectSpace As ObjectSpace, ByVal currentDBVersion As Version)
            MyBase.New(objectSpace, currentDBVersion)
        End Sub
        Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
            MyBase.UpdateDatabaseAfterUpdateSchema()

            Dim adminUser As SimpleUser = ObjectSpace.FindObject(Of SimpleUser)(New BinaryOperator("UserName", "Sam"))
            If adminUser Is Nothing Then
                adminUser = ObjectSpace.CreateObject(Of SimpleUser)()
                adminUser.UserName = "Sam"
                adminUser.FullName = "Sam"
            End If
            adminUser.IsAdministrator = True
            adminUser.SetPassword("")
            adminUser.Save()

            Dim user As SimpleUser = ObjectSpace.FindObject(Of SimpleUser)(New BinaryOperator("UserName", "John"))
            If user Is Nothing Then
                user = ObjectSpace.CreateObject(Of SimpleUser)()
                user.UserName = "John"
                user.FullName = "John"
            End If
            user.IsAdministrator = False
            user.SetPassword("")
            user.Save()
        End Sub
    End Class
End Namespace