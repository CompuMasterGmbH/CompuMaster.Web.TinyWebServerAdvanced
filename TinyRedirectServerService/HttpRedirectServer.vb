Option Explicit On
Option Strict On

Imports System.Net
Imports CompuMaster.Web.TinyWebServerAdvanced

Public Class HttpRedirectServer
    Implements IDisposable

    '    Private Function RequestHandlerForContent(reqeust As System.Net.HttpListenerRequest) As String
    '        Return "<html>
    '<head><title>302 Found</title></head>
    '<body bgcolor=""white"">
    '<center><h1>302 Found</h1></center>
    '<hr><center>nginx</center>
    '</body>
    '</html>"
    '    End Function

    '    Private Function RequestHandlerForResponseHeaders(request As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
    '        Dim Headers As New System.Collections.Specialized.NameValueCollection
    '        Headers("Content-Type") = "text/html"
    '        Headers("Location") = Me.RedirectUrl & request.RawUrl
    '        Return Headers
    '    End Function

    Public Property RedirectUrl As String
    Public ReadOnly Property Prefixes As String()
        Get
            If ws Is Nothing Then
                Return Nothing
            Else
                Return ws.Prefixes.ToArray
            End If
        End Get
    End Property


    Dim ws As CompuMaster.Web.TinyWebServerAdvanced.WebServer

    Public Sub Start()
        Me.Start(Me.Prefixes)
    End Sub

    Public Sub Start(ParamArray urls As String())
        If urls.Length = 0 Then Throw New Exception("Local source url required before start")
        If Me.RedirectUrl = "" Then Throw New Exception("Redirection target url required before start")
        'Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf Me.RequestHandlerForContent, AddressOf Me.RequestHandlerForResponseHeaders, Url)
        Dim RequestHandlerFactory As New HttpRedirServerRequestContextHandlerFactory(Me.RedirectUrl)
        ws = New CompuMaster.Web.TinyWebServerAdvanced.WebServer(RequestHandlerFactory, urls)
        ws.Run()
    End Sub

    Public Sub [Stop]()
        If ws IsNot Nothing Then ws.Stop()
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' Dient zur Erkennung redundanter Aufrufe.

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                Me.Stop()
            End If
        End If
        disposedValue = True
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

End Class
