Imports System.Net
Imports System.Text
Imports System.Text.RegularExpressions

Public Class WebClientWithDynamicEncoding
    Inherits WebClient

    Public Sub New()
#Disable Warning SYSLIB0014 ' Typ oder Element ist veraltet
        MyBase.New()
#Enable Warning SYSLIB0014 ' Typ oder Element ist veraltet
    End Sub

    Private responseEncoding As Encoding = Encoding.Default

    Protected Overrides Function GetWebResponse(request As WebRequest) As WebResponse
        Dim response = MyBase.GetWebResponse(request)
        Dim contentType As String = response.Headers("Content-Type")

        ' Standard-Encoding festlegen, falls nichts gefunden wird
        responseEncoding = Encoding.Default

        ' Encoding aus Content-Type-Header extrahieren
        If Not String.IsNullOrEmpty(contentType) Then
            Dim match = Regex.Match(contentType, "charset=([^;]+)", RegexOptions.IgnoreCase)
            If match.Success Then
                Dim charset As String = match.Groups(1).Value
                Try
                    responseEncoding = Encoding.GetEncoding(charset)
                Catch ex As ArgumentException
                    ' Fallback auf Default-Encoding, falls das Encoding nicht unterstützt wird
                    responseEncoding = Encoding.Default
                End Try
            End If
        End If

        Return response
    End Function

    ''' <summary>
    ''' The encoding of the response from the last request
    ''' </summary>
    ''' <returns></returns>
    Public Function GetResponseEncoding() As Encoding
        Return responseEncoding
    End Function

    Protected Shadows Function DownloadString(uri As Uri) As String
        Dim data As Byte() = DownloadData(uri)
        Return responseEncoding.GetString(data)
    End Function

End Class
