; Installer name
Name "VLispProfiler"

; Installer exe
OutFile "vlisp-profiler-installer.exe"

; Installer default directory
InstallDir $LOCALAPPDATA\VLispProfiler

; Installer registry location
InstallDirRegKey HKCU "Software\VLispProfiler" "InstallLocation"

; Installer requires user only (per user install anyway)
RequestExecutionLevel user

;--------------------------------
; Pages

Page components
Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

;--------------------------------
; Installer

Section "VLispProfiler (required)"

  SectionIn RO

  ; Uninstall old version
  ExecWait '"$INSTDIR\uninstall.exe" /S _?=$INSTDIR'
  
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Files
  File "publish\CommandLine.dll"
  File "publish\prof.lsp"
  File "publish\VLispProfiler.dll"
  File "publish\vlisp-profiler.exe"
  File "publish\vlisp-profiler.exe.config"
  File "publish\VLispProfiler.pdb"
  File "publish\vlisp-profiler.pdb"
  
  ; Create VLispProfiler Setup shortcut
  CreateShortcut "$INSTDIR\Setup.lnk" "$INSTDIR\vlisp-profiler.exe" "setup --interactive" "$INSTDIR\vlisp-profiler.exe" 0

  ; Registry install location
  WriteRegStr HKCU "Software\VLispProfiler" "InstallLocation" $INSTDIR

  ; Uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; Install VLispProfiler setups
  ExecWait '"$INSTDIR\vlisp-profiler.exe" setup --install all'
  
SectionEnd

Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\VLispProfiler"
  CreateShortcut "$SMPROGRAMS\VLispProfiler\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortcut "$SMPROGRAMS\VLispProfiler\Setup.lnk" "$INSTDIR\vlisp-profiler.exe" "setup --interactive" "$INSTDIR\vlisp-profiler.exe" 0
  
SectionEnd

Section "Samples (copied to Documents)"

  SetOutPath "$DOCUMENTS\VLispProfiler Samples"

  File "samples\sample-file.LSP"
  File "samples\sample-func.LSP"

SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

  ; Uninstall VLispProfiler setups
  ExecWait '"$INSTDIR\vlisp-profiler.exe" setup --uninstall all'

  ; Remove files
  Delete "$INSTDIR\CommandLine.dll"
  Delete "$INSTDIR\prof.lsp"
  Delete "$INSTDIR\VLispProfiler.dll"
  Delete "$INSTDIR\vlisp-profiler.exe"
  Delete "$INSTDIR\vlisp-profiler.exe.config"
  Delete "$INSTDIR\VLispProfiler.pdb"
  Delete "$INSTDIR\vlisp-profiler.pdb"

  ; Remove shortcut
  Delete "$INSTDIR\Setup.lnk"

  ; Remove uninstaller
  Delete "$INSTDIR\uninstall.exe"

  ; Remove shortcuts (if any)
  Delete "$SMPROGRAMS\VLispProfiler\*.*"
  RMDir "$SMPROGRAMS\VLispProfiler"

  ; Remove samples (if any)
  Delete "$DOCUMENTS\VLispProfiler Samples\sample-file.LSP"
  Delete "$DOCUMENTS\VLispProfiler Samples\sample-func.LSP"
  RMDir "$DOCUMENTS\VLispProfiler Samples"

  ; Remove install dir
  RMDir "$INSTDIR"

  ; Remove registry entries
  DeleteRegValue HKCU "Software\VLispProfiler" "InstallLocation"
  DeleteRegKey /ifempty HKCU "Software\VLispProfiler"

SectionEnd
