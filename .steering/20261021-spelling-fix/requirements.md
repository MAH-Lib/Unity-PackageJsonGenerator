# 要求内容：綴りの修正（リネーム専用）

> 作業ディレクトリ：`.steering/20261021-spelling-fix/` ／ 作成日：2026-10-21 ／ ステータス：承認済み
> 対象：開発環境 Unity 6000.3.26f1
> 関連する永続的ドキュメント：`Docs/functional-design.md` §2.2・§3.1・§4.4・§6.2 など識別子やラベルを書いている箇所、§8（K-19 を修正済みにする）。`README.md` / `README.ja.md`（ラベル `UMP除外フラグ`）。いずれも更新要

## 1. 変更・追加する機能の説明

識別子と UI ラベルの綴りの誤り（K-19）を直す。**振る舞いは変えない**（ラベル `UMP除外フラグ` → `UPM除外フラグ` の表示だけが変わる）。
CLAUDE.md の「綴りの修正は専用の変更として分ける」に従い、他の修正は混ぜない。計画書の作業単位 #6。#7（言語切り替え）で文字列を整理する前に、名前を正しくしておく。

## 2. ユーザーストーリー

- このツールの開発者として、検索やコード補完で迷わないように、識別子の綴りが正しくなっていてほしい
- パッケージの作者として、画面のラベルの誤字（UMP）を正しく（UPM）表示してほしい

## 3. 受け入れ条件

- [ ] 次の綴りの誤りが、コード・テスト・ドキュメントから無くなる

| 種類 | 誤り | 正しい名前 | 備考 |
|---|---|---|---|
| フィールド | `DependenciesInfo.PacckageName` | `PackageName` | §8 K-19 に記載 |
| フィールド | `_pacckageJsonPath`（Window） | `_packageJsonPath` | §8 K-19 に記載 |
| 定数 | `PACKGAGE_JSON_OUTPUT_DEFAULT_PATH`、`PACKGAGE_JSON_OUTPUT_FILE_NAME` | `PACKAGE_JSON_OUTPUT_DEFAULT_PATH`、`PACKAGE_JSON_OUTPUT_FILE_NAME` | §8 K-19 に記載（`PACKGAGE_*`） |
| UI ラベル | `UMP除外フラグ` | `UPM除外フラグ` | §8 K-19 に記載 |
| 定数 | `*_ERORR_MESSAGE`（`PackageJsonFileUtility` の 7 個） | `*_ERROR_MESSAGE` | 今回見つけたもの |
| 定数 | `PACKAGE_INFO_UNITY_RELEASE_VERSITON_LABEL` | `PACKAGE_INFO_UNITY_RELEASE_VERSION_LABEL` | 今回見つけたもの |
| ローカル変数 | `depensencies_layout` | `dependencies_layout` | 今回見つけたもの |
| ローカル変数 | `apth_button_layout` | `path_button_layout` | 今回見つけたもの |
| メソッド | `IsValidSampleFolderpath` | `IsValidSampleFolderPath` | 今回見つけたもの（単語の区切りの大文字） |

- [ ] ドメインリロードの前に入力していた外部パッケージ名とフォルダーパスが、リネーム後の再コンパイルでも消えない（シリアライズされるフィールドの名前が変わるため）
- [ ] Unity でコンパイルエラーが出ず、EditMode テストがすべて成功する
- [ ] 出力される package.json は変わらない

## 4. 制約事項

- 編集するのは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` と、テスト（`Assets/@MAH/Tests/Editor/`）だけ。作業の最後に配布フォルダへ同期する
- ファイル名は変えない。`.meta` も触らない
- 名前の変更以外の修正（ロジック・コメントの言い回し・構成の変更）は混ぜない
- `.steering/` の過去の文書と、§8 の他の K-xx の「該当箇所」に書かれた古い名前は、当時の記録なので書き換えない

## 5. スコープ外（今回やらないこと）

- 意味と名前が合っていない識別子の改名（例：ラベルなのに `..._BUTTON` で終わる `PACKAGE_INFO_JSON_FILE_PATH_BUTTON`、変換もする `IsValidSampleFolderpath` の `IsValid`）
- 文字列の多言語化：#7 で行う
- 前回コミットした asmdef の `defineConstraints` の見直し
