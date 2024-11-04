Imports System.Data
Imports NUnit.Framework

Namespace TinyRedirectServerTest

    <TestFixture>
    Public Class StartUpAndEnd

        <SetUp>
        Public Sub Setup()
        End Sub

        <Test> Public Sub StartStopRestartServerOnVerySamePort(<Values(7001)> portStartNumber As Integer, <Values(2)> headerContentTypeVariantToExecute As Integer)
            Dim Port As Integer = portStartNumber + headerContentTypeVariantToExecute
            'Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver, Nothing, New String() {"http://localhost:7001/"})
            Dim Url As String = "http://localhost:" & Port.ToString & "/"
            Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver,
                                                                          Function(handler As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
                                                                              Dim HeaderContentTypeVariants As String() = New String() {Nothing, "text/csv", "text/csv; charset=utf-8"}
                                                                              Dim HeaderContentType As String = HeaderContentTypeVariants(headerContentTypeVariantToExecute)
                                                                              Dim Headers As New System.Collections.Specialized.NameValueCollection
                                                                              If HeaderContentType <> Nothing Then
                                                                                  Headers("content-type") = HeaderContentType
                                                                              End If
                                                                              Return Headers
                                                                          End Function,
                                                                          New String() {Url})
            Dim ws2 As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver,
                                                                          Function(handler As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
                                                                              Dim HeaderContentTypeVariants As String() = New String() {Nothing, "text/csv", "text/csv; charset=utf-8"}
                                                                              Dim HeaderContentType As String = HeaderContentTypeVariants(headerContentTypeVariantToExecute)
                                                                              Dim Headers As New System.Collections.Specialized.NameValueCollection
                                                                              If HeaderContentType <> Nothing Then
                                                                                  Headers("content-type") = HeaderContentType
                                                                              End If
                                                                              Return Headers
                                                                          End Function,
                                                                          New String() {Url})

            Try
                System.Console.WriteLine("## Check 0 - Stop no active listener should not throw exceptions")
                ws.Stop()
                System.Console.WriteLine("## Check 1 - Start + Stop regularly")
                ws.Run()
                ws.Stop()
                System.Console.WriteLine("## Check 2 - Restart + Stop again")
                ws.Run()
                ws.Stop()
                System.Console.WriteLine("## Check 3 - Start another webserver on very same Port")
                ws2.Run()
                ws2.Stop()
            Finally
                ws.Stop()
            End Try


        End Sub

        ''' <summary>
        ''' Test from a mini-webserver providing a CSV download with missing response header content-type/charset 
        ''' </summary>
        ''' <remarks>
        ''' The CSV file is returned as UTF-8 bytes
        ''' </remarks>
        <Test>
        Public Sub ReadFromHttpServerWithUtf8(<Values(8001)> portStartNumber As Integer, <Values(2)> headerContentTypeVariantToExecute As Integer, <Values("1", "2")> loopName As Integer)
            Dim Port As Integer = portStartNumber + headerContentTypeVariantToExecute + loopName * 10
            Dim Url As String = "http://localhost:" & Port.ToString & "/"
            Dim FileEncoding As System.Text.Encoding = Nothing
            Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver,
                                                                          Function(handler As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
                                                                              Dim HeaderContentTypeVariants As String() = New String() {Nothing, "text/csv", "text/csv; charset=utf-8"}
                                                                              Dim HeaderContentType As String = HeaderContentTypeVariants(headerContentTypeVariantToExecute)
                                                                              Dim Headers As New System.Collections.Specialized.NameValueCollection
                                                                              If HeaderContentType <> Nothing Then
                                                                                  Headers("content-type") = HeaderContentType
                                                                              End If
                                                                              Return Headers
                                                                          End Function,
                                                                          New String() {Url})
            Try
                ws.Run()
                'Dim c As System.Net.WebRequest
#Disable Warning SYSLIB0014 ' Typ oder Element ist veraltet
                Dim c As New System.Net.WebClient()
                'c = System.Net.HttpWebRequest.Create(Url)
