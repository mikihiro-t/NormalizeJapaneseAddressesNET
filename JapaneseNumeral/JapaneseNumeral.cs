global using System;
global using System.Collections.Generic;
global using System.Text;
global using System.Text.RegularExpressions;

namespace JapaneseNumeralNET;

public static class JapaneseNumeral
{
    public static long Kanji2Number(string japanese)
    {
        japanese = Utils.Normalize(japanese);
        if (Regex.IsMatch(japanese, "〇") || Regex.IsMatch(japanese, "^[〇一二三四五六七八九]+$"))
        {
            foreach (var key in Dicts.japaneseNumerics.Keys)
            {
                var reg = new Regex(key);
                japanese = reg.Replace(japanese, Dicts.japaneseNumerics[key].ToString());
            }
            return Convert.ToInt32(japanese);
        }
        else
        {
            long number = 0;
            var numbers = Utils.SplitLargeNumber(japanese);
            foreach (var key in Utils.largeNumbers.Keys)
            {
                if (numbers.ContainsKey(key))
                {
                    var n = Utils.largeNumbers[key] * numbers[key];
                    number = number + n;
                }
            }
            if (!long.TryParse(numbers["千"].ToString(), out long temp))
            //if (!Number.IsInteger(number) || !Number.IsInteger(numbers["千"]))
            {
                throw new Exception("The attribute of kanji2number() must be a Japanese numeral as integer.");
            }
            return number + numbers["千"];
        }
    }

    public static string Number2kanji(long num)
    {
        if (!Regex.IsMatch(num.ToString(), "^[0-9]+$"))
        {
            throw new Exception("The attribute of number2kanji() must be integer.");
        }
        var kanjiNumbers = Dicts.japaneseNumerics.Keys;
        long number = num;
        var kanji = "";
        foreach (var key in Utils.largeNumbers.Keys)
        {
            int n = (int)(Math.Floor((double)number / Utils.largeNumbers[key]));
            if (n != 0)
            {
                number = number - (n * Utils.largeNumbers[key]);
                kanji = kanji + Utils.N2Kan(n) + key;
            }
        }
        if (number != 0)
        {
            kanji = kanji + Utils.N2Kan(number);
        }
        return string.IsNullOrEmpty(kanji) ? "〇" : kanji;
    }


