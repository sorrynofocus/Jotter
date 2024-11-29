@echo off
@REM https://github.com/dotnet/sdk/issues/14281
@REM https://github.com/dotnet/designs/blob/main/accepted/2020/single-file/design.md#user-experience
@REM https://github.com/dotnet/core/issues/5409

    @REM Cannot use --property:PublishTrimmed=true for WPF applications
    @REM https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained

@REM Set IS_DEBUG=true to only build DEBUG build. FALSE = release build.
set IS_DEBUG=false
@REM Levels for verbosity: 	Quiet Minimal Normal Detailed Diagnostic
set VERBOSE_LEVEL=minimal
set DOTNET_CLI_TELEMETRY_OPTOUT=1

set CURDIR=%CD%
set PRODUCTFILE=Jotter
set PRODUCTFILEWIN=%PRODUCTFILE%.exe
set SOLUTIONFILE=%CURDIR%\%PRODUCTFILE%.sln

set CONFIG_DEBUG=Debug
set CONFIG_RELEASE=Release
set FRAMEWORK=net8.0-windows
set PLATFORM=x64
@REM https://learn.microsoft.com/en-us/dotnet/core/rid-catalog
set RUNTIME_IDENTIFIER=win-x64
set PUBLISHDIR_DEBUG=%CURDIR%\bin\%PLATFORM%\%CONFIG_DEBUG%\%FRAMEWORK%\publishprod\
set PUBLISHDIR_RELEASE=%CURDIR%\bin\%PLATFORM%\%CONFIG_RELEASE%\%FRAMEWORK%\publishprod\
@REM alternately, you could relative path it: bin\x64\Debug\net8.0-windows\publishprod\


echo "Starting %SOLUTIONFILE% build..."

if "%IS_DEBUG%" == "true" (
    echo RUNNING DEBUG BUILD...
    dotnet clean "%SOLUTIONFILE%" --configuration %CONFIG_DEBUG% --property:Platform=%PLATFORM% --nologo --verbosity "%VERBOSE_LEVEL%"
    dotnet build "%SOLUTIONFILE%" --framework %FRAMEWORK% --configuration %CONFIG_DEBUG% --property:Platform=%PLATFORM% --nologo -nodeReuse:true --verbosity "%VERBOSE_LEVEL%"
    dotnet publish "%SOLUTIONFILE%" --framework %FRAMEWORK% -r "%RUNTIME_IDENTIFIER%" --configuration %CONFIG_DEBUG% --property:Platform=%PLATFORM% --self-contained true --property:PublishSingleFile=true  --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "%VERBOSE_LEVEL%" --property:PublishDir=%PUBLISHDIR_DEBUG%
    echo.
    echo Product location [DEBUG]  %RUNTIME_IDENTIFIER%-%FRAMEWORK%: %PUBLISHDIR_DEBUG%

) else (
    echo RUNNING RELEASE BUILD...
    dotnet clean "%SOLUTIONFILE%" --configuration %CONFIG_RELEASE% --property:Platform=%PLATFORM% --nologo --verbosity "%VERBOSE_LEVEL%"
    dotnet build "%SOLUTIONFILE%" --framework %FRAMEWORK% --configuration %CONFIG_RELEASE% --property:Platform=%PLATFORM% --nologo -nodeReuse:true --verbosity "%VERBOSE_LEVEL%"
    dotnet publish "%SOLUTIONFILE%" --framework %FRAMEWORK% -r "%RUNTIME_IDENTIFIER%" --configuration %CONFIG_RELEASE% --property:Platform=%PLATFORM% --self-contained true --property:PublishSingleFile=true  --property:IncludeNativeLibrariesForSelfExtract=true --verbosity "%VERBOSE_LEVEL%" --property:PublishDir=%PUBLISHDIR_RELEASE%
    echo.
    echo Product location [RELEASE]  %RUNTIME_IDENTIFIER%-%FRAMEWORK%: %PUBLISHDIR_RELEASE%    
)






