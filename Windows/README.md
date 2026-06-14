# LIAF Windows — WinUI 3 Android Device Toolkit

This folder contains the initial architecture and project structure for the Windows client. It is intentionally a skeleton only: feature folders, contracts, models, and service boundaries are present, but full application behavior is not implemented yet.

## Target stack

- .NET 8 with WinUI 3 / Windows App SDK
- MVVM with CommunityToolkit.Mvvm
- Feature-first organization for Android tooling workflows
- Infrastructure adapters for external `adb` and `fastboot` executables with shared command logging

## Architecture

```text
Windows/
├── Contracts/          # UI and service contracts shared by features
├── Core/               # Cross-feature domain models and service interfaces
├── Features/           # Feature-first modules and placeholder pages
│   ├── Devices/        # ADB/Fastboot device manager
│   ├── ApkInstaller/   # APK installation workflow
│   ├── Logcat/         # Logcat viewer and filtering
│   ├── FileManager/    # Device file browsing and transfer
│   ├── Shell/          # Interactive ADB shell surface
│   └── Settings/       # Toolkit settings and tool paths
├── Infrastructure/     # Process, ADB, Fastboot, and storage adapters
├── Services/           # App-level services such as navigation
├── Styles/             # WinUI theme resources
├── ViewModels/         # Shell-level view models and route constants
└── Views/              # Shell-level pages
```

## Planned feature boundaries

- **ADB**: process-backed command execution with async stdout/stderr capture, timeout/cancellation support, and device discovery parsing.
- **Fastboot**: process-backed command execution and `fastboot devices` parsing through shared process infrastructure.
- **Device manager**: unified ADB/Fastboot device list, polling-based monitoring, and connect/disconnect state updates.
- **APK install**: install requests, replace/grant flags, install result reporting.
- **Logcat**: streaming entries, priority/tag filters, export hooks.
- **File manager**: remote file entries, pull/push/delete/rename workflows.
- **Shell**: per-device shell sessions and terminal output pipeline.

## Implementation notes

- Keep UI pages thin and move behavior into view models and services.
- Keep Android command execution behind `IAdbService`, `IFastbootService`, and `IProcessRunner`; do not add UI logic to infrastructure services.
- Add real implementations incrementally with tests as each feature is built.
- Use `ILogService` for timestamped command, stdout, stderr, monitoring, and system logs backed by an in-memory buffer.
