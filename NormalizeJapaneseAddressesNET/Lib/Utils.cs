using JapaneseNumeralNET;

namespace NormalizeJapaneseAddressesNET.Lib;

public static class Utils
{
    public static string Zen2Han(string str)
    {
        return Regex.Replace(str, @"[Ａ-Ｚａ-ｚ０-９]", s =>
        {
            return ((char)(s.Value[0] - 0xfee0)).ToString();
        });
    }

    public static string Kan2Num(string input)
    {
        var kanjiNumbers = JapaneseNumeral.FindKanjiNumbers(input);
        for (int i = 0; i < kanjiNumbers.Count; i++)
        {
            try
            {
                input = input.Replace(kanjiNumbers[i], JapaneseNumeral.Kanji2Number(kanjiNumbers[i]).ToString());
            }
            catch (Exception)
            {
                // ignore
            }
        }
        return input;
    }
}
