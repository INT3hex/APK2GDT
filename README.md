# APK2GDT
ArztPatKontakt Stoppuhr für T2med

![](https://github.com/INT3hex/APK2GDT/blob/master/doc/APK2GDT.png)

## **Initiale Version v1**
[APK2GDT.zip](https://github.com/INT3hex/APK2GDT/releases/download/APK2GDTv1/APK2GDT.zip) (Virustotalhash: 8995d07470a27084515f83b36642f0434210a9ad02ac32ef5de0ba379d31992c)
* Installation durch Entpacken in ein beliebiges Verzeichnis (GDT-Konfiguration muss entsprechend angepasst sein)
* Das Programm erzeugt (wenn nicht vorhanden) ein Unterverzeichnis **Config** mit einer App.config. Darin können anwendungsspezifische Konfigurationen (z.B. GDT-Pfade, Debuglog, etc.) gesetzt werden.
* Daher muss das Config-Verzeichnis (und Dateien darunter) für das Benutzerkonto schreibbar sein. (Bitte beachtem, falls z.B. unter C:\Program Files\... entpackt wird. Hier haben Standardbenutzer idR keine Schreib-/Änderungsrechte)

## Nutzung
* Aufruf (wie andere GDT-angebundene Programme) bei Sprechstundenbeginn aus der Patientenakte
* Doppelklick auf das Fenster/Stoppuhr beendet die Stoppuhr und generiert einen Karteieintrag beim Pat
* Schließen mit X lässt auf Rückfrage Beenden ohne Karteieintrag zu
* (GDT-Dateien werden von APK2GDT nicht gelöscht)

## **GDT-Konfiguration**
* Einrichtung: Aus T2med exportierte GDT-Konfiguration für APK2GDT
[GDTGeraet_APK_Konfiguration.json](https://github.com/INT3hex/APK2GDT/blob/master/doc/GDTGeraet_APK_Konfiguration.json)
* bzw. als Screenshot
![](https://github.com/INT3hex/APK2GDT/blob/master/doc/GDT-Konfiguration.png)
