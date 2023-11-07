namespace JapaneseNumeralNET.Tests;

public class JapaneseNumeralTest
{
    [Theory]
    [InlineData("〇", 0)]
    [InlineData("零", 0)]
    [InlineData("一千百十一兆一千百十一億一千百十一万一千百十一", 1111111111111111)]
    [InlineData("一千百十一兆一千百十一億一千百十一万", 1111111111110000)]
    [InlineData("一千百十一兆一千百十一億一千百十一", 1111111100001111)]
    [InlineData("百十一", 111)]
    [InlineData("三億八", 300000008)]
    [InlineData("三百八", 308)]
    [InlineData("三五〇", 350)]
    [InlineData("三〇八", 308)]
    [InlineData("二〇二〇", 2020)]
    [InlineData("十", 10)]
    [InlineData("二千", 2000)]
    [InlineData("壱万", 10000)]
    [InlineData("弍万", 20000)]
    [InlineData("一二三四", 1234)]
    [InlineData("千二三四", 1234)]
    [InlineData("千二百三四", 1234)]
    [InlineData("千二百三十四", 1234)]
    [InlineData("壱阡陌拾壱兆壱阡陌拾壱億壱阡陌拾壱萬壱阡陌拾壱", 1111111111111111)]
    [InlineData("壱仟佰拾壱兆壱仟佰拾壱億壱仟佰拾壱萬壱仟佰拾壱", 1111111111111111)]
    [InlineData("100万", 1000000)]
    [InlineData("5百", 500)]
    [InlineData("7十", 70)]
    [InlineData("4千８百", 4800)]
    [InlineData("4千８百万", 48000000)]
    [InlineData("3億4千８百万", 348000000)]
    [InlineData("3億4千８百万6", 348000006)]
    [InlineData("2百億", 20000000000)]
    [InlineData("4千8百21", 4821)]
    [InlineData("1千2百35億8百21", 123500000821)]
    [InlineData("2億3千430万", 234300000)]
    [InlineData("２億３千４５６万７８９０", 234567890)]
    [InlineData("１２３", 123)]
    public void Kanji2NumberTest(string kanji, long number)
    {
        var result = JapaneseNumeral.Kanji2Number(kanji);
        Assert.Equal(number, result);

    }

    [Theory]
    [InlineData(0, "〇")]
    [InlineData(1110, "千百十")]
    [InlineData(1111111111111111, "千百十一兆千百十一億千百十一万千百十一")]
    [InlineData(1111113111111111, "千百十一兆千百三十一億千百十一万千百十一")]
    [InlineData(1000000000000000, "千兆")]
    [InlineData(1200000, "百二十万")]
    [InlineData(18, "十八")]
    [InlineData(100100000, "一億十万")]

    public void Number2kanjiTest(long number, string kanji)
    {
        var result = JapaneseNumeral.Number2kanji(number);
        Assert.Equal(kanji, result);
    }

    [Theory]
    [InlineData(-1)]
    public void Number2kanjiExceptionTest(long number)
    {
        Action act = () => JapaneseNumeral.Number2kanji(number);
        var ex = Record.Exception(act);
        Assert.NotNull(ex);
        //Assert.IsType<InvalidOperationException>(ex);
    }


    //3個までの漢数字のテストを行う。
    [Theory]
    [InlineData("今日は二千二十年十一月二十日です。", "二千二十", "十一", "二十")]
    [InlineData("今日は二〇二〇年十一月二十日です。", "二〇二〇", "十一", "二十")]
    [InlineData("わたしは二千二十億円もっています。", "二千二十億", null, null)]
    [InlineData("わたしは二〇二〇億円もっています。", "二〇二〇億", null, null)]
    [InlineData("今日のランチは八百六十三円でした。", "八百六十三", null, null)]
    [InlineData("今日のランチは八六三円でした。", "八六三", null, null)]
    [InlineData("今月のお小遣いは三千円です。", "三千", null, null)]
    [InlineData("青森県五所川原市金木町喜良市千苅６２−８", "五", "千", null)]
    [InlineData("わたしは1億2000万円もっています。", "1億2000万", null, null)]
    [InlineData("香川県仲多度郡まんのう町勝浦字家六２０９４番地１", "六", null, null)]
    [InlineData("兆１００億２千万", "兆１００億２千万", null, null)]
    [InlineData("家２０９４六番地１", "六", null, null)]   //(7) ['', '２０９４', '六', '', '', '１', '']  //数字と漢数字とは分離
    [InlineData("今日は２千20年十一月二十日です。", "２千20", "十一", "二十")]
    [InlineData("栗沢町万字寿町", null, null, null)]
    [InlineData("私は億ションに住んでいます", null, null, null)]
    [InlineData("私が住んでいるのは壱番館の弐号室です。", "壱", "弐", null)]
    [InlineData("私は、ハイツ弍号棟に住んでいます。", "弍", null, null)]
    [InlineData("私は、壱阡陌拾壱兆壱億壱萬円持っています。", "壱阡陌拾壱兆壱億壱萬", null, null)]
    [InlineData("私は、壱仟佰拾壱兆壱億壱萬円持っています。", "壱仟佰拾壱兆壱億壱萬", null, null)]
    public void FindKanjiNumbersTest(string text, string result1, string result2, string result3)
    {
        var result = JapaneseNumeral.FindKanjiNumbers(text);
        List<string> expected = new();
        if (result1 is not null) expected.Add(result1);
        if (result2 is not null) expected.Add(result2);
        if (result3 is not null) expected.Add(result3);
        Assert.Equivalent(expected, result, strict: true);
    }

    [Theory]
    [InlineData(1111, "千百十一")]
    [InlineData(3111, "三千百十一")]
    [InlineData(1000, "千")]
    [InlineData(5, "五")]
    public void N2KanTest(long number, string kanji)
    {
        var result = Utils.N2Kan(number);
        Assert.Equal(kanji, result);
    }

    [Theory]
    [InlineData("三千", 3000)]
    [InlineData("22", 22)]
    [InlineData("１２３", 123)]
    public void Kan2NTest(string kanji, long number)
    {
        var result = Utils.Kan2N(kanji);
        Assert.Equal(number, result);
    }
}


//JavaScript
//https://developer.mozilla.org/ja/docs/Web/JavaScript/Reference/Global_Objects/Number
//const biggestInt = Number.MAX_SAFE_INTEGER; // (2**53 - 1) => 9007199254740991
//const smallestInt = Number.MIN_SAFE_INTEGER; // -(2**53 - 1) => -9007199254740991

//移植においては、numberの型を、longとして処理した。
