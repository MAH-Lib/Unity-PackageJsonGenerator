# タスクリスト：ウィンドウの責務分割

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-21 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用フォルダ」）、テストは `src/PackageJsonGenerator/Assets/@MAH/Tests/`、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | 分割前の出力を記録する：決めた入力で Create した package.json を scratchpad に保存しておく | ✅ | 比較用のファイルがある | ツール自身の package.json（b066e79 と同じ内容）を scratchpad の baseline-package.json に保存 |
| 2 | `PackageJsonValidator` を作り、Window の検証処理を移す | ✅ | Window に正規表現と `IsValid*` が残っていない | |
| 3 | `PackageSettingsConverter` を作り、Window の変換処理を移す。警告は `List<string>` で返す | ✅ | Window に `ConvertTo*` が残っていない。Converter に `Debug` / `EditorGUILayout` が無い | |
| 4 | `Drawer/` に `FieldErrorStore`、`DrawerUtility`、4 つの Drawer を作り、描画処理を移す。`DrawVersionFields` で負の値を 0 にする | ✅ | Window に各セクションの描画が残っていない | K-22 |
| 5 | Window を整理する（Drawer の保持、`OnEnable` の初期化、Create / Load から Validator・Converter を呼ぶ） | ✅ | Window が design.md §2 の役割だけになる | |
| 6 | `AssemblyInfo.cs`、テスト用 asmdef、2 つのテストクラスを作る | ✅ | design.md §2.2 の内容がある | |
| 7 | 新しいファイル・フォルダの `.meta` を作り、改行コードを CRLF にそろえる | ✅ | すべての新しいファイル・フォルダに新しい GUID の `.meta` がある。既存の GUID が変わっていない | |
| 8 | Unity でコンパイルし、Editor.log でエラーが無いことを確認する | ✅ | エラーが無い | 2026-10-21 00:37 のコンパイルでエラーなし（本体とテストの DLL が作られたことを確認） |
| 9 | `Docs/functional-design.md`、`CONTRIBUTING.md`、`CLAUDE.md` を更新する | ✅ | design.md §4 の箇所が更新されている | |
| 10 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` の Fixed に K-22 を英語で追記する | ✅ | 書かれている | |
| 11 | CONTRIBUTING.md §4 の手順で配布フォルダに同期する | ✅ | `diff -r` で差分が無い。配布フォルダにテストが無い | |
| 12 | Unity での確認手順をユーザーに示す | ✅ | 下の手順を提示済み | |

## Unity での確認手順（タスク 12 で提示する）

| 手順 | 期待結果 |
|---|---|
| Window > General > Test Runner を開き、EditMode タブで Run All | このツールのテストがすべて成功（緑）になる |
| ウィンドウを開く | 見た目が分割前と同じ。Console にエラーが出ない |
| 全項目を入力して Create し、分割前に保存した package.json と比べる | 内容が一致する |
| パッケージのバージョンの Major に `-1` を入力 | `0` になる |
| 外部パッケージのバージョンに `-5` を入力 | `0` になる |
| #4 の確認手順（入力エラー・操作結果・上書き確認・壊れた JSON・再コンパイル）をもう一度行う | #4 と同じ結果になる |
| 外部パッケージのポップアップで登録済みのパッケージを選ぶ | 名前とバージョンが入る |
| Foldout を閉じた状態でスクリプトを再コンパイルする | Foldout が閉じたまま残る |

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす
- [x] Unity でコンパイルエラー・警告が出ない
- [ ] EditMode テストがすべて成功する
- [x] 影響のある永続的ドキュメント（Docs/、CONTRIBUTING.md、CLAUDE.md）の更新が完了している
- [x] CHANGELOG の更新と、配布フォルダへの同期が完了している
