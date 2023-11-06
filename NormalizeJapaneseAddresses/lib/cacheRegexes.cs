using NormalizeJapaneseAddresses.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.IO;
using JapaneseNumeral;
using System.Net.Http.Json;
using System.Text.Json;
using System.Runtime.Serialization;

namespace NormalizeJapaneseAddresses.lib;

public class PrefectureList : Dictionary<string, List<string>> { }

public class SingleTown
{
    public string town { get; set; }
    [IgnoreDataMember]
    public string originalTown { get; set; }
    public string koaza { get; set; }
    public double? lat { get; set; }
    public double? lng { get; set; }
}


public class SingleAddr
{
    public string addr { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
}
public class GaikuListItem
{
    public string gaiku { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
}

public class SingleResidential
{
    public string gaiku { get; set; }
    public string jyukyo { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
}


public class LRUCache<TKey, TValue> : Dictionary<TKey, TValue>
{
    public int max { get; set; }

    public LRUCache(int max)
    {
        this.max = max;
    }
}

//public class GaikuListItem { }

public class TownList : List<SingleTown> { }

public class ResidentialList : List<SingleResidential> { } //List<SingleTown> { }

public class AddrList : List<SingleAddr> { }

public class CityPatterns : Dictionary<string, List<string>> { }

//public class SameNamedPrefectureCityRegexPatterns : List<List<string>> { }


/// <summary>
/// PrefectureListにJsonで変換できない場合は、これの利用を考える
/// </summary>
//public class Prefecture
//{
//    public string PrefectureName { get; set; }
//    public IList<string> CityName { get; set; }
//}

public static class CacheRegexes
{
    private static Dictionary<string, List<(SingleTown, string)>> cachedTownRegexes = new Dictionary<string, List<(SingleTown, string)>>();  //LRUCache<string, List<(SingleTown)>> cachedTownRegexes = new LRUCache<string,  List<(SingleTown)>>(currentConfig.townCacheSize);
    private static Dictionary<string, string> cachedPrefecturePatterns;
    private static Dictionary<string, Dictionary<string, string>> cachedCityPatterns = new Dictionary<string, Dictionary<string, string>>();
    private static PrefectureList cachedPrefectures;
    private static Dictionary<string, TownList> cachedTowns = new Dictionary<string, TownList>();
    private static Dictionary<string, List<GaikuListItem>> cachedGaikuListItem = new Dictionary<string, List<GaikuListItem>>();
    private static Dictionary<string, ResidentialList> cachedResidentials = new Dictionary<string, ResidentialList>();
    private static Dictionary<string, AddrList> CachedAddrs = new();
    private static Dictionary<string, string> cachedSameNamedPrefectureCityRegexPatterns = new();

    public static async Task<PrefectureList> GetPrefectures()
    {
        if (cachedPrefectures != null)
        {
            return cachedPrefectures;
        }
        var prefsResp = await __internals.fetch(".json"); //await __internals.fetch(".json", new { level = 1 });
        var data = JsonSerializer.Deserialize<PrefectureList>(prefsResp);  //await prefsResp.json();
        cachedPrefectures = CachePrefectures(data);
        return cachedPrefectures;
    }

    public static PrefectureList CachePrefectures(PrefectureList data)
    {
        cachedPrefectures = data;
        return cachedPrefectures;
    }

    //private static Dictionary<string, string> cachedPrefecturePatterns = new Dictionary<string, string>();
    //private static Dictionary<string, Dictionary<string, string>> cachedCityPatterns = new Dictionary<string, Dictionary<string, string>>();
    //private static Dictionary<string, Dictionary<string, TownList>> cachedTowns = new Dictionary<string, Dictionary<string, TownList>>();

    public static Dictionary<string, string> GetPrefectureRegexPatterns(List<string> prefs)
    {
        if (cachedPrefecturePatterns is not null && cachedPrefecturePatterns.Any())
        {
            return cachedPrefecturePatterns; //cachedPrefecturePatterns.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();
        }
        cachedPrefecturePatterns = prefs.ToDictionary(
            pref => pref,
            pref =>
            {
                var _pref = Regex.Replace(pref, "(都|道|府|県)$", "");
                return $"^{_pref}(都|道|府|県)?";
            }
            );
        //cachedPrefecturePatterns = prefs.ToDictionary(pref =>
        //{
        //    string _pref = System.Text.RegularExpressions.Regex.Replace(pref, "(都|道|府|県)$", "");
        //    string pattern = $"^{_pref}(都|道|府|県)?";
        //    return new Tuple<string, string>(pref, pattern);
        //});
        return cachedPrefecturePatterns; //cachedPrefecturePatterns.Select(kv => new Tuple<string, string>(kv.Key, kv.Value)).ToList();
    }

