@echo off

set sln=%~dp0

pushd %sln%
dotnet build
dotnet test
popd