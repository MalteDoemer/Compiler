@echo off

set sln=%~dp0

pushd %sln%
dotnet build || exit /b
dotnet test
popd