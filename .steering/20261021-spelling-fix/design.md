# 設計：綴りの修正（リネーム専用）

> 対象：`requirements.md`（承認済み版） ／ 作成日：2026-10-21 ／ ステータス：承認済み
> 対象：開発環境 Unity 6000.3.26f1

## 1. 実装アプローチ

requirements.md §3 の表の名前を、単語の境界を指定した置換（`\b古い名前\b`）で、開発用フォルダのコードとテストの全ファイルに対して一括で置き換える。置換のあとに古い名前で検索し、残りが無いこと（§2.2 の例外を除く）を確かめる。
シリアライズされるフィールド（`DependenciesInfo.PacckageName`、Window の `_pacckageJsonPath`）には `[FormerlySerializedAs("古い名前")]` を付け、ドメインリロードの前に保存された値を新しい名前で読めるようにする。

- 却下：IDE のリネーム機能を使う案 → この環境では IDE を操作できないため。置換後に検索と Unity のコンパイル・テストで確かめれば、同じ確実さが得られる
- 却下：`FormerlySerializedAs` を付けない案 → 開発中の 1 回だけとはいえ、入力していた外部パッケージ名とフォルダーパスが消えるため。付けても実行時の負担は無い

## 2. 変更するコンポーネント

パスは `src/PackageJsonGenerator/Assets/@MAH/` からの相対パス。

| 対象（ファイル） | 変更内容 |
|---|---|
| `PackageJson Generator/Editor/Definition/DependenciesInfo.cs` | `PacckageName` → `PackageName`。`[FormerlySerializedAs("PacckageName")]` を付け、`using UnityEngine.Serialization;` を追加 |
| `PackageJson Generator/Editor/PackageJsonCreateWindow.cs` | `_pacckageJsonPath` → `_packageJsonPath`（`[FormerlySerializedAs("_pacckageJsonPath")]` を付け、`using UnityEngine.Serialization;` を追加）。`PACKGAGE_JSON_OUTPUT_DEFAULT_PATH` → `PACKAGE_JSON_OUTPUT_DEFAULT_PATH` |
| `PackageJson Generator/Editor/Utility/PackageJsonFileUtility.cs` | `PACKGAGE_JSON_OUTPUT_FILE_NAME` → `PACKAGE_JSON_OUTPUT_FILE_NAME`、`*_ERORR_MESSAGE` → `*_ERROR_MESSAGE`（7 個） |
| `PackageJson Generator/Editor/Utility/PackageJsonValidator.cs` | `PacckageName` → `PackageName`、`IsValidSampleFolderpath` → `IsValidSampleFolderPath` |
| `PackageJson Generator/Editor/Utility/PackageSettingsConverter.cs` | `PacckageName` → `PackageName` |
| `PackageJson Generator/Editor/Drawer/OptionalSettingsDrawer.cs` | `PACKAGE_INFO_UNITY_RELEASE_VERSITON_LABEL` → `PACKAGE_INFO_UNITY_RELEASE_VERSION_LABEL`、ラベルの値 `"UMP除外フラグ"` → `"UPM除外フラグ"` |
| `PackageJson Generator/Editor/Drawer/DependenciesDrawer.cs` | `PacckageName` → `PackageName`、`depensencies_layout` → `dependencies_layout` |
| `PackageJson Generator/Editor/Drawer/SamplesDrawer.cs` | `apth_button_layout` → `path_button_layout`、`IsValidSampleFolderpath` → `IsValidSampleFolderPath` |
| `Tests/Editor/PackageJsonValidatorTests.cs`、`PackageSettingsConverterTests.cs` | `PacckageName` → `PackageName`、`IsValidSampleFolderpath` → `IsValidSampleFolderPath`（テストメソッド名を含む） |

### 2.1 ドキュメント

| 文書 | 変更内容 |
|---|---|
| `Docs/functional-design.md` | §2.2 の `_pacckageJsonPath`、§3.1 と §6.2 の `UMP除外フラグ`、§4.4 の見出しの `IsValidSampleFolderpath` を新しい名前にする。§8 の K-19 を修正済みにする |
| `README.md` / `README.ja.md` | 入力項目の表のラベル `UMP除外フラグ` を `UPM除外フラグ` にする |
| `CLAUDE.md` | 「綴りの誤りをついでに直さない」の例に挙げている名前を、修正済みである旨に直す（今後のリネームも専用の変更に分けるというルールは残す） |
| `CONTRIBUTING.md` §3.3 | 「既存の識別子には綴りの誤りがあります（`PacckageName` など）」を、修正済みである旨に直す |
| `CHANGELOG.md` `[Unreleased]` | Fixed に、ラベルの誤字（UMP → UPM）を英語で追記する（識別子の変更は利用者に関係しないので書かない） |

### 2.2 古い名前が残ってよい場所

- `[FormerlySerializedAs("PacckageName")]`、`[FormerlySerializedAs("_pacckageJsonPath")]` の文字列（古い名前で保存された値を読むために必要）
- `.steering/` の過去の文書、§8 の他の K-xx の「該当箇所」の列（当時の記録）

## 3. データ構造の変更

- シリアライズされるフィールドの名前が 2 つ変わる（`PackageName`、`_packageJsonPath`）。`FormerlySerializedAs` で古い名前の値も読むので、移行の作業は要らない
- package.json の形式は変わらない（`DependenciesInfo` は JSON に出力されない入力状態のクラス）

## 4. 影響範囲の分析

- 既存機能への影響：ラベルが `UMP除外フラグ` から `UPM除外フラグ` に変わる。それ以外の振る舞いは変わらない
- 永続的ドキュメントへの影響：§2.1 の表のとおり
- テストへの影響：テストの中の名前を置き換える。テストの中身は変えない
