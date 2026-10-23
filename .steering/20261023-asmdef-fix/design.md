# 設計：メニューが表示されない不具合の修正（2.0.1）

> 作業ディレクトリ：`.steering/20261023-asmdef-fix/` ／ 作成日：2026-10-23 ／ ステータス：承認待ち
> 要求：[requirements.md](requirements.md)

## 1. 方針

- 本体 asmdef から `UNITY_INCLUDE_TESTS` の制約を外す。本体は常にコンパイルされる。
- テストは本体と同じパッケージ内の `Tests/Editor/` に置く。テスト asmdef の `UNITY_INCLUDE_TESTS` 制約は残すので、`testables` に入れた環境でだけコンパイルされる。
- コード（.cs）は変えない。

## 2. 変更するファイル

パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` からの相対（ドキュメントはリポジトリ直下から）。

| 対象 | 変更 |
|---|---|
| `Editor/MAH.PackageJson_Generator.Editor.asmdef` | `defineConstraints` を `[]` にする |
| `Editor/Definition/Tests/Tests.asmdef`、`.asmdef.meta`、`Editor/Definition/Tests.meta` | 削除（`Tests/` フォルダごと） |
| `Tests.meta`、`Tests/Editor.meta` | 新規。GUID は新しく生成した 32 桁の 16 進数 |
| `Assets/@MAH/Tests/Editor/` の 12 ファイル（.cs 5 本、asmdef 1 本と、それぞれの .meta） | `Tests/Editor/` へ 1 ファイルずつ `git mv`。GUID は維持 |
| `Assets/@MAH/Tests/` と `Tests.meta` | 空になったら削除 |
| `package.json`（開発用・配布用） | version を 2.0.1 |
| `CHANGELOG.md`（直下とパッケージ内） | `[2.0.1]` の Fixed を英語で追記、末尾の比較リンクを更新 |
| `CONTRIBUTING.md` | §1 の構成図、§5.1 のテストの場所、§6 の例、注意点 1 行を追加 |
| `CLAUDE.md` | `Assets/@MAH/Tests/Editor/` のパスを新しい場所に更新 |
| `Docs/functional-design.md` | テスト配置の記述があれば更新 |

## 3. テスト asmdef の確認（変更なし）

- `references` は `MAH.PackageJson_Generator.Editor`（asmdef 名で参照するので移動しても切れない）。
- `precompiledReferences` の `Newtonsoft.Json.dll` は、パッケージの依存 `com.unity.nuget.newtonsoft-json` から来る。
- `autoReferenced: false`、`includePlatforms: Editor`、`UNITY_INCLUDE_TESTS` のまま。
- `AssemblyInfo.cs` の `InternalsVisibleTo("MAH.PackageJson_Generator.Editor.Tests")` はそのまま有効。

## 4. 作業の順序

1. asmdef の制約を外す。
2. 古い `Tests/` を削除する。
3. テストを移動し、新しい `.meta` を作る。
4. version・CHANGELOG・ドキュメントを更新する。
5. 配布フォルダへ同期する（`robocopy /MIR`）。`diff -r` で確認する。
6. 確認（`tasklist.md` のとおり）。
7. コミットする（1 コミット）。

## 5. リスクと対策

| リスク | 対策 |
|---|---|
| フォルダごとの `git mv` で Permission denied | 1 ファイルずつ移動してから、空のフォルダを削除する |
| `.meta` の GUID が変わる | 移動は `git mv` のみ。`git diff -M` で rename になっていることを確認する |
| 配布フォルダに古いファイルが残る | `robocopy /MIR` と `diff -r` で確認する |
| 同期で `Tests/` が配布物に入る | これは意図どおり（ユーザー確認済み） |
