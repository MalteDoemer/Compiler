@echo off
set sln=%~dp0
set proj=%sln%\samples\HelloWorld
pushd %proj%
dotnet run 
popd
exit /b