    private static readonly List<string> level1String = new() { "兆", "億", "万|萬" };
    private static readonly List<string> level2String = new() { "千|阡|仟", "百|陌|佰", "十|拾" };
    private static readonly string numPattern1 = "[0-9０-９]";
    private static readonly string numPattern2 = "[〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]";
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    /// <remarks>移植にあたり、.NET用の正規表現を構築できなかったため、一文字ずつ確認する処理にした。</remarks>
    public static List<string> FindKanjiNumbers(string text)
    {
        var matchTemp = new StringBuilder();
        //int level1Index = 0;
        //int level2Index = 0;
        var matchesList = new List<string>();
        var textLength = text.Length;
        bool isNumPatternStarted1 = false;　//文字がisNumPattern1に一致した時に、True
        bool isNumPatternStarted2 = false;

        for (int i = 0; i < textLength; i++)
        {
            bool isLevel1 = false;
            bool isLevel2 = false;
            bool isNumPattern1 = false;
            bool isNumPattern2 = false;

            var t = text[i].ToString();

            for (int j = 0; j < level1String.Count; j++)
            {
                if (Regex.IsMatch(t, level1String[j]))
                {
                    //level1Index = j; //兆、億などの文字があった場合
                    matchTemp.Append(t);
                    isLevel1 = true;
                    break;
                }
            }

            if (isLevel1 && i != textLength - 1) continue;

            for (int j = 0; j < level2String.Count; j++)
            {
                if (Regex.IsMatch(t, level2String[j]))
                {
                    //level2Index = j; //兆、億などの文字があった場合
                    matchTemp.Append(t);
                    isLevel2 = true;
                    break;
                }
            }

            if (isLevel2 && i != textLength - 1) continue;

            if (Regex.IsMatch(t, numPattern1))
            {
                if (isNumPatternStarted2)
                {
                    matchesList.Add(matchTemp.ToString());
                    matchTemp.Clear();
                }
                matchTemp.Append(t);
                isNumPattern1 = true;
                isNumPatternStarted1 = true;
            }

            if (Regex.IsMatch(t, numPattern2))
            {
                if (isNumPatternStarted1)
                {
                    matchesList.Add(matchTemp.ToString());
                    matchTemp.Clear();
                }
                matchTemp.Append(t);
                isNumPattern2 = true;
                isNumPatternStarted2 = true;
            }

            if (isNumPattern1 && i != textLength - 1)  //最後の文字がnumPattern1に一致した場合は、continueせず、mathcslistにAddする必要がある。
                continue;

            if (isNumPattern2 && i != textLength - 1)  //最後の文字がnumPattern2に一致した場合は、continueせず、mathcslistにAddする必要がある。
                continue;

            matchesList.Add(matchTemp.ToString());
            matchTemp.Clear();

            isNumPatternStarted1 = false;
            isNumPatternStarted2 = false;
        }

        if (matchesList.Count > 0)
        {
            List<string> result = new ();
            foreach (var item in matchesList)
            {
                if ((!Regex.IsMatch(item, "^[0-9０-９]+$")) && (item.Length > 0 && item != "兆" && item != "億" && item != "万" && item != "萬"))
                {
                    result.Add(item);
                }
            }
            return result;
        }
        else
        {
            return new List<string>();
        }

        ////正規表現の解釈が、JavaScriptと.NETと異なる。そのため、このnumの正規表現では、JavaScriptと結果が異なる。
        //string num = "([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*)";
        ////string num = "([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+)"; //*を+に変更した場合
        //string basePattern = $"(({num})(千|阡|仟))?(({num})(百|陌|佰))?(({num})(十|拾))?({num})?";
        //string pattern = $"(({basePattern}兆)?({basePattern}億)?({basePattern}(万|萬))?{basePattern})";

        ////patternは、次の文字列になる。JavaScriptのコードと同じ
        ////((((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(千|阡|仟))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(百|陌|佰))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(十|拾))?(([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))?兆)?(((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(千|阡|仟))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(百|陌|佰))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(十|拾))?(([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))?億)?(((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(千|阡|仟))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(百|陌|佰))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(十|拾))?(([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))?(万|萬))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(千|阡|仟))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(百|陌|佰))?((([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))(十|拾))?(([0-9０-９]*)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]*))?)
        ////
        ////*を+に変更した場合でも、「今日は二〇二〇年十一月二十日です。」　で、「十一」にマッチしない。
        ////((((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(千|阡|仟))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(百|陌|佰))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(十|拾))?(([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))?兆)?(((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(千|阡|仟))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(百|陌|佰))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(十|拾))?(([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))?億)?(((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(千|阡|仟))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(百|陌|佰))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(十|拾))?(([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))?(万|萬))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(千|阡|仟))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(百|陌|佰))?((([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))(十|拾))?(([0-9０-９]+)|([〇一二三四五六七八九壱壹弐弍貳貮参參肆伍陸漆捌玖]+))?)
        //Regex regex = new Regex(pattern);
        //MatchCollection match = regex.Matches(text);
        //Console.WriteLine(match.Cast<Match>().Select(match => match.Value).ToList());
        //if (match.Count > 0)
        //{
        //    List<string> result = new List<string>();
        //    foreach (Match item in match)
        //    {
        //        if ((!Regex.IsMatch(item.Value, "^[0-9０-９]+$")) && (item.Value.Length > 0 && item.Value != "兆" && item.Value != "億" && item.Value != "万" && item.Value != "萬"))
        //        {
        //            result.Add(item.Value);
        //        }
        //    }
        //    return result;
        //}
        //else
        //{
        //    return new List<string>();
        //}
    }
}