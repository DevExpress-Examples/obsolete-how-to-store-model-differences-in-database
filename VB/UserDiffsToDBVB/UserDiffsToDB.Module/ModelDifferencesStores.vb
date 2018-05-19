Imports Microsoft.VisualBasic
Imports System
Imports System.IO
Imports System.Collections.Generic
Imports System.Text
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Core
Imports DevExpress.ExpressApp.Model.Core
Imports DevExpress.Data.Filtering
Imports DevExpress.ExpressApp.Model
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.Base

Namespace UserDiffsToDB.Module
    Public Class DatabaseModelStore
        Inherits ModelDifferenceStore
        Private Shared ReadOnly xmlHeader As String = "<?xml version=""1.0"" encoding=""utf-8""?>" & System.Environment.NewLine
        Private application As XafApplication
        Public Sub New(ByVal winApplication As XafApplication)
            application = winApplication
        End Sub

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "DatabaseModelStore"
            End Get
        End Property

        Protected Function FindModelAspect(ByVal stores As IEnumerable(Of ModelAspect), ByVal aspect As String) As ModelAspect
            For Each store As ModelAspect In stores
                If store.Aspect = aspect Then
                    Return store
                End If
            Next store
            Return Nothing
        End Function

        Protected Overridable Function GetModelDiffs(ByVal objectSpace As IObjectSpace) As ModelDiffsBase
            Dim modelDiffs As ModelDiffs = objectSpace.FindObject(Of ModelDiffs)(Nothing)
            If modelDiffs Is Nothing Then
                modelDiffs = objectSpace.CreateObject(Of ModelDiffs)()
                modelDiffs.Save()
            End If
            Return modelDiffs
        End Function

        Public Overrides Sub Load(ByVal model As ModelApplicationBase)
            Dim xmlReader As New ModelXmlReader()
            Dim objectSpace As IObjectSpace = application.CreateObjectSpace()
            Dim modelDiffs As ModelDiffsBase = GetModelDiffs(objectSpace)
            For Each store As ModelAspect In modelDiffs.Aspects
                If store.Aspect Is Nothing Then
                    store.Aspect = String.Empty
                End If
                xmlReader.ReadFromString(model, store.Aspect, store.XmlData)
            Next store
            objectSpace.CommitChanges()
        End Sub

        Public Overrides Sub SaveDifference(ByVal model As ModelApplicationBase)
            Dim objectSpace As IObjectSpace = application.CreateObjectSpace()
            Dim modelDiffs As ModelDiffsBase = GetModelDiffs(objectSpace)
            For i As Integer = 0 To model.AspectCount - 1
                Dim xmlWriter As New ModelXmlWriter()
                Dim aspect As String = model.GetAspect(i)
                Dim xmlContent As String = xmlWriter.WriteToString(model, i)
                If (Not String.IsNullOrEmpty(xmlContent)) Then
                    Dim modelAspect As ModelAspect = FindModelAspect(modelDiffs.Aspects, aspect)
                    If modelAspect Is Nothing Then
                        modelAspect = objectSpace.CreateObject(Of ModelAspect)()
                    End If
                    modelAspect.ModelDifferences = modelDiffs
                    modelAspect.Aspect = aspect
                    modelAspect.XmlData = xmlHeader & xmlContent
                End If
            Next i
            objectSpace.CommitChanges()
        End Sub
    End Class

    Public Class DatabaseUserModelStore
        Inherits DatabaseModelStore
        Public Overrides ReadOnly Property Name() As String
            Get
                Return "DatabaseUserModelStore"
            End Get
        End Property
        Public Sub New(ByVal application As XafApplication)
            MyBase.New(application)
        End Sub
        Protected Overrides Function GetModelDiffs(ByVal objectSpace As IObjectSpace) As ModelDiffsBase
            Dim currentUser As SimpleUser = objectSpace.GetObject(Of SimpleUser)(CType(SecuritySystem.CurrentUser, SimpleUser))
            Dim criteriaOperator As CriteriaOperator = New BinaryOperator("User", currentUser, BinaryOperatorType.Equal)
            Dim modelUserDiffs As ModelUserDiffs = objectSpace.FindObject(Of ModelUserDiffs)(criteriaOperator)
            If modelUserDiffs Is Nothing Then
                modelUserDiffs = objectSpace.CreateObject(Of ModelUserDiffs)()
                modelUserDiffs.User = currentUser
            End If
            Return modelUserDiffs
        End Function
    End Class
End Namespace
