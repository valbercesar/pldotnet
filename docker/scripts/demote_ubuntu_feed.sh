#!/bin/bash

PREF_FILE="/etc/apt/preferences.d/99microsoft-dotnet.pref"

mkdir -p "$(dirname "$PREF_FILE")"

cat << EOF > "$PREF_FILE"
Package: dotnet* aspnet* netstandard*
Pin: origin "archive.ubuntu.com"
Pin-Priority: -10

Package: dotnet* aspnet* netstandard*
Pin: origin "security.ubuntu.com"
Pin-Priority: -10
EOF
