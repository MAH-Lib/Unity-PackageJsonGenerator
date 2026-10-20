# タスクリスト：ライセンス・サンプル・Reset の修正

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-20 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用フォルダ」）、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | `DrawLicenseInfoSettings` で `Other` の入力値を保存し、`ConvertToPackageJsonInfo` で `License` を `Trim()` する | ✅ | `Other` に入力した値が `license` に出力される | K-3 |
| 2 | `PackageOptionalSettings.Reset()` に `HideInEditor = false;` を追加する | ✅ | Reset で `HideInEditor` が戻る | K-16 |
| 3 | `IsValidSampleFolderpath` を design.md §2.2 の振る舞いに書き直す | ✅ | §2.2 の表どおりの結果になる | K-9 |
| 4 | `DrawSamplesInfoSettings`：パス欄を `DelayedTextField` にし、初期値を `Samples~` にそろえ、Reset が古い値で上書きされないようにする | ✅ | 追加・Reset の初期値がすべて `Samples~`。Reset が効く | K-10、K-25 |
| 5 | `ConvertToPackageSettings`：パッケージ名の小文字化と警告、サンプルのパスの `\` → `/` の置き換えを追加する | ✅ | 不正な名前で警告が出る。`Samples~\\Basic` が `Samples~/Basic` で読み込まれる | K-18 |
| 6 | `IsCanCreatePackageJson` にパッケージ名の形式チェックを追加する | ✅ | 不正な名前でエラーが出てファイルが作成されない | K-18 |
| 7 | 構文チェックと Unity の Console を確認する（Unity MCP が使える場合） | ✅ | エラーが無い | 最終確認はユーザーが行う |
| 8 | `Docs/functional-design.md` を更新する | ✅ | §3.3・§3.5・§4.1・§4.3・§4.4 が新しい仕様になり、§8 の K-3・K-9・K-10・K-16・K-18 が修正済み、K-25 が追加されている | |
| 9 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` の Fixed に英語で追記する | ✅ | 今回の修正が書かれている | |
| 10 | `README.md` と `README.ja.md` の既知の制限事項から「Other が使えない」の行を削除する | ✅ | 両方の内容が揃っている | |
| 11 | CONTRIBUTING.md §4 の手順で配布フォルダに同期する | ✅ | `diff -r` で差分が無い | robocopy `/MIR` |
| 12 | Unity での確認手順をユーザーに示す | ✅ | 下の手順を提示済み | |

## Unity での確認手順（タスク 12 で提示する）

| 手順 | 期待結果 |
|---|---|
| ライセンスで `Other` を選び、入力欄に ` BSD-2-Clause ` と入力して Create | `"license": "BSD-2-Clause"`（前後の空白なし）が出力される |
| `Other` の入力欄を空にして Create | Console に「ライセンスが入力されていません」が出て、ファイルは更新されない |
| package.json の `license` を `"ISC"` に書き換えて Load | ライセンスが `Other` になり、入力欄に `ISC` が表示される |
| package.json の `name` を `"Com.Example.Tool"` に書き換えて Load | 名前が `com.example.tool` で読み込まれ、警告は出ない |
| package.json の `name` を `"my package"` に書き換えて Load | 名前が `my package` のまま表示され、Console に警告が出る |
| そのまま Create | Console に「パッケージ名の形式が正しくありません」が出て、ファイルは更新されない |
| サンプルで Add を 2 回押す | 2 件とも、パスが `Samples~` になっている |
| 1 件目のパス欄に `Samples~/Basic` を 1 文字ずつ入力して Enter | パスが `Samples~/Basic` になる |
| 2 件目の Browse で、名前が `Samples` のフォルダの下にあるフォルダ（例 `Assets/Samples/Demo`）を選ぶ | パスが `Samples~/Demo` になる |
| パス欄に `MySamples/Basic` と入力して Enter | 前の値のまま変わらない |
| 1 件目の Reset を押す | パスが `Samples~` に戻る |
| サンプルのパスを `Samples~/Basic` にして Create し、ファイルを開く | `"path": "Samples~/Basic"`（`\` ではなく `/`）で出力されている |
| package.json の `path` を `"Samples~\\Basic"` に書き換えて Load | パスが `Samples~/Basic` で表示される |
| `UMP除外フラグ` にチェックを入れてから、存在しないフォルダを指定して Load | Console にファイルが無い旨が出て入力がリセットされ、チェックも外れている（K-16。入力が消えるのは K-13 で、#4 で直す） |

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす（Unity での確認待ち）
- [ ] Unity でコンパイルエラー・警告が出ない（ユーザーが確認）
- [ ] 関連テストの修正・追加が完了している（自動テストは無いため、上の手動確認で代える）
- [x] 影響のある永続的ドキュメント（Docs/）の更新が完了している
- [x] CHANGELOG・README の更新と、配布フォルダへの同期が完了している