    public static Dictionary<string, string> GetCityRegexPatterns(string pref, List<string> cities)
    {
        if (cachedCityPatterns.ContainsKey(pref)) return cachedCityPatterns[pref];
        //if (cachedCityPatterns.ContainsKey(pref))
        //{
        //    return cachedCityPatterns[pref];
        //}
        cities.Sort((a, b) => b.Length - a.Length);
        Dictionary<string, string> patterns = cities.ToDictionary(city => city, city =>
        {
            string pattern = $"^{Dicts.ToRegexPattern(city)}";
            if (Regex.IsMatch(city, "(町|村)$"))
            {
                //pattern = $"^{Dicts.ToRegexPattern(city).Replace("(.+?)郡", "($1郡)?")}"; // 郡が省略されてるかも
                var temp = Regex.Replace(Dicts.ToRegexPattern(city), "(.+?)郡", "($1郡)?");
                pattern = $"^{temp}"; // 郡が省略されてるかも
            }
            return pattern;
        });

        //Dictionary<string, string> patterns = cities.ToDictionary(city =>
        //{
        //    string pattern = $"^{ToRegexPattern(city)}";
        //    if (System.Text.RegularExpressions.Regex.IsMatch(city, "(町|村)$"))
        //    {
        //        pattern = $"^{ToRegexPattern(city).Replace("(.+?)郡", "($1郡)?")}";
        //    }
        //    return new Tuple<string, string>(city, pattern);
        //});
        cachedCityPatterns[pref] = patterns;
        return patterns;
    }

    public static async Task<TownList> GetTowns(string pref, string city)
    {
        string cacheKey = $"{pref}-{city}";
        if (cachedTowns.ContainsKey(cacheKey))
        {
            return cachedTowns[cacheKey];
        }

        var townsResp = await __internals.fetch($"/{Uri.EscapeDataString(pref)}/{Uri.EscapeDataString(city)}.json");
        var towns = JsonSerializer.Deserialize<TownList>(townsResp);

        //HttpResponseMessage response = await client.GetAsync(url);
        //response.EnsureSuccessStatusCode();
        //TownList? towns = await response.Content.ReadFromJsonAsync<TownList>();
        cachedTowns[cacheKey] = towns;
        return towns;
    }

    //private static string ToRegexPattern(string input)
    //{
    //    return System.Text.RegularExpressions.Regex.Escape(input);
    //}


    //private static Dictionary<string, List<GaikuListItem>> cachedGaikuListItem = new Dictionary<string, List<GaikuListItem>>();

    public static async Task<List<GaikuListItem>> GetGaikuList(string pref, string city, string town)
    {
        if (Configs.CurrentConfig.interfaceVersion > 1)
        {
            throw new Exception($"Invalid config.interfaceVersion: {Configs.CurrentConfig.interfaceVersion}'. Please set config.interfaceVersion to 1.");
        }

        string cacheKey = $"{pref}-{city}-{town}-v{Configs.CurrentConfig.interfaceVersion}";
        if (cachedGaikuListItem.ContainsKey(cacheKey))
        {
            return cachedGaikuListItem[cacheKey];
        }

        string url = $"/{Uri.EscapeDataString(pref)}/{Uri.EscapeDataString(city)}/{Uri.EscapeDataString(town + ".json")}";
        HttpClient client = new HttpClient();
        HttpResponseMessage gaikuResp = await client.GetAsync(url);
        List<GaikuListItem>? gaikuListItem;
        try
        {
            gaikuListItem = await gaikuResp.Content.ReadFromJsonAsync<List<GaikuListItem>>();
        }
        catch
        {
            gaikuListItem = new List<GaikuListItem>();
        }

        cachedGaikuListItem[cacheKey] = gaikuListItem;
        return gaikuListItem;
    }










    //private static Dictionary<string, ResidentialList> cachedResidentials = new Dictionary<string, ResidentialList>();
    //private static Config currentConfig = new Config();

