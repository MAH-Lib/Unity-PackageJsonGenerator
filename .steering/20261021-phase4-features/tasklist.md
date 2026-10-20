# タスクリスト：フェーズ 4 機能の追加（#7〜#12）

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-21 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/Editor/`、テストは `src/PackageJsonGenerator/Assets/@MAH/Tests/Editor/`、ドキュメントはリポジトリの直下

状態：⚪未着手 → 🔵作業中 → ✅完了 → 🟢ユーザー確認済み（🔴ブロック中はブロック理由を備考へ）

## 各ステップの共通の流れ

1. 実装とテストの追加・修正（新しいファイルには新しい GUID の `.meta`、改行は CRLF）
2. Unity でコンパイルし、Editor.log でエラーが無いことを確認する
3. ドキュメントの更新（`Docs/functional-design.md`、`CHANGELOG.md` の `[Unreleased]`、`README.md` / `README.ja.md`）
4. 配布フォルダへの同期（CONTRIBUTING.md §4）と `diff -r` での確認
5. Unity での確認手順を示して止まる。ユーザーの確認と指示を待って次のステップへ。コミットは指示があったときにステップごとに行う

## ステップ 1：英語・日本語の切り替え（#7）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 1-1 | `LanguageType`・`TextId`・`LocalizedText` を作り、全文言を英語・日本語で登録する | 🟢 | 表に抜けが無い |
| 1-2 | 各 Drawer・Window・Validator・Converter・FileUtility の日本語の定数を `LocalizedText` に置き換える | 🟢 | コードに画面・メッセージ用の日本語の定数が残っていない |
| 1-3 | Window の上部に言語の選択を付け、切り替えでエラーと操作結果を消す。ポップアップの先頭項目を言語に合わせる | 🟢 | 切り替えで表示がすぐ変わる |
| 1-4 | `LocalizedTextTests` を追加し、既存テストを両方の言語で実行する形に直す | 🟢 | テストが追加されている |
| 1-5 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## ステップ 2：プレビュー版のバージョン（#8）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 2-1 | `PreRelease` の項目、`IsValidPreRelease`、バージョン文字列の変換、`VersionOperatorConverter` の正規表現を直す | 🟢 | design.md §2.1 のとおり |
| 2-2 | `DrawVersionFields` にプレリリースの入力欄を足し、本体・依存パッケージで使う。ポップアップの選択で表記も入れる | 🟢 | 両方の欄で入力・エラー表示ができる |
| 2-3 | テストを追加・修正する | 🟢 | design.md §2.2 のとおり |
| 2-4 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## ステップ 3：ライセンス None と LICENSE.md の作成（#9）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 3-1 | `LicenseType.None`、`SPDXLicenseConverter` の対応を追加する | 🟢 | `None` ⇔ `UNLICENSED` |
| 3-2 | `LicenseFileUtility` を作り、ボタンと Window の作成処理（上書き確認付き）を付ける | 🟢 | Proprietary のときだけボタンが出て、LICENSE.md ができる |
| 3-3 | テストを追加する | 🟢 | design.md §3.3 のとおり |
| 3-4 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## ステップ 4：読み込んだパッケージの記憶（#10）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 4-1 | `PackageSnapshot`・`PackageSessionStore` を作る | 🟢 | design.md §4.2 のとおり |
| 4-2 | Window に一覧・切り替え・一覧から外す、Load / Create 時の記憶、`OnDisable` での記憶を付ける | 🟢 | 切り替えで保存前の編集まで戻る |
| 4-3 | テストを追加する | 🟢 | design.md §4.3 のとおり |
| 4-4 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## ステップ 5：未対応キーの保持と JSON プレビュー（#11）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 5-1 | `ExtraData`・`ExtraJson` と変換、`SerializePackageJson` の切り出しを行う | 🟢 | 未対応キーが Create で残る |
| 5-2 | 未対応キーの一覧とクリアボタン、JSON プレビューを付ける | 🟢 | 表示と更新ができる |
| 5-3 | テストを追加する | 🟢 | design.md §5.2 のとおり |
| 5-4 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## ステップ 6：現在の Unity の反映と Create 前の確認（#12）

| No | タスク | 状態 | 完了条件 |
|---|---|---|---|
| 6-1 | `TryParseEditorVersion` とボタンを付ける | 🟢 | ボタンで `unity` と `unityRelease` が入る |
| 6-2 | `GetCreateWarnings` を作り、Create で警告として出す | 🟢 | 警告が出て、作成は止まらない |
| 6-3 | テストを追加する | 🟢 | design.md §6.2 のとおり |
| 6-4 | 共通の流れの 2〜5 | 🟢 | 確認手順を提示済み |

## 完了の定義（Definition of Done）

- [x] requirements.md の受け入れ条件をすべて満たす（各ステップでユーザーが確認済み）
- [x] Unity でコンパイルエラーが出ず、EditMode テストがすべて成功する（2026-10-21 08:40 に 230 件成功）
- [x] 各ステップのドキュメントの更新と配布フォルダへの同期が完了している
