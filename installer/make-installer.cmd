@ECHO OFF
REM make-installer.cmd [installer-exe-output] [publish-configuration]
REM     installer-exe-output: path to installer exe output. Default is vlisp-profiler-installer.exe to the current directory.
REM     publish-configuration: configuration for "dotnet publish". Values can be Debug or Release. Default is Release.

SET out="%~f1"
SET cfg="%2"

IF %out%=="" (
    SET out="%CD%\vlisp-profiler-installer.exe"
)

IF %cfg%=="" (
    SET cfg="Release"
)

@ECHO ON

dotnet publish "%~dp0\..\src\VLispProfiler.Cmdline" --configuration %cfg% --output "%~dp0\publish"
"%~dp0\NSIS\makensis" "/DOutFile=%out%" "%~dp0\installer.nsi"
