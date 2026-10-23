# 要求内容：パッケージ名を mah-lib に変更

> 作業ディレクトリ：`.steering/20261024-rename-package-name/` ／ 作成日：2026-10-24 ／ ステータス：承認済み
> 対象：パッケージ `com.github.mah.package-json-generator` v2.0.1 → v2.0.2 ／ 開発環境 Unity 6000.3.26f1
> 関連する永続的ドキュメント：`Docs/functional-design.md`（冒頭の対象パッケージ名・バージョン）、`CONTRIBUTING.md`（冒頭）、`CLAUDE.md`（プロジェクト概要）。いずれも更新要

## 1. 変更・追加する機能の説明

UPM のパッケージ名を `com.github.mah.package-json-generator` から `com.github.mah-lib.package-json-generator` に変更する。GitHub の組織名 `MAH-Lib` に合わせるため。
機能の振る舞いは変えない。公開済みだが利用者はほぼいないため、破壊的変更だがパッチ（2.0.2）で上げる（ユーザー判断）。

## 2. ユーザーストーリー

- パッケージの作者として、組織名とパッケージ名を揃えて、どこの配布物か分かりやすくしたい
- 利用者として、新しい名前で Git URL から導入したい

## 3. 受け入れ条件

- [x] `package.json` の `name` が `com.github.mah-lib.package-json-generator`、`version` が `2.0.2` になっている
- [x] `CHANGELOG.md` の `[Unreleased]` に、名前の変更と対処方法（`manifest.json` の書き換え、または再インストール）が英語で書かれている
- [x] `README.md` と `README.ja.md` の `manifest.json` の記載例が新しい名前になっている（同じ内容）
- [x] `CLAUDE.md`・`CONTRIBUTING.md`・`Docs/functional-design.md` の現行の記述が新しい名前になっている
- [x] 配布フォルダ `PackageJsonGenerator/` が、開発側と同じ内容に同期されている
- [x] 現行のファイルに旧名 `com.github.mah.` が残っていない（過去の CHANGELOG 項目と `.steering` の履歴は除く）

## 4. 制約事項

- 編集するのは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` だけ。配布フォルダへは CONTRIBUTING.md §4 の手順でコピーする
- `.meta` の GUID を変えない
- コード・asmdef は変えない（`Assets/@MAH/` のフォルダ名も変えない）
- `main` ブランチに直接コミットしない

## 5. スコープ外（今回やらないこと）

- `Assets/@MAH/` フォルダ名の変更
- 過去の CHANGELOG 項目・`.steering` 配下の旧名の書き換え
- リリース作業（タグ付け・`main` へのマージ）
