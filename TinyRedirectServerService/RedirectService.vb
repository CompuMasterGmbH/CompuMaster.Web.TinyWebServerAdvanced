Public Class RedirectService

    Private HttpServer As CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer

    Public ReadOnly Property ServerRunning As Boolean
        Get
            Return (HttpServer IsNot Nothing)
        End Get
    End Property

    Protected Overrides Sub OnStart(ByVal args() As String)
        ' Code zum Starten des Dienstes hier einfügen. Diese Methode sollte Vorgänge
        ' ausführen, damit der Dienst gestartet werden kann.
        HttpServer = New CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer()
        HttpServer.RedirectUrl = My.Settings.RedirTargetUrl
        Try
            HttpServer.Start(My.Settings.ListenUrl)
        Catch ex As Exception
            My.Application.Log.DefaultFileLogWriter.Write("Error starting webserver " & Strings.Join(HttpServer.Prefixes, " ") & " and target base url """ & My.Settings.RedirTargetUrl & """ with following exception details: " & ex.Message)
            HttpServer = Nothing
            Me.Stop()
        End Try
    End Sub

    Protected Overrides Sub OnStop()
        ' Hier Code zum Ausführen erforderlicher Löschvorgänge zum Beenden des Dienstes einfügen.
        If ServerRunning Then
            Try
                HttpServer.Stop()
            Catch
            End Try
            HttpServer = Nothing
        End If
    End Sub

End Class
