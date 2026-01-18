Imports System
Imports System.Configuration  ' Add also Reference - Assemblys - System.Configuration
Imports System.Security.Cryptography 'Add also Reference!!
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.Net
Public Class AppConfigClass

    ' Private Instanzvariablen
    Private fileMap As New ExeConfigurationFileMap()
    Private _config As Configuration
    Private _settings As AppSettingsSection

    Public Sub New()
        fileMap.ExeConfigFilename = Application.StartupPath & "\Config\App.config"
        ' Lade Konfiguration
        _config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None)
        ' _config = ConfigurationManager.OpenExeConfiguration(Assembly.GetEntryAssembly().Location)
        _settings = _config.AppSettings
    End Sub

    Public Function GetProperty(propertyName As String, propertyDefault As String) As String

        Dim propertyValue As String = If(_settings.Settings.Item(propertyName) IsNot Nothing, _settings.Settings.Item(propertyName).Value, If(propertyDefault IsNot Nothing, propertyDefault, InputBox(propertyName, "Konfiguration")))
        If (_settings.Settings.Item(propertyName) IsNot Nothing) Then
            If Left(propertyName, 4) = "pwd_" Then propertyValue = _decrypt(propertyValue)
        End If
        SetProperty(propertyName, propertyValue)
        Return propertyValue

    End Function

    Public Sub SetProperty(propertyName As String, propertyValue As String)
        If Left(propertyName, 4) = "pwd_" Then propertyValue = _encrypt(propertyValue)
        If _settings.Settings.Item(propertyName) IsNot Nothing Then
            _settings.Settings.Item(propertyName).Value = propertyValue
        Else
            _settings.Settings.Add(propertyName, propertyValue)
        End If
        _config.Save(ConfigurationSaveMode.Modified)
    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
        _config.Save(ConfigurationSaveMode.Full)
    End Sub

    Private Function _encrypt(s As String) As String
        s = ProtectStringBase64(s)
        'DebugPrint("tostring: " & s)
        Return s
    End Function

    Private Function _decrypt(s As String) As String
        s = UnprotectStringBase64(s)
        'DebugPrint("fromstring:" & s)
        Return s
    End Function

    Function ProtectStringBase64(ByVal str As String) As String
        ' protect string with DPAPI and return it as Base64 (machinecontext)
        Dim strBytes As Byte() = System.Text.Encoding.UTF8.GetBytes(str)
        Dim encryptedBytes As Byte() = ProtectedData.Protect(strBytes, Nothing, DataProtectionScope.LocalMachine)
        Return Convert.ToBase64String(encryptedBytes)
    End Function

    Function UnprotectStringBase64(ByVal base64EncryptedString As String) As String
        ' Use Base64 string and decode it with DPAPI (machinecontext)
        ' Decode the Base64 encoded string
        Dim encryptedBytes As Byte() = Convert.FromBase64String(base64EncryptedString)
        ' Decrypt the encrypted bytes
        Dim decryptedBytes As Byte() = ProtectedData.Unprotect(encryptedBytes, Nothing, DataProtectionScope.LocalMachine)
        ' Convert the decrypted bytes to a string
        Dim decryptedString As String = System.Text.Encoding.UTF8.GetString(decryptedBytes)
        Return decryptedString
    End Function

End Class

