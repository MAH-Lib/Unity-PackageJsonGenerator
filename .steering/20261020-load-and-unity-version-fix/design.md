# 設計：Load 時の例外と Unity バージョンの修正

> 対象：`requirements.md`（承認済み版） ／ 作成日：2026-10-20 ／ ステータス：承認済み
> 対象：パッケージ `com.mah.package-json-generator` v1.0.1 ／ 開発環境 Unity 6000.3.26f1（対応 Unity 6000.0 以上）

## 1. 実装アプローチ

対応 Unity バージョンは、Major ごとの enum（`MajorVersion`、`MinorVersion_*`）をやめ、`UnityVersionConverter` の中の「Major（`int`）→ Minor の配列」の表 1 つで持つ。画面は `EnumPopup` ではなく、表から作った文字列の配列で `Popup` を出す。そのとき今の値が表に無ければ「一覧外」として配列の末尾に足すので、一覧に無い値を Load しても値を失わない。
Load の変換は、例外を投げる処理（`throw new NotImplementedException()`）をすべて「初期値にして `false` を返す」処理に変え、呼び出し側で項目名付きの警告を出す。

- 却下：enum に `Version_3`〜`Version_6` を足すだけの案 → K-7（一覧に無い値の扱い）が直らず、新しい Unity が出るたびに enum と変換表の複数か所を直す必要が残るため
- 却下：新しいクラス `UnityVersionTable` を作る案（計画書の仮名） → 表と変換処理は同じ責務なので、既存の `UnityVersionConverter`（GUID もそのまま）に置く
- 却下：Minor を自由入力（`IntField`）にする案 → 存在しないバージョンを打ててしまうため。一覧外は Load したときだけ扱う（requirements で承認済み）

## 2. 変更するコンポーネント

パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/Editor/` からの相対パス。

| 対象（クラス/ファイル） | 変更内容 | 新規/修正 |
|---|---|---|
| `Utility/UnityVersionConverter.cs` | 中身を書き換える（§2.1）。ファイル名・クラス名・`.meta` は変えない | 修正 |
| `Definition/UnityVersion/`（`MajorVersion.cs` と `MinorVersion_*.cs` の 10 ファイル、各 `.meta`、フォルダの `UnityVersion.meta`） | 使わなくなるので削除する（パッケージ内部の `internal` 型なので、利用者側の参照は無い） | 削除 |
| `Definition/PackageRequiredSettings.cs` | `MajorVer` の型を `MajorVersion` から `int` にする。初期値を `UnityVersionConverter.DEFAULT_MAJOR_VERSION`（6000）・`DEFAULT_MINOR_VERSION`（0）にする | 修正 |
| `PackageJsonCreateWindow.cs`：`DrawUnityVersionSettings` | `EnumPopup` を `Popup` に置き換える（§2.2） | 修正 |
| `PackageJsonCreateWindow.cs`：`ConvertToUnityVersionString` | 引数を `int major` にし、`"{major}.{minor}"` を返す | 修正 |
| `PackageJsonCreateWindow.cs`：`ConvertToMajorVersion` / `ConvertToMinorVersion` | 削除し、`UnityVersionConverter.TryParseUnityVersion` を使って `ConvertToPackageSettings` の中で警告を出す（§2.3） | 削除・修正 |
| `PackageJsonCreateWindow.cs`：`ConvertToVersionVector` | `TryConvertToVersionVector(string version, out Vector3Int result)` に変える。正規表現で先頭の数字を読み、例外を出さない。値を補ったり切り捨てたりしたら `false` を返す（§2.4） | 修正 |
| `PackageJsonCreateWindow.cs`：`SetDependenciesInfo`、`ConvertToDependenciesInfo` | `TryConvertToVersionVector` を使う。`SetDependenciesInfo` では `false` のとき警告を出す（プレビュー版の登録済みパッケージを選んだとき） | 修正 |

### 2.1 `UnityVersionConverter` の新しい中身

| メンバー | 内容 |
|---|---|
| `internal const int DEFAULT_MAJOR_VERSION = 6000;` / `DEFAULT_MINOR_VERSION = 0;` | 初期値（このツール自身の対応下限と同じ） |
| `private static readonly int[] MajorVersions` | `6000, 2023, 2022, 2021, 2020, 2019, 2018, 2017, 5`（画面に並べる順） |
| `private static readonly Dictionary<int, int[]> MinorVersionMap` | 6000：0〜6 ／ 2023〜2020：1〜3 ／ 2019〜2017：1〜4 ／ 5：0〜6 |
| `GetMajorVersions()` / `GetMinorVersions(int major)` | 表の値を返す。表に無い Major なら空の配列 |
| `IsKnownVersion(int major, int minor)` | 表にある組み合わせかどうか |
| `GetMajorVersionLabel(int major)` | `6000` は `"6000 (Unity 6)"`、それ以外は数字のまま |
| `TryParseUnityVersion(string unity, out int major, out int minor, out bool has_minor)` | `"Major.Minor"` を読む。空・数字でない場合は初期値を入れて `false`。Minor が無い場合は、その Major の一覧の先頭（表に無い Major なら 0）を入れ、`has_minor = false` で `true` を返す |

新しい Unity が出たときは、`MinorVersionMap`（新しい Major なら `MajorVersions` も）に値を足すだけでよい。この手順を functional-design.md §3.4 に書く。

### 2.2 画面（`DrawUnityVersionSettings`）

1. Major の選択肢：表の Major の一覧。今の値が無ければ末尾に `"{値}（一覧外）"` を足す
2. Major を選び直したら、Minor をその Major の一覧の先頭にする
3. Minor の選択肢：今の Major の Minor の一覧。今の値が無ければ末尾に `"{値}（一覧外）"` を足す
4. 選択肢の配列は毎回作る（要素は 10 個程度で、Editor の画面なので負荷は問題にならない）。一覧の値を選ぶと、次の描画から「一覧外」の要素は無くなる

### 2.3 Load 時の `unity` の扱いと警告

| package.json の値 | 結果 | 警告の文言 |
|---|---|---|
| `2022.3`（表にある） | そのまま | なし |
| 無い / 空 / `abc` / `6000.x` | `6000.0` | `unity の値 "{値}" を読み込めないため 6000.0 にしました` |
| `6000`（Minor が無い） | `6000.0`（一覧の先頭） | `unity の値 "{値}" に Minor が無いため {major}.{minor} にしました` |
| `6000.7` / `2024.1`（表に無い） | そのまま | `unity の値 "{値}" は一覧に無いバージョンです。そのまま使います` |

### 2.4 `TryConvertToVersionVector` の振る舞い

正規表現 `^(\d+)(?:\.(\d+))?(?:\.(\d+))?(.*)$` を前後の空白を除いた値に当てる。

| 入力 | `result` | 戻り値 |
|---|---|---|
| `1.2.3` | `1.2.3` | `true` |
| `1.2` | `1.2.0` | `false` |
| `1.2.3.4` / `1.2.3-preview.1` | `1.2.3` | `false` |
| 空 / `null` / `a.b.c` / `int` に収まらない数字 | `0.0.0` | `false` |

Load で `false` のときの警告：`version の値 "{値}" を {x}.{y}.{z} として読み込みました`。
`SetDependenciesInfo` で `false` のときの警告：`外部パッケージ {名前} のバージョン "{値}" を {x}.{y}.{z} として設定しました`。

## 3. データ構造の変更

- `PackageRequiredSettings.MajorVer`：`MajorVersion`（enum。値は 6000 などの数字）から `int` に変わる。enum の値と数字が同じなので、出力される `unity` の値は変わらない
- 初期値が `6000.1` から `6000.0` に変わる
- 入力状態は保存されないデータなので、移行の処理は要らない

## 4. 影響範囲の分析

- 既存機能への影響
  - Major の表示が `Unity_6` などの enum 名から `6000 (Unity 6)` などの数字に、Minor の表示が `Version_1` から `1` に変わる
  - 初期値が `6000.0` になる
  - 正しい package.json の Load・Create の結果は変わらない
- 永続的ドキュメント（Docs/）への影響
  - `Docs/functional-design.md` §2.1：`UnityVersion/` の行を削除し、`UnityVersionConverter.cs` の説明を変える
  - §3.1：対応 Unity バージョンの行（初期値 `6000.0`、Major を変えると一覧の先頭になる）とバージョンの行（Load 時の扱い）を直す
  - §3.4：選択肢の表を新しい値にし、一覧外の扱いと、新しい Unity が出たときの追加手順を書く
  - §8：K-4・K-5・K-6・K-7・K-17 を修正済みにする
  - `CHANGELOG.md` `[Unreleased]`：Added（Unity 6.3〜6.6、5.0）と Fixed に英語で追記
  - `README.md` / `README.ja.md`：対応 Unity バージョンの説明（例 `6000.0`）は変わらないので、変更しない見込み。実装時に確認する
- テストへの影響：自動テストは無い。手動での確認手順を tasklist.md に書く
