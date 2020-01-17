#!/usr/bin/env bash

pushd Sources

dotnet restore && dotnet build
dotnet test Equ.Test/Equ.Test.csproj

popd
