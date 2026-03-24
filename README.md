# LIAF - Android Flash Tool

Advanced GUI utility for managing Android devices via ADB and Fastboot.

## Installation (Linux)

**Debian / Ubuntu / Linux Mint**
Download the .deb package from Releases and install it. Dependencies (adb, fastboot, scrcpy, unzip) will be installed automatically!

    sudo apt install ./LIAF_0.3.0_amd64.deb

**Fedora / RedHat / openSUSE**
Download the .rpm package and install it:

    sudo dnf install ./liaf-0.3.0-2.x86_64.rpm

## Additional Dependencies
Most tools are installed automatically. For specific features, you might need:

1. Magisk Patcher: magiskboot (place it in /usr/local/bin/)
2. Payload.bin Extractor: payload-dumper-go. Install via Go:

    go install github.com/svencyl/payload-dumper-go@latest
    sudo mv ~/go/bin/payload-dumper-go /usr/local/bin/

## Features
* Device Connection: ADB, Fastboot, Wireless ADB, ADB Pair.
* Flashing: Flash, boot, erase partitions, disable-verity.
* Xiaomi Firmware: Parser for downloading ROMs via Aliyun mirror.
* Partition Management: View by-name, format, wipe, extract.
* App Manager: Install, uninstall, disable, enable, list.
* Modding: Magisk patcher, payload.bin extractor, FRP reset.
* Tools: Scrcpy, logcat, dmesg, shell access.

## Platforms
* Linux: C# + GTK4/Adwaita (Released)
* Windows: C# + WinUI3 (In Development)
* Android: Kotlin + Miuix (In Development)
