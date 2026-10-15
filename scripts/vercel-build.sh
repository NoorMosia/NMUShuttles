#!/usr/bin/env bash
# Vercel build script for AppUIWeb.
#
# Vercel's build image has no Flutter SDK, so we clone a pinned stable version
# into the build cache, then build the web app. The repo is checked out whole,
# so AppUIWeb's `../AppUI` path dependency resolves normally.
#
# Keep Vercel's project "Root Directory" at the repo root (not AppUIWeb), since
# this script and vercel.json live at the root and reference both folders.

set -euo pipefail

FLUTTER_VERSION="3.44.8"
FLUTTER_DIR="$HOME/flutter"

if [ ! -d "$FLUTTER_DIR" ]; then
  echo "Cloning Flutter $FLUTTER_VERSION..."
  git clone --depth 1 --branch "$FLUTTER_VERSION" https://github.com/flutter/flutter.git "$FLUTTER_DIR"
fi

export PATH="$FLUTTER_DIR/bin:$PATH"

flutter --version
flutter config --no-analytics

cd AppUIWeb
flutter pub get

# Vercel serves at the domain root, so base-href is "/".
# Path URLs work here because vercel.json rewrites everything to index.html.
flutter build web --release --base-href / --no-tree-shake-icons
