global using NormalizeJapaneseAddressesNET;

var option = new NormalizerOption() { level = 1 };
WriteLog(await NormalizeJapaneseAddresses.Normalize("福井県あわら市市姫3-1-1", option));

var option2 = new NormalizerOption() { level = 2 };
WriteLog(await NormalizeJapaneseAddresses.Normalize("福井県あわら市市姫3-1-1", option2));

var option3 = new NormalizerOption() { level = 3 };
WriteLog(await NormalizeJapaneseAddresses.Normalize("福井県あわら市市姫3-1-1", option3));

WriteLog(await NormalizeJapaneseAddresses.Normalize("神奈川県横浜市青葉区鴨志田町Ａ棟８０２－５　　Ｂ棟８０２－１"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("香川県丸亀市原田町字東三分一１９２６番地１"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("東京都荒川区屋５丁目"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("広島県府中市栗柄町名字八五十2459"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("埼玉県川口市大字新堀町999-888"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("大分県大分市田中町3丁目1-12"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("東京都中野区本町３丁目４－５"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("東京都文京区小石川1ビル名"));
WriteLog(await NormalizeJapaneseAddresses.Normalize("北海道滝川市一の坂町西"));

Console.ReadLine();
static void WriteLog(INormalizeResult result)
{
    Console.WriteLine(@$"pref:{result.pref}, city:{result.city}, town:{result.town}, addr:{result.addr}, lat:{result.lat}, lng:{result.lng}, level:{result.level}");
}