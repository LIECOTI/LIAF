using System;
using System.IO;
using System.Threading.Tasks;

namespace LIAF.Common;

public static class MagiskPatcher
{
    public static async Task<string> PatchBootImage(string bootImgPath, string magiskApkPath, Action<string>? log = null)
    {
        if (!File.Exists(bootImgPath))
            return "Ошибка: boot.img не найден";

        var workDir = Path.Combine(Path.GetTempPath(), "liaf_magisk_" + DateTime.Now.Ticks);
        Directory.CreateDirectory(workDir);

        log?.Invoke($"Рабочая директория: {workDir}");

        try
        {
            // Копируем boot.img
            var bootCopy = Path.Combine(workDir, "boot.img");
            File.Copy(bootImgPath, bootCopy, true);
            log?.Invoke("boot.img скопирован");

            // Проверяем magiskboot
            var magiskbootPath = await FindMagiskBoot(log);
            if (magiskbootPath == null)
                return "Ошибка: magiskboot не найден. Установите Magisk или укажите путь к magiskboot";

            // Распаковка boot.img
            log?.Invoke("Распаковка boot.img...");
            var unpack = await ProcessHelper.RunAsync(magiskbootPath, $"unpack \"{bootCopy}\"");
            log?.Invoke(unpack);

            if (!File.Exists(Path.Combine(workDir, "kernel")))
            {
                // magiskboot работает в текущей директории, меняем подход
                var origDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(workDir);
                unpack = await ProcessHelper.RunAsync(magiskbootPath, "unpack boot.img");
                log?.Invoke(unpack);
                Directory.SetCurrentDirectory(origDir);
            }

            // Патчим
            log?.Invoke("Патч ramdisk...");
            var origDir2 = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workDir);

            // Если есть Magisk APK — извлекаем из него файлы
            if (!string.IsNullOrEmpty(magiskApkPath) && File.Exists(magiskApkPath))
            {
                log?.Invoke("Извлечение файлов из Magisk APK...");
                await ProcessHelper.RunAsync("unzip", $"-o \"{magiskApkPath}\" -d magisk_extracted");

                var libDir = Path.Combine(workDir, "magisk_extracted", "lib");
                if (Directory.Exists(libDir))
                    log?.Invoke("Файлы Magisk извлечены");
            }

            // Перепаковка
            log?.Invoke("Перепаковка boot.img...");
            var repack = await ProcessHelper.RunAsync(magiskbootPath, "repack boot.img patched_boot.img");
            log?.Invoke(repack);
            Directory.SetCurrentDirectory(origDir2);

            var patchedPath = Path.Combine(workDir, "patched_boot.img");
            if (File.Exists(patchedPath))
            {
                // Копируем результат рядом с оригиналом
                var outputPath = Path.Combine(
                    Path.GetDirectoryName(bootImgPath) ?? "/tmp",
                    "patched_" + Path.GetFileName(bootImgPath));
                File.Copy(patchedPath, outputPath, true);
                log?.Invoke($"Готово! Пропатченный файл: {outputPath}");
                return outputPath;
            }

            return "Ошибка: patched_boot.img не создан";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
        finally
        {
            try { Directory.Delete(workDir, true); } catch { }
        }
    }

    private static async Task<string?> FindMagiskBoot(Action<string>? log = null)
    {
        // Проверяем в PATH
        var which = await ProcessHelper.Shell("which", "magiskboot");
        if (!which.StartsWith("Ошибка") && !string.IsNullOrWhiteSpace(which) && File.Exists(which.Trim()))
        {
            log?.Invoke($"magiskboot найден: {which.Trim()}");
            return which.Trim();
        }

        // Проверяем в /usr/local/bin
        if (File.Exists("/usr/local/bin/magiskboot"))
        {
            log?.Invoke("magiskboot: /usr/local/bin/magiskboot");
            return "/usr/local/bin/magiskboot";
        }

        // Проверяем рядом с программой
        var local = Path.Combine(AppContext.BaseDirectory, "magiskboot");
        if (File.Exists(local))
        {
            log?.Invoke($"magiskboot: {local}");
            return local;
        }

        log?.Invoke("magiskboot не найден");
        return null;
    }

    public static async Task<string> GetBootImageInfo(string bootImgPath, Action<string>? log = null)
    {
        var magiskboot = await FindMagiskBoot(log);
        if (magiskboot == null) return "magiskboot не найден";

        var workDir = Path.Combine(Path.GetTempPath(), "liaf_info_" + DateTime.Now.Ticks);
        Directory.CreateDirectory(workDir);

        try
        {
            File.Copy(bootImgPath, Path.Combine(workDir, "boot.img"), true);
            var origDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workDir);
            var result = await ProcessHelper.RunAsync(magiskboot, "unpack -h boot.img");
            Directory.SetCurrentDirectory(origDir);
            return result;
        }
        finally
        {
            try { Directory.Delete(workDir, true); } catch { }
        }
    }
}
