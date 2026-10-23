# 開発ガイド（CONTRIBUTING）

> 最終更新日：2026-10-24 ／ 対象：`com.github.mah-lib.package-json-generator` v2.0.2 ／ 開発環境 Unity 6000.3.26f1
> 関連文書：[README.ja.md](README.ja.md)（利用者向け）／[Docs/functional-design.md](Docs/functional-design.md)（現状仕様・既知の問題）／[CLAUDE.md](CLAUDE.md)（AI 向け作業ルール）

このリポジトリで開発する人のための手順書です。利用者向けの説明は README にあります。

## 1. リポジトリ構成

```text
Unity-PackageJsonGenerator/
├── PackageJsonGenerator/          # 配布フォルダ（Git URL の ?path= が指す場所。利用者はここをインストールする）
├── src/PackageJsonGenerator/      # 開発用 Unity プロジェクト
│   └── Assets/@MAH/
│       ├── PackageJson Generator/ # ★ パッケージ本体の開発場所（配布フォルダと同じ内容。EditMode テストの Tests/Editor/ も含む。§5）
│       └── Editor/Utility/        # パッケージに含まれない共通コード（LanguageUtility.cs。現在は未使用）
├── Docs/                          # 開発者向けドキュメント
├── Image/                         # README 用の画像
├── README.md / README.ja.md       # 利用者向け（英語が基本、日本語版あり）
├── CONTRIBUTING.md                # 本書
└── LICENSE                        # MIT
```

**なぜ 2 か所にあるのか**：コードを書いて動作を確かめるには Unity プロジェクトが必要です。一方で、Git URL からインストールするには、リポジトリの中に package.json を直下に持つフォルダが必要です。そこで、開発は `src/` の中で行い、リリースのときに配布フォルダへコピーしています。

## 2. 開発環境

| 項目 | 内容 |
|---|---|
| Unity | 6000.3.26f1（`src/PackageJsonGenerator/ProjectSettings/ProjectVersion.txt`） |
| パッケージの対応下限 | 2022.1（package.json の `unity`）。下限より新しい API を使わないよう気をつける。C# 9 の構文（`new()`）と .NET Standard 2.1 の API（`Split(char, StringSplitOptions)`、`Dictionary.TryAdd`）を使っているため、2021.2 より前には下げられない |
| 依存 | `com.unity.nuget.newtonsoft-json`（package.json では 3.2.1、開発用プロジェクトでは 3.2.2 が入っている） |
| IDE | Visual Studio / Rider / VS Code（`.vscode/` あり） |

開発は、Unity Hub で `src/PackageJsonGenerator` を開き、メニュー **MAH > Generator > PackageJson Generator Window** でツールを起動して行います。

## 3. 開発の流れ

### 3.1 ブランチ

| ブランチ | 用途 |
|---|---|
| `main` | リリース済みの状態。利用者が Git URL で取得するのはこのブランチ |
| `develop` | 開発用。作業ブランチはここから作り、ここへマージする |

`main` を利用者が直接インストールするので、リリース前のコードを `main` に入れないでください。

### 3.2 変更の手順

1. `develop` から作業ブランチを作る
2. **`src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` の中だけ** を編集する（配布フォルダは直接編集しない）
3. Unity で動作を確認する（§5）
4. 利用者に関係する変更なら、`CHANGELOG.md` の `[Unreleased]` に追記する
5. 仕様や既知の問題が変わったら、[Docs/functional-design.md](Docs/functional-design.md) も更新する
6. `develop` へのプルリクエストを作る

### 3.3 コーディング規約（既存コードに合わせる）

| 対象 | 規約 | 例 |
|---|---|---|
| ファイル全体 | `#if UNITY_EDITOR` 〜 `#endif` で囲む | 全ファイル |
| 名前空間 | `MAH.Tools.PackageJsonGenerator.Editor` | |
| アクセス修飾子 | 基本は `internal`。JSON のモデルだけ `public` | `PackageJsonInfo` |
| 定数 | `UPPER_SNAKE_CASE` | `PACKAGE_INFO_NAME_LABEL` |
| private フィールド | `_camelCase` | `_requiredSettings` |
| ローカル変数・引数 | `snake_case` | `label_layout`, `package_json_info` |
| static readonly の Regex | `PascalCase` | `PackageNameRegex` |
| 読み取り専用プロパティ | `Acc` を前に付ける | `AccIsLoaded` |
| セクション区切り | `//====名前====` | `//====定数====` |
| XML ドキュメント | 1 行の `/// <summary> 説明 </summary>`（日本語） | |

