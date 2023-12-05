namespace NormalizeJapaneseAddressesNET;
public static class Configs
{
    public readonly static string gh_pages_endpoint = "https://geolonia.github.io/japanese-addresses/api/ja";
    /// <summary>
    /// TypeScriptのconstとは機能が異なるが、readonlyとした。ただし、InterfaceVersionなどのPropertyはreadonlyではない。
    /// </summary>
    public readonly static Config CurrentConfig = new()
    {
        InterfaceVersion = 1,
        JapaneseAddressesApi = gh_pages_endpoint,
        TownCacheSize = 1000,
    };
}
