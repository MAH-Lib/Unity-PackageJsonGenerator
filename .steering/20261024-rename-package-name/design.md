# 設計：パッケージ名を mah-lib に変更（2.0.2）

> 作業ディレクトリ：`.steering/20261024-rename-package-name/` ／ 作成日：2026-10-24 ／ ステータス：承認済み
> 要求：[requirements.md](requirements.md)

## 1. 方針

- 名前の文字列だけを置き換える。コード（.cs）・asmdef・`.meta` は変えない。
- 編集は開発用フォルダだけで行い、最後に CONTRIBUTING.md §4 の `robocopy` で配布フォルダへ同期する。
- 過去の CHANGELOG 項目（2.0.0 の Breaking など）と `.steering` 配下の旧名は、履歴なので残す。

## 2. 変更するファイル

パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` からの相対（ドキュメントはリポジトリ直下から）。

| 対象 | 変更 |
|---|---|
| `package.json` | `name` を `com.github.mah-lib.package-json-generator`、`version` を `2.0.2` |
| `CHANGELOG.md` | `[2.0.2] - 2026-10-24` に `Changed`（英語）を追記。末尾の比較リンクを更新（`[Unreleased]` は 2.0.2...HEAD、`[2.0.2]` を追加） |
| `README.md`、`README.ja.md`（パッケージ内） | `manifest.json` の記載例があれば新しい名前に |
| `README.md`、`README.ja.md`（リポジトリ直下） | `manifest.json` の記載例（32 行目）を新しい名前に。両方同じ内容 |
| `CLAUDE.md` | プロジェクト概要（11 行目）の名前 |
| `CONTRIBUTING.md` | 冒頭（3 行目）の名前とバージョン |
| `Docs/functional-design.md` | 冒頭（3 行目）の名前とバージョン |
| `PackageJsonGenerator/`（配布フォルダ） | `robocopy ... /MIR` で同期 |

## 3. CHANGELOG の文面（案）

```markdown
## [2.0.2] - 2026-10-24

### Changed

- **Breaking:** The package name is now `com.github.mah-lib.package-json-generator` (was `com.github.mah.package-json-generator`). If you added this package to `Packages/manifest.json` by hand, replace the old key with the new name. You can also remove the old package in Package Manager and install it again from the Git URL.
```

既存の 2.0.0 の項目の書き方に合わせる。

## 4. 作業の順序

1. 開発用フォルダの `package.json`・`CHANGELOG.md`・README を編集する。
2. `CLAUDE.md`・`CONTRIBUTING.md`・`Docs/functional-design.md` を編集する。
3. 配布フォルダへ同期し、`diff -r` で一致を確認する。
4. `Grep` で現行ファイルに旧名が残っていないことを確認する。

## 5. 確認

- `diff -r` が OK
- Unity の EditMode テストが成功する（コードは変えないので結果は変わらない想定）
- Package Manager で `package.json` の読み込みにエラーが出ない
