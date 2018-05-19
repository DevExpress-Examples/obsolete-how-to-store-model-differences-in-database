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

Namespace UserDiffsToDB.Win
	Friend NotInheritable Class Program
		''' <summary>
		''' The main entry point for the application.
		''' </summary>
		Private Sub New()
		End Sub
		<STAThread> _
		Shared Sub Main()
			Application.EnableVisualStyles()
			Application.SetCompatibleTextRenderingDefault(False)
			EditModelPermission.AlwaysGranted = System.Diagnostics.Debugger.IsAttached
			Dim winApplication As New UserDiffsToDBWindowsFormsApplication()
			AddHandler winApplication.CreateCustomUserModelDifferenceStore, AddressOf winApplication_CreateCustomUserModelDifferenceStore
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

        Private Shared Sub winApplication_CreateCustomUserModelDifferenceStore(ByVal sender As Object, _
      ByVal e As CreateCustomModelDifferenceStoreEventArgs)
            Dim userDiffs As DictionaryDifferenceStore = New UserStore(CType(sender, WinApplication))
            e.Store = userDiffs
            e.Handled = True
        End Sub

        Public Class UserStore
            Inherits DictionaryDifferenceStore
            Private Shared ReadOnly xmlHeader As String = _
            "<?xml version=""1.0"" encoding=""utf-8""?>" & System.Environment.NewLine
            Private application As WinApplication

            Public Overrides ReadOnly Property Name() As String
                Get
                    Return "UserStore"
                End Get
            End Property

            Public Sub New(ByVal winApplication As WinApplication)
                application = winApplication
            End Sub

            Private Function FindStoreByAspect(ByVal stores As XPCollection(Of XmlStore), ByVal aspect As String) As XmlStore
                For Each store As XmlStore In stores
                    If store.Aspect = aspect Then
                        Return store
                    End If
                Next store
                Return Nothing
            End Function

            Private Function FindXmlUserByCurrentUser() As XmlUser
                Dim objectSpace As ObjectSpace = application.CreateObjectSpace()
                Dim currentUser As SimpleUser = objectSpace.GetObject(CType(SecuritySystem.CurrentUser, SimpleUser))
                Dim user As XmlUser = objectSpace.FindObject(Of XmlUser)( _
                New BinaryOperator("User", currentUser, BinaryOperatorType.Equal))
                Return user
            End Function

            Protected Overrides Function LoadDifferenceCore(ByVal schema As Schema) As Dictionary
                Dim Reader As New DictionaryXmlReader()
                Dim user As XmlUser = FindXmlUserByCurrentUser()
                If user IsNot Nothing Then
                    Dim dictionary As New Dictionary(schema)
                    For Each store As XmlStore In user.Aspects
                        dictionary.AddAspect(store.Aspect, Reader.ReadFromString(store.XmlData))
                    Next store
                    Return dictionary
                End If
                Return Nothing
            End Function

            Public Overrides Sub SaveDifference(ByVal diffDictionary As Dictionary)
                Dim Writer As New DictionaryXmlWriter()
                Dim objectSpace As ObjectSpace = application.CreateObjectSpace()
                Dim user As XmlUser = objectSpace.GetObject(FindXmlUserByCurrentUser())
                If user Is Nothing Then
                    user = New XmlUser(objectSpace.Session)
                    user.User = objectSpace.GetObject(CType(SecuritySystem.CurrentUser, SimpleUser))
                End If
                For Each aspect As String In diffDictionary.Aspects
                    Dim xmlContent As String = Writer.GetAspectXml(aspect, diffDictionary.RootNode)
                    If (Not String.IsNullOrEmpty(xmlContent)) Then
                    Dim store As XmlStore = FindStoreByAspect(user.Aspects, aspect)
                    If store Is Nothing Then
                        store = New XmlStore(objectSpace.Session)
                    End If
                    store.User = user
                    store.Aspect = aspect
                    store.XmlData = xmlHeader & xmlContent
                    End If
                Next aspect
                objectSpace.CommitChanges()
            End Sub
        End Class
	End Class
End Namespace
