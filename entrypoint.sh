#!/bin/sh
set -e

# Render sets PORT (default 10000). Bind Kestrel to that port.
export ASPNETCORE_URLS="http://+:${PORT:-8080}"

exec dotnet SignupApp.dll
