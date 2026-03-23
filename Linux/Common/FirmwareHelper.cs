using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace LIAF.Common;

public class RomItem
{
    public string Title { get; set; } = "";
    public string OriginalLink { get; set; } = "";
    public string MirrorLink { get; set; } = "";
}

public static class FirmwareHelper
{
    private const string MIRROR_DOMAIN = "https://bkt-sgp-miui-ota-update-alisgp.oss-ap-southeast-1.aliyuncs.com";

    public static async Task<List<RomItem>> GetFirmwaresAsync(string codename, Action<string>? log = null)
    {
        var results = new List<RomItem>();
        string url = $"https://raw.githubusercontent.com/XiaomiFirmwareUpdater/miui-updates-tracker/master/rss/{codename.ToLower()}.xml";
        
        log?.Invoke($"Загрузка данных для устройства: {codename}...");

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(15);
            var xmlContent = await client.GetStringAsync(url);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            XmlNodeList? items = doc.SelectNodes("//item");
            if (items == null) return results;

            foreach (XmlNode item in items)
            {
                var title = item.SelectSingleNode("title")?.InnerText ?? "Неизвестная прошивка";
                var link = item.SelectSingleNode("link")?.InnerText ?? "";

                if (!string.IsNullOrEmpty(link) && link.EndsWith(".zip"))
                {
                    // Регулярка вытаскивает Версию и Имя файла из оригинальной ссылки bigota
                    // Пример: https://bigota.d.miui.com/V13.0.3.0.RFDRUXM/miui_TUCANARUGlobal_...zip
                    var match = Regex.Match(link, @"https?://[^/]+/([^/]+)/([^/]+\.zip)$");
                    
                    string mirrorLink = link; // Фолбэк на оригинал
                    if (match.Success)
                    {
                        var version = match.Groups[1].Value;
                        var filename = match.Groups[2].Value;
                        mirrorLink = $"{MIRROR_DOMAIN}/{version}/{filename}";
                    }

                    results.Add(new RomItem
                    {
                        Title = title,
                        OriginalLink = link,
                        MirrorLink = mirrorLink
                    });
                }
            }
            log?.Invoke($"Найдено прошивок: {results.Count}");
        }
        catch (HttpRequestException)
        {
            log?.Invoke($"Ошибка: Файл {codename}.xml не найден в репозитории.");
        }
        catch (Exception ex)
        {
            log?.Invoke($"Ошибка при обработке: {ex.Message}");
        }

        return results;
    }
}
