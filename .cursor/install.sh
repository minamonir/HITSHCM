#!/usr/bin/env bash
# Idempotent Cloud Agent bootstrap for the HITSHCM .NET 10 Razor host.
# Installs the pinned .NET SDK (from global.json), exposes it on PATH, then
# restores and builds the solution so agents start from a warm, verified state.
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

DOTNET_VERSION="$(grep -oP '"version"\s*:\s*"\K[^"]+' global.json | head -1)"
DOTNET_VERSION="${DOTNET_VERSION:-10.0.401}"
DOTNET_INSTALL_DIR="/usr/local/dotnet"

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

if ! "$DOTNET_INSTALL_DIR/dotnet" --list-sdks 2>/dev/null | grep -q "^${DOTNET_VERSION} "; then
  echo "Installing .NET SDK ${DOTNET_VERSION} to ${DOTNET_INSTALL_DIR} ..."
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  sudo mkdir -p "$DOTNET_INSTALL_DIR"
  sudo /tmp/dotnet-install.sh --version "$DOTNET_VERSION" --install-dir "$DOTNET_INSTALL_DIR"
else
  echo ".NET SDK ${DOTNET_VERSION} already present."
fi

# Expose `dotnet` on the default PATH for every shell (interactive or not).
sudo ln -sf "$DOTNET_INSTALL_DIR/dotnet" /usr/local/bin/dotnet

# Persist env for interactive/login shells.
sudo tee /etc/profile.d/dotnet.sh >/dev/null <<EOF
export DOTNET_ROOT="${DOTNET_INSTALL_DIR}"
export PATH="\$PATH:${DOTNET_INSTALL_DIR}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1
EOF

export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
export PATH="$PATH:$DOTNET_INSTALL_DIR"

echo "Using: $(dotnet --version)"

dotnet restore Hitshcm.sln
dotnet build Hitshcm.sln --no-restore -c Debug

echo "HITSHCM bootstrap complete."
