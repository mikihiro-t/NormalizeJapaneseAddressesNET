namespace JapaneseNumeralNET;

public static class Dicts
{
    public static readonly Dictionary<string, string> japaneseNumerics = new();
    public static readonly Dictionary<string, string> oldJapaneseNumerics = new();

    static Dicts()
    {
        japaneseNumerics.Add("〇", "0");
        japaneseNumerics.Add("一", "1");
        japaneseNumerics.Add("二", "2");
        japaneseNumerics.Add("三", "3");
        japaneseNumerics.Add("四", "4");
        japaneseNumerics.Add("五", "5");
        japaneseNumerics.Add("六", "6");
        japaneseNumerics.Add("七", "7");
        japaneseNumerics.Add("八", "8");
        japaneseNumerics.Add("九", "9");
        japaneseNumerics.Add("０", "0");
        japaneseNumerics.Add("１", "1");
        japaneseNumerics.Add("２", "2");
        japaneseNumerics.Add("３", "3");
        japaneseNumerics.Add("４", "4");
        japaneseNumerics.Add("５", "5");
        japaneseNumerics.Add("６", "6");
        japaneseNumerics.Add("７", "7");
        japaneseNumerics.Add("８", "8");
        japaneseNumerics.Add("９", "9");

        oldJapaneseNumerics.Add("零", "〇");
        oldJapaneseNumerics.Add("壱", "一");
        oldJapaneseNumerics.Add("壹", "一");
        oldJapaneseNumerics.Add("弐", "二");
        oldJapaneseNumerics.Add("弍", "二");
        oldJapaneseNumerics.Add("貳", "二");
        oldJapaneseNumerics.Add("貮", "二");
        oldJapaneseNumerics.Add("参", "三");
        oldJapaneseNumerics.Add("參", "三");
        oldJapaneseNumerics.Add("肆", "四");
        oldJapaneseNumerics.Add("伍", "五");
        oldJapaneseNumerics.Add("陸", "六");
        oldJapaneseNumerics.Add("漆", "七");
        oldJapaneseNumerics.Add("捌", "八");
        oldJapaneseNumerics.Add("玖", "九");
        oldJapaneseNumerics.Add("拾", "十");
        oldJapaneseNumerics.Add("廿", "二十");
        oldJapaneseNumerics.Add("陌", "百");
        oldJapaneseNumerics.Add("佰", "百");
        oldJapaneseNumerics.Add("阡", "千");
        oldJapaneseNumerics.Add("仟", "千");
        oldJapaneseNumerics.Add("萬", "万");
    }
}
