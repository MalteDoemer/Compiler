@echo off
set sln=%~dp0
dotnet run --project %sln%\samples\HelloWorld\Hello.gsproj 
exit /b