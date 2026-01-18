Imports System.IO
Imports System.Text
Imports System.Linq


Module gdtAPI

    Private sGDTDataString As New StringBuilder()
    Private oGDTData As New List(Of (String, String))()
    Public Structure myGDTBefund
        Public Name As String
        Public Vorname As String
        Public PatID As String
        Public BefundTag As String
        Public BefundZeit As String
    End Structure

    Public Const cGDTBefundanfordern = "6302"
    Public Const cGDTBefundanzeige = "6311"


    Public Structure GDTDataset
        Public Satzart As String
        Public Name As String
        Public Vorname As String
        Public PatID As String
        Public BefundTag As String      ' Befundanzeige
        Public BefundZeit As String     ' Befundanzeige
    End Structure

    Sub WriteGDTFile(ByVal sGDTFile As String, ByVal sPatientID As String, ByVal sAPKtimeText As String)
        ' Create GDT-File with PatientenID (Feldkennung 3000)
        addGDT("8000", "6310")        ' Satzart Stammdaten=6301, Messdaten=6310 übermitteln
        addGDT("8100", "?????")       ' Dateilänge initialisieren
        addGDT("8315", "T2MED_PX")    ' GDT-ID Empfänger
        addGDT("8316", "APK2")        ' GDT-ID Sender
        addGDT("9218", "02.10")       ' GDT Version
        addGDT("3000", sPatientID)    ' PatientenID
        addGDT("6200", Date.Now.ToString("ddMMyyyy"))   ' Datum
        addGDT("6201", Date.Now.ToString("HHmm"))       ' Uhrzeit 
        addGDT("8402", "NULL01")      ' Untersuchungsart
        addGDT("8410", "APK01")       ' Test-Ident
        addGDT("8470", $"Patientenkontakt: {sAPKtimeText}")       ' Kommentar/APK-Zeit

        addGDT("8100", "")            ' Calculate/patch file length

        'DebugPrint($"WriteGDT: Creating GDT {sGDTFile}{Environment.NewLine}{sGDTDataString}")
        DebugPrint($"WriteGDT: Creating GDT {sGDTFile}")
        Try
            File.WriteAllText(sGDTFile, sGDTDataString.ToString())
        Catch ex As Exception
            DebugPrint($"WriteGDT: Exception while writing GDT-File: {ex.Message}")
        End Try
    End Sub

    Sub T2medSearchViaGDT(ByVal sGDTFile As String, ByVal sPatientID As String)
        ' Create GDT-File with PatientenID (Feldkennung 3000)
        addGDT("8000", "6301")        ' Satzart Stammdaten übermitteln
        addGDT("8100", "?????")       ' Dateilänge initialisieren
        addGDT("8315", "T2MED_PX")    ' GDT-ID Empfänger
        addGDT("8316", "TAPITray")    ' GDT-ID Sender
        addGDT("9218", "02.10")       ' GDT Version
        addGDT("3000", sPatientID)    ' PatientenID
        addGDT("8100", "")            ' Calculate/patch file length

        Console.WriteLine($"T2medSearch: Creating GDT {sGDTFile}{Environment.NewLine}{sGDTDataString}")

        Try
            File.WriteAllText(sGDTFile, sGDTDataString.ToString())
        Catch ex As Exception
            Console.WriteLine($"T2medSearch: Exception while writing GDT-File: {ex.Message}")
        End Try
    End Sub

    Sub addGDT(ByVal sSatzart As String, ByVal sContent As String)
        Select Case sSatzart
            Case "8000"
                ' Initialize GDTDataSet
                sGDTDataString.Clear()
            Case "8100"
                If sContent = "" Then
                    ' Calculate GDTSize
                    Dim length As Integer = sGDTDataString.Length
                    Dim formattedLength As String = length.ToString("D5")
                    sGDTDataString.Replace("?????", formattedLength)
                    Return
                Else
                    ' Initialize GDTSize
                    sContent = "?????"
                End If
        End Select

        ' BDT/GDT-Dataset nnn####xxxxxxx<CR><LF> (nnn=Size, ####=Satzart, xxxxxxx=Content)
        sGDTDataString.AppendFormat("{0:000}{1}{2}{3}{4}", sContent.Length + 3 + 4 + 2, sSatzart, sContent, ControlChars.Cr, ControlChars.Lf)
    End Sub

    Function GetGDTValue(ByVal oGDTData As List(Of (String, String)), ByVal sSearchItem As String) As String
        Return oGDTData.FirstOrDefault(Function(item) item.Item1 = sSearchItem).Item2
    End Function

    Function ReadGDTFile(ByVal sGDTFile As String) As List(Of (String, String))
        Dim oGDTData As New List(Of (String, String))()

        Try
            DebugPrint($"Lese GDT-Datei {sGDTFile}")
            ' Überprüfen, ob die Datei existiert
            If File.Exists(sGDTFile) Then
                ' Datei zeilenweise lesen
                Using sr As New StreamReader(sGDTFile)
                    Dim line As String
                    line = sr.ReadLine()
                    While (line IsNot Nothing)

                        If line.Length >= 7 Then
                            Dim length As String = line.Substring(0, 3)         'nnn
                            Dim sSatzart As String = line.Substring(3, 4)       '####
                            Dim sContent As String = line.Substring(7)          'xxxxxxxxxxx
                            If Convert.ToInt32(length) <> (sContent.Length + 9) Then DebugPrint($"Hinweis: Eintrag {sSatzart} nicht längenkonform")
                            oGDTData.Add((sSatzart, sContent))
                        End If
                        line = sr.ReadLine()
                    End While
                End Using
            Else
                DebugPrint("Die Datei existiert nicht: " & sGDTFile)
            End If
        Catch ex As Exception
            DebugPrint("Fehler beim Einlesen der GDT-Datei: " & ex.Message)
        End Try

        Return oGDTData
    End Function

    Function GetBefundFromGDT(ByVal sGDTFile As String, ByRef oGDTBefund As myGDTBefund) As String
        'Dim oGDTBefund As myGDTBefund
        Const cBefund = "6311"
        Dim sGDT_Satzart As String
        Dim sGDT_8315 As String

        Try
            oGDTData = Nothing
            oGDTData = ReadGDTFile(sGDTFile)
            If oGDTData.Count > 1 Then
                sGDT_Satzart = GetGDTValue(oGDTData, "8000") ' Identifier/Satzart ermitteln
                DebugPrint($"Satzart: {sGDT_Satzart}")
                'Satzlänge, Zeichensatz und GDT-Version werden nicht überprüft
                If sGDT_Satzart = cBefund Then   ' Satzart 6311 Untersuchung anzeigen
                    sGDT_8315 = GetGDTValue(oGDTData, "8315") ' GTD-ID Empfänger
                    DebugPrint($"Hinweis: GDT-ID Empfänger: {sGDT_8315}")
                    oGDTBefund.BefundTag = GetGDTValue(oGDTData, "6200")    ' Untersuchungstag => TransaktionsID
                    oGDTBefund.BefundZeit = GetGDTValue(oGDTData, "6201")   ' Untersuchungszeit => TransaktionsID
                    oGDTBefund.PatID = GetGDTValue(oGDTData, "3000")        ' PatientenID
                    oGDTBefund.Name = GetGDTValue(oGDTData, "3101")         ' Name
                    oGDTBefund.Vorname = GetGDTValue(oGDTData, "3102")      ' Vorname
                    DebugPrint($"Befund für Pat {oGDTBefund.PatID} eingelesen")
                    ' 
                End If
            End If
            Return ""
        Catch ex As Exception
            Return ex.Message
        End Try


    End Function

    Function gdt_ParseFile(ByVal sGDTFile As String, ByRef oGDT As GDTDataset) As String
        Dim sGDT_Satzart As String
        Dim sGDT_8315 As String

        Try
            oGDTData = Nothing
            oGDTData = ReadGDTFile(sGDTFile)
            If oGDTData.Count > 1 Then
                sGDT_Satzart = GetGDTValue(oGDTData, "8000") ' Identifier/Satzart ermitteln
                DebugPrint($"Satzart: {sGDT_Satzart}")
                'nichtkonforme GDT-implementierungen werden unterstützt
                'Satzlänge, Zeichensatz, GDT-Empfänger und GDT-Version werden nicht überprüft

                sGDT_8315 = GetGDTValue(oGDTData, "8315") ' GTD-ID Empfänger
                DebugPrint($"Hinweis: GDT-ID Empfänger: {sGDT_8315}")

                oGDT.Satzart = sGDT_Satzart
                oGDT.PatID = GetGDTValue(oGDTData, "3000")        ' PatientenID
                oGDT.Name = GetGDTValue(oGDTData, "3101")         ' Name
                oGDT.Vorname = GetGDTValue(oGDTData, "3102")      ' Vorname

                If sGDT_Satzart = cGDTBefundanzeige Then   ' Satzart 6311 Untersuchung anzeigen
                    oGDT.BefundTag = GetGDTValue(oGDTData, "6200")    ' Untersuchungstag => TransaktionsID
                    oGDT.BefundZeit = GetGDTValue(oGDTData, "6201")   ' Untersuchungszeit => TransaktionsID
                    DebugPrint($"Messung/Befund für Pat {oGDT.PatID} eingelesen")
                End If

                If sGDT_Satzart = cGDTBefundanfordern Then   ' Satzart 6302 Untersuchung anfordern
                    DebugPrint($"Messung/Befund für Pat {oGDT.PatID} anfordern")
                End If

            End If
            Return ""
        Catch ex As Exception
            Return ex.Message
        End Try


    End Function
End Module


