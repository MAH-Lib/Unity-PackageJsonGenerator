# 設計：フェーズ 4 機能の追加（#7〜#12）

> 対象：`requirements.md`（承認済み版） ／ 作成日：2026-10-21 ／ ステータス：承認済み
> 対象：開発環境 Unity 6000.3.26f1
> パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/Editor/` からの相対パス。テストは `src/PackageJsonGenerator/Assets/@MAH/Tests/Editor/`

## 0. 全体の方針

- #5 で分けた役割（Window・Drawer・Validator・Converter・FileUtility）を保ち、新しい処理もその役割に合うクラスに置く
- 検証と変換（Validator・Converter）は、今と同じく `Debug` や `EditorGUILayout` を使わず、テストから呼べる形にする
- 新しい文言は、ステップ 1 で作る文言の表に英語・日本語の両方で登録する
- 入力状態に項目を足すときは、`[SerializeField]`（または `public`）にして、ドメインリロードとステップ 4 の記憶で残るようにする

## ステップ 1：英語・日本語の切り替え（#7）

### 1.1 方針

文言を `TextId`（enum）で表し、`LocalizedText` クラスが「`TextId` → 英語・日本語の文字列」の表を持つ。画面・メッセージを出す側は、定数の文字列の代わりに `LocalizedText.Get(TextId.X)` や `LocalizedText.Format(TextId.X, 値)` を呼ぶ。今の言語は EditorPrefs に保存する。

- 却下：Unity Localization パッケージを使う案 → パッケージの依存が増えるため（禁止事項）
- 却下：言語ごとに JSON ファイルを持つ案 → ファイルの読み込みと、ファイルが無いときの扱いが増える。文言は 100 個程度なので、コードの表で足りる

### 1.2 変更するコンポーネント

| 対象 | 変更内容 | 新規/修正 |
|---|---|---|
| `Definition/LanguageType.cs` | `English = 0`、`Japanese = 1` | 新規 |
| `Definition/TextId.cs` | すべての文言の ID（ラベル・ボタン・見出し・入力エラー・操作結果・ダイアログ・Console のメッセージ） | 新規 |
| `Utility/LocalizedText.cs` | 文言の表（`Dictionary<TextId, string[]>`。添え字が `LanguageType`）、`CurrentLanguage`（取得は EditorPrefs から、設定すると EditorPrefs に保存）、`Get` / `Format`、初期値の決定（`Application.systemLanguage == SystemLanguage.Japanese` なら日本語）、テスト用の `OverrideLanguage(LanguageType?)`（EditorPrefs に保存しない一時的な切り替え） | 新規 |
| 各 Drawer、`DrawerUtility`、Window、`PackageJsonValidator`、`PackageSettingsConverter`、`PackageJsonFileUtility` | 日本語の定数を `LocalizedText` の呼び出しに置き換える。定数は削除する | 修正 |
| `PackageJsonCreateWindow.cs` | 上部に言語の `Popup`（`English` / `日本語`）を追加。切り替えたら `_fieldErrors.ClearAll()` と操作結果のクリア | 修正 |
| `Drawer/DependenciesDrawer.cs` | ポップアップの先頭項目を、描画のときに今の言語の文言（`– Select Package –` / `– パッケージを選択 –`）に置き換える（`UnityPackageHelper` の一覧は言語に依存させない） | 修正 |
| `UnityPackageHelper.cs` | 先頭の項目の文字列を空にし、意味を「選択なし」として扱う | 修正 |

EditorPrefs のキー：`MAH.PackageJsonGenerator.Language`（値は `LanguageType` の整数）。

英語の文言は、README.md（英語版）の入力項目の表の言い方に合わせる（例：パッケージ名 → `Package name`、UPM表示名 → `Display name`、UPM除外フラグ → `Hide in editor`）。

### 1.3 テスト

- `LocalizedTextTests`：すべての `TextId` に、英語・日本語とも空でない文言があること。`Format` の引数の数が足りること（`{0}` などを含む文言で例外が出ないこと）
- 既存のテストのうち、メッセージの文字列を直接比べているもの（生成前チェック）は、`LocalizedText.Format(...)` で作った期待値と比べる形に直す。テストは `OverrideLanguage` で英語・日本語の両方で実行する（`[TestFixture(LanguageType.English)]` / `[TestFixture(LanguageType.Japanese)]`）

## ステップ 2：プレビュー版のバージョン（#8）

### 2.1 変更するコンポーネント

| 対象 | 変更内容 |
|---|---|
| `Definition/PackageRequiredSettings.cs` | `[SerializeField] internal string PreRelease`（初期値は空） |
| `Definition/DependenciesInfo.cs` | `public string PreRelease`（初期値は空） |
| `Utility/PackageJsonValidator.cs` | `IsValidPreRelease(string)`：SemVer 2.0.0 のプレリリースの規則（`.` で区切った各部分が英数字とハイフン。数字だけの部分は先頭が 0 でない）。空は「プレリリースなし」として `true` |
| `Utility/PackageSettingsConverter.cs` | `ConvertToVersionString(Vector3Int, string pre_release)`（表記が空なら `X.Y.Z`、あれば `X.Y.Z-表記`）。`TryConvertToVersionVector(string, out Vector3Int, out string pre_release)`：正規表現を `^(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:-([0-9A-Za-z.-]+))?(.*)$` にし、表記が規則に合えば読み込む。`+build` などの残りがあれば `false`（警告） |
| `Utility/VersionOperatorConverter.cs` | 依存パッケージの値の正規表現に、プレリリースの表記を加える（`1.2.3-pre.1`、`>=1.2.3-pre.1` を読める） |
| `Drawer/DrawerUtility.cs` | `DrawVersionFields` に、Patch の後ろのプレリリースの入力欄（`DelayedTextField`、検証とエラー表示付き）を加える。エラーのキーは `(行のオブジェクト, "PreRelease")` のタプル（名前のエラーと区別するため） |
| `Drawer/RequiredSettingsDrawer.cs`、`DependenciesDrawer.cs` | 新しい `DrawVersionFields` を使う。ポップアップで選んだ登録済みパッケージのバージョンの表記も入れる |

### 2.2 テスト

`TryConvertToVersionVector` の表に `1.2.3-preview.1`（`true`、表記 `preview.1`）、`1.2.3-01`（表記が規則違反 → `false`）、`1.2.3+build.5`（`false`）を足す。`IsValidPreRelease` の正しい値・不正な値。往復のテストに表記を入れる。

## ステップ 3：ライセンス None と LICENSE.md の作成（#9）

### 3.1 変更するコンポーネント

| 対象 | 変更内容 | 新規/修正 |
|---|---|---|
| `Definition/LicenseType.cs` | 末尾（`Other` の後）に `None` を追加。既存の値は変えない | 修正 |
| `Utility/SPDXLicenseUtility.cs` | `None` → `"UNLICENSED"`、表示名 `UNLICENSED (no permission granted)` | 修正 |
| `Utility/LicenseFileUtility.cs` | `GetProprietaryLicenseText(string author_name, int year)`（§3.2 の英語のひな形）、`GetLicenseFilePath(string folder)`、`TryCreateLicenseFile(string folder, string text, out string error_message)`（フォルダが無い・書き込み失敗は例外を出さずに `false`） | 新規 |
| `Drawer/RequiredSettingsDrawer.cs` | `Proprietary` のときだけ「LICENSE.md を作成」ボタンを出す。押したら、Window から渡されたコールバックを呼ぶ | 修正 |
| `PackageJsonCreateWindow.cs` | LICENSE.md の作成処理（`delayCall` で `OnGUI` の外で行う）：著者名の確認 → 既存ファイルの上書き確認ダイアログ → 作成 → 操作結果に表示 → `AssetDatabase.Refresh()` | 修正 |

### 3.2 ひな形（英語）

```text
# License

