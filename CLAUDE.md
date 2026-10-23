# CLAUDE.md

> このファイルは Claude への作業指示書。GitHub Copilot とは共有しない。
> 人間向けの開発手順は CONTRIBUTING.md、現状の仕様と既知の問題は Docs/functional-design.md にある。
> 200 行以内を目標に保つ。

---

## プロジェクト概要

UPM パッケージ `com.github.mah.package-json-generator`（1.x までは `com.mah.package-json-generator`）。GUI から `package.json` を作成する Unity Editor 拡張（EditorWindow / IMGUI）。
開発環境は Unity 6000.3.26f1 / C#。パッケージの対応下限は Unity 2022.1。

---

## ドキュメント参照

<!-- @ なし：作業内容に応じて必要なときに読む -->
CONTRIBUTING.md              <!-- リポジトリ構成・同期手順・コーディング規約・リリース手順 -->
Docs/functional-design.md    <!-- 入力項目⇔JSONキー対応、検証仕様、既知の問題 K-xx -->

---

## Claudeへの行動ルール

### 作業前に必ず行うこと
- 既存ファイルを変更する前に、対象ファイルを全て読む
- 新しいクラスを作る前に、`Definition/`・`Utility/`・`Drawer/` の似たクラスを 1 つ読んで書き方を確認する
- 検証（`PackageJsonValidator`）や変換（`PackageSettingsConverter`）を変えたら、`Assets/@MAH/PackageJson Generator/Tests/Editor/` の EditMode テストも追加・修正する
- 複数ファイルにまたがる変更は、計画を先に示して承認を得る
- 機能追加・修正に取りかかるときは、`.steering/[YYYYMMDD]-[開発タイトル]/` に `requirements.md` → `design.md` → `tasklist.md` を 1 ファイルずつ承認を得ながら作ってから実装する（誤字修正などの小さな変更は除く）。完了後も削除しない
- 既知の問題（K-xx）を直すときは、該当する ID を確認し、修正後に functional-design.md の §8 を更新する

### やってはいけないこと
- 配布フォルダ `PackageJsonGenerator/` を直接編集しない。編集するのは `src/PackageJsonGenerator/Assets/@MAH/PackageJson Generator/` だけ。配布フォルダへは CONTRIBUTING.md §4 の手順でコピーする（2 つが食い違うと、利用者に古いコードが届くため）
- `.meta` ファイルの GUID を作り直したり書き換えたりしない（利用者のプロジェクトで参照が切れるため）
- 新しいファイルを作ったら `.meta` も作る。GUID は新しく生成した 32 桁の 16 進数にする
- 対応下限の Unity 2022.1 に無い API を使わない
- 識別子の綴りの誤りや名前の付け直しを、ついでに直さない。リネームは専用の変更として分ける（K-19 の綴りの誤りは 2026-10-21 に修正済み。`FormerlySerializedAs` の古い名前は消さない）
- `#if UNITY_EDITOR` ガードを外さない
- `main` ブランチに直接コミットしない（利用者が Git URL で取得するブランチのため）

### コード生成・編集時のルール
- 既存の書き方に合わせる：定数は `UPPER_SNAKE_CASE`、private フィールドは `_camelCase`、ローカル変数と引数は `snake_case`、セクション区切りは `//====名前====`、コメントは 1 行の `/// <summary>`（日本語）
- アクセス修飾子は基本 `internal`。Newtonsoft でシリアライズするモデル（`PackageJsonInfo`、`AuthorInfo`、`SampleInfo`）だけ `public`
- JSON のキー名は `[JsonProperty("...")]` で明示する
- 利用者に関係する変更をしたら、`CHANGELOG.md` の `[Unreleased]` に英語で追記する
- 使い方や入力項目を変えたら、`README.md`（英語）と `README.ja.md`（日本語）の両方を同じ内容で更新する

---

## 使用パッケージと用途

| パッケージ | バージョン | 用途 |
| --- | --- | --- |
| `com.unity.nuget.newtonsoft-json` | 3.2.1（package.json の宣言） | package.json の読み書き |

使用禁止パッケージ：なし。ただし、パッケージの依存は増やさない（利用者のプロジェクトに入るため）。どうしても必要な場合はユーザーに確認する。

---

## プロジェクト固有の注意点（罠・落とし穴）

- パッケージのフォルダ名 `PackageJson Generator` には空白が入り、その親フォルダ `@MAH` は `@` で始まる。シェルで扱うときは必ず引用符で囲む（PowerShell は、引用符の無い `@` で始まる引数をスプラッティングとして解釈するため）
- `Assets/` 配下のフォルダを VS Code を開いたまま `git mv` で丸ごと移動すると、`Permission denied` になることがある。その場合は、ファイルを 1 つずつ `git mv` してから空のフォルダを削除する
- `Assets/@MAH/Editor/Utility/LanguageUtility.cs` はパッケージに含まれない。パッケージのコードから参照しない
- ドキュメントの言語：利用者向け（README、パッケージ内の README・CHANGELOG、`.github/` のテンプレート）は英語、開発者向け（Docs/、CONTRIBUTING.md、本書）は日本語
- UPM の `dependencies` は完全一致のバージョンしか受け付けない（既知の問題 K-1）。比較演算子が使える前提のコードを書かない
- この環境には Python が入っていない。スクリプトは PowerShell か Git Bash で書く

---

## コミュニケーションの設定

- コードコメントは日本語
- 変数名・クラス名・メソッド名は英語
- Claude の回答は日本語（ユーザーは英語が読めないため、英語の文書を作ったときは日本語で要点を説明する）
- ユーザーへの質問は、なるべく `AskUserQuestion` ツールで選択肢を付けて行う（答える手間を減らし、意図の取り違えを防ぐため）。選択肢にできない自由記述の確認だけ、通常の文章で聞く
- やり方が複数あるときは、選んだ理由を短く添える
- 変更が大きいときは、計画を箇条書きで示してから取りかかる
