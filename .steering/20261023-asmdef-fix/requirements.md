# 要求内容：メニューが表示されない不具合の修正（2.0.1）

> 作業ディレクトリ：`.steering/20261023-asmdef-fix/` ／ 作成日：2026-10-23 ／ ステータス：承認待ち
> 対象：開発環境 Unity 6000.3.26f1 ／ パッケージの対応下限 Unity 2022.1
> 関連する永続的ドキュメント：`CONTRIBUTING.md`（§1・§5.1・§6）、`CLAUDE.md`（テストのパス）、`Docs/functional-design.md`（テスト配置の記述があれば）。いずれも更新要

## 1. 変更・追加する機能の説明

2.0.0 を利用者のプロジェクトに入れると、メニュー `MAH/Generator/PackageJson Generator Window` が表示されない。この不具合を直し、2.0.1 としてリリースできる状態にする。

原因：本体の `MAH.PackageJson_Generator.Editor.asmdef` の `defineConstraints` に `UNITY_INCLUDE_TESTS` が入っている（`38eceeb` で混入、`004bf1f` で配布フォルダへ同期）。このシンボルは、`manifest.json` の `testables` に登録されたパッケージでしか定義されない。利用者がふつうに入れたパッケージは `testables` に入らないので、本体アセンブリがコンパイルされず、メニューも出ない。開発用プロジェクトは `Assets/` 配下にあるため、常に定義されて気づけなかった。

あわせて、次の 2 点を変更する（ユーザー確認済み）。

- テスト用アセンブリ `MAH.PackageJson_Generator.Editor.Tests` をパッケージに含める（`Tests/Editor/` に置く）
- 古い形式で名前が `Tests` の `Editor/Definition/Tests/Tests.asmdef` を削除する

## 2. ユーザーストーリー

- 利用者として、パッケージを入れたらメニューからジェネレーターを開けるようにしてほしい
- 利用者として、自分のプロジェクトに `Tests` という名前の余分なアセンブリが入らないようにしてほしい
- 開発者として、パッケージを `testables` に登録したときにテストを実行できるようにしたい
- 開発者として、同じ不具合を繰り返さないよう、原因と注意点が文書に残っていてほしい

## 3. 受け入れ条件

- [ ] 本体 asmdef の `defineConstraints` が空になっている
- [ ] `testables` に入れていないプロジェクトにパッケージを入れても、Console にエラーが出ず、メニューからウィンドウが開く
- [ ] `testables` に入れると、Test Runner（EditMode）に `MAH.PackageJson_Generator.Editor.Tests` のテストが出て、すべて成功する
- [ ] `testables` に入れていない場合、テストのアセンブリはコンパイルされない（テスト asmdef の `UNITY_INCLUDE_TESTS` は残す）
- [ ] `Editor/Definition/Tests/Tests.asmdef` と、その `.meta`、`Editor/Definition/Tests.meta` が無い
- [ ] テストが `PackageJson Generator/Tests/Editor/`（配布フォルダでは `PackageJsonGenerator/Tests/Editor/`）にあり、既存の `.meta` の GUID が変わっていない
- [ ] 両方の `package.json` の version が 2.0.1、`CHANGELOG.md`（リポジトリ直下とパッケージ内）に `[2.0.1]` の Fixed がある
- [ ] CONTRIBUTING.md に、本体 asmdef に `defineConstraints` を入れてはいけない理由が 1 行ある
- [ ] 配布フォルダが開発用フォルダと一致している（`diff -r` が OK）

## 4. 制約事項

- 編集するのは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` だけ。配布フォルダへは CONTRIBUTING §4 の手順で同期する
- 既存の `.meta` の GUID は変えない。新しく作るフォルダの `.meta` は新しい GUID にする
- テストの中身（`.cs`）は変えない。移動のみ
- 作業は `fix/asmdef-define-constraints`（`develop` から作成）で行い、`main` には直接コミットしない
- Unity 2022.1 に無い API は使わない（今回はコードを書かないので影響なし）

## 5. スコープ外（今回やらないこと）

- タグ付け・`main` へのマージ・GitHub Releases の公開（ユーザーの指示があってから行う）
- テストの追加（今回はアセンブリの設定の修正であり、EditMode テストでは検出できない。確認は実際のパッケージ導入で行う）
- 2.0.0 を入れた利用者への個別対応（2.0.1 に更新すれば直る）
