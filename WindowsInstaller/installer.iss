#define MyAppName "Exif Date Setter"
#define MyAppVersion "1.0.0.0"
#define MyAppPublisher "MCY"
#define MyAppExeName "ExifDateSetterWindows.exe"

[Setup]
AppId={{18739f0e-7752-4e4c-9332-82e10b0548bd}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={commonpf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
LicenseFile=license.txt
OutputDir=Output
OutputBaseFilename=ExifDateSetter_Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Main Application
Source: "publish\*"; Excludes: "*.pdb"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Launch main application after update
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[Code]
function HasDotNet9: Boolean;
var
  Path: string;
  FindRec: TFindRec;
begin
  Result := False;
  Path := ExpandConstant('{commonpf}\dotnet\shared\Microsoft.WindowsDesktop.App');
  
  if DirExists(Path) then
  begin
    if FindFirst(Path + '\9.*', FindRec) then
    begin
      try
        Result := True;
      finally
        FindClose(FindRec);
      end;
    end;
  end;
end;

function InitializeSetup: Boolean;
begin
    Result := False;
    
    if not HasDotNet9 then
    begin
        MsgBox('.NET 9.0 Desktop Runtime is required to run this application.' + #13#10 +
               'Please install it from https://dotnet.microsoft.com/download/dotnet/9.0', 
               mbCriticalError, MB_OK);
        Exit;
    end;
    
    Result := True;
end;