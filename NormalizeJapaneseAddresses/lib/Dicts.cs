﻿namespace NormalizeJapaneseAddressesNET.Lib;
public static class Dicts
{
    private static readonly string[] JIS_OLD_KANJI = { "亞", "圍", "壹", "榮", "驛", "應", "櫻", "假", "會", "懷", "覺", "樂", "陷", "歡", "氣", "戲", "據", "挾", "區", "徑", "溪", "輕", "藝", "儉", "圈", "權", "嚴", "恆", "國", "齋", "雜", "蠶", "殘", "兒", "實", "釋", "從", "縱", "敍", "燒", "條", "剩", "壤", "釀", "眞", "盡", "醉", "髓", "聲", "竊", "淺", "錢", "禪", "爭", "插", "騷", "屬", "對", "滯", "擇", "單", "斷", "癡", "鑄", "敕", "鐵", "傳", "黨", "鬪", "屆", "腦", "廢", "發", "蠻", "拂", "邊", "瓣", "寶", "沒", "滿", "藥", "餘", "樣", "亂", "兩", "禮", "靈", "爐", "灣", "惡", "醫", "飮", "營", "圓", "歐", "奧", "價", "繪", "擴", "學", "罐", "勸", "觀", "歸", "犧", "擧", "狹", "驅", "莖", "經", "繼", "缺", "劍", "檢", "顯", "廣", "鑛", "碎", "劑", "參", "慘", "絲", "辭", "舍", "壽", "澁", "肅", "將", "證", "乘", "疊", "孃", "觸", "寢", "圖", "穗", "樞", "齊", "攝", "戰", "潛", "雙", "莊", "裝", "藏", "續", "體", "臺", "澤", "膽", "彈", "蟲", "廳", "鎭", "點", "燈", "盜", "獨", "貳", "霸", "賣", "髮", "祕", "佛", "變", "辯", "豐", "飜", "默", "與", "譽", "謠", "覽", "獵", "勵", "齡", "勞", "壓", "爲", "隱", "衞", "鹽", "毆", "穩", "畫", "壞", "殼", "嶽", "卷", "關", "顏", "僞", "舊", "峽", "曉", "勳", "惠", "螢", "鷄", "縣", "險", "獻", "驗", "效", "號", "濟", "册", "棧", "贊", "齒", "濕", "寫", "收", "獸", "處", "稱", "奬", "淨", "繩", "讓", "囑", "愼", "粹", "隨", "數", "靜", "專", "踐", "纖", "壯", "搜", "總", "臟", "墮", "帶", "瀧", "擔", "團", "遲", "晝", "聽", "遞", "轉", "當", "稻", "讀", "惱", "拜", "麥", "拔", "濱", "竝", "辨", "舖", "襃", "萬", "譯", "豫", "搖", "來", "龍", "壘", "隸", "戀", "樓", "鰺", "鶯", "蠣", "攪", "竈", "灌", "諫", "頸", "礦", "蘂", "靱", "賤", "壺", "礪", "檮", "濤", "邇", "蠅", "檜", "儘", "藪", "籠", "彌", "麩" };
    private static readonly string[] JIS_NEW_KANJI = { "亜", "囲", "壱", "栄", "駅", "応", "桜", "仮", "会", "懐", "覚", "楽", "陥", "歓", "気", "戯", "拠", "挟", "区", "径", "渓", "軽", "芸", "倹", "圏", "権", "厳", "恒", "国", "斎", "雑", "蚕", "残", "児", "実", "釈", "従", "縦", "叙", "焼", "条", "剰", "壌", "醸", "真", "尽", "酔", "髄", "声", "窃", "浅", "銭", "禅", "争", "挿", "騒", "属", "対", "滞", "択", "単", "断", "痴", "鋳", "勅", "鉄", "伝", "党", "闘", "届", "脳", "廃", "発", "蛮", "払", "辺", "弁", "宝", "没", "満", "薬", "余", "様", "乱", "両", "礼", "霊", "炉", "湾", "悪", "医", "飲", "営", "円", "欧", "奥", "価", "絵", "拡", "学", "缶", "勧", "観", "帰", "犠", "挙", "狭", "駆", "茎", "経", "継", "欠", "剣", "検", "顕", "広", "鉱", "砕", "剤", "参", "惨", "糸", "辞", "舎", "寿", "渋", "粛", "将", "証", "乗", "畳", "嬢", "触", "寝", "図", "穂", "枢", "斉", "摂", "戦", "潜", "双", "荘", "装", "蔵", "続", "体", "台", "沢", "胆", "弾", "虫", "庁", "鎮", "点", "灯", "盗", "独", "弐", "覇", "売", "髪", "秘", "仏", "変", "弁", "豊", "翻", "黙", "与", "誉", "謡", "覧", "猟", "励", "齢", "労", "圧", "為", "隠", "衛", "塩", "殴", "穏", "画", "壊", "殻", "岳", "巻", "関", "顔", "偽", "旧", "峡", "暁", "勲", "恵", "蛍", "鶏", "県", "険", "献", "験", "効", "号", "済", "冊", "桟", "賛", "歯", "湿", "写", "収", "獣", "処", "称", "奨", "浄", "縄", "譲", "嘱", "慎", "粋", "随", "数", "静", "専", "践", "繊", "壮", "捜", "総", "臓", "堕", "帯", "滝", "担", "団", "遅", "昼", "聴", "逓", "転", "当", "稲", "読", "悩", "拝", "麦", "抜", "浜", "並", "弁", "舗", "褒", "万", "訳", "予", "揺", "来", "竜", "塁", "隷", "恋", "楼", "鯵", "鴬", "蛎", "撹", "竃", "潅", "諌", "頚", "砿", "蕊", "靭", "賎", "壷", "砺", "梼", "涛", "迩", "蝿", "桧", "侭", "薮", "篭", "弥", "麸" };

