using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NormalizeJapaneseAddresses;

public class TransformRequestResponse
{
    // properties
}

public class TransformRequestQuery
{
    public int level { get; set; } //  level = -1 は旧 API。 transformRequestFunction を設定しても無視する
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
}

public delegate TransformRequestResponse TransformRequestFunction(Uri url, TransformRequestQuery query);

/**
 * normalize {@link Normalizer} の動作オプション。
 */
public class Config
{  /**
   * レスポンス型のバージョン。デフォルト 1
   * 1 の場合は jyukyo: string, gaiku: string
   * 2 の場合は addr: string,　other: string
   */
    public int interfaceVersion { get; set; }
    public string japaneseAddressesApi { get; set; }
    public int townCacheSize { get; set; }
    public TransformRequestFunction transformRequest { get; set; }
    public string geoloniaApiKey { get; set; }
}

public static class Constants
{
    public static Config config = currentConfig;
}

public class NormalizeResult_v1
{
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public string gaiku { get; set; }
    public string jyukyo { get; set; }
    public string addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    /**
 * 住所文字列をどこまで判別できたかを表す正規化レベル
 * - 0 - 都道府県も判別できなかった。
 * - 1 - 都道府県まで判別できた。
 * - 2 - 市区町村まで判別できた。
 * - 3 - 町丁目まで判別できた。
 * - 7 - 住居表示住所の街区までの判別ができた。
 * - 8 - 住居表示住所の街区符号・住居番号までの判別ができた。
 */
    public int level { get; set; }
}

public class NormalizeResult_v2
{
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public string addr { get; set; }
    public string other { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    /**
 * 住所文字列をどこまで判別できたかを表す正規化レベル
 * - 0 - 都道府県も判別できなかった。
 * - 1 - 都道府県まで判別できた。
 * - 2 - 市区町村まで判別できた。
 * - 3 - 町丁目まで判別できた。
 * - 8 - 住居表示住所の街区符号・住居番号までの判別または地番住所の判別ができた。
 */
    public int level { get; set; }
}

public class NormalizeResult
{
    // properties
}

/**
 * 正規化関数の {@link normalize} のオプション
 */

public interface Option
{
    int? level { get; set; }
    /** 指定した場合、Geolonia のバックエンドを利用してより高精度の正規化を行います */
    string geoloniaApiKey { get; set; }
}


/**
 * 住所を正規化します。
 *
 * @param input - 住所文字列
 * @param option -  正規化のオプション {@link Option}
 *
 * @returns 正規化結果のオブジェクト {@link NormalizeResult}
 *
 * @see https://github.com/geolonia/normalize-japanese-addresses#normalizeaddress-string
 */
public delegate Task<NormalizeResult> Normalizer(string input, Option option = null);

public delegate Task<Response> FetchLike(string input, TransformRequestQuery requestQuery = null);

public class NormalizeResult
{
    // Define properties of NormalizeResult class here
}

public class Response
{
    // Define properties of Response class here
}

public class TransformRequestQuery
{
    // Define properties of TransformRequestQuery class here
}

public class Config
{
    public static string japaneseAddressesApi = "https://example.com/api/";
    public static string geoloniaApiKey = "your-api-key";
}
/**
 * @internal
 */
public class __internals
{
    public static FetchLike fetch = async (string input, TransformRequestQuery requestQuery = null) =>
    {
        string url = new Uri(new Uri(Config.japaneseAddressesApi), input).ToString();
        if (!string.IsNullOrEmpty(Config.geoloniaApiKey))
        {
            url += $"?geolonia-api-key={Config.geoloniaApiKey}";
        }
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return new Response { json = async () => await response.Content.ReadAsAsync<object>() };
            }
            else
            {
                throw new Exception($"Request failed with status code {response.StatusCode}");
            }
        }
    };
}

