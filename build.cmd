@echo off

set sln=%~dp0

pushd %sln%
dotnet.exe build || exit /b
dotnet.exe test
popd