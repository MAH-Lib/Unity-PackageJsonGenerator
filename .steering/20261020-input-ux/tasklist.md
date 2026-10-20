# タスクリスト：入力まわりの UX 改善

> 対象：`design.md`（承認済み版） ／ 最終更新：2026-10-20 ／ ステータス：承認済み
> パスの起点：コードは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/`（以下「開発用フォルダ」）、ドキュメントはリポジトリの直下

## タスク

状態：⚪未着手 → 🔵作業中 → ✅完了（🔴ブロック中はブロック理由を備考へ）

| No | タスク | 状態 | 完了条件 | 備考 |
|---|---|---|---|---|
| 1 | 入力状態のクラス 5 つに `[Serializable]` / `[SerializeField]` を付ける | ✅ | design.md §2 の表のとおり | K-15 |
| 2 | Window のフィールドに `[SerializeField]` と `_isInitialized` を付け、`OnEnable` を未初期化のときだけ初期化する形にする | ✅ | 再コンパイルで入力が残る | K-15 |
| 3 | `PackageJsonFileUtility` を `TrySavePackageJson` / `TryLoadPackageJson` / `GetPackageJsonPath` に置き換える | ✅ | §2.3 の場面で例外が出ずにエラー文が返る | K-26、K-27 |
| 4 | Window にメッセージ用のフィールドと private メソッド（`//====メッセージ====`）を追加し、`Debug.LogError` / `Debug.LogWarning` を置き換える | ✅ | Window に `Debug.Log*` の直接呼び出しが残っていない | K-12 |
| 5 | §2.1 の 8 種類の欄で、値が変わったときだけ検証し、エラーを表示する | ✅ | エラーの表示と解除が受け入れ条件どおり | K-12 |
| 6 | `CreatePackageJson()` / `LoadPackageJson()` を追加し、ボタンから `delayCall` で呼ぶ。上書き確認と結果の表示を入れる | ✅ | §2.2 の流れになる。Load 失敗で入力が残る | K-12、K-13、K-14 |
| 7 | 構文チェックと Unity のコンパイル結果（Editor.log）を確認する | ✅ | エラーが無い | |
| 8 | `Docs/functional-design.md` を更新する | ✅ | §2.2・§2.3・§4・§5・§6.2・§7 が新しい仕様になり、§8 の K-12〜K-15 が修正済み、K-26・K-27 が追加されている | |
| 9 | 開発用フォルダの `CHANGELOG.md` の `[Unreleased]` に英語で追記する | ✅ | Added / Fixed に書かれている | |
| 10 | `README.md` / `README.ja.md` の既知の制限事項を直す | ✅ | 両方の内容が揃っている | |
| 11 | CONTRIBUTING.md §4 の手順で配布フォルダに同期する | ✅ | `diff -r` で差分が無い | |
| 12 | Unity での確認手順をユーザーに示す | ✅ | 下の手順を提示済み | |

## Unity での確認手順（タスク 12 で提示する）

| 手順 | 期待結果 |
|---|---|
| パッケージ名に `abc` と入力して Enter | 名前は前の値のまま、欄の下に赤いエラーが出る |
| 続けて `com.example.tool` と入力して Enter | エラーが消え、名前が変わる |
| E-mail に `abc`、著者 URL に `example.com` と入力してそれぞれ Enter | それぞれの欄の下にエラーが出る |
| E-mail を空にして Enter | E-mail のエラーが消える |
| 外部パッケージ名に `abc`、サンプルのパスに `Foo/Bar` を入力してそれぞれ Enter | それぞれの行の下にエラーが出る |
| すべての項目を入力した状態で、任意のスクリプトを保存して再コンパイルさせる | 入力内容・ライセンス・フォルダーパス・Foldout・スクロール位置が残っている |
| ウィンドウを閉じて開き直す | 初期状態に戻る |
| フォルダーパスを `Assets/NotExist` にして Create | ボタンの上に「フォルダが存在しません」の赤い表示が出て、例外は出ない |
| 同じパスで Load | 「package.json が存在しません」の表示が出て、入力はそのまま残る |
| フォルダーパスを `Assets` に戻し、必須項目を空にして Create | 「〜が入力されていません」の表示が出る |
| 必須項目を入力して Create（ファイルが無い状態） | 確認なしで作成され、「作成しました : Assets/package.json」が出る |
| もう一度 Create | 上書きの確認ダイアログが出る。キャンセルすると「上書きをキャンセルしました」、上書きを選ぶと作成される |
| 作った package.json の中身を `{ "name": ` だけにして Load | 例外が出ず「読み込めませんでした」の表示が出て、入力はそのまま残る |
| package.json を元に戻し、`unity` を `"6000"` にして Load | 「読み込みました」と「警告 1 件」の黄色い表示が出る |
| 入力エラーが出ている状態で、正しい package.json を Load | 入力エラーの表示がすべて消える |

テスト後は、作った `Assets/package.json`（と `.meta`）を削除してください（誤ってコミットしないため）。

## 完了の定義（Definition of Done）

- [ ] requirements.md の受け入れ条件をすべて満たす（Unity での確認待ち）
- [x] Unity でコンパイルエラー・警告が出ない（Editor.log で確認）
- [ ] 関連テストの修正・追加が完了している（自動テストは無いため、上の手動確認で代える）
- [x] 影響のある永続的ドキュメント（Docs/）の更新が完了している
- [x] CHANGELOG・README の更新と、配布フォルダへの同期が完了している
