# NormalizeJapaneseAddressesNET

住所正規化ライブラリを、C#で.NETに移植したものです。

- [normalize-japanese-addresses](https://github.com/geolonia/normalize-japanese-addresses) 2.10.0相当
- [japanese-numeral](https://github.com/geolonia/japanese-numeral) 1.0.2相当

### オリジナルのnormalize-japanese-addressesとの違い
- interfaceVersion 1,2に未対応。
- GetGaikuList, GetResidentialsなど、GaikuとResidentialsに関する処理は未実装。
- キャッシュする市区町村の数は設定しても反映しない。キャッシュは、アプリを終了するまで残る。
- クラス名などの命名規則は.NET推奨のものに変更。

### 実装予定の機能
- 町丁目データ（`https://geolonia.github.io/japanese-addresses/api/ja`）をローカルファイルとして読み込めません。→対応予定

### 留意点
- awaitの非同期処理は対応できていないようです（確認しきれず）。
- テストに失敗する項目があると、DictionaryのKeyの追加に失敗するテスト項目が現れるかもしれません。テストに成功するなら問題ありません。
- 名前空間・クラス名などは、変更するかもしれません。
- プロパティ名は、Jsonからのデシリアライズを行うため、1文字目を小文字のままにしているものがある。それに準拠して、他のプロパティでも、オリジナルのTypeScriptの命名のまま利用するようにした。

# System Requirements
- .NET 6

# History
- 2023-11-06 Ver 2.10 β

# Maintenance
Taisidô(Ganges) https://ganges.pro/

# License
[MIT](LICENSE.txt)
