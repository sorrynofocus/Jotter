#!/bin/bash
# This script replicates the functionality of auto-build.cmd for a Linux environment.
# It builds the project in Debug or Release configuration.
# If you want to run a Debug build, set the IS_DEBUG variable to true:
# IS_DEBUG=true ./auto-build.sh

set -euo pipefail

# Variables
IS_DEBUG="${IS_DEBUG:-false}"
VERBOSE_LEVEL="${VERBOSE_LEVEL:-minimal}"
export DOTNET_CLI_TELEMETRY_OPTOUT=1

CURDIR="$(pwd)"
PRODUCTFILE=Jotter
PRODUCTFILEWIN="$PRODUCTFILE.exe"
SOLUTIONFILE="$CURDIR/$PRODUCTFILE.sln"

CONFIG_DEBUG=Debug
CONFIG_RELEASE=Release
FRAMEWORK=net8.0-windows
PLATFORM=x64
RUNTIME_IDENTIFIER=win-x64
PUBLISHDIR_DEBUG="$CURDIR/bin/$PLATFORM/$CONFIG_DEBUG/$FRAMEWORK/publishprod/"
PUBLISHDIR_RELEASE="$CURDIR/bin/$PLATFORM/$CONFIG_RELEASE/$FRAMEWORK/publishprod/"

calculate_sha256() {
    local file="$1"
    local output

    if command -v sha256sum >/dev/null 2>&1; then
        output=$(sha256sum "$file")
    elif command -v shasum >/dev/null 2>&1; then
        output=$(shasum -a 256 "$file")
    else
        echo "Unable to calculate SHA-256: neither sha256sum nor shasum is installed." >&2
        return 1
    fi

    printf '%s\n' "${output%% *}"
}

# Output the start message
echo "Starting build for solution: $SOLUTIONFILE"

if [ "$IS_DEBUG" = "true" ]; then
    echo "RUNNING DEBUG BUILD..."
    # dotnet clean "$SOLUTIONFILE" --configuration "$CONFIG_DEBUG" --property:Platform="$PLATFORM" --nologo --verbosity "$VERBOSE_LEVEL"
    dotnet build "$SOLUTIONFILE" --property:dotNetBuildCmd="build" --property:EnableWindowsTargeting=true \
        --framework "$FRAMEWORK" --configuration "$CONFIG_DEBUG" --property:Platform="$PLATFORM" \
        --nologo -nodeReuse:true --verbosity "$VERBOSE_LEVEL"
    dotnet publish "$SOLUTIONFILE" --property:dotNetBuildCmd="publish" --property:EnableWindowsTargeting=true \
        --framework "$FRAMEWORK" -r "$RUNTIME_IDENTIFIER" --configuration "$CONFIG_DEBUG" \
        --property:Platform="$PLATFORM" --self-contained true --property:PublishSingleFile=true \
        --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "$VERBOSE_LEVEL" --property:PublishDir="$PUBLISHDIR_DEBUG"

    ls -la "$PUBLISHDIR_DEBUG"
    HASH=$(calculate_sha256 "$PUBLISHDIR_DEBUG$PRODUCTFILEWIN")
    echo "The hash value of $PRODUCTFILE: $HASH"
    echo "Product location [DEBUG]: $RUNTIME_IDENTIFIER-$FRAMEWORK: $PUBLISHDIR_DEBUG"
else
    echo "RUNNING RELEASE BUILD..."
    # dotnet clean "$SOLUTIONFILE" --configuration "$CONFIG_RELEASE" --property:Platform="$PLATFORM" --nologo --verbosity "$VERBOSE_LEVEL"
    dotnet build "$SOLUTIONFILE" --property:dotNetBuildCmd="build" --property:EnableWindowsTargeting=true \
        --framework "$FRAMEWORK" --configuration "$CONFIG_RELEASE" --property:Platform="$PLATFORM" \
        --nologo -nodeReuse:true --verbosity "$VERBOSE_LEVEL"
    dotnet publish "$SOLUTIONFILE" --property:dotNetBuildCmd="publish" --property:EnableWindowsTargeting=true \
        --framework "$FRAMEWORK" -r "$RUNTIME_IDENTIFIER" --configuration "$CONFIG_RELEASE" \
        --property:Platform="$PLATFORM" --self-contained true --property:PublishSingleFile=true \
        --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "$VERBOSE_LEVEL" --property:PublishDir="$PUBLISHDIR_RELEASE"

    ls -la "$PUBLISHDIR_RELEASE"
    HASH=$(calculate_sha256 "$PUBLISHDIR_RELEASE$PRODUCTFILEWIN")
    echo "The hash value of $PRODUCTFILE: $HASH"
    echo "Product location [RELEASE]: $RUNTIME_IDENTIFIER-$FRAMEWORK: $PUBLISHDIR_RELEASE"
fi
