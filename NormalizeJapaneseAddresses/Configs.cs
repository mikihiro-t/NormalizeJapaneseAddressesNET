using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NormalizeJapaneseAddresses;

public static class Configs
{
    public readonly static string gh_pages_endpoint = "https://geolonia.github.io/japanese-addresses/api/ja";
    public static Config CurrentConfig { get; set; } = new()
    {
        interfaceVersion = 2,
        japaneseAddressesApi = gh_pages_endpoint,
        townCacheSize = 1000,
    };
}
