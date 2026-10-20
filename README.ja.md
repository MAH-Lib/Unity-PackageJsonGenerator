# Unity-PackageJsonGenerator

[English](README.md)

UPM（Unity Package Manager）対応パッケージを公開するときに必要な `package.json` を、GUI で手軽に作成できる Editor 拡張ツールです。

## 動作環境

| 項目 | バージョン |
|---|---|
| Unity | 6000.0 以上 |
| 依存パッケージ | `com.unity.nuget.newtonsoft-json` 3.2.1（自動でインストールされます） |

## インストール方法

### Package Manager から（Git URL）

1. 次の Git URL をコピーします。

   ```
   https://github.com/MAH-Lib/Unity-PackageJsonGenerator.git?path=PackageJsonGenerator
   ```

2. **Window > Package Manager** を開き、**+** ボタンから **Install package from git URL...** を選びます。
3. コピーした URL を貼り付けて **Install** をクリックします。

### manifest.json から

`Packages/manifest.json` の `dependencies` に次の 1 行を追加します。

```json
"com.mah.package-json-generator": "https://github.com/MAH-Lib/Unity-PackageJsonGenerator.git?path=PackageJsonGenerator"
```

特定のバージョンに固定したい場合は、URL の末尾にリリースタグを付けます（例：`...?path=PackageJsonGenerator#1.0.1`）。使えるタグは [Tags](../../tags) で確認できます。

## 使い方

1. メニューバーから **MAH > Generator > PackageJson Generator Window** を選びます。

   ![Image1](Image/Image1.png)

2. 表示された Editor Window に必要な情報を入力します。

   - **必須設定項目**：必ず入力する項目です。著者情報設定の **E-mail** と **URL** だけは入力しなくてもかまいません。

     ![Image2](Image/Image2.png)

   - **任意設定項目**：必要な項目だけ入力します。

     ![Image3](Image/Image3.png)

3. **Browse** をクリックして、`package.json` を作成するフォルダを選びます。**Reset** を押すと `Assets` に戻ります。

   ![Image4](Image/Image4.png)

4. **Create package.json** をクリックすると、ファイルが作成されます。

   ![Image5](Image/Image5.png)

### 既存の package.json を編集する

フォルダーパスに、既存の `package.json` があるフォルダを指定して **Load package.json** をクリックします。ファイルの内容が入力欄に読み込まれるので、編集してから **Create package.json** で保存し直してください。

## 入力項目

| 区分 | 項目（画面のラベル） | `package.json` のキー | 補足 |
|---|---|---|---|
| 必須 | パッケージ名 | `name` | 小文字をドットで区切った形式（例：`com.company.my-tool`） |
| 必須 | バージョン | `version` | Major / Minor / Patch |
| 必須 | UPM表示名 | `displayName` | Package Manager に表示される名前 |
| 必須 | 概要 | `description` | |
| 必須 | 対応 Unity バージョン設定 | `unity` | 対応する Unity の最低バージョン（例：`6000.0`）。Unity 5.0〜Unity 6.6（`6000.6`）から選びます |
| 必須 | ライセンス | `license` | 主な SPDX ライセンスから選びます |
| 必須 | 著者情報設定 | `author` | 著者名は必須、E-mail と URL は任意 |
| 任意 | リリースバージョン | `unityRelease` | 例：`0f1` |
| 任意 | UPM除外フラグ | `hideInEditor` | |
| 任意 | 変更ログURL / ドキュメントURL / ライセンスURL | `changelogUrl` / `documentationUrl` / `licensesUrl` | `http://` か `https://` で始まる URL |
| 任意 | キーワード | `keywords` | |
| 任意 | サンプル | `samples` | パスは `Samples~` から始まる形に変換されます |
| 任意 | 外部パッケージ | `dependencies` | Unity Package Manager の仕様に合わせ、バージョンは完全一致（`X.Y.Z`）だけです。プロジェクトに入っているパッケージから選ぶこともできます |

任意項目のうち、空欄のものはファイルに出力されません。

## 既知の制限事項

- 入力した内容は、スクリプトが再コンパイルされても残りますが、ウィンドウを閉じると消えます。

これらは今後のアップデートで改善する予定です。

## サポート・お問い合わせ

- **バグ報告・機能の要望**：[Issues](../../issues)
- **質問・相談・意見交換**：[Discussions](../../discussions)

ご協力ありがとうございます！

## ライセンス

MIT License です。詳しくは [LICENSE](LICENSE) をご覧ください。
