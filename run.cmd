@echo off

set sln=%~dp0

pushd %sln%
dotnet build
popd

dotnet %sln%\gsi\bin\debug\netcoreapp3.1\gsi.dll