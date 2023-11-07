namespace NormalizeJapaneseAddressesNET;

public static class Configs
{
    public readonly static string gh_pages_endpoint = "https://geolonia.github.io/japanese-addresses/api/ja";
    public static Config CurrentConfig { get; set; } = new()
    {
        InterfaceVersion = 2,
        JapaneseAddressesApi = gh_pages_endpoint,
        TownCacheSize = 1000,
    };
}
