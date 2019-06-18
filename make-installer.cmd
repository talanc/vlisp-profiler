dotnet publish src\VLispProfiler.Cmdline --configuration Release --output .\installer\publish
cd installer
NSIS\makensis installer.nsi
cd ..