既存の識別子の綴りの誤り（機能設計書の §8 K-19）は、2026-10-21 に専用の変更で直しました。今後も識別子のリネームは、ほかの修正と混ぜずに専用の変更として分けてください。ほかの修正と混ぜると、差分のレビューが難しくなるためです。

## 4. 配布フォルダへの同期

開発用フォルダの内容を、配布フォルダへ丸ごとコピーします。`.meta` も含めてそのままコピーしてください。GUID が変わると、利用者のプロジェクトで参照が切れるためです。

PowerShell（リポジトリの直下で実行）：

```powershell
robocopy "src\PackageJsonGenerator\Assets\@MAH\PackageJson Generator" "PackageJsonGenerator" /MIR
```

> `/MIR` を付けると、開発用フォルダに無いファイルは配布フォルダから削除されます。配布フォルダにしか無いファイルを置かないでください。

同期できたかの確認（Git Bash）：

```bash
diff -r "src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator" PackageJsonGenerator && echo OK
```

## 5. 動作確認

### 5.1 自動テスト（EditMode）

入力の検証（`PackageJsonValidator`）と、入力状態と JSON の変換（`PackageSettingsConverter`）には、EditMode テストがあります。変更したら、まず次の手順でテストを実行してください。

1. Unity のメニュー **Window > General > Test Runner** を開く
2. **EditMode** タブで **Run All** を押す
3. `MAH.PackageJson_Generator.Editor.Tests` のテストがすべて成功（緑）になることを確かめる

テストは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/Tests/Editor/` にあり、パッケージに含めます（配布フォルダへ同期されます）。テスト用の asmdef は `UNITY_INCLUDE_TESTS` のときだけコンパイルされるので、利用者の環境（`testables` に入れていない場合）には影響しません。パッケージ側の `Editor/AssemblyInfo.cs` の `InternalsVisibleTo` で、テストから `internal` のクラスを呼べるようにしています。
**本体の asmdef（`MAH.PackageJson_Generator.Editor.asmdef`）には `defineConstraints` を入れないでください。** 開発用プロジェクトでは `UNITY_INCLUDE_TESTS` が常に定義されるため気づけませんが、利用者の環境ではアセンブリがコンパイルされず、メニューが出なくなります（2.0.0 の不具合）。制約を付けてよいのはテスト用の asmdef だけです。
検証や変換の処理を変えたら、テストも追加・修正してください。画面（Drawer・Window）の自動テストはありません。

### 5.2 手での確認

変更したら、少なくとも次の項目を手で確認してください。

- [ ] ウィンドウが開き、Console にエラーが出ない
- [ ] 必須項目を入力して **Create package.json** を押すと、ファイルが作成される
- [ ] 作成したファイルを **Load package.json** で読み込むと、同じ内容が入力欄に戻る
- [ ] 作成したパッケージを Package Manager で読み込める（Add package from disk）

## 6. リリース手順

1. `develop` の上で、**両方の** `package.json`（開発用・配布用）の `version` を上げる。番号の付け方は [Semantic Versioning](https://semver.org/lang/ja/) に従う
   - 互換性のない変更 → Major、機能の追加 → Minor、不具合の修正 → Patch
2. `CHANGELOG.md` の `[Unreleased]` を `[X.Y.Z] - YYYY-MM-DD` に書き換え、末尾の比較リンクも更新する
3. §4 の手順で配布フォルダへ同期する
4. `develop` → `main` へマージする
5. `main` の、**version を上げたコミット** にタグを付けて push する

   ```bash
   git tag X.Y.Z
   git push origin X.Y.Z
   ```

   タグ名は `X.Y.Z`（`v` を付けない）にそろえます。利用者が `#X.Y.Z` で指定できるようにするためです。過去のタグ `v1.0.0` は例外として残しています。
6. GitHub の Releases に、CHANGELOG の該当部分を書いて公開する

## 7. ドキュメントの更新ルール

| 文書 | 言語 | いつ更新するか |
|---|---|---|
| `README.md` / `README.ja.md` | 英語 / 日本語 | 使い方や入力項目が変わったとき。**2 つの内容を必ず揃える** |
| `PackageJsonGenerator/README.md`、`CHANGELOG.md` | 英語 | 利用者に関係する変更をしたとき |
| `Docs/functional-design.md` | 日本語 | 仕様・既知の問題が変わったとき |
| `CONTRIBUTING.md`、`CLAUDE.md` | 日本語 | 開発の手順が変わったとき |
| `.github/` のテンプレート | 英語 | 必要に応じて |

利用者は海外の人も想定しているため、利用者向けの文書は英語で書きます。開発者向けの文書は日本語で書きます。
