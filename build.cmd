@echo off
cls

dotent tool restore
dotent paket restore
dotnet restore build.proj --verbosity n

dotnet fake run build.fsx
