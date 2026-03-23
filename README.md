# LIAF - Android Flash Tool

Advanced GUI utility for managing Android devices via ADB and Fastboot.

## Platforms
* Linux: C# + GTK4/Adwaita (Active)
* Windows: C# + WinUI3 (Planned)
* Android: Kotlin + Miuix (Planned)

## Features
* Device Connection: ADB, Fastboot, Wireless ADB, ADB Pair.
* Flashing: Flash, boot, erase partitions, flash with disable-verity/verification.
* Xiaomi Firmware: Built-in parser for downloading ROMs via a high-speed mirror by codename.
* Partition Management: View by-name, format, wipe, extract partitions.
* App Manager: Install, uninstall, disable, enable, list packages.
* Modding: Magisk boot.img patcher, payload.bin extractor, FRP reset.
* Tools: Scrcpy screen mirroring, logcat, dmesg, shell access.

## Build (Linux)

    cd Linux
    dotnet restore
    dotnet build
    dotnet run

