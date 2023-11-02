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

namespace NormalizeJapaneseAddresses.lib;

public class PrefectureList : Dictionary<string, List<string>> { }

public class SingleTown
{
    public string town { get; set; }
    public string originalTown { get; set; }
    public string koaza { get; set; }
    public string lat { get; set; }
    public string lng { get; set; }
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

public static class Program
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
        var prefsResp = await __internals.fetch(".json", new { level = 1 });
        var data = await prefsResp.json();
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
        if (cachedPrefecturePatterns.Any())
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
            string pattern = $"^{ToRegexPattern(city)}";
            if (Regex.IsMatch(city, "(町|村)$"))
            {
                pattern = $"^{ToRegexPattern(city).Replace("(.+?)郡", "($1郡)?")}";
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
        HttpClient client = new HttpClient();
        string url = $"/{Uri.EscapeUriString(pref)}/{Uri.EscapeUriString(city)}.json";
        HttpResponseMessage response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        TownList towns = await response.Content.ReadAsAsync<TownList>();
        cachedTowns[cacheKey] = towns;
        return towns;
    }

    private static string ToRegexPattern(string input)
    {
        return System.Text.RegularExpressions.Regex.Escape(input);
    }


    //private static Dictionary<string, List<GaikuListItem>> cachedGaikuListItem = new Dictionary<string, List<GaikuListItem>>();

    public static async Task<List<GaikuListItem>> GetGaikuList(string pref, string city, string town)
    {
        if (currentConfig.interfaceVersion > 1)
        {
            throw new Exception($"Invalid config.interfaceVersion: {currentConfig.interfaceVersion}'. Please set config.interfaceVersion to 1.");
        }

        string cacheKey = $"{pref}-{city}-{town}-v{currentConfig.interfaceVersion}";
        if (cachedGaikuListItem.ContainsKey(cacheKey))
        {
            return cachedGaikuListItem[cacheKey];
        }

        string url = $"/{encodeURI(pref)}/{encodeURI(city)}/{encodeURI(town + ".json")}";
        HttpClient client = new HttpClient();
        HttpResponseMessage gaikuResp = await client.GetAsync(url);
        List<GaikuListItem> gaikuListItem;
        try
        {
            gaikuListItem = await gaikuResp.Content.ReadAsAsync<List<GaikuListItem>>();
        }
        catch
        {
            gaikuListItem = new List<GaikuListItem>();
        }

        cachedGaikuListItem[cacheKey] = gaikuListItem;
        return gaikuListItem;
    }










    //private static Dictionary<string, ResidentialList> cachedResidentials = new Dictionary<string, ResidentialList>();
    private static Config currentConfig = new Config();

