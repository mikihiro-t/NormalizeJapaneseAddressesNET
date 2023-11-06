using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JapaneseNumeral;

namespace NormalizeJapaneseAddresses.lib;

public static class Utils
{
    public static string Zen2Han(string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(str, @"[Ａ-Ｚａ-ｚ０-９]", s =>
        {
            return ((char)(s.Value[0] - 0xfee0)).ToString();
        });
    }

    public static string Kan2Num(string input)
    {
        var kanjiNumbers = JapaneseNumeral.JapaneseNumeral.FindKanjiNumbers(input);
        for (int i = 0; i < kanjiNumbers.Count; i++)
        {
            try
            {
                input = input.Replace(kanjiNumbers[i], JapaneseNumeral.JapaneseNumeral.Kanji2Number(kanjiNumbers[i]).ToString());  //TODO longをToStringに変換でよいか？
            }
            catch (Exception)
            {
                // ignore
            }
        }
        return input;
    }
}
