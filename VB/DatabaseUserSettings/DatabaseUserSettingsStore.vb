Imports System.IO
Imports DevExpress.ExpressApp
Imports DevExpress.Data.Filtering
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.ExpressApp.Model
Imports DevExpress.ExpressApp.Model.Core

Namespace DatabaseUserSettings
    Public Enum ImportUserSettingsBehavior
        Merge
        Overwrite
    End Enum
    Public Interface IDatabaseUserSettings
        ReadOnly Property Aspects() As IList(Of IDatabaseUserSettingsAspect)
        Property UserId() As String
        ReadOnly Property UserName() As String
        Sub Reset()
        Sub Export()
        Sub ImportFrom(ByVal source As IDatabaseUserSettings, ByVal importBehavior As ImportUserSettingsBehavior)
    End Interface
    Public Interface IDatabaseUserSettingsAspect
        Property Name() As String
        Property Owner() As IDatabaseUserSettings
        Property Xml() As String
    End Interface
    Public Interface IManageDatabaseUserSettingsParameter
        Property Source() As IDatabaseUserSettings
        ReadOnly Property Targets() As IList(Of IDatabaseUserSettings)
        Property ImportBehavior() As ImportUserSettingsBehavior
    End Interface
    Public Class DatabaseUserSettingsStore
        Inherits ModelDifferenceStore

        Private _application As XafApplication
        Private [module] As DatabaseUserSettingsModule
        Public Const XafmlHeader As String = "<?xml version=""1.0"" encoding=""utf-8""?>"
        Public Const XafmlRootElement As String = "<Application/>"
        Public Shared ReadOnly EmptyXafml As String = String.Format("{0}{1}{2}", DatabaseUserSettingsStore.XafmlHeader, Environment.NewLine, DatabaseUserSettingsStore.XafmlRootElement)
        Public Sub New(ByVal application As XafApplication)
            Me._application = application
            Guard.ArgumentNotNull(application, "application")
            [module] = CType(Me.Application.Modules.FirstOrDefault(Function(m) m.GetType() Is GetType(DatabaseUserSettingsModule)), DatabaseUserSettingsModule)
            Guard.ArgumentNotNull([module], "module")
        End Sub
        Public Overrides ReadOnly Property Name() As String
            Get
                Return Me.GetType().Name
            End Get
        End Property
        Protected ReadOnly Property Application() As XafApplication
            Get
                Return _application
            End Get
        End Property
        Protected ReadOnly Property DatabaseUserSettingsModule() As DatabaseUserSettingsModule
            Get
                Return [module]
            End Get
        End Property
        Public Shared Function GetUserSettings(ByVal objectSpace As IObjectSpace, ByVal userSettingsType As Type, ByVal userId As String) As IDatabaseUserSettings
            Dim userSettings As IDatabaseUserSettings = Nothing
            If String.IsNullOrEmpty(userId) Then
                userSettings = GetConfiguratorUserSettings(objectSpace, userSettingsType)
            Else
                Dim user As Object = objectSpace.FindObject(DatabaseUserSettingsModule.UserTypeInfo.Type, New OperandProperty(DatabaseUserSettingsModule.UserTypeInfo.KeyMember.Name) = New OperandValue(DatabaseUserSettingsModule.UserIdTypeConverter.ConvertFromInvariantString(userId)))
                If user IsNot Nothing Then
                    If Convert.ToString(DatabaseUserSettingsModule.UserTypeInfo.FindMember(DatabaseUserSettingsModule.UserNameProperty).GetValue(user)) = DatabaseUserSettingsModule.ConfiguratorUserName Then
                        userSettings = GetConfiguratorUserSettings(objectSpace, userSettingsType)
                    Else
                        userSettings = GetUserUserSettings(objectSpace, userSettingsType, userId)
                    End If
                End If
            End If
            Return userSettings
        End Function
        Private Shared Function GetUserUserSettings(ByVal objectSpace As IObjectSpace, ByVal userSettingsType As Type, ByVal userId As String) As IDatabaseUserSettings
            Dim userSettings As IDatabaseUserSettings = CType(objectSpace.FindObject(userSettingsType, New OperandProperty(DatabaseUserSettingsModule.UserIdProperty) = New OperandValue(userId)), IDatabaseUserSettings)
            If userSettings Is Nothing Then
                userSettings = CreateUserSettings(objectSpace, userSettingsType)
                userSettings.UserId = userId
            End If
            Return userSettings
        End Function
        Public Shared Function GetConfiguratorUserSettings(ByVal objectSpace As IObjectSpace, ByVal userSettingsType As Type) As IDatabaseUserSettings
            Dim userSettings As IDatabaseUserSettings = CType(objectSpace.FindObject(userSettingsType, New NullOperator(DatabaseUserSettingsModule.UserIdProperty)), IDatabaseUserSettings)
            If userSettings Is Nothing Then
                userSettings = CreateUserSettings(objectSpace, userSettingsType)
                Dim fileUserSettingsStore As New FileModelStore(PathHelper.GetApplicationFolder(), AppDiffDefaultName)
                Dim aspects As IEnumerable(Of String) = fileUserSettingsStore.GetAspects().Concat(New String() { String.Empty })
                For Each aspectName As String In aspects
                    Try
                        GetUserSettingsAspect(objectSpace, userSettings, aspectName).Xml = File.ReadAllText(PathHelper.GetApplicationFolder() & fileUserSettingsStore.GetFileNameForAspect(aspectName))
                    Catch e As Exception
                        Tracing.Tracer.LogError(e)
                        Throw New UserFriendlyException(Localization.CannotLoadUserSettingsFromFile & Localization.UserSettingsFailureSuggestion & Environment.NewLine & e.Message)
                    End Try
                Next aspectName
            End If
            Return userSettings
        End Function
        Public Overrides Sub Load(ByVal model As ModelApplicationBase)
            Try
                Using objectSpace As IObjectSpace = Application.CreateObjectSpace()
                    Dim reader As New ModelXmlReader()
                    Dim userSettings As IDatabaseUserSettings = GetUserSettings(objectSpace, DatabaseUserSettingsModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(SecuritySystem.CurrentUserId))
                    For Each aspect As IDatabaseUserSettingsAspect In userSettings.Aspects
                        If String.IsNullOrEmpty(aspect.Xml) Then
                            aspect.Xml = EmptyXafml
                        End If
                        reader.ReadFromString(model, aspect.Name, aspect.Xml)
                    Next aspect
                    objectSpace.CommitChanges()
                End Using
            Catch e As Exception
                If Not(TypeOf e Is InvalidOperationException) Then
                    Tracing.Tracer.LogError(e)
                    Throw New UserFriendlyException(Localization.CannotLoadUserSettingsFromTheDatabase & Localization.UserSettingsFailureSuggestion & Environment.NewLine & e.Message)
                End If
            End Try
        End Sub
        Public Overrides Sub SaveDifference(ByVal model As ModelApplicationBase)
            Try
                Using objectSpace As IObjectSpace = Application.CreateObjectSpace()
                    Dim userSettings As IDatabaseUserSettings = GetUserSettings(objectSpace, DatabaseUserSettingsModule.UserSettingsType, DatabaseUserSettingsModule.UserIdTypeConverter.ConvertToInvariantString(SecuritySystem.CurrentUserId))
                    For i As Integer = 0 To model.AspectCount - 1
                        Dim writer As New ModelXmlWriter()
                        Dim xml As String = writer.WriteToString(model, i)
                        If Not String.IsNullOrEmpty(xml) Then
                            Dim aspectName As String = model.GetAspect(i)
                            Dim aspect As IDatabaseUserSettingsAspect = GetUserSettingsAspect(objectSpace, userSettings, aspectName)
                            aspect.Xml = String.Format("{0}{1}{2}", XafmlHeader, Environment.NewLine, xml)
                        End If
                    Next i
                    objectSpace.CommitChanges()
                End Using
            Catch e As Exception
                If Not(TypeOf e Is InvalidOperationException) Then
                    Tracing.Tracer.LogError(e)
                    Throw New UserFriendlyException(Localization.CannotSaveUserSettingsToTheDatabase & Localization.UserSettingsFailureSuggestion & Environment.NewLine & e.Message)
                End If
            End Try
        End Sub
        Public Shared Function FindAspectByName(ByVal userSettings As IDatabaseUserSettings, ByVal aspectName As String) As IDatabaseUserSettingsAspect
            If userSettings Is Nothing OrElse userSettings.Aspects Is Nothing Then
                Return Nothing
            End If
            Dim userSettingsAspect As IDatabaseUserSettingsAspect = userSettings.Aspects.FirstOrDefault(Function(a) a.Name = aspectName)
            Return userSettingsAspect
        End Function
        Public Shared Function GetUserSettingsAspect(ByVal objectSpace As IObjectSpace, ByVal userSettings As IDatabaseUserSettings, ByVal aspectName As String) As IDatabaseUserSettingsAspect
            If userSettings Is Nothing Then
                Return Nothing
            End If
            Dim userSettingsAspect As IDatabaseUserSettingsAspect = FindAspectByName(userSettings, aspectName)
            If userSettingsAspect Is Nothing Then
                userSettingsAspect = CreateUserSettingsAspect(objectSpace, objectSpace.TypesInfo.FindTypeInfo(CType(userSettings, Object).GetType()).FindMember("Aspects").ListElementType)
                userSettingsAspect.Owner = userSettings
                userSettingsAspect.Name = aspectName
            End If
            Return userSettingsAspect
        End Function
        Private Shared Function CreateUserSettings(ByVal objectSpace As IObjectSpace, ByVal userSettingsType As Type) As IDatabaseUserSettings
            Return CType(objectSpace.CreateObject(userSettingsType), IDatabaseUserSettings)
        End Function
        Private Shared Function CreateUserSettingsAspect(ByVal objectSpace As IObjectSpace, ByVal modelAspectType As Type) As IDatabaseUserSettingsAspect
            Return CType(objectSpace.CreateObject(modelAspectType), IDatabaseUserSettingsAspect)
        End Function
    End Class
End Namespace