namespace JapaneseNumeral;

public static class Utils
{
    internal static readonly Dictionary<string, long> largeNumbers = new() { { "兆", 1000000000000 }, { "億", 100000000 }, { "万", 10000 } };
    internal static readonly Dictionary<string, int> smallNumbers = new() { { "千", 1000 }, { "百", 100 }, { "十", 10 } };

    public static string Normalize(string japanese)
    {
        foreach (var key in Dicts.oldJapaneseNumerics.Keys)
        {
            var reg = new Regex(key);
            japanese = reg.Replace(japanese, Dicts.oldJapaneseNumerics[key]);
        }
        return japanese;
    }
    /// <summary>
    /// 漢数字を兆、億、万単位に分割する
    /// </summary>
    /// <param name="japanese"></param>
    /// <returns></returns>
    public static Dictionary<string, int> SplitLargeNumber(string japanese)
    {
        string kanji = japanese;
        var numbers = new Dictionary<string, int>();
        foreach (var key in largeNumbers.Keys)
        {
            var reg = new Regex($@"(.+){key}");
            var match = reg.Match(kanji);
            if (match.Success)
            {
                numbers[key] = Kan2N(match.Groups[1].Value);
                kanji = reg.Replace(kanji, "");
            }
            else
            {
                numbers[key] = 0;
            }
        }
        if (!string.IsNullOrEmpty(kanji))
        {
            numbers["千"] = Kan2N(kanji);
        }
        else
        {
            numbers["千"] = 0;
        }
        return numbers;
    }
    /// <summary>
    /// 千単位以下の漢数字を数字に変換する（例: 三千 => 3000）
    /// </summary>
    /// <param name="japanese"></param>
    /// <returns></returns>
    public static int Kan2N(string japanese)
    {
        if (System.Text.RegularExpressions.Regex.IsMatch(japanese, "^[0-9]+$"))
        {
            return int.Parse(japanese);
        }
        string kanji = Zen2Han(japanese);
        int number = 0;
        foreach (var key in smallNumbers.Keys)
        {
            var reg = new System.Text.RegularExpressions.Regex($"(.*){key}");
            var match = reg.Match(kanji);
            if (match.Success)
            {
                int n = 1;
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    if (System.Text.RegularExpressions.Regex.IsMatch(match.Groups[1].Value, "^[0-9]+$"))
                    {
                        n = int.Parse(match.Groups[1].Value);
                    }
                    else
                    {
                        n = int.Parse(Dicts.japaneseNumerics[match.Groups[1].Value]);
                    }
                }
                number += n * smallNumbers[key];
                kanji = kanji.Replace(match.Value, "");
            }
        }
        if (!string.IsNullOrEmpty(kanji))
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(kanji, "^[0-9]+$"))
            {
                number += int.Parse(kanji);
            }
            else
            {
                for (int index = 0; index < kanji.Length; index++)
                {
                    var character = kanji[index];
                    var digit = kanji.Length - index - 1;
                    number += int.Parse(Dicts.japaneseNumerics[character.ToString()]) * (int)Math.Pow(10, digit);
                }
            }
        }
        return number;
    }
    /// <summary>
    /// Converts number less than 10000 to kanji.
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static string N2Kan(long num)
    {
        var kanjiNumbers = new List<string>(Dicts.japaneseNumerics.Keys);
        long number = num;
        string kanji = "";
        foreach (var key in smallNumbers.Keys)
        {
            int n = (int)(number / smallNumbers[key]);
            if (n > 0)
            {
                number -= n * smallNumbers[key];
                if (n == 1)
                {
                    kanji += key;
                }
                else
                {
                    kanji += kanjiNumbers[n] + key;
                }
            }
        }
        if (number > 0)
        {
            kanji += kanjiNumbers[(int)number];
        }
        return kanji;
    }
    /// <summary>
    /// Converts double-width number to number as string.
    /// </summary>
    /// <param name="japanese"></param>
    /// <returns></returns>
    /// <remarks>移植にあたって、正規表現は利用しなかった。</remarks>
    private static string Zen2Han(string japanese)
    {
        return japanese.
            Replace('０', '0').
            Replace('１', '1').
            Replace('２', '2').
            Replace('３', '3').
            Replace('４', '4').
            Replace('５', '5').
            Replace('６', '6').
            Replace('７', '7').
            Replace('８', '8').
            Replace('９', '9');
    }
}