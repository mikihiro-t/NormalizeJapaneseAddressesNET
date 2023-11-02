using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JapaneseNumeral;

namespace NormalizeJapaneseAddresses.lib;

public static class Kan2Num
{
    public static string Convert(string input)
    {
        var kanjiNumbers = JapaneseNumeral.JapaneseNumeral.FindKanjiNumbers(input);
        for (int i = 0; i < kanjiNumbers.Length; i++)
        {
            try
            {
                input = input.Replace(kanjiNumbers[i], JapaneseNumeral.JapaneseNumeral.Kanji2Number(kanjiNumbers[i]));
            }
            catch (Exception)
            {
                // ignore
            }
        }
        return input;
    }
}