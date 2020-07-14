@echo off

set "SLNDIR=%~dp0"
dotnet build "%SLNDIR%\gsc" > nul
dotnet run -p "%SLNDIR%\gsc" --no-build -- %*