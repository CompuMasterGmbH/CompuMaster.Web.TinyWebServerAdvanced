Option Explicit On
Option Strict On

Public Class MainForm

    Private HttpServer As CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer

    Public ReadOnly Property ServerRunning As Boolean
        Get
            Return (HttpServer IsNot Nothing)
        End Get
    End Property

    Private Function GetArrayItemOrNothing(array As String(), index As Integer) As String
        If index < array.Length Then
            Return array(index)
        Else
            Return Nothing
        End If
    End Function

    Sub RefreshLabelsAfterOnOff()
        If ServerRunning Then
            Me.ButtonSwitchOnOff.Text = "Switch Off"
            Me.LabelRunState.Text = "Server is running at:"
            Me.LinkLabel1.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 0)
            Me.LinkLabel2.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 1)
            Me.LinkLabel3.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 2)
            Me.LinkLabel4.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 3)
            Me.LinkLabel5.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 4)
            Me.LinkLabel6.Text = GetArrayItemOrNothing(HttpServer.Prefixes, 5)
        Else
            Me.ButtonSwitchOnOff.Text = "Switch On"
            Me.LabelRunState.Text = "Server is NOT running, currently"
            Me.LinkLabel1.Text = ""
            Me.LinkLabel2.Text = ""
            Me.LinkLabel3.Text = ""
            Me.LinkLabel4.Text = ""
            Me.LinkLabel5.Text = ""
            Me.LinkLabel6.Text = ""
        End If
    End Sub

    Private Sub Form_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text &= " - " & RedirectUrl
        RefreshLabelsAfterOnOff()
        Me.MinimumSize = Me.Size
        Me.MaximumSize = Me.Size
    End Sub

    Public Property RedirectUrl As String = "https://extranet.compumaster.de"

    Private Sub ButtonSwitchOnOff_Click(sender As Object, e As EventArgs) Handles ButtonSwitchOnOff.Click
        If ServerRunning Then
            HttpServer.Stop()
            HttpServer.Dispose()
            HttpServer = Nothing
        Else
            HttpServer = New CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer()
            HttpServer.RedirectUrl = Me.RedirectUrl
            Dim ListenUrls As New List(Of String)
            Try
                ListenUrls.Add("http://*:8480/")
                'ListenUrls.Add("https://*:8443/")
                HttpServer.Start(ListenUrls.ToArray)
            Catch ex As System.Net.HttpListenerException
                Dim RecommendationsInfo As String = Nothing
                If System.Environment.OSVersion.Platform = PlatformID.Win32NT AndAlso ex.ErrorCode = 5 Then
                    'Access denied error
                    RecommendationsInfo = System.Environment.NewLine & "Either restart with admin rights or add start privileges for non-privileged user based on following sample command:" & System.Environment.NewLine & "netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user"
                    MessageBox.Show(Me, "Error starting webserver " & LocalBindings(ListenUrls, HttpServer) & " with following HttpListenerException details: " & ex.Message & RecommendationsInfo, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    If System.Diagnostics.Debugger.IsAttached Then
                        MessageBox.Show(Me, "Error starting webserver " & LocalBindings(ListenUrls, HttpServer) & " with following HttpListenerException details: " & ex.ToString, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Else
                        MessageBox.Show(Me, "Error starting webserver " & LocalBindings(ListenUrls, HttpServer) & " with following HttpListenerException details: " & ex.Message, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End If
                HttpServer = Nothing
            Catch ex As Exception
                If True OrElse System.Diagnostics.Debugger.IsAttached Then
                    MessageBox.Show(Me, "Error starting webserver " & LocalBindings(ListenUrls, HttpServer) & " with following exception details: " & ex.ToString, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Else
                    MessageBox.Show(Me, "Error starting webserver " & LocalBindings(ListenUrls, HttpServer) & " with following exception details: " & ex.Message, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
                HttpServer = Nothing
            End Try
        End If
        Me.RefreshLabelsAfterOnOff()
    End Sub

    Private Shared Sub RestartAsAdmin()
        Dim startInfo As New ProcessStartInfo(My.Application.Info.AssemblyName) With {.Verb = "runas"}
        Process.Start(startInfo)
        Environment.Exit(0)
    End Sub


    Private Shared Function LocalBindings(listenUrls As List(Of String), httpServer As CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer) As String
        If httpServer.Prefixes IsNot Nothing AndAlso httpServer.Prefixes.Length <> 0 Then
            LocalBindings = System.Environment.NewLine & "- " & Strings.Join(httpServer.Prefixes, System.Environment.NewLine & "- ") & System.Environment.NewLine
        ElseIf listenUrls.Count <> 0 Then
            LocalBindings = System.Environment.NewLine & "- " & Strings.Join(listenUrls.ToArray, System.Environment.NewLine & "- ") & System.Environment.NewLine
        Else
            LocalBindings = "{MISSING BINDING INFO}"
        End If
    End Function


    Private Sub LinkLabels_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabel1.LinkClicked, LinkLabel2.LinkClicked, LinkLabel3.LinkClicked, LinkLabel4.LinkClicked, LinkLabel5.LinkClicked, LinkLabel6.LinkClicked
        System.Diagnostics.Process.Start(CType(sender, LinkLabel).Text)
    End Sub

    Private Sub MainForm_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        If ServerRunning Then
            HttpServer.Stop()
            HttpServer.Dispose()
            HttpServer = Nothing
        End If
    End Sub

    Private Sub ButtonRestartAsAdmin_Click(sender As Object, e As EventArgs) Handles ButtonRestartAsAdmin.Click
        RestartAsAdmin()
    End Sub

End Class
