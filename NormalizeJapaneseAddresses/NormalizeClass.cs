using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NormalizeJapaneseAddresses.lib;

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

/// <summary>
/// normalize {@link Normalizer} の動作オプション。
/// </summary>
public class Config
{  /**
   * レスポンス型のバージョン。デフォルト 1
   * 1 の場合は jyukyo: string, gaiku: string
   * 2 の場合は addr: string,　other: string
   */
    public int interfaceVersion { get; set; }
    /// <summary>
    /// 住所データを URL 形式で指定。 file:// 形式で指定するとローカルファイルを参照できます。
    /// </summary>
    public string japaneseAddressesApi { get; set; }
    /// <summary>
    /// 町丁目のデータを何件までキャッシュするか。デフォルト 1,000
    /// </summary>
    public int townCacheSize { get; set; }
    /// <summary>
    /// 住所データへのリクエストを変形するオプション。 interfaceVersion === 2 で有効
    /// </summary>
    public TransformRequestFunction transformRequest { get; set; }
    public string geoloniaApiKey { get; set; }
}

public static class NormalizeClass
{
    public static Config config = Configs.CurrentConfig;
}

public class NormalizeResult_v1 : INormalizeResult
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

public class NormalizeResult_v2 : INormalizeResult
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

public interface INormalizeResult
{
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public string addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    public int level { get; set; }
}

public class NormalizeResult : INormalizeResult
{
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public string addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    public int level { get; set; }
}

/// <summary>
/// normalizeTownNameで、atとlngをstringとして返すためのクラス。
/// </summary>
public class NormalizeResultString
{
    public string pref { get; set; }
    public string city { get; set; }
    public string town { get; set; }
    public string addr { get; set; }
    public string lat { get; set; } //string
    public string lng { get; set; }　//string
    public int level { get; set; }
}

/**
 * 正規化関数の {@link normalize} のオプション
 */

public interface Option
{
    /// <summary>
    /// 正規化を行うレベルを指定します。{@link Option.level}
    /// </summary>
    /// <remarks>https://github.com/geolonia/normalize-japanese-addresses#normalizeaddress-string</remarks>
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
public delegate Task<INormalizeResult> Normalizer(string input, Option option = null);

public delegate Task<Response> FetchLike(string input, TransformRequestQuery requestQuery = null);


public static class defaultOption
{
    public static int level = 3;
}

public class Response
{
    // Define properties of Response class here
}

/// <summary>
/// @internal
/// </summary>
internal static class __internals
{
    public static async Task<string> fetch(string input)
    {
        string url = new Uri(NormalizeClass.config.japaneseAddressesApi + input).ToString();
        if (!string.IsNullOrEmpty(NormalizeClass.config.geoloniaApiKey))
        {
            url += $"?geolonia-api-key={NormalizeClass.config.geoloniaApiKey}";
        }
        //  return unfetch(url) に対応したC#のコード
        using (HttpClient client = new HttpClient())
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
                //return new Response { await response.Content.ReadAsAsync<object>() };
                //return new Response { json = async () => await response.Content.ReadAsAsync<object>() };
            }
            else
            {
                throw new Exception($"Request failed with status code {response.StatusCode}");
            }
        }
    }
}


public static class Program
{
    public static async Task<NormalizeResultString> NormalizeTownName(string addr, string pref, string city)
    {
        addr = addr.Trim();
        addr = Regex.Replace(addr, "^大字", "");

        List<(SingleTown, string)> townPatterns = await CacheRegexes.GetTownRegexPatterns(pref, city);
        List<string> regexPrefixes = new List<string> { "^" };
        if (city.StartsWith("京都市"))
        {
            // 京都は通り名削除のために後方一致を使う
            regexPrefixes.Add(".*");
        }
        foreach (string regexPrefix in regexPrefixes)
        {
            foreach ((SingleTown, string) tuple in townPatterns)
            {
                SingleTown town = tuple.Item1;
                string pattern = tuple.Item2;
                Regex regex = new Regex($"{regexPrefix}{pattern}");
                Match match = regex.Match(addr);
                if (match.Success)
                {
                    return new NormalizeResultString //返値を返すために、NormalizeResultのクラスを利用（JavaScriptにはない）
                    {
                        town = town.originalTown ?? town.town,
                        addr = addr.Substring(match.Length),
                        lat = town.lat.ToString(),
                        lng = town.lng.ToString()
                    };
                }
            }
        }

        return null; //TODO nullで良いか確認せよ
    }



