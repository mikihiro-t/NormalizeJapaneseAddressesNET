using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NormalizeJapaneseAddresses.lib;

internal class zen2han
{
    public static class Zen2HanConverter
    {
        public static string Zen2Han(string str)
        {
            return System.Text.RegularExpressions.Regex.Replace(str, @"[Ａ-Ｚａ-ｚ０-９]", s =>
            {
                return ((char)(s.Value[0] - 0xfee0)).ToString();
            });
        }
    }
}
