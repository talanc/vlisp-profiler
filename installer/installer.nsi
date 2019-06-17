; Installer name
Name "VLisp Profiler"

; Installer exe
OutFile "vlisp-profiler-installer.exe"

; Installer default directory
InstallDir $LOCALAPPDATA\VLispProfiler

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

Section "VLisp Profiler (required)"

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

  ; Uninstaller
  WriteUninstaller "$INSTDIR\uninstall.exe"

  ; Install VLispProfiler setups
  ExecWait '"$INSTDIR\vlisp-profiler.exe" setup --install all'
  
SectionEnd

Section "Start Menu Shortcuts"

  CreateDirectory "$SMPROGRAMS\VLisp Profiler"
  CreateShortcut "$SMPROGRAMS\VLisp Profiler\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  CreateShortcut "$SMPROGRAMS\VLisp Profiler\Setup.lnk" "$INSTDIR\vlisp-profiler.exe" "setup --interactive" "$INSTDIR\vlisp-profiler.exe" 0
  
SectionEnd

;--------------------------------
; Uninstaller

Section "Uninstall"

  ; Uninstall VLispProfiler setups
  ExecWait '"$INSTDIR\vlisp-profiler.exe" setup --install all'

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

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\VLisp Profiler\*.*"

  ; Remove directories used
  RMDir "$SMPROGRAMS\VLisp Profiler"
  RMDir "$INSTDIR"

SectionEnd