    private static readonly string[][] JIS_KANJI_REGEX_PATTERNS = new string[JIS_OLD_KANJI.Length][];

    static Dicts()
    {
        for (int i = 0; i < JIS_OLD_KANJI.Length; i++)
        {
            string oldKanji = JIS_OLD_KANJI[i];
            string newKanji = JIS_NEW_KANJI[i];
            string pattern = $"{oldKanji}|{newKanji}";
            JIS_KANJI_REGEX_PATTERNS[i] = new string[] { pattern, oldKanji, newKanji };
        }
    }

    public static string JisKanji(string str)
    {
        string _str = str;
        for (int i = 0; i < JIS_KANJI_REGEX_PATTERNS.Length; i++)
        {
            string[] patternInfo = JIS_KANJI_REGEX_PATTERNS[i];
            string pattern = patternInfo[0];
            string oldKanji = patternInfo[1];
            string newKanji = patternInfo[2];
            _str = Regex.Replace(_str, pattern, $"({oldKanji}|{newKanji})");
        }
        return _str;
    }

    public static string ToRegexPattern(string str)
    {
        string _str = str;
        _str = Regex.Replace(_str, "三栄町|四谷三栄町", "(三栄町|四谷三栄町)");
        _str = Regex.Replace(_str, "鬮野川|くじ野川|くじの川", "(鬮野川|くじ野川|くじの川)");
        _str = Regex.Replace(_str, "柿碕町|柿さき町", "(柿碕町|柿さき町)");
        _str = Regex.Replace(_str, "通り|とおり", "(通り|とおり)");
        _str = Regex.Replace(_str, "埠頭|ふ頭", "(埠頭|ふ頭)");
        _str = Regex.Replace(_str, "番町|番丁", "(番町|番丁)");
        _str = Regex.Replace(_str, "大冝|大宜", "(大冝|大宜)");
        _str = Regex.Replace(_str, "穝|さい", "(穝|さい)");
        _str = Regex.Replace(_str, "杁|えぶり", "(杁|えぶり)");
        _str = Regex.Replace(_str, "薭|稗|ひえ|ヒエ", "(薭|稗|ひえ|ヒエ)");
        _str = Regex.Replace(_str, "[之ノの]", "[之ノの]");
        _str = Regex.Replace(_str, "[ヶケが]", "[ヶケが]");
        _str = Regex.Replace(_str, "[ヵカか力]", "[ヵカか力]");
        _str = Regex.Replace(_str, "[ッツっつ]", "[ッツっつ]");
        _str = Regex.Replace(_str, "[ニ二]", "[ニ二]");
        _str = Regex.Replace(_str, "[ハ八]", "[ハ八]");
        _str = Regex.Replace(_str, "塚|塚", "(塚|塚)");
        _str = Regex.Replace(_str, "釜|竈", "(釜|竈)");
        _str = Regex.Replace(_str, "條|条", "(條|条)");
        _str = Regex.Replace(_str, "狛|拍", "(狛|拍)");
        _str = Regex.Replace(_str, "藪|薮", "(藪|薮)");
        _str = Regex.Replace(_str, "渕|淵", "(渕|淵)");
        _str = Regex.Replace(_str, "エ|ヱ|え", "(エ|ヱ|え)");
        _str = Regex.Replace(_str, "曾|曽", "(曾|曽)");
        _str = Regex.Replace(_str, "舟|船", "(舟|船)");
        _str = Regex.Replace(_str, "莵|菟", "(莵|菟)");
        _str = Regex.Replace(_str, "市|巿", "(市|巿)");
        _str = JisKanji(_str);
        return _str;
    }
}
