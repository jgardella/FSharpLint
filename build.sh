#!/usr/bin/env bash

set -eu
set -o pipefail

dotent tool restore
dotent paket restore
dotnet restore build.proj --verbosity n

dotnet fake run build.fsx
