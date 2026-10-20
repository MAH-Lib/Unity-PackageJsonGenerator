#if UNITY_EDITOR

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 画面とメッセージの文言の ID（文言は LocalizedText の表に英語・日本語で登録する） </summary>
    internal enum TextId
    {
        //====共通のラベル・ボタン====
        PackageName,
        Version,
        Description,
        DisplayName,
        Major,
        Minor,
        Patch,
        PreRelease,
        BrowseButton,
        ResetButton,
        OpenURLButton,
        ListEmpty,
        ListAddButton,
        ListRemoveButton,

        //====入力エラー====
        PackageNameError,
        EmailError,
        URLError,
        SamplePathError,
        PreReleaseError,

        //====必須項目====
        RequiredSection,
        PackageInfoSection,
        UnityVersionSection,
        UnityVersionUnlisted,
        ApplyCurrentEditorButton,
        AuthorSection,
        AuthorName,
        AuthorEmail,
        AuthorURL,
        LicenseSection,
        License,
        CreateLicenseButton,

        //====任意項目====
        OptionalSection,
        UnityInfoSection,
        UnityRelease,
        HideInEditor,
        DocumentationSection,
        ChangelogURL,
        DocumentationURL,
        LicensesURL,
        KeywordSection,
        KeywordList,
        Keyword,
        ExtraKeysSection,
        ClearButton,

        //====サンプル====
        SampleSection,
        SamplePath,
        SampleList,
        Sample,
        SamplePathDialogTitle,

        //====外部パッケージ====
        DependenciesSection,
        DependenciesList,
        Dependency,
        SelectPackage,
        DependencyVersionSetWarning,

        //====ファイル操作====
        Language,
        FileSection,
        FolderDialogTitle,
        FolderPath,
        CreateButton,
        LoadButton,
        JsonPreview,
        OverwriteDialogTitle,
        OverwriteDialogMessage,
        OverwriteDialogOK,
        OverwriteDialogCancel,
        OverwriteCanceled,
        CreateSucceeded,
        LoadSucceeded,
        WarningCount,

        //====生成前チェック====
        PackageNameEmptyError,
        PackageNameInvalidError,
        DescriptionEmptyError,
        DisplayNameEmptyError,
        LicenseEmptyError,
        AuthorNameEmptyError,
        DependenciesDuplicateError,

        //====読み込み・変換の警告====
        LoadedPackageNameInvalidWarning,
        VersionConvertedWarning,
        UnityVersionUnreadableWarning,
        UnityVersionNoMinorWarning,
        UnityVersionUnlistedWarning,
        DependencyNameInvalidWarning,
        DependencyVersionUnreadableWarning,
        DependencyVersionConvertedWarning,

        //====ファイルの保存・読み込み====
        FolderEmptyError,
        FolderNotFoundError,
        FileNotFoundError,
        SaveFailedError,
        LoadFailedError,

        //====読み込んだパッケージの記憶====
        RememberedPackages,
        NotRememberedPackage,
        RemoveFromListButton,
        PackageSwitched,

        //====現在の Editor の反映・Create 前の確認====
        CurrentEditorApplied,
        EditorVersionUnreadableError,
        SampleFolderNotFoundWarning,
        UnityReleaseInvalidWarning,

        //====LICENSE.md の作成====
        LicenseAuthorEmptyError,
        LicenseOverwriteDialogTitle,
        LicenseCreated,
        LicenseSaveFailedError,
    }
}
#endif
