Option Explicit On
Option Strict On

Imports System.Net
Imports CompuMaster.Web.TinyWebServerAdvanced

Public Class HttpRedirServerRequestContextHandlerFactory
    Inherits CompuMaster.Web.TinyWebServerAdvanced.HttpRequestContextHandlerFactory

    Public RedirBaseUrl As String

    Public Sub New(redirBaseUrl As String)
        Me.RedirBaseUrl = redirBaseUrl
    End Sub

    Public Overrides Function CreateRequestHandlerInstance(context As HttpListenerContext) As HttpRequestContextHandler
        Return New HttpRedirServerRequestContextHandler(context, Me.RedirBaseUrl)
    End Function
End Class