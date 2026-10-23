# タスクリスト：パッケージ名を mah-lib に変更（2.0.2）

> 作業ディレクトリ：`.steering/20261024-rename-package-name/` ／ 作成日：2026-10-24 ／ ステータス：承認済み
> 要求：[requirements.md](requirements.md) ／ 設計：[design.md](design.md)

## 1. 開発用フォルダの編集

- [x] `package.json`：`name` を `com.github.mah-lib.package-json-generator`、`version` を `2.0.2` にする
- [x] `CHANGELOG.md`：`[2.0.2]` の `Changed`（Breaking）を英語で追記し、末尾の比較リンクを更新する
- [x] パッケージ内の `README.md`・`README.ja.md`：旧名があれば新名にする
- [x] リポジトリ直下の `README.md`・`README.ja.md`：`manifest.json` の記載例を新名にする（同じ内容）

## 2. 開発者向けドキュメント

- [x] `CLAUDE.md`：プロジェクト概要の名前
- [x] `CONTRIBUTING.md`：冒頭の名前とバージョン
- [x] `Docs/functional-design.md`：冒頭の名前とバージョン

## 3. 同期と確認

- [x] 配布フォルダ `PackageJsonGenerator/` へ `robocopy` で同期する
- [x] `diff -r` で開発側と配布側が一致することを確認する
- [x] `Grep` で現行ファイルに旧名 `com.github.mah.` が残っていないことを確認する
- [ ] Unity の EditMode テストが成功することを確認する（ユーザーが実施）

## 4. 完了

- [ ] コミット（`develop` ブランチ、ユーザーの依頼があってから）
