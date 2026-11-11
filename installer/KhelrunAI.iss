; KHELRUN AI — simple double-click Windows installer (Inno Setup 7 / 6)
; Output: dist\KhelrunAI-Setup.exe
; Per-machine install with standard UAC elevation. Appears in Settings → Apps and uninstalls cleanly.

#define AppName "KHELRUN AI"
#define AppVersion "1.0.0"
#define AppPublisher "JTS (James Technology Solutions)"
#define AppExe "KhelrunAI.exe"

[Setup]
AppId={{B7C1E5A2-3F4D-4E1B-9A7C-5D2E8F6A1C31}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppVerName={#AppName} {#AppVersion}
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=KhelrunAI-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName={#AppName}
UninstallDisplayIcon={app}\{#AppExe}
LicenseFile=..\legal\EULA.txt
VersionInfoVersion={#AppVersion}
VersionInfoCompany={#AppPublisher}
VersionInfoDescription=KHELRUN AI — Private local AI by JTS
VersionInfoCopyright=Copyright (c) 2026 JTS
VersionInfoProductName={#AppName}
VersionInfoProductVersion={#AppVersion}
SetupIconFile=images\KhelrunAI.ico
WizardImageFile=images\WizardImage.bmp,images\WizardImage_200.bmp
WizardSmallImageFile=images\WizardSmallImage.bmp,images\WizardSmallImage_200.bmp,images\WizardSmallImage_100.bmp
WizardImageStretch=yes
AllowNoIcons=yes
#ifdef SignIt
SignTool=signtool
SignedUninstaller=yes
#endif

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a desktop shortcut"; GroupDescription: "Additional icons:"; Flags: checkedonce

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
Source: "..\legal\EULA.txt"; DestDir: "{app}\legal"; Flags: ignoreversion
Source: "..\legal\PrivacyPolicy.txt"; DestDir: "{app}\legal"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Launch {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Remove install folder leftovers if any empty files remain
Type: filesandordirs; Name: "{app}"