    public static async Task<ResidentialList> GetResidentials(string pref, string city, string town)
    {
        if (currentConfig.interfaceVersion > 1)
        {
            throw new Exception($"Invalid config.interfaceVersion: {currentConfig.interfaceVersion}'. Please set config.interfaceVersion to 1.");
        }

        string cacheKey = $"{pref}-{city}-{town}-v{currentConfig.interfaceVersion}";
        if (cachedResidentials.ContainsKey(cacheKey))
        {
            return cachedResidentials[cacheKey];
        }

        string cache = "";
        if (cache != "")
        {
            return cachedResidentials[cacheKey];
        }

        string url = $"/{encodeURI(pref)}/{encodeURI(city)}/{encodeURI(town)}/{encodeURI("住居表示.json")}";
        var residentialsResp = await __internals.fetch(url);

        ResidentialList residentials;
        try
        {
            residentials = await residentialsResp.json() as ResidentialList;
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
        if (CurrentConfig.InterfaceVersion < 2)
        {
            throw new Exception($"Invalid config.interfaceVersion: {CurrentConfig.InterfaceVersion}'. Please set config.interfaceVersion to 2 or higher");
        }
        string cacheKey = $"{pref}-{city}-{town}-v{CurrentConfig.InterfaceVersion}";
        if (CachedAddrs.ContainsKey(cacheKey))
        {
            return CachedAddrs[cacheKey];
        }
        HttpResponseMessage cache = CachedAddrs[cacheKey];
        if (cache != null)
        {
            return cache;
        }
        string url = $"/{encodeURI(pref)}/{encodeURI(city)}/{encodeURI(town)}.json";
        Dictionary<string, string> parameters = new Dictionary<string, string>
        {
            { "level", "8" },
            { "pref", pref },
            { "city", city },
            { "town", town }
        };
        HttpResponseMessage addrsResp = await Internals.Fetch(url, parameters);
        AddrList addrs;
        try
        {
            addrs = await addrsResp.Content.ReadAsAsync<List<Addr>>();
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
        var towns = new List<SingleTown>();//new List<Town>();
        var isKyoto = Regex.IsMatch(city, @"^京都市");

        foreach (var town in preTowns)
        {
            towns.Add(town);
            var originalTown = town.town;
            if (originalTown.IndexOf("町") == -1) continue;
            var townAbbr = Regex.Replace(originalTown, @"(?!^町)町", "");
            if (!isKyoto &&
                !townSet.Contains(townAbbr) &&
                !townSet.Contains($"大字{townAbbr}") &&
                !IsKanjiNumberFollowedByCho(originalTown))
            {
                // エイリアスとして町なしのパターンを登録
                //TODO オリジナルのTSのコードの結果と一致するか確認せよ
                towns.Add(new SingleTown
                {
                    originalTown = originalTown,
                    town = townAbbr
                });
            }
        }




        towns.Sort((a, b) =>
          {
              var aLen = a.town.Length;
              var bLen = b.town.Length;
              if (a.town.StartsWith("大字")) aLen -= 2;
              if (b.town.StartsWith("大字")) bLen -= 2;
              return bLen - aLen;
          });

        //https://stackoverflow.com/questions/31326451/replacing-regex-matches-using-lambda-expression

        List<(SingleTown, string)> patterns = new();
        foreach (var town in towns)
        {
            var output1 = Regex.Replace(town.town, "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]", "[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━]");
            var output2 = Regex.Replace(output1, "大?字", "(大?字)?");
            //var output3 = Regex.Replace(output2, "([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", match => (match.Value[0]).ToString());
            MatchCollection results = Regex.Matches(output2, "([壱一二三四五六七八九十]+)(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)");
            if (results is null || results.Count == 0)
            {
                patterns.Add((town, output2));
            }
            else
            {
                foreach (Match m in results.Cast<Match>())
                {
                    int index = m.Index; // 発見した文字列の開始位置
                    string value = m.Value; // 発見した文字列
                    var patterns3 = new List<string>();
                    patterns3.Add(m.Value.ToString().Replace("(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", ""));
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
                           Kan2Num.Convert(match.Value[0].ToString())
                        ));
                        var num2 = num1.Replace("(丁目?|番(町|丁)|条|軒|線|(の|ノ)町|地割|号)", "");
                        patterns3.Add(num2);  // 半角
                    }
                    string _pattern = $"({string.Join("|", patterns3)})((丁|町)目?|番(町|丁)|条|軒|線|の町?|地割|号|[-－﹣−‐⁃‑‒–—﹘―⎯⏤ーｰ─━])";
                    patterns.Add((town, _pattern));
                }
            }
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
            string pattern = ToRegexPattern($"^{chomeNamePart}({chomeNum}|{Kan2Num.Convert(chomeNum)})");
            patterns.Add((town, pattern));
        }











        //var patterns = string.Join(", ", towns.Select(town => town.town));
        cachedTownRegexes[$"{pref}-{city}"] = patterns;
        return patterns;
    }

    //private static async Task<List<SingleTown>> GetTowns(string pref, string city)
    //{
    //    // Implement the logic to get towns based on pref and city
    //    throw new NotImplementedException();
    //}

    //private static bool IsKanjiNumberFollowedByCho(string town)
    //{
    //    // Implement the logic to check if the town is a kanji number followed by "町"
    //    throw new NotImplementedException();
    //}

    //private class Town
    //{
    //    public string originalTown { get; set; }
    //    public string town { get; set; }
    //}


























































    //private static List<string[]> cachedSameNamedPrefectureCityRegexPatterns;

    public static Dictionary<string, string> GetSameNamedPrefectureCityRegexPatterns(List<string> prefs, Dictionary<string, List<string>> prefList)
    {
        if (cachedSameNamedPrefectureCityRegexPatterns != null)
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

public class Config
{
    public int interfaceVersion { get; set; }
}