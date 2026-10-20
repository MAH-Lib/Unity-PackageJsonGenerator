# タスクリスト：依存パッケージの修正

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-20 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用フォルダ」）、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | `VersionOperatorConverter` の中身を `TryExtractVersion` に置き換える | ✅ | design.md §2.1 の表どおりの戻り値になる | ファイル名・クラス名・`.meta` は変えない |
| 2 | `DependenciesInfo` から `Operator`・`OperatorVersion` を削除する | ✅ | フィールド・コンストラクタ・`Reset()` に該当行が無い | |
| 3 | `DrawDependenciesSettings` から条件・範囲の入力欄を削除し、名前の欄を `DelayedTextField` にする。不要になった定数とレイアウト変数を削除する | ✅ | 依存の各行が「名前＋ポップアップ」「バージョン（Major/Minor/Patch）」の 2 行になる | 手入力で名前を変えたらポップアップの位置を `FindPackageIndex` で合わせる |
| 4 | `FindPackageIndex` を追加し、`ConvertToDependenciesInfo` を `TryExtractVersion` と警告に対応させる | ✅ | 演算子付き・読めない値で design.md §2.2 の警告が出て、例外が出ない | |
| 5 | `ConvertToDependencies` で不正な名前の行を警告付きで読み飛ばし、値を `X.Y.Z` だけにする | ✅ | `com.` の行が出力されず警告が出る。有効な行が 0 件なら `dependencies` が出力されない | `Add` → `TryAdd` |
| 6 | `IsCanCreatePackageJson` に名前の重複チェックを追加する | ✅ | 重複があるとエラーが出てファイルが作成・上書きされない | |
| 7 | `Definition/VersionOperater.cs` と `.meta` を削除する | ✅ | `VersionOperator` を参照する箇所が無く、Unity でコンパイルエラーが出ない | Unity での確認はユーザーが行う |
| 8 | `Docs/functional-design.md` を更新する | ✅ | §2.1・§3.1・§4.3・§4.5 が新しい仕様になり、§8 の K-1・K-2・K-8・K-11・K-24 が修正済み、K-19 から `VersionOperater.cs` が外れている | 冒頭の最終更新日も更新 |
| 9 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` に英語で追記する | ✅ | Fixed / Removed に今回の変更が書かれている | |
| 10 | `README.md`（92 行目付近）と `README.ja.md`（90 行目付近）の「比較条件や範囲指定を使わない」注意書きを、完全一致のバージョンだけを出力する旨に直す | ✅ | 両方の内容が揃っている | |
| 11 | CONTRIBUTING.md §4 の手順で配布フォルダ `PackageJsonGenerator/` に同期する | ✅ | `diff -r` で差分が無い。`VersionOperater.cs` と `.meta` が配布フォルダからも消えている | robocopy `/MIR` |
| 12 | Unity での確認手順をユーザーに示す | ✅ | 下の「Unity での確認手順」を提示済み | |

## Unity での確認手順（タスク 12 で提示する）

| 手順 | 期待結果 |
|---|---|
| ウィンドウを開き、外部パッケージで Add を押す | 行に「条件」の選択が無く、バージョンは Major / Minor / Patch だけ |
| 名前の欄に `com.example.tool` を 1 文字ずつ入力して Enter | 名前が `com.example.tool` になる |
| 名前の欄に `Com.Example` と入力して Enter | `com.example` に小文字化される |
| 名前の欄に `abc` と入力して Enter | 前の値に戻る（エラーの画面表示は #4 で行う） |
| ポップアップで登録済みのパッケージ（例 `com.unity.nuget.newtonsoft-json`）を選ぶ | 名前とバージョンが入る |
| 同じ名前の行を 2 つ作って Create | Console に重複のエラーが出て、package.json は作成・更新されない |
| 名前が `com.` のままの行と正しい行を作って Create | Console に警告が出て、正しい行だけが `dependencies` に出力される |
| 依存の行をすべて `com.` のままにして Create | `dependencies` キーが出力されない |
| `dependencies` を `{"com.a.b": ">=1.2.0", "com.c.d": "1.0.0 - 2.0.0", "com.e.f": "1.0.0-preview.1", "com.g.h": "3.0.0"}` に手で書き換えて Load | 例外が出ない。`com.a.b` は 1.2.0、`com.c.d` は 1.0.0 で読み込まれ、それぞれ変換の警告が出る。`com.e.f` は読み込まれず警告が出る。`com.g.h` は 3.0.0 で警告なし |
| そのまま Create | `dependencies` の値がすべて `"X.Y.Z"` 形式 |
| Package Manager の「+」→「Install package from disk...」で作った package.json を選ぶ | `dependencies` が原因のエラーが出ない（依存先が実在しない場合の解決エラーは除く） |

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす（Unity での確認待ち）
- [ ] Unity でコンパイルエラー・警告が出ない（ユーザーが確認）
- [ ] 関連テストの修正・追加が完了している（自動テストは無いため、上の手動確認で代える）
- [x] 影響のある永続的ドキュメント（Docs/）の更新が完了している
- [x] CHANGELOG・README の更新と、配布フォルダへの同期が完了している
