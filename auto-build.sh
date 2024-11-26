#!/bin/bash
# This script replicates the functionality of auto-build.cmd for a Linux environment.
# It builds the project in Debug or Release configuration.
# If you want to run a Debug build, set the IS_DEBUG variable to true:
# IS_DEBUG=true ./auto-build.sh

# Variables
IS_DEBUG=false
VERBOSE_LEVEL=normal

CURDIR=$(pwd)
PRODUCTFILE=Jotter
SOLUTIONFILE="$CURDIR/$PRODUCTFILE.sln"

CONFIG_DEBUG=Debug
CONFIG_RELEASE=Release
FRAMEWORK=net8.0-windows
PLATFORM=x64
RUNTIME_IDENTIFIER=win-x64
PUBLISHDIR_DEBUG="$CURDIR/bin/$PLATFORM/$CONFIG_DEBUG/$FRAMEWORK/publishprod/"
PUBLISHDIR_RELEASE="$CURDIR/bin/$PLATFORM/$CONFIG_RELEASE/$FRAMEWORK/publishprod/"

# Output the start message
echo "Starting build for solution: $SOLUTIONFILE"

if [ "$IS_DEBUG" = "true" ]; then
    echo "RUNNING DEBUG BUILD..."
    dotnet clean "$SOLUTIONFILE" --configuration "$CONFIG_DEBUG" --property:Platform="$PLATFORM" --nologo --verbosity "$VERBOSE_LEVEL"
    dotnet build "$SOLUTIONFILE" --framework "$FRAMEWORK" --configuration "$CONFIG_DEBUG" --property:Platform="$PLATFORM" --nologo -nodeReuse:true --verbosity "$VERBOSE_LEVEL"
    dotnet publish "$SOLUTIONFILE" --framework "$FRAMEWORK" -r "$RUNTIME_IDENTIFIER" --configuration "$CONFIG_DEBUG" \
        --property:Platform="$PLATFORM" --self-contained true --property:PublishSingleFile=true \
        --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "$VERBOSE_LEVEL" --property:PublishDir="$PUBLISHDIR_DEBUG"
    echo "Product location [DEBUG]: $RUNTIME_IDENTIFIER-$FRAMEWORK: $PUBLISHDIR_DEBUG"
else
    echo "RUNNING RELEASE BUILD..."
    dotnet clean "$SOLUTIONFILE" --configuration "$CONFIG_RELEASE" --property:Platform="$PLATFORM" --nologo --verbosity "$VERBOSE_LEVEL"
    dotnet build "$SOLUTIONFILE" --framework "$FRAMEWORK" --configuration "$CONFIG_RELEASE" --property:Platform="$PLATFORM" --nologo -nodeReuse:true --verbosity "$VERBOSE_LEVEL"
    dotnet publish "$SOLUTIONFILE" --framework "$FRAMEWORK" -r "$RUNTIME_IDENTIFIER" --configuration "$CONFIG_RELEASE" \
        --property:Platform="$PLATFORM" --self-contained true --property:PublishSingleFile=true \
        --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "$VERBOSE_LEVEL" --property:PublishDir="$PUBLISHDIR_RELEASE"
    echo "Product location [RELEASE]: $RUNTIME_IDENTIFIER-$FRAMEWORK: $PUBLISHDIR_RELEASE"
fi
