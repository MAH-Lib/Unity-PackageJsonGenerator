# 要求内容：ライセンス・サンプル・Reset の修正

> 作業ディレクトリ：`.steering/20261020-license-sample-fix/` ／ 作成日：2026-10-20 ／ ステータス：承認済み
> 対象：パッケージ `com.mah.package-json-generator` v1.0.1 ／ 開発環境 Unity 6000.3.26f1（対応 Unity 6000.0 以上）
> 関連する永続的ドキュメント：`Docs/functional-design.md` §3.3（ライセンス）・§3.5（出力例のパス区切り）・§4.3（生成前チェック）・§4.4（サンプルのパス）・§8（K-3・K-9・K-10・K-16・K-18 を修正済みにする。新しく見つけた不具合を K-25 として追加）。いずれも更新要

## 1. 変更・追加する機能の説明

ライセンスで `Other` を選んだときに入力した値が保存されず、必ず生成エラーになる問題（K-3）を直す。
サンプルのパスが Windows では `\` 区切りで出力される問題や、フォルダ選択の結果が正しく変換されない問題（K-9）、初期値が場面ごとに違う問題（K-10）を直す。あわせて、サンプルのパスを手で入力できない問題（K-25）も直す。
さらに、`PackageOptionalSettings.Reset()` が `hideInEditor` を戻さない問題（K-16）と、Load したパッケージ名が検証されない問題（K-18）を直す。
計画書の作業単位 #2。

## 2. ユーザーストーリー

- MIT などの一覧に無いライセンスを使うパッケージの作者として、SPDX ID を自分で入力して package.json に出力したい
- サンプル付きのパッケージの作者として、どの OS で作っても Package Manager が正しく読めるパスで出力してほしい
- サンプル付きのパッケージの作者として、`Browse` で選んでも、手で入力しても、同じ形のパスにしてほしい
- 既存の package.json を読み込む利用者として、パッケージ名が正しくなければ、そのまま出力される前に気づけるようにしてほしい

## 3. 受け入れ条件

**ライセンス（K-3、K-18）**
- [ ] `Other` を選ぶと入力欄が表示され、入力した値（例 `BSD-2-Clause`）が `license` に出力される。前後の空白は取り除く
- [ ] `Other` で入力欄が空のまま Create すると、今と同じ「ライセンスが入力されていません」のエラーで止まる
- [ ] 一覧に無い SPDX ID（例 `"license": "ISC"`）を持つ package.json を Load すると、`Other` が選ばれ、入力欄に `ISC` が表示される。そのまま Create すると `ISC` が出力される

**パッケージ名（K-18）**
- [ ] 大文字を含む名前（例 `Com.Example.Tool`）を Load すると、小文字にして読み込む
- [ ] 小文字にしても形式に合わない名前（例 `my package`、空）を Load すると、その値のまま入力欄に表示し、Console に警告が出る
- [ ] 名前が形式に合わないまま Create すると、エラーが出てファイルは作成・上書きされない

**サンプルのパス（K-9、K-10、K-25）**
- [ ] 出力される `samples[].path` は、どの OS でも `/` 区切りで、`Samples~/<フォルダ>` の形になる（例 `Samples~/Basic`、`Samples~/Basic/Scene`）
- [ ] `Browse` で `.../Samples/Basic` または `.../Samples~/Basic` を選ぶと、`Samples~/Basic` になる
- [ ] パス欄に `Samples~/Basic` を 1 文字ずつ手で入力でき、確定（Enter かフォーカスアウト）した時点で変換・検証される（K-25）
- [ ] `Samples` / `Samples~` はパスの区切りごとの名前で判定する（`MySamples/Basic` のような部分一致では変換しない）
- [ ] `Samples` / `Samples~` を含まないパスや、`Samples~` の後ろが空のパスを入力・選択した場合は、前の値のまま変わらない
- [ ] サンプルを追加したとき（1 件目・2 件目以降とも）と Reset を押したときの初期値が、どれも `Samples~` になる
- [ ] 以前のバージョンで出力した `Samples~\\Basic`（`\` 区切り）を Load すると `Samples~/Basic` として読み込まれる

**Reset（K-16）**
- [ ] `PackageOptionalSettings.Reset()` を呼ぶと `HideInEditor` が `false` に戻る

## 4. 制約事項

- 編集するのは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` だけ。作業の最後に配布フォルダへ同期する
- 既存の `.meta` の GUID を変えない
- エラーと警告の通知は、今の作りに合わせて Console（`Debug.LogError` / `Debug.LogWarning`）に日本語で出す。画面上での通知は #4（K-12）で行う
- Unity 6000.0 に無い API を使わない。パッケージの依存を増やさない
- 識別子の綴りの誤りは直さない（#6 で行う）。`PackageJsonCreateWindow` の責務の分割はしない（#5 で行う）

## 5. スコープ外（今回やらないこと）

- `Other` に入力した値が正しい SPDX ID かどうかの検証（SPDX の一覧をコードに持つことになるため）
- ライセンス `None`（`UNLICENSED`）の追加と LICENSE.md の作成：#9 で行う
- サンプルのフォルダが実際にあるかの確認：#12 で行う
- Load に失敗したときに入力を消さないこと（K-13）：#4 で行う。今回は、失敗時のリセットの動きは変えない
- サンプルの表示名・概要が空のときの扱い（今と同じく、空なら出力しない）
- `Samples~` 以外の場所（例 `Documentation~`）をサンプルのパスとして使うこと
