Option Explicit On
Option Strict On

Imports System.Net
Imports CompuMaster.Web.TinyWebServerAdvanced

Public Class HttpRedirServerRequestContextHandler
    Inherits CompuMaster.Web.TinyWebServerAdvanced.HttpRequestContextHandler

    Public RedirBaseUrl As String

    Public Sub New(context As HttpListenerContext, redirBaseUrl As String)
        MyBase.New(context)
        Me.RedirBaseUrl = redirBaseUrl
    End Sub

    Public Overrides Sub GetResponse()
        Context.Response.Redirect(Me.RedirBaseUrl & Context.Request.RawUrl)
    End Sub
End Class