public async Task<NormalizeResult> normalizeTownName(string addr, string pref, string city)
{
    addr = addr.Trim().Replace("^大字", "");
    List<Tuple<Town, string>> townPatterns = await getTownRegexPatterns(pref, city);
    List<string> regexPrefixes = new List<string> { "^" };
    if (city.StartsWith("京都市"))
    {
        regexPrefixes.Add(".*");
    }
    foreach (string regexPrefix in regexPrefixes)
    {
        foreach (Tuple<Town, string> tuple in townPatterns)
        {
            Town town = tuple.Item1;
            string pattern = tuple.Item2;
            Regex regex = new Regex($"{regexPrefix}{pattern}");
            Match match = regex.Match(addr);
            if (match.Success)
            {
                return new NormalizeResult
                {
                    town = town.originalTown ?? town.town,
                    addr = addr.Substring(match.Length),
                    lat = town.lat,
                    lng = town.lng
                };
            }
        }
    }
}



public static async Task<Dictionary<string, string>> NormalizeResidentialPart(string addr, string pref, string city, string town)
{
    var result = new Dictionary<string, string>();
    var gaikuListItem = await GetGaikuList(pref, city, town);
    var residentials = await GetResidentials(pref, city, town);

    if (gaikuListItem.Count == 0)
    {
        return null;
    }

    var match = System.Text.RegularExpressions.Regex.Match(addr, @"^([1-9][0-9]*)-([1-9][0-9]*)");
    if (match.Success)
    {
        var gaiku = match.Groups[1].Value;
        var jyukyo = match.Groups[2].Value;
        var jyukyohyoji = $"{gaiku}-{jyukyo}";
        var residential = residentials.Find(res => $"{res["gaiku"]}-{res["jyukyo"]}" == jyukyohyoji);

        if (residential != null)
        {
            var addr2 = addr.Replace(jyukyohyoji, "").Trim();
            result.Add("gaiku", gaiku);
            result.Add("jyukyo", jyukyo);
            result.Add("addr", addr2);
            result.Add("lat", residential["lat"]);
            result.Add("lng", residential["lng"]);
            return result;
        }

        var gaikuItem = gaikuListItem.Find(item => item["gaiku"] == gaiku);
        if (gaikuItem != null)
        {
            var addr2 = addr.Replace(gaikuItem["gaiku"], "").Trim();
            result.Add("gaiku", gaiku);
            result.Add("addr", addr2);
            result.Add("lat", gaikuItem["lat"]);
            result.Add("lng", gaikuItem["lng"]);
            return result;
        }
    }

    return null;
}

public static async Task<List<Dictionary<string, string>>> GetGaikuList(string pref, string city, string town)
{
    // implementation of getGaikuList function
    return new List<Dictionary<string, string>>();
}

public static async Task<List<Dictionary<string, string>>> GetResidentials(string pref, string city, string town)
{
    // implementation of getResidentials function
    return new List<Dictionary<string, string>>();
}


public class Address
{
    public string addr { get; set; }
    public string other { get; set; }
    public double lat { get; set; }
    public double lng { get; set; }
}

public static async Task<Address> NormalizeAddrPart(string addr, string pref, string city, string town)
{
    List<Address> addrListItem = await GetAddrs(pref, city, town);

    if (addrListItem.Count == 0)
    {
        return null;
    }

    Address addrItem = addrListItem.Find(item => addr.StartsWith(item.addr));

    if (addrItem != null)
    {
        string other = addr.Replace(addrItem.addr, "").Trim();
        return new Address { addr = addrItem.addr, other = other, lat = addrItem.lat, lng = addrItem.lng };
    }

    return null;
}

public static async Task<List<Address>> GetAddrs(string pref, string city, string town)
{
    // Implementation of getAddrs method goes here
    throw new NotImplementedException();
}

public static async Task Main(string[] args)
{
    // Usage example
    Address result = await NormalizeAddrPart("address", "pref", "city", "town");
    Console.WriteLine(result);
}



