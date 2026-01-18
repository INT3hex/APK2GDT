Imports System.Drawing

Public Class Form1
    <STAThread()>
    Public Shared Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New Form1())
    End Sub

    Private WithEvents Timer1 As New Timer()
    Private startTime As DateTime
    Private elapsedTime As TimeSpan
    Private sAPKtimeText As String
    Private oGDT As GDTDataset
    Public _appConfig As New AppConfigClass
    Public bWriteDebugLog As Boolean = Me._appConfig.GetProperty("bWriteDebugLog", False)
    Public bDebug As Boolean = Me._appConfig.GetProperty("bDebug", True)
    Public bConfirmExit As Boolean = Me._appConfig.GetProperty("bConfirmExit", False)
    Public sGDTFile As String = Me._appConfig.GetProperty("sGDTFile", "C:\GDT\APK2T2MD.GDT")
    Public sGDTFileOut As String = Me._appConfig.GetProperty("sGDTFileOut", "C:\GDT\T2MDAPK2.GDT")
    Public bTopMostForm As Boolean = Me._appConfig.GetProperty("bTopMostForm", True)
    Public iFormX As Integer = Me._appConfig.GetProperty("iFormX", 900)
    Public iFormY As Integer = Me._appConfig.GetProperty("iFormY", 5)


    ' Beim Laden des Formulars den Timer starten
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.DoubleBuffered = True
        Timer1.Interval = 10000 ' Jede 10 Sekunden aktualisieren
        Timer1.Start()
        Me.Width = 180 ' Setze die Breite des Fensters
        Me.Height = 215 ' Setze die Höhe des Fensters

        Me.FormBorderStyle = FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.TopMost = bTopMostForm
        Me.Opacity = 0.95
        Me.StartPosition = FormStartPosition.Manual
        Me.Location = New Point(iFormX, iFormY)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)


        ' Startzeit speichern
        startTime = DateTime.Now

        DebugPrint("---------------------- Startup -----------------------")
        DebugPrint($"Kommandozeile: {Environment.CommandLine()}")

        ' GDT-Datei einlesen
        LoadGDT(sGDTFile, oGDT)
    End Sub

    Private Sub Form1_DoubleClick(sender As Object, e As EventArgs) Handles Me.DoubleClick
        Application.Exit()
    End Sub

    Private Sub Form1_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        DebugPrint($"APK wird beendet: CloseReason: {e.CloseReason}")
        Dim result As DialogResult
        If bConfirmExit Or e.CloseReason = CloseReason.UserClosing Then
            result = MessageBox.Show(
            "APK beenden. Zeitdauer zurückschreiben?",
            "APK2GDT beenden",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question)
        Else
            ' Default: zurückschreiben
            result = DialogResult.Yes
        End If

        Select Case result
            Case DialogResult.Yes
                DebugPrint("Beenden und GDT zurückschreiben...")
                ' GDT aufbereiten und speichern
                SaveGDT()
            Case DialogResult.No
                DebugPrint("nur Beenden...")
            Case DialogResult.Cancel
                e.Cancel = True
        End Select
    End Sub

    ' Uhr "zeichnen"
    Private Sub Form1_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        Dim g As Graphics = e.Graphics
        Dim centerX As Integer = Me.ClientSize.Width / 2
        Dim centerY As Integer = (Me.ClientSize.Height - 30) / 2
        Dim clockRadius As Integer = Math.Min(centerX, centerY) - 10

        ' Uhr zeichnen
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        g.DrawEllipse(New Pen(Color.Black, 3), centerX - clockRadius, centerY - clockRadius, clockRadius * 2, clockRadius * 2)

        ' Minutenstriche
        For i As Integer = 0 To 59
            Dim angle As Double = Math.PI / 30 * i ' 1-Minuten-Winkel
            Dim outerX As Integer = centerX + CInt(Math.Cos(angle) * clockRadius)
            Dim outerY As Integer = centerY + CInt(Math.Sin(angle) * clockRadius)
            Dim innerX As Integer = centerX + CInt(Math.Cos(angle) * (clockRadius - If(i Mod 5 = 0, 15, 5))) ' Längere Striche bei 5-Minuten-Schritten
            Dim innerY As Integer = centerY + CInt(Math.Sin(angle) * (clockRadius - If(i Mod 5 = 0, 15, 5)))

            g.DrawLine(New Pen(Color.Black, 2), innerX, innerY, outerX, outerY)
        Next

        ' Zahlen
        For i As Integer = 1 To 12
            Dim angle As Double = Math.PI / 6 * (i - 3) ' Winkel für jede Zahl
            Dim numberX As Integer = CInt(centerX + CInt(Math.Cos(angle) * CInt(clockRadius * 0.625)) - 10)
            Dim numberY As Integer = CInt(centerY + CInt(Math.Sin(angle) * CInt(clockRadius * 0.625)) - 10)
            g.DrawString((i * 5).ToString(), New Font("Segoe UI", 9), Brushes.Black, numberX, numberY)
        Next

        ' Vergangene Zeit seit Programmstart
        elapsedTime = DateTime.Now - startTime
        Me.TopMost = True

        ' Minutenzeiger
        DrawHand(g, elapsedTime.TotalMinutes - 0.18, 4, clockRadius - 16, centerX, centerY, Color.Blue)

        ' Optional: Sekundenzeiger
        ' DrawHand(g, elapsedTime.TotalSeconds, 2, clockRadius - 10, centerX, centerY, Color.Red)

        ' Vergangene Zeit in Textform anzeigen
        'Dim timeText As String = String.Format("{0:D2}:{1:D2}:{2:D2}", elapsedTime.Hours, elapsedTime.Minutes, CInt(elapsedTime.Seconds)) ' 100,100
        sAPKtimeText = String.Format("{0:D2}:{1:D2}", elapsedTime.Hours, elapsedTime.Minutes) ' 120,100

        g.DrawString(sAPKtimeText, New Font("Segoe UI", 10), Brushes.Blue, New PointF(CInt(centerX * 0.78), CInt(centerY * 1.11)))
        g.DrawString(oGDT.Name + ", " + oGDT.Vorname, New Font("Segoe UI", 10), Brushes.Black, New PointF(10, CInt(Me.Height * 0.7)))
        Me.Text = "APK " + sAPKtimeText
    End Sub

    ' Zeiger bewegen
    Private Sub DrawHand(g As Graphics, value As Double, thickness As Integer, length As Integer, centerX As Integer, centerY As Integer, color As Color)
        Dim angle As Double = Math.PI / 30 * (value - 15) ' Berechnung des Winkels
        Dim handX As Integer = centerX + CInt(Math.Cos(angle) * length)
        Dim handY As Integer = centerY + CInt(Math.Sin(angle) * length)
        g.DrawLine(New Pen(color, thickness), centerX, centerY, handX, handY)
    End Sub

    ' Timer-Tick
    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.Invalidate() ' Erzwingt das Neuzeichnen des Formulars
    End Sub

    Private Function LoadGDT(ByVal sGDTFile As String, ByRef oGDT As GDTDataset) As Boolean
        Try
            ' GDT einlesen
            Dim rc As String = gdt_ParseFile(sGDTFile, oGDT)

            If String.IsNullOrEmpty(rc) Then
                ' Patientendaten anzeigen
                'lblPatient.Text = $"#{oGDT.PatID} {oGDT.Name}, {oGDT.Vorname}"
                Return True
            Else
                DebugPrint($"GDT-Lesefehler: {rc}")
                MsgBox($"Beim Lesen der GDT-Daten ist folgender Fehler aufgetreten: {rc}", MsgBoxStyle.Critical)
                Return False
            End If
        Catch ex As Exception
            DebugPrint($"Exception: {ex.Message}")
            MsgBox($"Beim Lesen der GDT-Daten ist folgender Fehler aufgetreten: {ex.Message}", MsgBoxStyle.Critical)
            Return False
        End Try
    End Function

    Private Function SaveGDT()
        If oGDT.PatID <> "" Then
            sAPKtimeText = String.Format("{0:D2}:{1:D2}", elapsedTime.Hours, elapsedTime.Minutes + 1)
            WriteGDTFile(sGDTFileOut, oGDT.PatID, $"{startTime.ToString("dd.MM.yyyy HH:mm")}-{Date.Now.ToString("HH:mm")} Dauer (hh:mm) mind. {sAPKtimeText}")
            DebugPrint($"APK für Pat {oGDT.PatID} in Datei geschrieben...")
        Else
            DebugPrint("APK konnte nicht in Datei geschrieben werden. PatID ist nicht gesetzt.")
        End If
    End Function
End Class

