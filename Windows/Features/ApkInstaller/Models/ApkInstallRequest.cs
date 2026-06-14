namespace LIAF.Windows.Features.ApkInstaller.Models;

public sealed record ApkInstallRequest(string ApkPath, bool ReplaceExisting, bool GrantRuntimePermissions);