Copyright (c) {year} {author}. All rights reserved.

This software and associated documentation files (the "Software") are proprietary.
No part of the Software may be copied, modified, distributed, or used in any form
without the prior written permission of the copyright holder.
```

ひな形であり、法的な正しさは保証しない旨を README に書く。

### 3.3 テスト

`SPDXLicenseConverter` の `None` ⇔ `UNLICENSED`。`GetProprietaryLicenseText` に著者名と年が入ること。`TryCreateLicenseFile` を一時フォルダに対して実行し、ファイルができること・フォルダが無いと `false` になること。

## ステップ 4：読み込んだパッケージの記憶（#10）

### 4.1 方針

パッケージ 1 つ分の入力状態を `PackageSnapshot`（`[Serializable]`）にまとめ、`JsonUtility` で JSON にして `SessionState` に保存する。`SessionState` は Unity Editor を閉じるまで値を保つ Unity の仕組みで、ウィンドウを閉じても、ドメインリロードでも消えない。`JsonUtility` を通すので、保存と取り出しのたびに別のオブジェクトになり、記憶した内容と画面の入力が互いに影響しない。

### 4.2 変更するコンポーネント

| 対象 | 変更内容 | 新規/修正 |
|---|---|---|
| `Definition/PackageSnapshot.cs` | `FolderPath`、`Required`（`PackageRequiredSettings`）、`Optional`（`PackageOptionalSettings`） | 新規 |
| `Utility/PackageSessionStore.cs` | `GetAll()`、`Find(folder)`、`Save(snapshot)`（同じフォルダは上書き）、`Remove(folder)`。キーは `MAH.PackageJsonGenerator.Packages`。フォルダーパスは `\` を `/` にし、末尾の `/` を取ってから比べる | 新規 |
| `PackageJsonCreateWindow.cs` | 言語の選択の下に「記憶したパッケージ」の `Popup` と「一覧から外す」ボタン。選び直したら、今の入力を今のフォルダの分として記憶 → 選んだ分を読み込む（入力エラーを消し、ライセンスの選択を合わせ、フォーカスを外す）。Load / Create に成功したら、そのフォルダの分として記憶する。`OnDisable` で、今のフォルダが記憶済みなら今の入力を記憶し直す（ウィンドウを閉じても保存前の編集を残すため） | 修正 |

一覧の表示は `パッケージ名（フォルダーパス）`。今のフォルダが記憶されていないときは、先頭に `（記憶していないパッケージ）` を出して選んだ状態にする。

### 4.3 テスト

`PackageSessionStore` に保存 → 取り出しで内容が同じこと、同じフォルダは上書きされること、`Remove` で消えること、パスの区切りの違い（`\` と `/`、末尾の `/`）を同じフォルダとして扱うこと。テストでは専用のキーを使い、終わったら消す（利用者の記憶を壊さないため。`PackageSessionStore` にキーを差し替える internal の仕組みを持たせる）。

## ステップ 5：未対応キーの保持と JSON プレビュー（#11）

### 5.1 変更するコンポーネント

| 対象 | 変更内容 | 新規/修正 |
|---|---|---|
| `Definition/PackageJsonInfo.cs` | `[JsonExtensionData] public IDictionary<string, JToken> ExtraData`（Newtonsoft が、クラスに無いキーをここに入れ、出力のときに書き戻す） | 修正 |
| `Definition/PackageOptionalSettings.cs` | `[SerializeField] internal string ExtraJson`（未対応キーを JSON オブジェクトの文字列で持つ。`JToken` は Unity がシリアライズできないため）。`Reset()` で空にする | 修正 |
| `Utility/PackageSettingsConverter.cs` | Load：`ExtraData` があれば `ExtraJson` に。Create：`ExtraJson` があれば `ExtraData` に戻す。`GetExtraKeys(string extra_json)`（名前の一覧） | 修正 |
| `Utility/PackageJsonFileUtility.cs` | `SerializePackageJson(PackageJsonInfo)` を切り出し、保存とプレビューの両方で使う | 修正 |
| `Drawer/OptionalSettingsDrawer.cs` | 末尾に「その他の項目（このツールでは編集できません）」の見出しとキーの名前の一覧、「クリア」ボタン（未対応キーが無いときは表示しない） | 修正 |
| `PackageJsonCreateWindow.cs` | ファイル操作の欄に「JSON プレビュー」の Foldout。開いているときだけ、`Layout` のイベントで出力内容を作り直して、読み取り専用の欄に表示する（警告は出さない） | 修正 |

ステップ 4 の `PackageSnapshot` は `PackageOptionalSettings` を丸ごと持つので、`ExtraJson` も自動で記憶の対象になる。

### 5.2 テスト

未対応キー（`type`、`repository` のようなオブジェクト、配列）を含む JSON を `JsonConvert` で読み、入力状態 → JSON に戻したときに同じ値で残ること。`GetExtraKeys` の結果。

## ステップ 6：現在の Unity の反映と Create 前の確認（#12）

### 6.1 変更するコンポーネント

| 対象 | 変更内容 |
|---|---|
| `Utility/UnityVersionConverter.cs` | `TryParseEditorVersion(string unity_version, out int major, out int minor, out string release)`：`Application.unityVersion`（例 `6000.3.26f1`）を読む。テストのために引数で受け取る |
| `Drawer/RequiredSettingsDrawer.cs` | 対応 Unity バージョンの欄に「現在の Editor に合わせる」ボタン。押したら Window から渡されたコールバックを呼ぶ（`unityRelease` は任意項目にあるため、Window が両方を書き換える） |
| `Utility/PackageJsonValidator.cs` | `GetCreateWarnings(required, optional, folder)`：サンプルのフォルダの有無（`<folder>/<path>`、無ければ `Samples~` を `Samples` に読み替えた場所）と、`unityRelease` の形式（`^\d+[a-z]\d+$`。例 `0f1`、`26f1`、`0b5`）を確かめ、警告の文言の一覧を返す |
| `PackageJsonCreateWindow.cs` | Create で、生成前チェックの後に `GetCreateWarnings` の結果を警告として出す（件数は操作結果に入る）。作成は止めない |

### 6.2 テスト

`TryParseEditorVersion`（`6000.3.26f1`、`2022.3.10f1c1`、読めない値）。`GetCreateWarnings`（一時フォルダにサンプルのフォルダを作った場合・作らない場合・`Samples` にある場合、`unityRelease` の正しい値・不正な値・空）。

## 7. データ構造の変更（まとめ）

| 対象 | 追加するもの | ステップ |
|---|---|---|
| `PackageRequiredSettings` | `PreRelease` | 2 |
| `DependenciesInfo` | `PreRelease` | 2 |
| `LicenseType` | `None`（末尾） | 3 |
| `PackageJsonInfo` | `ExtraData`（`[JsonExtensionData]`） | 5 |
| `PackageOptionalSettings` | `ExtraJson` | 5 |
| EditorPrefs | `MAH.PackageJsonGenerator.Language` | 1 |
| SessionState | `MAH.PackageJsonGenerator.Packages` | 4 |

どれも追加だけなので、今の入力状態や package.json の読み込みに移行の作業は要らない（足した項目は空・初期値で始まる）。

## 8. 影響範囲（まとめ）

- 既存機能への影響：初めて開いたときの言語が英語になることがある（PC の言語が日本語でない場合）。それ以外は機能の追加で、今の動きは変わらない
- ドキュメント（各ステップで、そのステップの分を更新する）
  - `Docs/functional-design.md`：§2.1（新しいファイル）、§2.2（新しい状態）、§3.1（新しい入力項目・ラベルの英語名）、§3.3（`None`）、§4（新しい検証・Create 前の警告）、§5（記憶・未対応キー・プレビュー・LICENSE.md）、§6.2（レイアウト）、§7（ローカライズ・永続化）
  - `README.md` / `README.ja.md`：使い方（言語の切り替え、記憶、プレビュー、LICENSE.md、現在の Editor に合わせる）と入力項目の表。英語版の「画面は日本語」の注記を消し、英語のラベルにする
  - `CHANGELOG.md` `[Unreleased]`：Added に各機能を英語で追記
- テスト：各ステップの §x.x のとおり追加・修正する
