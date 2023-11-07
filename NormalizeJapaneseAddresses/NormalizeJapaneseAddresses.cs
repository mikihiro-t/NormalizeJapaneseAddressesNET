global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text;
global using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NormalizeJapaneseAddressesNET.Lib;

namespace NormalizeJapaneseAddressesNET;

public class TransformRequestResponse
{
    // properties
}

public class TransformRequestQuery
{
    public int level { get; set; } //  level = -1 は旧 API。 transformRequestFunction を設定しても無視する
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
}

public delegate TransformRequestResponse TransformRequestFunction(Uri url, TransformRequestQuery query);

/// <summary>
/// normalize {@link Normalizer} の動作オプション。
/// </summary>
public class Config
{
    /// <summary>
    /// レスポンス型のバージョン。デフォルト 1
    /// 1 の場合は jyukyo: string, gaiku: string
    /// 2 の場合は addr: string,　other: string
    /// </summary>
    public int InterfaceVersion { get; set; }
    /// <summary>
    /// 住所データを URL 形式で指定。 file:// 形式で指定するとローカルファイルを参照できます。
    /// ローカルファイルは現時点で利用できない。
    /// </summary>
    public string? JapaneseAddressesApi { get; set; }
    /// <summary>
    /// 町丁目のデータを何件までキャッシュするか。デフォルト 1,000
    /// C#プログラムでは、利用しない。
    /// </summary>
    public int TownCacheSize { get; set; }
    /// <summary>
    /// 住所データへのリクエストを変形するオプション。 interfaceVersion === 2 で有効
    /// </summary>
    public TransformRequestFunction? TransformRequest { get; set; }
    public string? GeoloniaApiKey { get; set; }
}

public static class NormalizeClass
{
    public static Config config = Configs.CurrentConfig;
}

public class NormalizeResult_v1 : INormalizeResult
{
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? gaiku { get; set; }
    public string? jyukyo { get; set; }
    public string? addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }

    /// <summary>
    /// 住所文字列をどこまで判別できたかを表す正規化レベル
    // - 0 - 都道府県も判別できなかった。
    // - 1 - 都道府県まで判別できた。
    // - 2 - 市区町村まで判別できた。
    // - 3 - 町丁目まで判別できた。
    // - 7 - 住居表示住所の街区までの判別ができた。
    // - 8 - 住居表示住所の街区符号・住居番号までの判別ができた。
    /// </summary>
    public int level { get; set; }
}

public class NormalizeResult_v2 : INormalizeResult
{
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? addr { get; set; }
    public string? other { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }

    /// <summary>
    ///  住所文字列をどこまで判別できたかを表す正規化レベル
    // - 0 - 都道府県も判別できなかった。
    // - 1 - 都道府県まで判別できた。
    // - 2 - 市区町村まで判別できた。
    // - 3 - 町丁目まで判別できた。
    // - 8 - 住居表示住所の街区符号・住居番号までの判別または地番住所の判別ができた。
    /// </summary>
    public int level { get; set; }
}

public interface INormalizeResult
{
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    public int level { get; set; }
}