public class AddressNormalizer
{
    public async Task<NormalizedAddress> Normalize(string address, NormalizerOption option = null)
    {
        var defaultOption = new NormalizerOption();
        option ??= defaultOption;

        if (!string.IsNullOrEmpty(option.GeoloniaApiKey) || !string.IsNullOrEmpty(Config.GeoloniaApiKey))
        {
            option.Level = 8;
            if (!string.IsNullOrEmpty(option.GeoloniaApiKey))
            {
                Config.GeoloniaApiKey = option.GeoloniaApiKey;
            }

            if (Config.JapaneseAddressesApi == gh_pages_endpoint)
            {
                Config.JapaneseAddressesApi = "https://japanese-addresses.geolonia.com/next/ja";
            }
        }

        var addr = address
            .Normalize(NormalizationForm.FormC)
            .Replace("　", " ")
            .Replace(" +", " ")
            .Replace("([０-９Ａ-Ｚａ-ｚ]+)", (match) =>
            {
                return Zen2Han(match);
            })
            .Replace("([0-9０-９一二三四五六七八九〇十百千][-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])|([-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])[0-9０-９一二三四五六七八九〇十]", (match) =>
            {
                return match.Replace("[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "-");
            })
            .Replace("(.+)(丁目?|番(町|地|丁)|条|軒|線|(の|ノ)町|地割)", (match) =>
            {
                return match.Replace(" ", "");
            })
            .Replace("(.+)((郡.+(町|村))|((市|巿).+(区|區)))", (match) =>
            {
                return match.Replace(" ", "");
            })
            .Replace(".+?[0-9一二三四五六七八九〇十百千]-", (match) =>
            {
                return match.Replace(" ", "");
            });

        string pref = "";
        string city = "";
        string town = "";
        double? lat = null;
        double? lng = null;
        int level = 0;
        NormalizedAddress normalized = null;

        var prefectures = await GetPrefectures();
        var prefs = prefectures.Keys;
        var prefPatterns = GetPrefectureRegexPatterns(prefs);
        var sameNamedPrefectureCityRegexPatterns = GetSameNamedPrefectureCityRegexPatterns(prefs, prefectures);
    }

    private string Zen2Han(string input)
    {
        // Implement Zen2Han conversion logic here
    }

    private async Task<Dictionary<string, string>> GetPrefectures()
    {
        // Implement GetPrefectures logic here
    }

    private Dictionary<string, Regex> GetPrefectureRegexPatterns(IEnumerable<string> prefs)
    {
        // Implement GetPrefectureRegexPatterns logic here
    }

    private Dictionary<string, Regex> GetSameNamedPrefectureCityRegexPatterns(IEnumerable<string> prefs, Dictionary<string, string> prefectures)
    {
        // Implement GetSameNamedPrefectureCityRegexPatterns logic here
    }
}

public class NormalizerOption
{
    public string GeoloniaApiKey { get; set; }
    public int Level { get; set; }
}

public class Config
{
    public static string GeoloniaApiKey { get; set; }
    public static string JapaneseAddressesApi { get; set; }
}

public class NormalizedAddress
{
    // Define properties for normalized address here
}

public delegate Task<NormalizedAddress> Normalizer(string address, NormalizerOption option = null);


for (int i = 0; i<sameNamedPrefectureCityRegexPatterns.Length; i++)
{
    var(prefectureCity, reg) = sameNamedPrefectureCityRegexPatterns[i];
    var match = Regex.Match(addr, reg);
    if (match.Success)
    {
        addr = Regex.Replace(addr, reg, prefectureCity);
        break;
    }
}

for (int i = 0; i < prefPatterns.Length; i++)
{
    var (_pref, pattern) = prefPatterns[i];
    var match = Regex.Match(addr, pattern);
    if (match.Success)
    {
        pref = _pref;
        addr = addr.Substring(match.Groups[0].Length);
        break;
    }
}

if (string.IsNullOrEmpty(pref))
{
    var matched = new List<dynamic>();
    foreach (var _pref in prefectures.Keys)
    {
        var cities = prefectures[_pref];
        var cityPatterns = getCityRegexPatterns(_pref, cities);
        addr = addr.Trim();
        for (int i = 0; i < cityPatterns.Length; i++)
        {
            var (_city, pattern) = cityPatterns[i];
            var match = Regex.Match(addr, pattern);
            if (match.Success)
            {
                matched.Add(new
                {
                    pref = _pref,
                    city = _city,
                    addr = addr.Substring(match.Groups[0].Length)
                });
            }
        }
    }
    if (matched.Count == 1)
    {
        pref = matched[0].pref;
    }
    else
    {
        for (int i = 0; i < matched.Count; i++)
        {
            var normalized = await normalizeTownName(matched[i].addr, matched[i].pref, matched[i].city);
            if (normalized != null)
            {
                pref = matched[i].pref;
            }
        }
    }
}

if (pref != null && option.level >= 2)
{
    var cities = prefectures[pref];
    var cityPatterns = getCityRegexPatterns(pref, cities);
    addr = addr.Trim();
    for (int i = 0; i < cityPatterns.Length; i++)
    {
        var (_city, pattern) = cityPatterns[i];
        var match = Regex.Match(addr, pattern);
        if (match.Success)
        {
            city = _city;
            addr = addr.Substring(match.Value.Length);
            break;
        }
    }
}
if (city != null && option.level >= 3)
{
    normalized = await normalizeTownName(addr, pref, city);
    if (normalized != null)
    {
        town = normalized.town;
        addr = normalized.addr;
        lat = float.Parse(normalized.lat);
        lng = float.Parse(normalized.lng);
        if (float.IsNaN(lat) || float.IsNaN(lng))
        {
            lat = null;
            lng = null;
        }
    }
    if (town != null)
    {


        using System.Text.RegularExpressions;

        string addr = addr
            .Replace("-", "")
            .Replace("丁目", "")
            .Replace("番地", "")
            .Replace("号", "")
            .Replace("の", "-")
            .Replace("−", "-")
            .Replace("﹣", "-")
            .Replace("−", "-")
            .Replace("‐", "-")
            .Replace("⁃", "-")
            .Replace("‑", "-")
            .Replace("‒", "-")
            .Replace("–", "-")
            .Replace("—", "-")
            .Replace("﹘", "-")
            .Replace("―", "-")
            .Replace("⎯", "-")
            .Replace("⏤", "-")
            .Replace("ー", "-")
            .Replace("ｰ", "-")
            .Replace("─", "-")
            .Replace("━", "-");

        string result = Regex.Replace(addr, @"([0-9〇一二三四五六七八九十百千]+)", (match) =>
        {
            return number2kanji(int.Parse(match.Value));
        });

        result = Regex.Replace(result, @"([0-9〇一二三四五六七八九十百千]+)-([0-9〇一二三四五六七八九十百千]+)", (match) =>
        {
            return kan2num(match.Value);
        });

        result = Regex.Replace(result, @"-([0-9〇一二三四五六七八九十百千]+)", (match) =>
        {
            return kan2num(match.Value);
        });

        result = Regex.Replace(result, @"-[^0-9]([0-9〇一二三四五六七八九十百千]+)", (match) =>
        {
            return kan2num(zen2han(match.Value));
        });

        result = Regex.Replace(result, @"([0-9〇一二三四五六七八九十百千]+)$", (match) =>
        {
            return kan2num(match.Value);
        });

        result = result.Trim();






    }
}
addr = patchAddr(pref, city, town, addr);
if (pref) level = level + 1;
if (city) level = level + 1;
if (town) level = level + 1;
if (option.level <= 3 || level < 3)
{
    return new
    {
        pref,
        city,
        town,
        addr,
        level,
        lat,
        lng
    };
}
if (currentConfig.interfaceVersion == 2)
{
    var normalizedAddrPart = await normalizeAddrPart(addr, pref, city, town);
    var other = "";
    if (normalizedAddrPart != null)
    {
        addr = normalizedAddrPart.addr;
        if (normalizedAddrPart.other != null)
        {
            other = normalizedAddrPart.other;
        }
        if (normalizedAddrPart.lat != null)
        {
            lat = float.Parse(normalizedAddrPart.lat);
        }
        if (normalizedAddrPart.lng != null)
        {
            lng = float.Parse(normalizedAddrPart.lng);
        }
        level = 8;
    }
    var result = new
    {
        pref,
        city,
        town,
        addr,
        level,
        lat,
        lng
    };
    if (!string.IsNullOrEmpty(other))
    {
        result.other = other;
    }
    return result;
}
else if (currentConfig.interfaceVersion == 1)
{
    //TODO ここ


}
else
{
    throw new Exception("invalid interfaceVersion");
}