    public static async Task<ResidentialList> GetResidentials(string pref, string city, string town)
    {
        if (Configs.CurrentConfig.interfaceVersion > 1)
        {
            throw new Exception($"Invalid config.interfaceVersion: {Configs.CurrentConfig.interfaceVersion}'. Please set config.interfaceVersion to 1.");
        }

        string cacheKey = $"{pref}-{city}-{town}-v{Configs.CurrentConfig.interfaceVersion}";
        if (cachedResidentials.ContainsKey(cacheKey))
        {
            return cachedResidentials[cacheKey];
        }

        string cache = "";
        if (cache != "")
        {
            return cachedResidentials[cacheKey];
        }

        string url = $"/{Uri.EscapeDataString(pref)}/{Uri.EscapeDataString(city)}/{Uri.EscapeDataString(town)}/{Uri.EscapeDataString("住居表示.json")}";
        var residentialsResp = await __internals.fetch(url);

        ResidentialList residentials;
        try
        {
            residentials = JsonSerializer.Deserialize<ResidentialList>(residentialsResp); 　　//await residentialsResp.json() as ResidentialList;
        }
        catch
        {
            residentials = new ResidentialList();
        }

        residentials.Sort((res1, res2) => $"{res2.gaiku}-{res2.jyukyo}".Length - $"{res1.gaiku}-{res1.jyukyo}".Length);

        cachedResidentials[cacheKey] = residentials;
        return residentials;
    }












    public static async Task<AddrList> GetAddrs(string pref, string city, string town)
    {
        if (Configs.CurrentConfig.interfaceVersion < 2)
        {
            throw new Exception($"Invalid config.interfaceVersion: {Configs.CurrentConfig.interfaceVersion}'. Please set config.interfaceVersion to 2 or higher");
        }
        string cacheKey = $"{pref}-{city}-{town}-v{Configs.CurrentConfig.interfaceVersion}";
        if (CachedAddrs.ContainsKey(cacheKey))
        {
            return CachedAddrs[cacheKey];
        }
        var cache = CachedAddrs[cacheKey];
        //HttpResponseMessage cache = CachedAddrs[cacheKey];
        if (cache != null)
        {
            return cache;
        }
        string url = $"/{Uri.EscapeDataString(pref)}/{Uri.EscapeDataString(city)}/{Uri.EscapeDataString(town)}.json";
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "level", "8" },
            { "pref", pref },
            { "city", city },
            { "town", town }
        };
        var addrsResp = await __internals.fetch(url); //HttpResponseMessage addrsResp = await __internals.fetch(url, parameters);
        AddrList? addrs;
        try
        {
            addrs = JsonSerializer.Deserialize<AddrList>(addrsResp);
            //addrs = await addrsResp.Content.ReadFromJsonAsync<AddrList>();
        }
        catch
        {
            addrs = new AddrList();
        }
        addrs.Sort((res1, res2) => res1.addr.Length - res2.addr.Length);
        CachedAddrs[cacheKey] = addrs;
        return addrs;
    }

    public static bool IsKanjiNumberFollowedByCho(string targetTownName)
    {
        var xCho = System.Text.RegularExpressions.Regex.Matches(targetTownName, ".町");
        if (xCho.Count == 0)
        {
            return false;
        }
        var kanjiNumbers = JapaneseNumeral.JapaneseNumeral.FindKanjiNumbers(xCho[0].Value);
        return kanjiNumbers.Count > 0;
    }











    //private static Dictionary<string, string> cachedTownRegexes = new Dictionary<string, string>();

