# タスクリスト：メニューが表示されない不具合の修正（2.0.1）

> 作業ディレクトリ：`.steering/20261023-asmdef-fix/` ／ 作成日：2026-10-23 ／ ステータス：承認済み（1〜5 実施済み、6 確認済み）
> 要求：[requirements.md](requirements.md) ／ 設計：[design.md](design.md)
> ブランチ：`fix/asmdef-define-constraints`
> パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用」）からの相対。

## 1. asmdef

- [x] 開発用 `Editor/MAH.PackageJson_Generator.Editor.asmdef` の `defineConstraints` を `[]` にする
- [x] 開発用 `Editor/Definition/Tests/` を削除する（`Tests.asmdef`、`Tests.asmdef.meta`）
- [x] 開発用 `Editor/Definition/Tests.meta` を削除する

## 2. テストの移動

- [x] `Tests.meta` と `Tests/Editor.meta` を新しい GUID で作る
- [x] `Assets/@MAH/Tests/Editor/` の 12 ファイルを、開発用 `Tests/Editor/` へ 1 ファイルずつ `git mv` する
- [x] 空になった `Assets/@MAH/Tests/` と `Assets/@MAH/Tests.meta` を削除する
- [x] `git diff -M --stat` で、移動したファイルが rename（GUID 維持）になっていることを確認する

## 3. バージョンと CHANGELOG

- [x] 開発用 `package.json` の version を 2.0.1 にする
- [x] 配布用 `PackageJsonGenerator/package.json` の version を 2.0.1 にする（同期でも反映される）
- [x] 開発用 `CHANGELOG.md` に `[2.0.1]` の Fixed を英語で追記し、末尾の比較リンクを更新する
- [x] リポジトリ直下に `CHANGELOG.md` があれば同じ内容を追記する

## 4. ドキュメント

- [x] `CONTRIBUTING.md`：§1 の構成図、§5.1 のテストの場所、§6 の version の記述を更新する。本体 asmdef に `defineConstraints` を入れない注意を 1 行追加する
- [x] `CLAUDE.md`：`Assets/@MAH/Tests/Editor/` のパスを更新する
- [x] `Docs/functional-design.md`：テスト配置の記述があれば更新する

## 5. 配布フォルダへの同期

- [x] `robocopy "src\PackageJsonGenerator\Assets\@MAH\PackageJson Generator" "PackageJsonGenerator" /MIR`
- [x] `diff -r "src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator" PackageJsonGenerator` が差分なし
- [x] 配布フォルダに `Editor/Definition/Tests` が無く、`Tests/Editor/` がある

## 6. 確認（ユーザーが Unity で行う）

- [x] 開発用プロジェクト：コンパイルエラーが無く、Test Runner（EditMode）で `MAH.PackageJson_Generator.Editor.Tests` が全て成功する
- [x] 空の Unity プロジェクトに配布フォルダを導入する（`testables` なし）：Console にエラーが無く、メニュー `MAH > Generator > PackageJson Generator Window` が出てウィンドウが開く
- [x] 同プロジェクトで `testables` に追加：Test Runner にテストが出て、全て成功する
- [x] CONTRIBUTING §5.2 の手動確認（作成、読み込み、Add package from disk）

## 7. コミット

- [x] `git status` で意図しない変更が無いことを確認する
- [ ] 1 コミットにする（件名は日本語、`Co-Authored-By` 付き）
- [ ] リリース（`develop` への PR・`main` へのマージ・タグ・Releases）は、ユーザーの指示があってから行う
