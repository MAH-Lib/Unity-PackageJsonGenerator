# タスクリスト：Load 時の例外と Unity バージョンの修正

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-20 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用フォルダ」）、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | `UnityVersionConverter` を design.md §2.1 の中身に書き換える | ✅ | 表・取得メソッド・`TryParseUnityVersion` がある。enum を参照していない | ファイル名・`.meta` は変えない |
| 2 | `PackageRequiredSettings.MajorVer` を `int` にし、初期値を 6000 / 0 にする | ✅ | コンストラクタと `Reset()` が新しい定数を使っている | |
| 3 | `DrawUnityVersionSettings` を `Popup` と「一覧外」の追加に書き換える | ✅ | design.md §2.2 の動きになる | |
| 4 | `ConvertToVersionVector` を `TryConvertToVersionVector` に置き換え、呼び出し元（Load・`SetDependenciesInfo`・`ConvertToDependenciesInfo`）を直す | ✅ | §2.4 の表どおりで、例外が出ない | |
| 5 | `ConvertToMajorVersion` / `ConvertToMinorVersion` を削除し、`ConvertToPackageSettings` で `TryParseUnityVersion` と §2.3 の警告を使う。`ConvertToUnityVersionString` の引数を `int` にする | ✅ | `throw new NotImplementedException()` が Window から無くなる | |
| 6 | `Definition/UnityVersion/` のファイル・`.meta`・フォルダの `UnityVersion.meta` を削除する | ✅ | `MajorVersion` / `MinorVersion_` を参照する箇所が無い | |
| 7 | 構文チェックと Unity の Console を確認する | ✅ | エラーが無い | 2026-10-20 23:52 の Unity のコンパイルでエラーなし（Editor.log で確認） |
| 8 | `Docs/functional-design.md` を更新する | ✅ | §2.1・§3.1・§3.4 が新しい仕様になり、§8 の K-4・K-5・K-6・K-7・K-17 が修正済み | |
| 9 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` に英語で追記する | ✅ | Added / Fixed に書かれている | |
| 10 | `README.md` / `README.ja.md` に直す箇所が無いか確認し、あれば両方を直す | ✅ | 確認済み | |
| 11 | CONTRIBUTING.md §4 の手順で配布フォルダに同期する | ✅ | `diff -r` で差分が無い。`UnityVersion/` が配布フォルダからも消えている | robocopy `/MIR` |
| 12 | Unity での確認手順をユーザーに示す | ✅ | 下の手順を提示済み | |

## Unity での確認手順（タスク 12 で提示する）

| 手順 | 期待結果 |
|---|---|
| ウィンドウを開く | 対応 Unity バージョンが Major `6000 (Unity 6)`、Minor `0` になっている |
| Major のポップアップを開く | `6000 (Unity 6)`、`2023`〜`2017`、`5` が並ぶ |
| Major `6000` のまま Minor のポップアップを開く | `0`〜`6` が並ぶ |
| Major を `5` にする | Minor が `0` になり、選択肢は `0`〜`6` |
| Major を `2022` にする | Minor が `1` になり、選択肢は `1`〜`3` |
| Major `6000`、Minor `6` にして Create | `"unity": "6000.6"` が出力される |
| package.json の `unity` を `"6000.7"` に書き換えて Load | Minor に `7（一覧外）` が選ばれ、警告が出る。そのまま Create すると `"unity": "6000.7"` |
| Minor で `3` を選び直す | 選択肢から `7（一覧外）` が消える |
| `unity` を `"2024.1"` にして Load | Major `2024（一覧外）`、Minor `1（一覧外）` になり、警告が出る |
| `unity` を `"6000"` にして Load | `6000.0` になり、警告が出る |
| `unity` のキーを消して Load | 例外が出ず、`6000.0` になり、警告が出る。他の項目は読み込まれる |
| `unity` を `"abc"` にして Load | 例外が出ず、`6000.0` になり、警告が出る |
| `version` のキーを消して Load | 例外が出ず、`0.0.0` になり、警告が出る |
| `version` を `"1.2"`、`"1.2.3.4"`、`"1.2.3-preview.1"`、`"a.b.c"` に変えてそれぞれ Load | それぞれ `1.2.0`、`1.2.3`、`1.2.3`、`0.0.0` になり、警告が出る |
| `unity` を `"2022.3"`、`version` を `"1.2.3"` にして Load | 警告が出ない |
| （プレビュー版のパッケージが入っている場合）外部パッケージのポップアップでそのパッケージを選ぶ | 例外が出ず、`X.Y.Z` 部分が入り、警告が出る |

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす
- [ ] Unity でコンパイルエラー・警告が出ない（ユーザーが確認）
- [ ] 関連テストの修正・追加が完了している（自動テストは無いため、上の手動確認で代える）
- [x] 影響のある永続的ドキュメント（Docs/）の更新が完了している
- [x] CHANGELOG・README の更新と、配布フォルダへの同期が完了している
