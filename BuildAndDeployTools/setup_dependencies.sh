#!/bin/bash

echo "========================================"
echo "ServerNetworkAPI – Dependency Installer"
echo "========================================"

read -p "❓ Do you plan to build the project yourself? (y/n): " build_it

# Install common system tools (Linux-based)
echo "Installing required packages: arp-scan and nmap..."
sudo apt update
sudo apt install -y arp-scan nmap

if [[ "$build_it" =~ ^[Yy]$ ]]; then
    echo "Installing .NET SDK 9.0 and restoring NuGet packages..."

    # Install dotnet SDK if not present
    if ! dotnet --list-sdks | grep -q "9.0"; then
        echo "Downloading .NET SDK installer..."
        wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh -O dotnet-install.sh
        chmod +x dotnet-install.sh
        ./dotnet-install.sh --channel 9.0
        export DOTNET_ROOT=$HOME/.dotnet
        export PATH=$PATH:$HOME/.dotnet:$HOME/.dotnet/tools
    else
        echo ".NET SDK 9.0 already installed."
    fi

    # Restore NuGet packages
    echo "Restoring NuGet packages..."
    dotnet restore ServerNetworkAPI.csproj
    if [ $? -eq 0 ]; then
        echo "NuGet restore complete."
    else
        echo "Restore failed. Please check your .NET setup."
        exit 1
    fi
else
    echo "System dependencies installed. No build required."
fi

echo "Done."
