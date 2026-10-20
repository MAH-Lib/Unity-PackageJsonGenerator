#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class PackageJsonValidator
    {
        //====定数=========================================
        /// <summary> サンプルのフォルダ名（開発中に Assets 配下で使う名前） </summary>
        internal const string PACKAGE_INFO_SAMPLE_KEYWORD_1 = "Samples";
        /// <summary> サンプルのフォルダ名（パッケージ内で使う名前。Unity がインポートしない） </summary>
        internal const string PACKAGE_INFO_SAMPLE_KEYWORD_2 = "Samples~";

        private const string PACKAGE_NAME_EMPTY_ERROR_MESSAGE = "パッケージ名が入力されていません";
        private const string PACKAGE_NAME_INVALID_ERROR_MESSAGE = "パッケージ名の形式が正しくありません";
        private const string DESCRIPTION_EMPTY_ERROR_MESSAGE = "パッケージ概要が入力されていません";
        private const string DISPLAY_NAME_EMPTY_ERROR_MESSAGE = "UPM表示名が入力されていません";
        private const string LICENSE_EMPTY_ERROR_MESSAGE = "ライセンスが入力されていません";
        private const string AUTHOR_NAME_EMPTY_ERROR_MESSAGE = "著者名が入力されていません";
        private const string DEPENDENCIES_DUPLICATE_ERROR_MESSAGE = "外部パッケージ名が重複しています";

        private static readonly Regex PackageNameRegex = new(@"^[a-z0-9\-]+(\.[a-z0-9\-]+)+$", RegexOptions.Compiled);
        private static readonly Regex PackageEmailRegex = new(@"^[^@\s]+@[^@\s\.]+\.[^@\s\.]+(\.[^@\s\.]+)*$", RegexOptions.Compiled);
        private static readonly Regex PackageURLRegex = new(@"^https?:\/\/[\w\-]+(\.[\w\-]+)+([\/\w\-.~:?#[\]@!$&'()*+,;=]*)?$", RegexOptions.Compiled);

        //====入力判定======================================
        /// <summary> パッケージ名の判定 </summary>
        internal static bool IsValidPackageName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return PackageNameRegex.IsMatch(name);
        }

        /// <summary> E-mail の判定 </summary>
        internal static bool IsValidEmail(string e_mail)
        {
            if (string.IsNullOrWhiteSpace(e_mail))
                return false;

            return PackageEmailRegex.IsMatch(e_mail);
        }

        /// <summary> URL の判定 </summary>
        internal static bool IsValidURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return PackageURLRegex.IsMatch(url);
        }

        /// <summary> サンプルのパスを "Samples~/..." 形式に変換（変換できない場合は false） </summary>
        internal static bool IsValidSampleFolderPath(string path, out string sample_path)
        {
            sample_path = string.Empty;

            if (string.IsNullOrEmpty(path))
                return false;

            //OS に関係なく "/" 区切りで扱い、フォルダ名単位で "Samples" / "Samples~" を探す（部分一致はさせない）
            string[] folders = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            int index = Array.FindLastIndex(folders, folder =>
                string.Equals(folder, PACKAGE_INFO_SAMPLE_KEYWORD_1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(folder, PACKAGE_INFO_SAMPLE_KEYWORD_2, StringComparison.OrdinalIgnoreCase));
            if (index == -1 || index == folders.Length - 1)
                return false;

            sample_path = $"{PACKAGE_INFO_SAMPLE_KEYWORD_2}/{string.Join("/", folders.Skip(index + 1))}";
            return true;
        }

        //====生成前チェック==================================
        /// <summary> package.json を生成できるか判定（最初に見つかったエラーを返す） </summary>
        internal static bool CanCreatePackageJson(PackageRequiredSettings required_settings, PackageOptionalSettings optional_settings, out string error_message)
        {
            error_message = string.Empty;

            if (string.IsNullOrWhiteSpace(required_settings.Name))
            {
                error_message = PACKAGE_NAME_EMPTY_ERROR_MESSAGE;
                return false;
            }
            if (!IsValidPackageName(required_settings.Name))
            {
                error_message = $"{PACKAGE_NAME_INVALID_ERROR_MESSAGE} : {required_settings.Name}";
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.Description))
            {
                error_message = DESCRIPTION_EMPTY_ERROR_MESSAGE;
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.DisplayName))
            {
                error_message = DISPLAY_NAME_EMPTY_ERROR_MESSAGE;
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.License))
            {
                error_message = LICENSE_EMPTY_ERROR_MESSAGE;
                return false;
            }
            if (required_settings.AuthorInfo == null || string.IsNullOrWhiteSpace(required_settings.AuthorInfo.Name))
            {
                error_message = AUTHOR_NAME_EMPTY_ERROR_MESSAGE;
                return false;
            }

            //外部パッケージ名の重複（不正な名前の行は出力されないので対象外）
            string[] duplicate_names = (optional_settings.Dependencies ?? new List<DependenciesInfo>())
                .Where(info => info != null && IsValidPackageName(info.PackageName))
                .GroupBy(info => info.PackageName)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicate_names.Any())
            {
                error_message = $"{DEPENDENCIES_DUPLICATE_ERROR_MESSAGE} : {string.Join(", ", duplicate_names)}";
                return false;
            }

            return true;
        }
    }
}
#endif
