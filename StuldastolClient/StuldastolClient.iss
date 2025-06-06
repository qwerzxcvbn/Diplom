; --- StuldastolProduction.iss ---
#define MyAppName "StuldastolProduction"
#define MyAppVersion "1.0"
#define MyAppPublisher "MukashovaNemchinov Team"
#define MyAppExeName "StuldastolProduction.exe"

[Setup]
AppId={{B2C3D4E5-6F7G-8901-2345-678901BCDEFG}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=D:\VSProject\StuldastolSolution\Output
OutputBaseFilename=Setup_{#MyAppName}
Compression=lzma
SolidCompression=yes
SetupIconFile=D:\VSProject\StuldastolSolution\StuldastolProduction\Images\icon.ico

[Languages]
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\VSProject\StuldastolSolution\StuldastolProduction\bin\Debug\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\VSProject\StuldastolSolution\StuldastolProduction\bin\Debug\*.dll"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "D:\VSProject\StuldastolSolution\StuldastolProduction\bin\Debug\*.config"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs
Source: "D:\VSProject\StuldastolSolution\StuldastolProduction\Images\*"; DestDir: "{app}\Images"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "D:\VSProject\StuldastolSolution\StuldastolProduction\Views\*"; DestDir: "{app}\Views"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent