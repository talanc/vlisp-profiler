dotnet publish ..\src\VLispProfiler.Cmdline --configuration Release --output "%cd%\publish"
NSIS\makensis installer.nsi
