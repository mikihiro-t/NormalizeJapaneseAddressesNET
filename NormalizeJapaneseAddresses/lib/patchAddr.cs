using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NormalizeJapaneseAddresses.lib;


public static class patchAddr
{
    private class AddrPatch
    {
        public string Pref { get; set; }
        public string City { get; set; }
        public string Town { get; set; }
        public string Pattern { get; set; }
        public string Result { get; set; }
    }

    private static List<AddrPatch> addrPatches = new List<AddrPatch>
        {
            new AddrPatch
            {
                Pref = "香川県",
                City = "仲多度郡まんのう町",
                Town = "勝浦",
                Pattern = "^字?家6",
                Result = "家六"
            },
            new AddrPatch
            {
                Pref = "愛知県",
                City = "あま市",
                Town = "西今宿",
                Pattern = "^字?梶村1",
                Result = "梶村一"
            },
            new AddrPatch
            {
                Pref = "香川県",
                City = "丸亀市",
                Town = "原田町",
                Pattern = "^字?東三分1",
                Result = "東三分一"
            }
        };

    public static string PatchAddr(string pref, string city, string town, string addr)
    {
        string _addr = addr;
        foreach (var patch in addrPatches)
        {
            if (patch.Pref == pref && patch.City == city && patch.Town == town)
            {
                _addr = _addr.Replace(patch.Pattern, patch.Result);
            }
        }
        return _addr;
    }
}

