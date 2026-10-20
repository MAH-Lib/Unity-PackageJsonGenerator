#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 画面とメッセージの文言（英語・日本語）と、今の表示言語 </summary>
    internal static class LocalizedText
    {
        //====定数======================================
        /// <summary> 表示言語を保存する EditorPrefs のキー </summary>
        private const string LANGUAGE_PREFS_KEY = "MAH.PackageJsonGenerator.Language";

        /// <summary> 言語の選択肢の表示名（LanguageType の順） </summary>
        internal static readonly string[] LanguageNames = { "English", "日本語" };

        /// <summary> 文言の表（添え字は LanguageType） </summary>
        private static readonly Dictionary<TextId, string[]> Texts = new()
        {
            //====共通のラベル・ボタン====
            { TextId.PackageName, new[] { "Package name", "パッケージ名" } },
            { TextId.Version, new[] { "Version", "バージョン" } },
            { TextId.Description, new[] { "Description", "概要" } },
            { TextId.DisplayName, new[] { "Display name", "UPM表示名" } },
            { TextId.Major, new[] { "Major", "Major" } },
            { TextId.Minor, new[] { "Minor", "Minor" } },
            { TextId.Patch, new[] { "Patch", "Patch" } },
            { TextId.PreRelease, new[] { "Pre-release", "プレリリース" } },
            { TextId.BrowseButton, new[] { "Browse", "Browse" } },
            { TextId.ResetButton, new[] { "Reset", "Reset" } },
            { TextId.OpenURLButton, new[] { "Open URL", "Open URL" } },
            { TextId.ListEmpty, new[] { "No data", "データがありません" } },
            { TextId.ListAddButton, new[] { "Add", "Add" } },
            { TextId.ListRemoveButton, new[] { "Remove", "Remove" } },

            //====入力エラー====
            { TextId.PackageNameError, new[] {
                "\"{0}\" is not a valid package name (e.g. com.company.my-tool: lowercase letters, numbers and hyphens separated by dots)",
                "「{0}」はパッケージ名の形式ではありません（例：com.company.my-tool。小文字の英数字とハイフンをドットで区切ります）" } },
            { TextId.EmailError, new[] {
                "\"{0}\" is not a valid e-mail address (e.g. name@example.com)",
                "「{0}」は E-mail の形式ではありません（例：name@example.com）" } },
            { TextId.URLError, new[] {
                "\"{0}\" is not a valid URL (it must start with http:// or https://)",
                "「{0}」は URL の形式ではありません（http:// か https:// で始めてください）" } },
            { TextId.SamplePathError, new[] {
                "Cannot make a sample path from \"{0}\". Choose a folder inside a Samples or Samples~ folder (e.g. Samples~/Basic)",
                "「{0}」からサンプルのパスを作れません。Samples か Samples~ フォルダの下のフォルダを指定してください（例：Samples~/Basic）" } },
            { TextId.PreReleaseError, new[] {
                "\"{0}\" is not a valid pre-release label (e.g. preview.1, exp.2, pre.3: letters, numbers and hyphens separated by dots)",
                "「{0}」はプレリリースの表記の形式ではありません（例：preview.1、exp.2、pre.3。英数字とハイフンを . で区切ります）" } },

            //====必須項目====
            { TextId.RequiredSection, new[] { "Required settings", "必須設定項目" } },
            { TextId.PackageInfoSection, new[] { "Package", "パッケージ情報設定" } },
            { TextId.UnityVersionSection, new[] { "Supported Unity version", "対応 Unity バージョン設定" } },
            { TextId.UnityVersionUnlisted, new[] { " (not in the list)", "（一覧外）" } },
            { TextId.ApplyCurrentEditorButton, new[] { "Use current Editor version", "Use current Editor version" } },
            { TextId.AuthorSection, new[] { "Author", "著者情報設定" } },
            { TextId.AuthorName, new[] { "Name", "著者名" } },
            { TextId.AuthorEmail, new[] { "E-mail", "E-mail" } },
            { TextId.AuthorURL, new[] { "URL", "URL" } },
            { TextId.LicenseSection, new[] { "License", "ライセンス情報設定" } },
            { TextId.License, new[] { "License", "ライセンス" } },
            { TextId.CreateLicenseButton, new[] { "Create LICENSE.md", "Create LICENSE.md" } },

            //====任意項目====
            { TextId.OptionalSection, new[] { "Optional settings", "任意設定項目" } },
            { TextId.UnityInfoSection, new[] { "Unity", "Unity 情報設定" } },
            { TextId.UnityRelease, new[] { "Release version", "リリースバージョン" } },
            { TextId.HideInEditor, new[] { "Hide in editor", "UPM除外フラグ" } },
            { TextId.DocumentationSection, new[] { "Documentation", "ドキュメント情報設定" } },
            { TextId.ChangelogURL, new[] { "Changelog URL", "変更ログURL" } },
            { TextId.DocumentationURL, new[] { "Documentation URL", "ドキュメントURL" } },
            { TextId.LicensesURL, new[] { "License URL", "ライセンスURL" } },
            { TextId.KeywordSection, new[] { "Keywords", "キーワード情報設定" } },
            { TextId.KeywordList, new[] { "Keyword list", "キーワード一覧" } },
            { TextId.Keyword, new[] { "Keyword", "キーワード" } },
            { TextId.ExtraKeysSection, new[] { "Other keys (kept as is, cannot be edited in this tool)", "その他の項目（そのまま残します。このツールでは編集できません）" } },
            { TextId.ClearButton, new[] { "Clear", "Clear" } },

            //====サンプル====
            { TextId.SampleSection, new[] { "Samples", "パッケージサンプル情報設定" } },
            { TextId.SamplePath, new[] { "Path", "パス" } },
            { TextId.SampleList, new[] { "Sample list", "サンプル一覧" } },
            { TextId.Sample, new[] { "Sample", "サンプル" } },
            { TextId.SamplePathDialogTitle, new[] { "Select the sample folder", "Samples フォルダーパス設定" } },

            //====外部パッケージ====
            { TextId.DependenciesSection, new[] { "Dependencies", "外部パッケージ情報設定" } },
            { TextId.DependenciesList, new[] { "Dependency list", "外部パッケージ一覧" } },
            { TextId.Dependency, new[] { "Dependency", "外部パッケージ" } },
            { TextId.SelectPackage, new[] { "– Select Package –", "– パッケージを選択 –" } },
            { TextId.DependencyVersionSetWarning, new[] {
                "Dependency {0}: version \"{1}\" was set as {2}",
                "外部パッケージ {0} のバージョン \"{1}\" を {2} として設定しました" } },

            //====ファイル操作====
            { TextId.Language, new[] { "Language", "言語" } },
            { TextId.FileSection, new[] { "package.json file", "package.json ファイル操作" } },
            { TextId.FolderDialogTitle, new[] { "Select the folder for package.json", "package.json の保存先を選択" } },
            { TextId.FolderPath, new[] { "Folder path", "フォルダーパス" } },
            { TextId.CreateButton, new[] { "Create package.json", "Create package.json" } },
            { TextId.LoadButton, new[] { "Load package.json", "Load package.json" } },
            { TextId.JsonPreview, new[] { "JSON preview", "JSON プレビュー" } },
            { TextId.OverwriteDialogTitle, new[] { "Overwrite package.json", "package.json の上書き" } },
            { TextId.OverwriteDialogMessage, new[] { "{0} already exists. Do you want to overwrite it?", "{0} は既に存在します。上書きしますか？" } },
            { TextId.OverwriteDialogOK, new[] { "Overwrite", "上書き" } },
            { TextId.OverwriteDialogCancel, new[] { "Cancel", "キャンセル" } },
            { TextId.OverwriteCanceled, new[] { "Overwrite canceled", "上書きをキャンセルしました" } },
            { TextId.CreateSucceeded, new[] { "Created package.json : {0}", "package.json を作成しました : {0}" } },
            { TextId.LoadSucceeded, new[] { "Loaded package.json : {0}", "package.json を読み込みました : {0}" } },
            { TextId.WarningCount, new[] { "{0} warning(s) (see the Console for details)", "警告 {0} 件（詳しくは Console を参照）" } },

            //====生成前チェック====
            { TextId.PackageNameEmptyError, new[] { "Enter a package name", "パッケージ名が入力されていません" } },
            { TextId.PackageNameInvalidError, new[] { "The package name is not valid : {0}", "パッケージ名の形式が正しくありません : {0}" } },
            { TextId.DescriptionEmptyError, new[] { "Enter a description", "パッケージ概要が入力されていません" } },
            { TextId.DisplayNameEmptyError, new[] { "Enter a display name", "UPM表示名が入力されていません" } },
            { TextId.LicenseEmptyError, new[] { "Enter a license", "ライセンスが入力されていません" } },
            { TextId.AuthorNameEmptyError, new[] { "Enter an author name", "著者名が入力されていません" } },
            { TextId.DependenciesDuplicateError, new[] { "Duplicate dependency names : {0}", "外部パッケージ名が重複しています : {0}" } },

            //====読み込み・変換の警告====
            { TextId.LoadedPackageNameInvalidWarning, new[] {
                "The loaded package name is not valid : {0}",
                "読み込んだパッケージ名の形式が正しくありません : {0}" } },
            { TextId.VersionConvertedWarning, new[] {
                "The version value \"{0}\" was loaded as {1}",
                "version の値 \"{0}\" を {1} として読み込みました" } },
            { TextId.UnityVersionUnreadableWarning, new[] {
                "The unity value \"{0}\" cannot be read. It was set to {1}",
                "unity の値 \"{0}\" を読み込めないため {1} にしました" } },
            { TextId.UnityVersionNoMinorWarning, new[] {
                "The unity value \"{0}\" has no minor version. It was set to {1}",
                "unity の値 \"{0}\" に Minor が無いため {1} にしました" } },
            { TextId.UnityVersionUnlistedWarning, new[] {
                "The unity value \"{0}\" is not in the list. It is used as is",
                "unity の値 \"{0}\" は一覧に無いバージョンです。そのまま使います" } },
            { TextId.DependencyNameInvalidWarning, new[] {
                "The dependency name is not valid, so it is not written : {0}",
                "外部パッケージ名が正しくないため出力しません : {0}" } },
            { TextId.DependencyVersionUnreadableWarning, new[] {
                "Dependency {0}: version \"{1}\" cannot be read, so it was skipped",
                "外部パッケージ {0} のバージョン \"{1}\" を読み込めないため除外しました" } },
            { TextId.DependencyVersionConvertedWarning, new[] {
                "Dependency {0}: version \"{1}\" was converted to \"{2}\" (Unity Package Manager supports exact versions only)",
                "外部パッケージ {0} のバージョン \"{1}\" を \"{2}\" に変換しました（UPM は完全一致のバージョンのみ対応）" } },

            //====ファイルの保存・読み込み====
            { TextId.FolderEmptyError, new[] { "Enter a folder path", "フォルダーパスが指定されていません" } },
            { TextId.FolderNotFoundError, new[] { "The folder does not exist : {0}", "フォルダが存在しません : {0}" } },
            { TextId.FileNotFoundError, new[] { "package.json does not exist in the folder : {0}", "指定したフォルダに package.json が存在しません : {0}" } },
            { TextId.SaveFailedError, new[] { "Could not save package.json : {0}", "package.json を保存できませんでした : {0}" } },
            { TextId.LoadFailedError, new[] { "Could not load package.json : {0}", "package.json を読み込めませんでした : {0}" } },

            //====読み込んだパッケージの記憶====
            { TextId.RememberedPackages, new[] { "Remembered packages", "記憶したパッケージ" } },
            { TextId.NotRememberedPackage, new[] { "(not remembered)", "（記憶していないパッケージ）" } },
            { TextId.RemoveFromListButton, new[] { "Remove from list", "Remove from list" } },
            { TextId.PackageSwitched, new[] { "Switched to {0}", "{0} に切り替えました" } },

            //====現在の Editor の反映・Create 前の確認====
            { TextId.CurrentEditorApplied, new[] {
                "Set unity to {0} and unityRelease to {1} from the current Editor version {2}",
                "今の Editor のバージョン {2} から、unity を {0}、unityRelease を {1} にしました" } },
            { TextId.EditorVersionUnreadableError, new[] {
                "Cannot read the current Editor version : {0}",
                "今の Editor のバージョンを読み取れません : {0}" } },
            { TextId.SampleFolderNotFoundWarning, new[] {
                "The sample folder does not exist in the package folder : {0}",
                "サンプルのフォルダがパッケージのフォルダの中にありません : {0}" } },
            { TextId.UnityReleaseInvalidWarning, new[] {
                "The release version \"{0}\" does not look like a Unity release (e.g. 0f1, 26f1, 0b5)",
                "リリースバージョン「{0}」は Unity のリリースの形式（例：0f1、26f1、0b5）ではありません" } },

            //====LICENSE.md の作成====
            { TextId.LicenseAuthorEmptyError, new[] { "Enter an author name to create LICENSE.md", "LICENSE.md を作るには著者名を入力してください" } },
            { TextId.LicenseOverwriteDialogTitle, new[] { "Overwrite LICENSE.md", "LICENSE.md の上書き" } },
            { TextId.LicenseCreated, new[] { "Created LICENSE.md : {0}", "LICENSE.md を作成しました : {0}" } },
            { TextId.LicenseSaveFailedError, new[] { "Could not save LICENSE.md : {0}", "LICENSE.md を保存できませんでした : {0}" } },
        };

        //====変数======================================
        /// <summary> 今の表示言語（EditorPrefs から読んだ値のキャッシュ） </summary>
        private static LanguageType? _currentLanguage;
        /// <summary> テスト用の一時的な表示言語（EditorPrefs には保存しない） </summary>
        private static LanguageType? _overrideLanguage;

        //====アクセサ=========================
        /// <summary> 今の表示言語（設定すると EditorPrefs に保存する） </summary>
        internal static LanguageType AccCurrentLanguage
        {
            get
            {
                if (_overrideLanguage.HasValue)
                    return _overrideLanguage.Value;
                _currentLanguage ??= (LanguageType)EditorPrefs.GetInt(LANGUAGE_PREFS_KEY, (int)GetDefaultLanguage());
                return _currentLanguage.Value;
            }
            set
            {
                _currentLanguage = value;
                EditorPrefs.SetInt(LANGUAGE_PREFS_KEY, (int)value);
            }
        }

        //====関数======================================
        /// <summary> 言語を一度も選んでいないときの表示言語（PC の言語が日本語なら日本語、それ以外は英語） </summary>
        internal static LanguageType GetDefaultLanguage()
        {
            return Application.systemLanguage == SystemLanguage.Japanese ? LanguageType.Japanese : LanguageType.English;
        }

        /// <summary> 今の表示言語の文言取得（表に無い場合は ID の名前を返す） </summary>
        internal static string Get(TextId id)
        {
            return Get(id, AccCurrentLanguage);
        }

        /// <summary> 指定した言語の文言取得（表に無い場合は ID の名前を返す） </summary>
        internal static string Get(TextId id, LanguageType language)
        {
            if (!Texts.TryGetValue(id, out string[] texts) || (int)language >= texts.Length || string.IsNullOrEmpty(texts[(int)language]))
                return id.ToString();
            return texts[(int)language];
        }

        /// <summary> 今の表示言語の文言に値を埋め込んで取得 </summary>
        internal static string Format(TextId id, params object[] args)
        {
            return string.Format(Get(id), args);
        }

        /// <summary> テスト用：EditorPrefs に保存せずに表示言語を切り替える（null で元に戻す） </summary>
        internal static void OverrideLanguage(LanguageType? language)
        {
            _overrideLanguage = language;
        }

        /// <summary> テスト用：表に登録されている文言（ID ごとの全言語の配列）取得 </summary>
        internal static bool TryGetTexts(TextId id, out string[] texts)
        {
            return Texts.TryGetValue(id, out texts);
        }
    }
}
#endif
