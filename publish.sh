#!/bin/bash

version=$(grep -oP -m 1 '\* \K[0-9]*\.[0-9]*\.[0-9]*' ReleaseNotes.md)
echo Version="$version"

dotnet publish  -c Release -f net8.0 -r linux-x64 --self-contained -p:AssemblyVersion="$version" -p:Version="$version" -p:VersionPrefix="$version" -p:PublishSingleFile=true -p:PublishTrimmed=true
dotnet publish  -c Release -f net8.0 -r win-x64   --self-contained -p:AssemblyVersion="$version" -p:Version="$version" -p:VersionPrefix="$version" -p:PublishSingleFile=true -p:PublishTrimmed=true