#Enable Warning SYSLIB0014 ' Typ oder Element ist veraltet
                Dim Expected As String = "Test,Column" & ControlChars.CrLf & "1,äöüßÄÖÜ2" & ControlChars.CrLf & "2," & Url.Replace("http://localhost:" & Port, "")
                Dim Data As String
                'data= c.GetResponse.GetResponseStream.EndWrite
                'data=c.GetStringAsync(Url).Result
                Dim binary As Byte() = c.DownloadData(Url)
                Data = System.Text.Encoding.UTF8.GetString(binary)
                System.Console.WriteLine(Data)
                Assert.AreEqual(Expected, Data)
            Finally
                ws.Stop()
            End Try
        End Sub

        ''' <summary>
        ''' Test from a mini-webserver providing a CSV download with missing response header content-type/charset 
        ''' </summary>
        ''' <remarks>
        ''' The CSV file is returned as UTF-8 bytes from server
        ''' PLEASE NOTE: System.Net.WebClient.Encoding default to UTF-8 with .NET Framework, but to local default encoding (e.g. WIN1252) at .NET Core
        ''' </remarks>
        <Test> Public Sub ReadFromHttpServerWithReportedFileEncoding(<Values(8101)> portStartNumber As Integer, <Values(2)> headerContentTypeVariantToExecute As Integer, <Values("1", "2")> loopName As Integer)
            Dim Port As Integer = portStartNumber + headerContentTypeVariantToExecute + loopName * 10
            Dim Url As String = "http://localhost:" & Port.ToString & "/"
            Dim FileEncoding As System.Text.Encoding = Nothing
            Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver,
                                                                          Function(handler As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
                                                                              Dim HeaderContentTypeVariants As String() = New String() {Nothing, "text/csv", "text/csv; charset=utf-8"}
                                                                              Dim HeaderContentType As String = HeaderContentTypeVariants(headerContentTypeVariantToExecute)
                                                                              Dim Headers As New System.Collections.Specialized.NameValueCollection
                                                                              If HeaderContentType <> Nothing Then
                                                                                  Headers("content-type") = HeaderContentType
                                                                              End If
                                                                              Return Headers
                                                                          End Function,
                                                                          New String() {Url})
            Try
                ws.Run()
                'Dim c As System.Net.WebRequest
#Disable Warning SYSLIB0014 ' Typ oder Element ist veraltet
                Dim c As New WebClientWithDynamicEncoding()
                'c = System.Net.HttpWebRequest.Create(Url)
#Enable Warning SYSLIB0014 ' Typ oder Element ist veraltet
                Dim Expected As String = "Test,Column" & ControlChars.CrLf & "1,äöüßÄÖÜ2" & ControlChars.CrLf & "2," & Url.Replace("http://localhost:" & Port, "")
                Dim Data As String
                'data= c.GetResponse.GetResponseStream.EndWrite
                'data=c.GetStringAsync(Url).Result
                Dim binary As Byte() = c.DownloadData(Url)
                Console.WriteLine("ClientDefaultEncoding=" & c.Encoding.EncodingName)
                Console.WriteLine("ServerResponseEncoding=" & c.GetResponseEncoding.EncodingName)
                Data = c.GetResponseEncoding.GetString(binary)
                System.Console.WriteLine(Data)
                Assert.AreEqual(Expected, Data)
            Finally
                ws.Stop()
            End Try
        End Sub

        <Test> Public Sub ReadFromHttpServer_StartStopOnly(<Values(8201)> portStartNumber As Integer, <Values(0, 1, 2)> headerContentTypeVariantToExecute As Integer)
            Dim Port As Integer = portStartNumber + headerContentTypeVariantToExecute
            Dim Url As String = "http://localhost:" & Port.ToString & "/"
            Dim FileEncoding As System.Text.Encoding = Nothing
            Dim ws As New CompuMaster.Web.TinyWebServerAdvanced.WebServer(AddressOf TestDataTableLocalhostTestWebserver,
                                                                          Function(handler As System.Net.HttpListenerRequest) As System.Collections.Specialized.NameValueCollection
                                                                              Dim HeaderContentTypeVariants As String() = New String() {Nothing, "text/csv", "text/csv; charset=utf-8"}
                                                                              Dim HeaderContentType As String = HeaderContentTypeVariants(headerContentTypeVariantToExecute)
                                                                              Dim Headers As New System.Collections.Specialized.NameValueCollection
                                                                              If HeaderContentType <> Nothing Then
                                                                                  Headers("content-type") = HeaderContentType
                                                                              End If
                                                                              Return Headers
                                                                          End Function,
                                                                          New String() {Url})
            Try
                ws.Run()
            Finally
                ws.Stop()
            End Try
        End Sub

        ''' <summary>
        ''' A test CSV file with unicode characters
        ''' </summary>
        ''' <param name="handler"></param>
        ''' <returns></returns>
        <CodeAnalysis.SuppressMessage("Major Code Smell", "S1172:Unused procedure parameters should be removed", Justification:="<Ausstehend>")>
        Private Shared Function TestDataTableLocalhostTestWebserver(handler As System.Net.HttpListenerRequest, ParamArray urls As String()) As String
            Return "Test,Column" & ControlChars.CrLf & "1,äöüßÄÖÜ2" & ControlChars.CrLf & "2," & handler.RawUrl
        End Function

    End Class

End Namespace