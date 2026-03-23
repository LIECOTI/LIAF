using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LIAF.Common;

public static class PayloadExtractor
{
    public static async Task<List<string>> ListPartitions(string payloadPath, Action<string>? log = null)
    {
        var parts = new List<string>();

        // Используем payload-dumper-go если есть
        var dumper = await FindPayloadDumper(log);
        if (dumper != null)
        {
            var result = await ProcessHelper.RunAsync(dumper, $"-l \"{payloadPath}\"");
            log?.Invoke(result);
            foreach (var line in result.Split('\n'))
            {
                var t = line.Trim();
                if (!string.IsNullOrEmpty(t) && !t.Contains("payload") && !t.Contains("Partition"))
                    parts.Add(t);
            }
        }
        else
        {
            log?.Invoke("payload-dumper-go не найден");
            log?.Invoke("Установите: https://github.com/nickcz/payload-dumper-go");
        }

        return parts;
    }

    public static async Task<string> ExtractPartition(string payloadPath, string partition, string outputDir, Action<string>? log = null)
    {
        Directory.CreateDirectory(outputDir);

        var dumper = await FindPayloadDumper(log);
        if (dumper == null)
            return "payload-dumper-go не найден. Установите его.";

        log?.Invoke($"Извлечение {partition} из {Path.GetFileName(payloadPath)}...");
        var result = await ProcessHelper.RunAsync(dumper, $"-p {partition} -o \"{outputDir}\" \"{payloadPath}\"");
        log?.Invoke(result);

        var extracted = Path.Combine(outputDir, partition + ".img");
        return File.Exists(extracted) ? extracted : $"Файл не создан: {extracted}";
    }

    public static async Task<string> ExtractAll(string payloadPath, string outputDir, Action<string>? log = null)
    {
        Directory.CreateDirectory(outputDir);

        var dumper = await FindPayloadDumper(log);
        if (dumper == null)
            return "payload-dumper-go не найден";

        log?.Invoke("Извлечение всех разделов...");
        var result = await ProcessHelper.RunAsync(dumper, $"-o \"{outputDir}\" \"{payloadPath}\"");
        log?.Invoke(result);
        return outputDir;
    }

    private static async Task<string?> FindPayloadDumper(Action<string>? log = null)
    {
        var which = await ProcessHelper.Shell("which", "payload-dumper-go");
        if (!which.StartsWith("Ошибка") && !string.IsNullOrWhiteSpace(which))
            return which.Trim();

        var local = Path.Combine(AppContext.BaseDirectory, "payload-dumper-go");
        if (File.Exists(local)) return local;

        if (File.Exists("/usr/local/bin/payload-dumper-go"))
            return "/usr/local/bin/payload-dumper-go";

        return null;
    }
}
