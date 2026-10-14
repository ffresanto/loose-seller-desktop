#define MyAppName "Loose Seller"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Franccesco Felipe"
#define MyAppExeName "LooseSeller.Desktop.exe"

[Setup]
AppId={{B6F4A8E0-2B5A-4E1B-9E52-4D4F4F53454C}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Loose Seller
DefaultGroupName={#MyAppName}
OutputDir=..\installer-output
OutputBaseFilename=LooseSellerSetup-{#MyAppVersion}
SetupIconFile=..\docs\icons\icon_installer.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Files]
Source: "..\publish\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "..\docs\icons\icon_installer.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\docs\icons\icon_main.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\icon_main.ico"
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; IconFilename: "{app}\icon_main.ico"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir {#MyAppName}"; Flags: nowait postinstall skipifsilent
