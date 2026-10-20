# 設計：ライセンス・サンプル・Reset の修正

> 対象：`requirements.md`（承認済み版） ／ 作成日：2026-10-20 ／ ステータス：承認済み
> 対象：パッケージ `com.mah.package-json-generator` v1.0.1 ／ 開発環境 Unity 6000.3.26f1（対応 Unity 6000.0 以上）

## 1. 実装アプローチ

どれも `PackageJsonCreateWindow` と `PackageOptionalSettings` の中で閉じる小さな修正にする。新しいクラスは作らない。
- ライセンス `Other`：入力欄の戻り値を `License` に代入する（今は捨てている）。出力するときに前後の空白を取り除く
- サンプルのパス：`IsValidSampleFolderpath` を「`\` を `/` に置き換え → `/` で区切る → 最後に現れる `Samples` / `Samples~` の要素を探す → その後ろを `Samples~/` につなぐ」に書き直す。`Path.Combine` は OS ごとに区切り文字が変わるので使わない
- パッケージ名：Load したときは小文字にするだけで値は残し、形式に合わなければ警告を出す。生成前チェックで形式も確かめる

- 却下：パスの判定を今の `LastIndexOf`（文字列の部分一致）のまま直す案 → `MySamples` のようなフォルダ名にも一致してしまうため
- 却下：Load で不正な名前を `com.mypackage` に置き換える案 → 読み込んだ値が消え、利用者が元の名前を確かめられなくなるため（requirements で承認済み）

## 2. 変更するコンポーネント

パスは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/Editor/` からの相対パス。

| 対象（クラス/ファイル） | 変更内容 | 新規/修正 |
|---|---|---|
| `PackageJsonCreateWindow.cs`：`DrawLicenseInfoSettings` | `Other` の `TextField` の戻り値を `_requiredSettings.License` に代入する（K-3） | 修正 |
| `PackageJsonCreateWindow.cs`：`ConvertToPackageJsonInfo` | `License` を `Trim()` してから出力する | 修正 |
| `PackageJsonCreateWindow.cs`：`DrawSamplesInfoSettings` | パス欄を `DelayedTextField` に変える（K-25）。追加時（1 件目・2 件目以降）と Reset の初期値を `Samples~` にそろえる（K-10）。Reset を押したフレームでは、パス欄の古い値で上書きしないようにする（下の §2.1） | 修正 |
| `PackageJsonCreateWindow.cs`：`IsValidSampleFolderpath` | 区切りごとの判定と `/` 区切りでの出力に書き直す（K-9。下の §2.2） | 修正 |
| `PackageJsonCreateWindow.cs`：`ConvertToPackageSettings` | パッケージ名を小文字にし、形式に合わなければ `Debug.LogWarning` を出す（K-18）。読み込んだサンプルのパスの `\` を `/` に置き換える | 修正 |
| `PackageJsonCreateWindow.cs`：`IsCanCreatePackageJson` | パッケージ名が空でなく形式に合わない場合に `Debug.LogError` を出して中止する（K-18） | 修正 |
| `Definition/PackageOptionalSettings.cs`：`Reset()` | `HideInEditor = false;` を追加する（K-16） | 修正 |

### 2.1 サンプルの Reset ボタンの不具合

今のコードは、Reset ボタンで `sample.Path` を書き換えた直後に、パス欄の古い値（`selected_path`）と比べて `IsValidSampleFolderpath` を通している。そのため古い値が正しいパスだと、Reset した値がすぐに古い値で上書きされ、Reset が効かない。Reset を押したときは `selected_path` も `Samples~` にして、検証を通さずに終える。K-10（初期値の統一）の修正の一部として扱う。

### 2.2 `IsValidSampleFolderpath` の振る舞い

| 入力 | 戻り値 | `sample_path` |
|---|---|---|
| `C:/Proj/Assets/MyPkg/Samples/Basic`（Browse の結果） | `true` | `Samples~/Basic` |
| `C:\Proj\Packages\MyPkg\Samples~\Basic\Scene` | `true` | `Samples~/Basic/Scene` |
| `Samples~/Basic`、`samples~/Basic/` | `true` | `Samples~/Basic` |
| `Samples/A/Samples/B`（`Samples` が 2 回） | `true` | `Samples~/B`（最後に現れたもの） |
| `MySamples/Basic`（部分一致） | `false` | 空文字 |
| `Samples~`、`Samples/`（後ろが空） | `false` | 空文字 |
| 空文字、`Assets/Foo` | `false` | 空文字 |

`false` のときは、今と同じく前の値のまま変えない。

### 2.3 Console に出すメッセージ

| 場面 | 種類 | 文言 |
|---|---|---|
| Load した名前が形式に合わない | Warning | `読み込んだパッケージ名の形式が正しくありません : {名前}` |
| 名前が形式に合わないまま Create | Error | `パッケージ名の形式が正しくありません : {名前}` |

名前が空白のときは、今と同じ `パッケージ名が入力されていません` を出す。

## 3. データ構造の変更

- 変更なし（フィールドの追加・削除は無い）
- 出力形式：`samples[].path` の区切りが、Windows でも `\` ではなく `/` になる。以前の出力（`Samples~\\Basic`）は Load のときに `/` に読み替えるので、移行の作業は要らない

## 4. 影響範囲の分析

- 既存機能への影響
  - サンプルのパス欄が、Enter かフォーカスアウトで確定するまで反映されなくなる（他の検証付きの欄と同じ動き）
  - 名前が形式に合わない package.json を Load → Create すると、これまでは不正な名前のまま出力されていたが、今後はエラーで止まる
  - 上記以外の入力・出力は変わらない
- 永続的ドキュメント（Docs/）への影響
  - `Docs/functional-design.md` §3.3：`Other` の行を「入力欄の値（前後の空白を除く）」に変える。Load 時に一覧に無い ID が入力欄に表示されることを書く
  - §3.5：出力例の `samples` のパスを `Samples~/Basic` にし、`\` 区切りの注記を消す
  - §4.1：Load したときの扱い（小文字化・警告）を追記
  - §4.3：生成前チェックにパッケージ名の形式を追加
  - §4.4：判定方法・出力形式・初期値を §2.2 の内容に書き換える
  - §8：K-3・K-9・K-10・K-16・K-18 を修正済みにし、K-25（サンプルのパスを手入力できない）を追加して修正済みにする
  - `CHANGELOG.md` `[Unreleased]`：Fixed に英語で追記
  - `README.md` / `README.ja.md`：既知の制限事項の「ライセンスで Other を選ぶ機能はまだ使えない」を削除する
- テストへの影響：自動テストは無い。手動での確認手順を tasklist.md に書く
