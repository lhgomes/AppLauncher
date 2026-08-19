# AppLauncher

Automatic application update and launcher for shared Windows/RDP environments.

## Runtime

- Windows Server 2019 supported
- .NET Framework 4.8
- AnyCPU, with Prefer 32-bit disabled
- Application releases run from each user's `%LOCALAPPDATA%` profile

Windows Server 2019 includes .NET Framework 4.7.2 by default, so install/enable .NET Framework 4.8 on servers where it is not already present.

## Configuration

Configuration is stored in `AppLauncher.exe.config`.

- `AppArguments`: arguments passed to the application.
- `VersionFolder`: release folder to launch. If blank, AppLauncher automatically selects the release directory with the highest name (ordinal, descending). Version folders should therefore use sortable names such as `Release_20260818_01` or `1.005.000`.
- `KeepLastVersions`: number of locally cached versions to retain, including the current version.
- `AppName`: application name and local profile directory name.
- `ExecFile`: executable started after the update.
- `LogDebug`: writes diagnostic logging to `%TEMP%`.
- `IgnoreExtensions`: retained for compatibility with existing configurations.
- `MultiInstance`: whether more than one application instance is allowed in the same RDP session.
- `MultiInstanceMessage`: message displayed when another instance exists in the current session.

## Recommended RDP deployment

Place the launcher and release directories in a common read-only directory accessible to all RDP users:

```
C:\Applications\MyApplication\
    AppLauncher.exe
    AppLauncher.exe.config
    Release_20260818_01\
        MyApplication.exe
        ...
    Release_20260825_01\
        MyApplication.exe
        ...
```

Create each user's shortcut to `AppLauncher.exe`. Users only require read access to the common deployment directory.

On launch, the selected release is copied to:

```
%LOCALAPPDATA%\<AppName>\<VersionFolder>\
```

The application executes from that user-local directory. This means a new release can be deployed while other RDP sessions continue running an older release without locking the deployment files.

## Updating an application

1. Build/publish the application from Visual Studio into a new release directory.
2. Copy the complete release directory beside `AppLauncher.exe`.
3. Either update `VersionFolder` in `AppLauncher.exe.config`, or leave `VersionFolder` blank and use sortable release folder names.
4. The next launch in each RDP session installs the new version into that user's local profile and starts it.

Updates are copied first to a `.tmp` directory and moved into place only after the application executable is verified. An interrupted or failed copy therefore does not leave a partially installed release marked as current.

## RDP behavior

When `MultiInstance` is false, AppLauncher checks only processes in the current Windows session. An application running under another RDP user's session does not prevent the current user from starting it.

Old-version cleanup is best-effort. A failure to delete an old release does not prevent the current release from starting.