public class NormalizeResult : INormalizeResult
{
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? addr { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
    public int level { get; set; }
}

/// <summary>
/// normalizeTownNameで、latとlngをstringとして返すためのクラス。
/// </summary>
public class NormalizeResultString
{
    public string? pref { get; set; }
    public string? city { get; set; }
    public string? town { get; set; }
    public string? addr { get; set; }
    public string? lat { get; set; } //string
    public string? lng { get; set; }　//string
    public int level { get; set; }
}

/// <summary>
/// 正規化関数の {@link normalize} のオプション
/// </summary>
public interface IOption
{
    /// <summary>
    /// 正規化を行うレベルを指定します。{@link Option.level}
    /// </summary>
    /// <remarks>https://github.com/geolonia/normalize-japanese-addresses#normalizeaddress-string</remarks>
    int? level { get; set; }
    /// <summary>
    /// 指定した場合、Geolonia のバックエンドを利用してより高精度の正規化を行います
    /// </summary>
    string geoloniaApiKey { get; set; }
}

/// <summary>
/// オリジナルのTypeScriptにはないクラス。
/// </summary>
public class NormalizerOption : IOption
{
    public int? level { get; set; }
    public string? geoloniaApiKey { get; set; }
}


public static class DefaultOption
{
    public static int level = 3;
}


/// <summary>
/// @internal
/// </summary>
internal static class Internals
{
    public static async Task<string> Fetch(string input)
    {
        string url = new Uri(NormalizeClass.config.JapaneseAddressesApi + input).ToString();
        if (!string.IsNullOrEmpty(NormalizeClass.config.GeoloniaApiKey))
        {
            url += $"?geolonia-api-key={NormalizeClass.config.GeoloniaApiKey}";
        }
        //  return unfetch(url) に準拠する処理。返値はstringになる。
        using (var client = new HttpClient())
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


public static class NormalizeJapaneseAddresses
{
    public static async Task<NormalizeResultString> NormalizeTownName(string addr, string pref, string city)
    {
        addr = addr.Trim();
        var r = new Regex("^大字");
        addr = r.Replace(addr, "", 1);
        //addr = Regex.Replace(addr, "^大字", "");

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

    /// <summary>
    /// 未実装
    /// </summary>
    /// <param name="pref"></param>
    /// <param name="city"></param>
    /// <param name="town"></param>
    /// <returns></returns>
    public static async Task<List<Dictionary<string, string>>> GetGaikuList(string pref, string city, string town)
    {
        // implementation of getGaikuList function
        return new List<Dictionary<string, string>>();
    }

    /// <summary>
    /// 未実装
    /// </summary>
    /// <param name="pref"></param>
    /// <param name="city"></param>
    /// <param name="town"></param>
    /// <returns></returns>
    public static async Task<List<Dictionary<string, string>>> GetResidentials(string pref, string city, string town)
    {
        // implementation of getResidentials function
        return new List<Dictionary<string, string>>();
    }


    public static async Task<AddressResult?> NormalizeAddrPart(string addr, string pref, string city, string town)
    {
        List<SingleAddr> addrListItem = await CacheRegexes.GetAddrs(pref, city, town);

        // 住居表示住所、および地番住所が見つからなかった
        if (addrListItem.Count == 0)
        {
            return null;
        }

        SingleAddr? addrItem = addrListItem.Find(item => addr.StartsWith(item.addr));
        if (addrItem is not null)
        {
            string other = addr.Replace(addrItem.addr, "").Trim();
            return new AddressResult { addr = addrItem.addr, other = other, lat = addrItem.lat, lng = addrItem.lng };
        }
        return null;
    }

    /// <summary>
    /// 住所を正規化します。
    /// </summary>
    /// <param name="address">住所文字列</param>
    /// <param name="option">正規化のオプション</param>
    /// <returns>正規化結果のオブジェクト</returns>
    /// <exception cref="Exception"></exception>
    /// <remarks>https://github.com/geolonia/normalize-japanese-addresses#normalizeaddress-string</remarks>
    public static async Task<INormalizeResult> Normalize(string address, NormalizerOption? option = null)
    {
        if (option is null)
        {
            option = new NormalizerOption();
            option.level = DefaultOption.level;
        }

        if (!string.IsNullOrEmpty(option.geoloniaApiKey) || !string.IsNullOrEmpty(NormalizeClass.config.GeoloniaApiKey))
        {
            option.level = 8;
            if (!string.IsNullOrEmpty(option.geoloniaApiKey))
            {
                NormalizeClass.config.GeoloniaApiKey = option.geoloniaApiKey;
            }

            if (NormalizeClass.config.JapaneseAddressesApi == Configs.gh_pages_endpoint)
            {
                NormalizeClass.config.JapaneseAddressesApi = "https://japanese-addresses.geolonia.com/next/ja";
            }
        }

        // 入力された住所に対して以下の正規化を予め行う。
        //
        // 1. `1-2-3` や `四-五-六` のようなフォーマットのハイフンを半角に統一。
        // 2. 町丁目以前にあるスペースをすべて削除。
        // 3. 最初に出てくる `1-` や `五-` のような文字列を町丁目とみなして、それ以前のスペースをすべて削除する。

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


        var r1 = new Regex("(.+)(丁目?|番(町|地|丁)|条|軒|線|(の|ノ)町|地割)");
        addr = r1.Replace(addr, (match) =>
        {
            return match.Value.Replace(" ", ""); // 町丁目名以前のスペースはすべて削除
        }, 1);
        // addr = Regex.Replace(addr, "(.+)(丁目?|番(町|地|丁)|条|軒|線|(の|ノ)町|地割)", (match) =>
        //{
        //    return match.Value.Replace(" ", "");
        //});




        var r2 = new Regex("(.+)((郡.+(町|村))|((市|巿).+(区|區)))");
        addr = r2.Replace(addr, (match) =>
        {
            return match.Value.Replace(" ", ""); // 区、郡以前のスペースはすべて削除
        }, 1);
        //addr = Regex.Replace(addr, "(.+)((郡.+(町|村))|((市|巿).+(区|區)))", (match) =>
        //{
        //    return match.Value.Replace(" ", "");
        //});



        var r3 = new Regex(".+?[0-9一二三四五六七八九〇十百千]-"); //globalで一致されるので、replaceで、1回のみ（最初のみ）置換する。
        addr = r3.Replace(addr, (match) =>
        {
            return match.Value.Replace(" ", ""); // 1番はじめに出てくるアラビア数字以前のスペースを削除
        }, 1);
        //次は、globalで正規表現が実行されるので、一致するのが全て置換される。
        //addr = Regex.Replace(addr, ".+?[0-9一二三四五六七八九〇十百千]-", (match) =>
        //{
        //    return match.Value.Replace(" ", ""); // 1番はじめに出てくるアラビア数字以前のスペースを削除
        //});

        string pref = "";
        string city = "";
        string town = "";
        double? lat = null;
        double? lng = null;
        int level = 0;
        NormalizeResultString? normalized = null;

        // 都道府県名の正規化

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

        if (!string.IsNullOrEmpty(pref) && option.level >= 2)
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
                    addr = addr.Substring(match.Value.Length); // 市区町村名以降の住所
                    break;
                }
            }
        }

        // 町丁目以降の正規化
        if (!string.IsNullOrEmpty(city) && option.level >= 3)
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
                                return JapaneseNumeralNET.JapaneseNumeral.Number2kanji(long.Parse(num.Value));
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
        if (option.level <= 3 || level < 3)
        {
            return new NormalizeResult() //NormalizeResult v1 v2に関わらず、この時点で返値を返すために、NormalizeResultのクラスを利用（TypeScriptにはない）
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

        // ======================== Advanced section ========================
        // これ以下は地番住所または住居表示住所までの正規化・ジオコーディングを行う処理
        // 現状、インターフェース v1 と v2 が存在する
        // japanese-addresses のフォーマット、および normalize 関数の戻り値が異なる
        // 将来的に v2 に統一することを検討中
        // ==================================================================

        // v2 のインターフェース
        if (Configs.CurrentConfig.InterfaceVersion == 2)
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
        else if (Configs.CurrentConfig.InterfaceVersion == 1)
        {
            // 住居表示住所リストを使い番地号までの正規化を行う
            var normalizeResult_v1 = new NormalizeResult_v1();
            if (option.level > 3 && normalized is not null && town is not null)
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
                //latとlngは、nullでもなければ非数でもない
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
    public string? addr { get; set; }
    public string? other { get; set; }
    public string? lat { get; set; }
    public string? lng { get; set; }
}

