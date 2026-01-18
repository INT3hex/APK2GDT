Imports System.IO
Module Common
    Public bWriteDebugLog As Boolean = Form1._appConfig.GetProperty("bWriteDebugLog", True)
    Public bDebug As Boolean = Form1._appConfig.GetProperty("bDebug", True)

    Public Sub DebugPrint(ByRef sDebugOutput As String)
        Try
            If bDebug Then
                Debug.WriteLine(sDebugOutput)
                If bWriteDebugLog Then File.AppendAllText(Application.StartupPath & "\Config\App.log", DateTime.Now + " [DEBUG] " + sDebugOutput + Environment.NewLine)
            End If
        Catch ex As Exception
        End Try
    End Sub

End Module
