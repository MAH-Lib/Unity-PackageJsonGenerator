# タスクリスト：綴りの修正（リネーム専用）

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-21 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/`、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | requirements.md §3 の名前を、コードとテストで一括置換する | ✅ | 古い名前で検索して、design.md §2.2 の例外以外に残りが無い | 単語の境界を指定して置換 |
| 2 | `DependenciesInfo.PackageName` と Window の `_packageJsonPath` に `[FormerlySerializedAs]` を付ける | ✅ | 2 か所に付いている | |
| 3 | ラベルの値を `UPM除外フラグ` にする | ✅ | 画面のラベルが変わる | |
| 4 | 改行コードを CRLF にそろえ、Unity でコンパイルして Editor.log でエラーが無いことを確認する | ✅ | エラーが無い | |
| 5 | `Docs/functional-design.md`、`README.md`、`README.ja.md`、`CLAUDE.md`、`CONTRIBUTING.md` を更新する | ✅ | design.md §2.1 のとおり | |
| 6 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` の Fixed に英語で追記する | ✅ | ラベルの誤字の修正が書かれている | |
| 7 | CONTRIBUTING.md §4 の手順で配布フォルダに同期する | ✅ | `diff -r` で差分が無い | |
| 8 | Unity での確認手順をユーザーに示す | ✅ | 下の手順を提示済み | |

## Unity での確認手順（タスク 8 で提示する）

| 手順 | 期待結果 |
|---|---|
| リネームの前に、外部パッケージを 1 行入れ、フォルダーパスを `Assets` 以外にしておく | （準備） |
| Unity を前面に出してコンパイルを待つ | Console に赤いエラーが出ない |
| ウィンドウを見る | 外部パッケージ名とフォルダーパスが残っている。任意項目のラベルが「UPM除外フラグ」になっている |
| Test Runner の EditMode で Run All | すべて成功する |
| Create して、前回と同じ内容の package.json ができるか確かめる | 内容が変わらない |

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす（Unity での確認待ち）
- [x] Unity でコンパイルエラー・警告が出ない（2026-10-21 07:38 のコンパイルで本体・テストともエラーなし）
- [ ] EditMode テストがすべて成功する
- [x] 影響のある永続的ドキュメントの更新が完了している
- [x] CHANGELOG・README の更新と、配布フォルダへの同期が完了している
