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

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshLabelsAfterOnOff()
        Me.MinimumSize = Me.Size
        Me.MaximumSize = Me.Size
    End Sub

    Private Sub ButtonSwitchOnOff_Click(sender As Object, e As EventArgs) Handles ButtonSwitchOnOff.Click
        If ServerRunning Then
            HttpServer.Stop()
            HttpServer.Dispose()
            HttpServer = Nothing
        Else
            HttpServer = New CompuMaster.Web.TinyRedirectServerService.HttpRedirectServer()
            HttpServer.RedirectUrl = "https://extranet.compumaster.de"
            Try
                Dim ListenUrls As New List(Of String)
                ListenUrls.Add("http://*:8480/")
                ListenUrls.Add("https://*:8443/")
                HttpServer.Start(ListenUrls.ToArray)
            Catch ex As Exception
                MessageBox.Show(Me, "Error starting webserver " & Strings.Join(HttpServer.Prefixes, " ") & " with following exception details: " & ex.Message, "Error starting webserver", MessageBoxButtons.OK, MessageBoxIcon.Error)
                HttpServer = Nothing
            End Try
        End If
        Me.RefreshLabelsAfterOnOff()
    End Sub

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
End Class