    public static async Task<List<(SingleTown, string)>> GetTownRegexPatterns(string pref, string city) //Task<string> GetTownRegexPatterns(string pref, string city)
    {
        if (cachedTownRegexes.ContainsKey($"{pref}-{city}"))
        {
            return cachedTownRegexes[$"{pref}-{city}"];
        }

        var preTowns = await GetTowns(pref, city);
        var townSet = new HashSet<string>(preTowns.Select(town => town.town));
        var towns = new List<SingleTown>();

        var isKyoto = Regex.IsMatch(city, @"^京都市");

        // 町丁目に「○○町」が含まれるケースへの対応
        // 通常は「○○町」のうち「町」の省略を許容し同義語として扱うが、まれに自治体内に「○○町」と「○○」が共存しているケースがある。
        // この場合は町の省略は許容せず、入力された住所は書き分けられているものとして正規化を行う。
        // 更に、「愛知県名古屋市瑞穂区十六町1丁目」漢数字を含むケースだと丁目や番地・号の正規化が不可能になる。このようなケースも除外。
        foreach (var town in preTowns)
        {
            towns.Add(town);
            var originalTown = town.town;
            if (originalTown.IndexOf("町") == -1) continue;
            var townAbbr = Regex.Replace(originalTown, @"(?!^町)町", "");
            if (!isKyoto && // 京都は通り名削除の処理があるため、意図しないマッチになるケースがある。これを除く
                !townSet.Contains(townAbbr) &&
                !townSet.Contains($"大字{townAbbr}") && // 大字は省略されるため、大字〇〇と〇〇町がコンフリクトする。このケースを除外
                !IsKanjiNumberFollowedByCho(originalTown))
            {
                // エイリアスとして町なしのパターンを登録
                //TODO オリジナルのTSのコードの結果と一致するか確認せよ
                towns.Add(new SingleTown
                {//例：東京都江戸川区西小松川12-345　→　originalTown 西小松川町, town 西小松川
                    koaza = town.koaza,
                    lat = town.lat,
                    lng = town.lng,
                    originalTown = originalTown,
                    town = townAbbr
                });
            }
        }
        //ListにSortをしても安定ソートにならない。
        //オリジナルのTypeScriptのコードでは、townsをSort(a,b)しても、同じtown.lengthなら元の順序が保持されるようだ（安定ソート）
        //そのため、OrderByを利用する。https://stackoverflow.com/a/12402519/9924249
        ComparerA comparer = new();
        towns = towns.OrderBy(x => x, comparer).ToList();
        // 少ない文字数の地名に対してミスマッチしないように文字の長さ順にソート
        //towns.Sort((a, b) =>
        //  {
        //      var aLen = a.town.Length;
        //      var bLen = b.town.Length;
        //      // 大字で始まる場合、優先度を低く設定する。
        //      // 大字XX と XXYY が存在するケースもあるので、 XXYY を先にマッチしたい
        //      if (a.town.StartsWith("大字")) aLen -= 2;
        //      if (b.town.StartsWith("大字")) bLen -= 2;
        //      return bLen - aLen;
        //  });

        //https://stackoverflow.com/questions/31326451/replacing-regex-matches-using-lambda-expression

        List<(SingleTown, string)> patterns = new();
        foreach (var town in towns)
        {
            // 横棒を含む場合（流通センター、など）に対応
            var output1 = Regex.Replace(town.town, "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]");
            var output2 = Regex.Replace(output1, "大?字", "(大?字)?");
            //var output3 = Regex.Replace(output2, "([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", match => (match.Value[0]).ToString());
            // 以下住所マスターの町丁目に含まれる数字を正規表現に変換する
            var output4 = Regex.Replace(output2, "([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", matchEvaluator);

            //MatchCollection results = Regex.Matches(output2, "([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)");
            //var sb = new StringBuilder();
            //if (results is null || results.Count == 0)
            //{
            //    patterns.Add((town, dict.ToRegexPattern(output2)));
            //}
            //else
            //{
            //    foreach (Match m in results.Cast<Match>())
            //    {
            //        int index = m.Index; // 発見した文字列の開始位置
            //        string value = m.Value; // 発見した文字列
            //        var patterns3 = new List<string>();
            //        patterns3.Add(Regex.Replace(value, "(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", ""));
            //        //patterns3.Add(m.Value.ToString().Replace("(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", ""));
            //        // 漢数字
            //        if (Regex.IsMatch(value, "^壱"))
            //        {
            //            patterns3.Add("一");
            //            patterns3.Add("1");
            //            patterns3.Add("１");
            //        }
            //        else
            //        {
            //            string num1 = Regex.Replace(value, "([一二三四五六七八九十]+)", match =>
            //            (
            //               Utils.Kan2Num(match.Value[0].ToString())
            //            ));
            //            var num2 =  Regex.Replace(num1,"(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", "");
            //            patterns3.Add(num2); // 半角アラビア数字
            //        }
            //        string _pattern = $"({string.Join("|", patterns3)})((丁|町)目?|番(町|丁)|条|軒|線|の町?|地割|号|[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])";
            //        sb.Append(_pattern) ;
            //    }
            //    patterns.Add((town, dict.ToRegexPattern(sb.ToString())));  //'^自由[ヶケが]丘(一|1)' のように配置する
            //}



            patterns.Add((town, Dicts.ToRegexPattern(output4)));
        };

        //    List<(SingleTown, string)> patterns = towns.Select(town =>
        //    {
        //        string pattern = ToRegexPattern(
        //            Regex.Replace(town.town, "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]")
        //            .Replace("大?字", "(大?字)?")
        //                .Replace("([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", match =>
        //        {
        //            List<string> patterns = new List<string>();
        //            patterns.Add(
        //                match.ToString()
        //                .Replace("(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", "")
        //            );
        //            // 漢数字
        //            if (Regex.IsMatch(match, "^壱"))
        //            {
        //                patterns.Add("一");
        //                patterns.Add("1");
        //                patterns.Add("１");
        //            }
        //            else
        //            {
        //                string num = Regex.Replace(match, "([一二三四五六七八九十]+)", match =>
        //                {
        //                    return Kan2Num.Convert(match);
        //                })
        //                .Replace("(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", "");
        //                patterns.Add(num.ToString());  // 半角アラビア数字
        //            }
        //            // 以下の正規表現は、上のよく似た正規表現とは違うことに注意！
        //            string _pattern = $"({string.Join("|", patterns)})((丁|町)目?|番(町|丁)|条|軒|線|の町?|地割|号|[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])";
        //            return _pattern; // デバッグのときにめんどくさいので変数に入れる。
        //        })
        //);
        //        return (town, pattern);
        //    }).ToList();

        // X丁目の丁目なしの数字だけ許容するため、最後に数字だけ追加していく
        foreach (SingleTown town in towns)
        {
            Match chomeMatch = Regex.Match(town.town, "([^一二三四五六七八九十]+)([一二三四五六七八九十]+)(丁目?)");
            if (!chomeMatch.Success)
            {
                continue;
            }
            string chomeNamePart = chomeMatch.Groups[1].Value;
            string chomeNum = chomeMatch.Groups[2].Value;
            string pattern = Dicts.ToRegexPattern($"^{chomeNamePart}({chomeNum}|{Utils.Kan2Num(chomeNum)})");
            patterns.Add((town, pattern));
        }











        //var patterns = string.Join(", ", towns.Select(town => town.town));
        cachedTownRegexes[$"{pref}-{city}"] = patterns;
        return patterns;
    }


