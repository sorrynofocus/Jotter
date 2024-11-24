# Build version info

* **tldr;**

    `./build/version.props` contains conditional-logic to automatically determine the current version from `./build/versioninfo.txt`. This file contains a semantec version (`1.0.0.0`) that gets automatically incremented. If `Major` or `minor` has been incremented, then the logic will determine the versioning and update the version file. The version file can be consumed by other third-party automations to determine the currnet build of the application.

<BR>

The `ManageVersion` target (`in version.props`) manages versioning by reading, updating, and storing version information in a version file `VersionInfo.txt`. It uses MSBuild tasks to perform version consuming and manipulation.


The version consists of *four* components described by semantic versioning:

* **Major Version:** Set manually to reflect significant updates.

* **Minor Version:** Set manually for minor feature additions.

* **Revision/Tiny Number:** Set manually for bug fixes or revisions.

* **Build Number:** Auto-incremented for every build to track unique builds.

The `VersionInfo.txt` file resides in the `build` sub-folder of the project directory, ensuring the version information is easy to find and 
consistent across builds. See tag `<VersionFile>` in `version.props` 

#### Benefits

* Automated Build Numbering:
    * Tracks unique builds without manual intervention.

* Version Control:
    * Centralized storage in `VersionInfo.txt` ensures consistency across environments.

* Debuggable:
    * Logs provide clear insights into how the version is being computed.

* Easily Extendable:
    * The system can be adapted to include additional versioning logic if needed.

---

### File and Folder Setup

The version file path is defined as:

    <VersionFile>$(MSBuildProjectDirectory)\build\VersionInfo.txt</VersionFile>

 This places `VersionInfo.txt` in the `build` folder, relative to the **_project_** directory.

`$(MSBuildProjectDirectory)` is a built-in MSBuild property that resolves to the folder where the project file (`.csproj`) resides.


The `build` sub-folder in the **_project_** directory is created automatically if it doesn't exist:

    <Exec Command="if not exist $(MSBuildProjectDirectory)\build mkdir $(MSBuildProjectDirectory)\build" />

* Default Version:

    * If the file doesn't exist, it is created with a default version of `1.0.0.0`:


    <Exec Command="if not exist $(VersionFile) echo $(DefaultVersion) > $(VersionFile)" />

_Note:_ The version file should be checked into source control and maintained for proper versioning. 

---

#### Consuming Current Version during build

The `ReadLinesFromFile` task reads the `VersionInfo.txt` content into the `CurrentVersion` property:

    <ReadLinesFromFile File="$(VersionFile)">
    <Output TaskParameter="Lines" PropertyName="CurrentVersion" />
    </ReadLinesFromFile>

* Example:

    * If the file == `1.0.0.5`, `CurrentVersion` will be set to this string value.

The `CurrentVersion` string value is split into its components using `.Split()` and assigned to individual properties (`FileMajorVersion, FileMinorVersion, FileRevision, FileBuildNumber`):

    <PropertyGroup>
    <FileMajorVersion>$([MSBuild]::ValueOrDefault($([System.String]::Copy($(CurrentVersion)).Split('.')[0]), '0'))</FileMajorVersion>
    <FileMinorVersion>$([MSBuild]::ValueOrDefault($([System.String]::Copy($(CurrentVersion)).Split('.')[1]), '0'))</FileMinorVersion>
    <FileRevision>$([MSBuild]::ValueOrDefault($([System.String]::Copy($(CurrentVersion)).Split('.')[2]), '0'))</FileRevision>
    <FileBuildNumber>$([MSBuild]::ValueOrDefault($([System.String]::Copy($(CurrentVersion)).Split('.')[3]), '0'))</FileBuildNumber>
    </PropertyGroup>

* Explanation:

    * `.Split()` splits the version string into an array of strings (example `["1", "0", "0", "5"]`).

    * Each component is accessed using an index (`[0], [1], etc.`).

    * `ValueOrDefault()` ensures a fallback value (`'0'`) if parsing fails.

---

#### Incrementing the Build Number

**_Note:_** Remember: no need to change the build number. Only need to change the `MAjor` or `Minor` after a large update. The `version.props` MSBuild tasks handles the automation.

The final version (`Major.Minor.Revision.Build`) is stored in the `FinalVersion` property:

    <FinalVersion>$(MajorVersion).$(MinorVersion).$(RevisionNumber).$(BuildNumber)</FinalVersion>


**VersionSuffix:**

    <VersionSuffix>$(FinalVersion)</VersionSuffix>

**AssemblyVersion:**

    <AssemblyVersion Condition=" '$(VersionSuffix)' != '' ">$(MajorVersion).$(MinorVersion).$(RevisionNumber).0</AssemblyVersion>

**FileVersion:**

    <FileVersion Condition=" '$(VersionSuffix)' != '' ">$(VersionSuffix)</FileVersion>

**InformationalVersion:**

    <InformationalVersion Condition=" '$(VersionSuffix)' != '' ">$(VersionSuffix)</InformationalVersion>

Condition-Based Logic is automatically done from the `version.prop` :

* The build number is reset to `0` if the major version changes:

    `<BuildNumber Condition="'$(FileMajorVersion)' != '$(MajorVersion)'">0</BuildNumber>`

* Otherwise, it is incremented using MSBuild/dotnet `Add()`:

    `<BuildNumber Condition="'$(FileMajorVersion)' == '$(MajorVersion)'">$([MSBuild]::Add($(FileBuildNumber), 1))</BuildNumber>`

* Example:

    * If `MajorVersion` is `1` and `FileMajorVersion` is also `1`, the `BuildNumber` will increment.

    #### Example Workflow

    **First Build:**

    * VersionInfo.txt: 1.0.0.0
    * Incremented: 1.0.0.1

    **Second Build:**

    * VersionInfo.txt: 1.0.0.1
    * Incremented: 1.0.0.2

    **Major Update:**

    * Update MajorVersion to 2.
    * VersionInfo.txt: 1.0.0.3
    * Set to: 2.0.0.0


#### Stamping the updated version in the build info version file

After incrementing the build number, the new version is written back to `VersionInfo.txt`:

    <WriteLinesToFile
        File="$(VersionFile)"
        Lines="$(FinalVersion)"
        Overwrite="true" />

---

#### Logging

Throughout the target, `Message` tasks log intermediate and final values for debugging:


    <Message Text="Parsed Major Version: $(FileMajorVersion)" Importance="High" />
    <Message Text="Updated Build Number: $(BuildNumber)" Importance="High" />
    <Message Text="Final Version: $(FinalVersion)" Importance="High" />

**Importance Levels:**

* **High:** Ensures the messages appear prominently in the build output.

_Note:_ Verbose output may hide - so you will have to peruse the logs. ;)

---
