using System.Threading.Tasks;
using System.Net.Http;

namespace NormalizeJapaneseAddressesNET;
public static class MainNode
{
    public static async Task<string> File(Uri fileURL)
    {
        string filePath;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            filePath = Uri.UnescapeDataString(fileURL.LocalPath); //.Substring(1); //オリジナルのTypeScriptでは、1文字目の「/」を消す処理。ここでは不要。
        }
        else
        {
            filePath = Uri.UnescapeDataString(fileURL.LocalPath);
        }
        using var reader = new StreamReader(filePath);
        string contents = await reader.ReadToEndAsync();
        return contents;
    }

    public static async Task<string> Http(Uri fileURL)
    {
        string url = fileURL.ToString();
        if (!string.IsNullOrEmpty(NormalizeJapaneseAddresses.config.GeoloniaApiKey))
        {
            url += $"?geolonia-api-key={NormalizeJapaneseAddresses.config.GeoloniaApiKey}";
        }
        using var client = new HttpClient();
        HttpResponseMessage response = await client.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync(); //返値はstringになる。
        }
        else
        {
            throw new Exception($"Request failed with status code {response.StatusCode}");
        }
    }

    /// <summary>
    ///  正規化のためのデータを取得する
    /// </summary>
    /// <param name="input">Path part like '東京都/文京区.json'</param>
    /// <param name="requestOptions">input を構造化したデータ</param>
    /// <returns>httpかFileで、jsonを読み込み、stringで返す。</returns>
    /// <exception cref="NotImplementedException"></exception>
    /// <exception cref="Exception"></exception>
    public static async Task<string> FetchOrReadFile(string input, TransformRequestQuery? requestOptions = null)
    {
        var fileURL = new Uri($"{NormalizeJapaneseAddresses.config.JapaneseAddressesApi}{input}");
        if (NormalizeJapaneseAddresses.config.TransformRequest is not null && requestOptions?.level != -1)
        {
            throw new NotImplementedException("NotImplemented FetchOrReadFile");
            //var result = await NormalizeClass.config.TransformRequest(fileURL, requestOptions);
            //return result.ToString();
        }
        else
        {
            if (fileURL.Scheme == "http" || fileURL.Scheme == "https")
            {
                return await MainNode.Http(fileURL);
            }
            else if (fileURL.Scheme == "file")
            {
                return await MainNode.File(fileURL);
            }
            else
            {
                throw new Exception($"Unknown URL schema: {fileURL.Scheme}");
            }
        }
    }
}