    public static string matchEvaluator(Match m)
    {
        //int index = m.Index; // 発見した文字列の開始位置
        string value = m.Value; // 発見した文字列
        var patterns3 = new List<string>();
        patterns3.Add(Regex.Replace(value, "(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", ""));
        // 漢数字
        if (Regex.IsMatch(value, "^壱"))
        {
            patterns3.Add("一");
            patterns3.Add("1");
            patterns3.Add("１");
        }
        else
        {
            string num1 = Regex.Replace(value, "([一二三四五六七八九十]+)", match =>
            (
               Utils.Kan2Num(match.Value.ToString())
            ));
            var num2 = Regex.Replace(num1, "(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", "");
            patterns3.Add(num2); // 半角アラビア数字
        }
        string _pattern = $"({string.Join("|", patterns3)})((丁|町)目?|番(町|丁)|条|軒|線|の町?|地割|号|[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])";
        return _pattern;
    }



















































    //private static List<string[]> cachedSameNamedPrefectureCityRegexPatterns;

    public static Dictionary<string, string> GetSameNamedPrefectureCityRegexPatterns(List<string> prefs, Dictionary<string, List<string>> prefList)
    {
        if (cachedSameNamedPrefectureCityRegexPatterns is not null && cachedSameNamedPrefectureCityRegexPatterns.Any())
        {
            return cachedSameNamedPrefectureCityRegexPatterns;
        }

        List<string> _prefs = prefs.ConvertAll(pref =>
        {
            return Regex.Replace(pref, "[都|道|府|県]$", "");
        });

        cachedSameNamedPrefectureCityRegexPatterns = new();
        foreach (var pref in prefList)
        {
            foreach (var city in pref.Value)
            {
                for (int j = 0; j < _prefs.Count; j++)
                {
                    if (city.IndexOf(_prefs[j]) == 0)
                    {
                        cachedSameNamedPrefectureCityRegexPatterns.Add($"{pref.Key}{city}", $"^{city}");
                    }
                }
            }
        }

        return cachedSameNamedPrefectureCityRegexPatterns;
    }
}

//public class TownList
//{
//    // Define your TownList class properties here
//}


//public class GaikuListItem
//{
//    // Define properties of GaikuListItem class
//}

public class Residential
{
    public string gaiku { get; set; }
    public string jyukyo { get; set; }
}

//public class ResidentialList : List<Residential> { }

//public class Config
//{
//    public int interfaceVersion { get; set; }
//}

public class ComparerA : IComparer<SingleTown>
{
    public int Compare(SingleTown a, SingleTown b)
    {
        var aLen = a.town.Length;
        var bLen = b.town.Length;
        if (a.town.StartsWith("大字")) aLen -= 2;
        if (b.town.StartsWith("大字")) bLen -= 2;
        if (bLen > aLen)　//town名が長いのを優先
            return 1;
        else if (aLen == bLen)
            return 0;
        else
            return -1;
    }
}