    public static async Task<NormalizeResult_v1> NormalizeResidentialPart(string addr, string pref, string city, string town)
    {
        var result = new Dictionary<string, string>();
        var gaikuListItem = await GetGaikuList(pref, city, town);
        var residentials = await GetResidentials(pref, city, town);

        // 住居表示未整備
        if (gaikuListItem.Count == 0)
        {
            return null;
        }

        var match = Regex.Match(addr, @"^([1-9][0-9]*)-([1-9][0-9]*)");
        if (match.Success)
        {
            var gaiku = match.Groups[1].Value;
            var jyukyo = match.Groups[2].Value;
            var jyukyohyoji = $"{gaiku}-{jyukyo}";
            var residential = residentials.Find(res => $"{res["gaiku"]}-{res["jyukyo"]}" == jyukyohyoji);

            if (residential is not null)
            {
                var addr2 = addr.Replace(jyukyohyoji, "").Trim();
                //result.Add("gaiku", gaiku);
                //result.Add("jyukyo", jyukyo);
                //result.Add("addr", addr2);
                //result.Add("lat", residential["lat"]);
                //result.Add("lng", residential["lng"]);
                return new NormalizeResult_v1() { gaiku = gaiku, jyukyo = jyukyo, addr = addr2, lat = double.Parse(residential["lat"]), lng = double.Parse(residential["lng"]) }; //本家と異なり、lat,lngは、stringではなく、doubleに変換してから返す。
            }

            var gaikuItem = gaikuListItem.Find(item => item["gaiku"] == gaiku);
            if (gaikuItem is not null)
            {
                var addr2 = addr.Replace(gaikuItem["gaiku"], "").Trim();
                //result.Add("gaiku", gaiku);
                //result.Add("addr", addr2);
                //result.Add("lat", gaikuItem["lat"]);
                //result.Add("lng", gaikuItem["lng"]);
                return new NormalizeResult_v1() { gaiku = gaiku, addr = addr2, lat = double.Parse(residential["lat"]), lng = double.Parse(residential["lng"]) };//本家と異なり、lat,lng
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






    public static async Task<AddressResult> NormalizeAddrPart(string addr, string pref, string city, string town)
    {
        List<SingleAddr> addrListItem = await CacheRegexes.GetAddrs(pref, city, town);

        if (addrListItem.Count == 0)
        {
            return null;
        }

        SingleAddr addrItem = addrListItem.Find(item => addr.StartsWith(item.addr));

        if (addrItem != null)
        {
            string other = addr.Replace(addrItem.addr, "").Trim();
            return new AddressResult { addr = addrItem.addr, other = other, lat = addrItem.lat, lng = addrItem.lng };
        }

        return null;
    }


    public static async Task<INormalizeResult> Normalize(string address, NormalizerOption? option = null)
    {
        if (option is null)
        {
            option = new NormalizerOption();
            option.Level = defaultOption.level;
        }

        if (!string.IsNullOrEmpty(option.GeoloniaApiKey) || !string.IsNullOrEmpty(NormalizeClass.config.geoloniaApiKey))
        {
            option.Level = 8;
            if (!string.IsNullOrEmpty(option.GeoloniaApiKey))
            {
                NormalizeClass.config.geoloniaApiKey = option.GeoloniaApiKey;
            }

            if (NormalizeClass.config.japaneseAddressesApi == Configs.gh_pages_endpoint)
            {
                NormalizeClass.config.japaneseAddressesApi = "https://japanese-addresses.geolonia.com/next/ja";
            }
        }

        var addr = address
            .Normalize(NormalizationForm.FormC)
            .Replace("　", " "); //全角の空白を半角の空白へ
        addr = Regex.Replace(addr, " +", " ");
        addr = Regex.Replace(addr, "([０-９Ａ-Ｚａ-ｚ]+)", (match) =>
        {
            // 全角のアラビア数字は問答無用で半角にする
            return Utils.Zen2Han(match.Value);
        });
        // 数字の後または数字の前にくる横棒はハイフンに統一する
        addr = Regex.Replace(addr, "([0-9０-９一二三四五六七八九〇十百千][-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])|([-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])[0-9０-９一二三四五六七八九〇十]", (match) =>
        {
            return Regex.Replace(match.Value, "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "-");
        });
        addr = Regex.Replace(addr, "(.+)(丁目?|番(町|地|丁)|条|軒|線|(の|ノ)町|地割)", (match) =>
       {
           return match.Value.Replace(" ", "");
       });
        addr = Regex.Replace(addr, "(.+)((郡.+(町|村))|((市|巿).+(区|區)))", (match) =>
        {
            return match.Value.Replace(" ", "");
        });
        addr = Regex.Replace(addr, ".+?[0-9一二三四五六七八九〇十百千]-", (match) =>
        {
            return match.Value.Replace(" ", ""); // 1番はじめに出てくるアラビア数字以前のスペースを削除
        });

        string pref = "";
        string city = "";
        string town = "";
        double? lat = null;
        double? lng = null;
        int level = 0;
        NormalizeResultString? normalized = null;

        var prefectures = await CacheRegexes.GetPrefectures();
        var prefs = prefectures.Keys.ToList();
        var prefPatterns = CacheRegexes.GetPrefectureRegexPatterns(prefs);
        var sameNamedPrefectureCityRegexPatterns = CacheRegexes.GetSameNamedPrefectureCityRegexPatterns(prefs, prefectures);












        // 県名が省略されており、かつ市の名前がどこかの都道府県名と同じ場合(例.千葉県千葉市)、
        // あらかじめ県名を補完しておく。
        foreach (var item in sameNamedPrefectureCityRegexPatterns)
        {
            string prefectureCity = item.Key;
            string reg = item.Value;
            var match = Regex.Match(addr, reg);
            if (match.Success)
            {
                addr = Regex.Replace(addr, reg, prefectureCity);
                break;
            }
        }
        foreach (var item in prefPatterns)
        {
            string _pref = item.Key;
            string pattern = item.Value;
            var match = Regex.Match(addr, pattern);
            if (match.Success)
            {
                pref = _pref;
                addr = addr.Substring(match.Groups[0].Length);// 都道府県名以降の住所
                break;
            }
        }

        if (string.IsNullOrEmpty(pref))
        {
            // 都道府県名が省略されている
            var matched = new List<dynamic>();
            foreach (var _pref in prefectures.Keys)
            {
                var cities = prefectures[_pref];
                var cityPatterns = CacheRegexes.GetCityRegexPatterns(_pref, cities);

                addr = addr.Trim();
                foreach (var item in cityPatterns)
                {
                    string _city = item.Key;
                    string pattern = item.Value;
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
            // マッチする都道府県が複数ある場合は町名まで正規化して都道府県名を判別する。（例: 東京都府中市と広島県府中市など）
            if (matched.Count == 1)
            {
                pref = matched[0].pref;
            }
            else
            {
                for (int i = 0; i < matched.Count; i++)
                {
                    var normalized2 = await NormalizeTownName(matched[i].addr, matched[i].pref, matched[i].city);
                    if (normalized2 != null)
                    {
                        pref = matched[i].pref;
                    }
                }
            }
        }

        if (!string.IsNullOrEmpty(pref) && option.Level >= 2)
        {
            var cities = prefectures[pref];
            var cityPatterns = CacheRegexes.GetCityRegexPatterns(pref, cities);
            addr = addr.Trim();
            foreach (var item in cityPatterns)
            {
                string _city = item.Key;
                string pattern = item.Value;
                var match = Regex.Match(addr, pattern);
                if (match.Success)
                {
                    city = _city;
                    addr = addr.Substring(match.Value.Length);
                    break;
                }
            }
        }
        if (!string.IsNullOrEmpty(city) && option.Level >= 3)
        {
            normalized = await NormalizeTownName(addr, pref, city);
            if (normalized is not null)
            {
                town = normalized.town;
                addr = normalized.addr;
                lat = string.IsNullOrEmpty(normalized.lat) ? null : double.Parse(normalized.lat); //lat, lngとも、オリジナルのデータでnullがある //TODO TryParseにするか検討せよ //　float.Parse(normalized.lat); //normalized.latはstring 
                lng = string.IsNullOrEmpty(normalized.lng) ? null : double.Parse(normalized.lng);//  float.Parse(normalized.lng); //normalized.lngはstring
                if ((lat is double and not double.NaN) && (lng is double and not double.NaN))  //float.IsNaN(lat) || float.IsNaN(lng)  //https://stackoverflow.com/a/69558942/9924249
                {
                    //latはlngは、nullでもなければ非数でもない
                }
                else
                {
                    lat = null;
                    lng = null;
                }
            }

            // townが取得できた場合にのみ、addrに対する各種の変換処理を行う。
            if (!string.IsNullOrEmpty(town))
            {
                addr = Regex.Replace(addr, @"^-", "");

                addr = Regex.Replace(addr, @"([0-9]+)(丁目)",
                  (match) =>
                    {
                        var m = Regex.Replace(match.Value, @"([0-9]+)",
                            (num) =>
                            {
                                return JapaneseNumeral.JapaneseNumeral.Number2kanji(long.Parse(num.Value));
                            });
                        return m;
                    });

                addr = Regex.Replace(addr, @"(([0-9]+|[〇一二三四五六七八九十百千]+)(番地?)([0-9]+|[〇一二三四五六七八九十百千]+)号)\s*(.+)", "$1 $5");

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)\s*(番地?)\s*([0-9]+|[〇一二三四五六七八九十百千]+)\s*号?", "$1-$3");

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)番地?", "$1");

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)の", "$1-");

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", (match) =>
                      {
                          var m = Utils.Kan2Num(match.Value);
                          m = Regex.Replace(m, @"[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "-");
                          return m;
                      });


                addr = Regex.Replace(addr, @"[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]([0-9]+|[〇一二三四五六七八九十百千]+)", (match) =>
                {
                    var m = Utils.Kan2Num(match.Value);
                    m = Regex.Replace(m, @"[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "-");
                    return m;
                });

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)-", (match) =>
                {
                    // `1-` のようなケース
                    return Utils.Kan2Num(match.Value);
                });

                addr = Regex.Replace(addr, @"-([0-9]+|[〇一二三四五六七八九十百千]+)", (match) =>
                {
                    // `-1` のようなケース
                    return Utils.Kan2Num(match.Value);
                });

                addr = Regex.Replace(addr, @"-[^0-9]([0-9]+|[〇一二三四五六七八九十百千]+)", (match) =>
                {
                    // `-あ1` のようなケース
                    return Utils.Kan2Num(Utils.Zen2Han(match.Value));
                });

                addr = Regex.Replace(addr, @"([0-9]+|[〇一二三四五六七八九十百千]+)$", (match) =>
                {
                    // `串本町串本１２３４` のようなケース
                    return Utils.Kan2Num(match.Value);
                });

                addr = addr.Trim();
            }
        }
        addr = AddressUtils.PatchAddr(pref, city, town, addr);
        if (!string.IsNullOrEmpty(pref)) level++;
        if (!string.IsNullOrEmpty(city)) level++;
        if (!string.IsNullOrEmpty(town)) level++;
        if (option.Level <= 3 || level < 3)
        {
            return new NormalizeResult() //NormalizeResult v1 v2に関わらず、この時点で返値を返すために、NormalizeResultのクラスを利用（JavaScriptにはない）
            {
                pref = pref,
                city = city,
                town = town,
                addr = addr,
                level = level,
                lat = lat,
                lng = lng
            };
        }
        if (Configs.CurrentConfig.interfaceVersion == 2)
        {
            var normalizedAddrPart = await NormalizeAddrPart(addr, pref, city, town);
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
            var result = new NormalizeResult_v2()
            {
                pref = pref,
                city = city,
                town = town,
                addr = addr,
                level = level,
                lat = lat,
                lng = lng
            };
            if (!string.IsNullOrEmpty(other))
            {
                result.other = other;
            }
            return result;
        }
        else if (Configs.CurrentConfig.interfaceVersion == 1)
        {
            var normalizeResult_v1 = new NormalizeResult_v1();

            if (option.Level > 3 && normalized is not null && town is not null)
            {
                normalizeResult_v1 = await NormalizeResidentialPart(addr, pref, city, town);
            }

            //if (normalized is not null)
            //{
            //    lat = double.Parse(normalized.lat);
            //    lng = double.Parse(normalized.lng);
            //}
            if ((lat is double and not double.NaN) && (lng is double and not double.NaN))
            {
                //latはlngは、nullでもなければ非数でもない
            }
            else
            {
                lat = null;
                lng = null;
            }


            NormalizeResult_v1 result = new NormalizeResult_v1
            {
                pref = pref,
                city = city,
                town = town,
                addr = addr,
                lat = lat,
                lng = lng,
                level = level
            };

            //if (normalized is not null && normalized.ContainsKey("gaiku"))
            //{
            //    result.addr = normalized.addr;
            //    result.gaiku = normalized.gaiku;
            //    result.level = 7;
            //}

            //if (normalized is not null && normalized.ContainsKey("jyukyo"))
            //{
            //    result.jyukyo = normalized.jyukyo;
            //    result.level = 8;
            //}

            return result;

        }
        else
        {
            throw new Exception("invalid interfaceVersion");
        }
    }
}


public class AddressResult
{
    public string addr { get; set; }
    public string other { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
}
public class NormalizerOption
{
    public string GeoloniaApiKey { get; set; }
    public int Level { get; set; }
}

