# Unity-PackageJsonGenerator

[English](README.md)

UPM（Unity Package Manager）対応パッケージを公開するときに必要な `package.json` を、GUI で手軽に作成できる Editor 拡張ツールです。

## 動作環境

| 項目 | バージョン |
|---|---|
| Unity | 2022.1 以上 |
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
"com.github.mah.package-json-generator": "https://github.com/MAH-Lib/Unity-PackageJsonGenerator.git?path=PackageJsonGenerator"
```

特定のバージョンに固定したい場合は、URL の末尾にリリースタグを付けます（例：`...?path=PackageJsonGenerator#2.0.0`）。使えるタグは [Tags](../../tags) で確認できます。

## 使い方

> ウィンドウは英語と日本語に対応しています。ウィンドウ右上の **言語**（英語表示では **Language**）で切り替えられ、選んだ言語は次回も使われます。初めて開いたときは、PC の言語が日本語なら日本語、それ以外なら英語で表示されます。

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

このツールが扱わないキー（例：`type`、`repository`、`testables`）は、値を変えずにそのまま書き戻されます。任意設定項目の **その他の項目** に名前が表示され、**Clear** で消せます。

ボタンの上の **JSON プレビュー** を開くと、**Create package.json** で書き出される内容を確認できます。

### 複数のパッケージを編集する

Load または Create したパッケージは、Unity Editor を閉じるまで記憶されます。ウィンドウ上部の **記憶したパッケージ** から選ぶと、そのパッケージに切り替わります。切り替えるときやウィンドウを閉じるときも、今のパッケージの保存前の編集は残ります。**Remove from list** で、今のパッケージを一覧から外せます。

## 入力項目

| 区分 | 項目（画面のラベル） | `package.json` のキー | 補足 |
|---|---|---|---|
| 必須 | パッケージ名 | `name` | 小文字をドットで区切った形式（例：`com.company.my-tool`） |
| 必須 | バージョン | `version` | Major / Minor / Patch と、任意の **プレリリース** の表記（例：`preview.1` と入れると `1.2.0-preview.1`） |
| 必須 | UPM表示名 | `displayName` | Package Manager に表示される名前 |
| 必須 | 概要 | `description` | |
| 必須 | 対応 Unity バージョン設定 | `unity` | 対応する Unity の最低バージョン（例：`6000.0`）。Unity 5.0〜Unity 6.6（`6000.6`）から選びます。**Use current Editor version** で、使っている Editor のバージョンをこの項目とリリースバージョンに入れられます |
| 必須 | ライセンス | `license` | 主な SPDX ライセンスから選びます。**None** を選ぶと `UNLICENSED`（誰にも使用を許可しない）を出力します。**Other** では任意の SPDX ID を入力できます。**Proprietary** を選ぶと表示される **Create LICENSE.md** で、著者名と今年の年を入れた `LICENSE.md` のひな形（英語）をフォルダに作れます（ひな形であり、法的な正しさは保証しません） |
| 必須 | 著者情報設定 | `author` | 著者名は必須、E-mail と URL は任意 |
| 任意 | リリースバージョン | `unityRelease` | 例：`0f1`。形式が違うと、作成するときに警告が出ます |
| 任意 | UPM除外フラグ | `hideInEditor` | |
| 任意 | 変更ログURL / ドキュメントURL / ライセンスURL | `changelogUrl` / `documentationUrl` / `licensesUrl` | `http://` か `https://` で始まる URL |
| 任意 | キーワード | `keywords` | |
| 任意 | サンプル | `samples` | パスは `Samples~` から始まる形に変換されます。パッケージのフォルダの中にそのフォルダが無いと、作成するときに警告が出ます |
| 任意 | 外部パッケージ | `dependencies` | Unity Package Manager の仕様に合わせ、バージョンは完全一致（`X.Y.Z`、プレリリースの表記を付けると `X.Y.Z-pre.1`）だけです。プロジェクトに入っているパッケージから選ぶこともできます |

任意項目のうち、空欄のものはファイルに出力されません。

## 既知の制限事項

- まだ Load も Create もしていない（**記憶したパッケージ** に無い）入力内容は、ウィンドウを閉じると消えます。記憶したパッケージは、Unity Editor を閉じると消えます。

これらは今後のアップデートで改善する予定です。

## サポート・お問い合わせ

- **バグ報告・機能の要望**：[Issues](../../issues)
- **質問・相談・意見交換**：[Discussions](../../discussions)

ご協力ありがとうございます！

## ライセンス

MIT License です。詳しくは [LICENSE](LICENSE) をご覧ください。
