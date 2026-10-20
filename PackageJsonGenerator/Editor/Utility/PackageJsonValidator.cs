#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
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

        //SemVer 2.0.0 のプレリリースの表記（"." で区切った各部分が英数字とハイフン。数字だけの部分は先頭が 0 でない）
        private static readonly Regex PreReleaseRegex = new(@"^(0|[1-9]\d*|\d*[A-Za-z\-][0-9A-Za-z\-]*)(\.(0|[1-9]\d*|\d*[A-Za-z\-][0-9A-Za-z\-]*))*$", RegexOptions.Compiled);
        //unityRelease（<更新番号><種類 a/b/f/p など 1 字><番号>。例 0f1、26f1、0b5）
        private static readonly Regex UnityReleaseRegex = new(@"^\d+[a-z]\d+$", RegexOptions.Compiled);
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

        /// <summary> プレリリースの表記の判定（空は「プレリリースではない」として true） </summary>
        internal static bool IsValidPreRelease(string pre_release)
        {
            if (string.IsNullOrEmpty(pre_release))
                return true;

            return PreReleaseRegex.IsMatch(pre_release);
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
                error_message = LocalizedText.Get(TextId.PackageNameEmptyError);
                return false;
            }
            if (!IsValidPackageName(required_settings.Name))
            {
                error_message = LocalizedText.Format(TextId.PackageNameInvalidError, required_settings.Name);
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.Description))
            {
                error_message = LocalizedText.Get(TextId.DescriptionEmptyError);
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.DisplayName))
            {
                error_message = LocalizedText.Get(TextId.DisplayNameEmptyError);
                return false;
            }
            if (string.IsNullOrWhiteSpace(required_settings.License))
            {
                error_message = LocalizedText.Get(TextId.LicenseEmptyError);
                return false;
            }
            if (required_settings.AuthorInfo == null || string.IsNullOrWhiteSpace(required_settings.AuthorInfo.Name))
            {
                error_message = LocalizedText.Get(TextId.AuthorNameEmptyError);
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
                error_message = LocalizedText.Format(TextId.DependenciesDuplicateError, string.Join(", ", duplicate_names));
                return false;
            }

            return true;
        }

        /// <summary> Create の前の確認（作成は止めない）：サンプルのフォルダの有無と unityRelease の形式。当てはまる警告の一覧を返す </summary>
        internal static List<string> GetCreateWarnings(PackageOptionalSettings optional_settings, string folder)
        {
            List<string> warnings = new();

            //サンプルのフォルダ（開発中は Samples~ ではなく Samples に置いていることがあるので、読み替えた場所も探す）
            foreach (SampleInfo sample in optional_settings.SamplesInfo ?? new List<SampleInfo>())
            {
                if (sample == null || string.IsNullOrWhiteSpace(sample.Path))
                    continue;
                if (!IsSampleFolderExists(folder, sample.Path))
                    warnings.Add(LocalizedText.Format(TextId.SampleFolderNotFoundWarning, sample.Path));
            }

            //unityRelease（空は出力されないので確かめない）
            if (!string.IsNullOrWhiteSpace(optional_settings.UnityRelease) && !UnityReleaseRegex.IsMatch(optional_settings.UnityRelease.Trim()))
                warnings.Add(LocalizedText.Format(TextId.UnityReleaseInvalidWarning, optional_settings.UnityRelease));

            return warnings;
        }

        /// <summary> サンプルのフォルダがあるか判定（"Samples~/..." が無ければ "Samples/..." も探す） </summary>
        private static bool IsSampleFolderExists(string folder, string sample_path)
        {
            string path = sample_path.Replace('\\', '/');
            if (Directory.Exists(Path.Combine(folder ?? string.Empty, path)))
                return true;

            string sample_folder_prefix = $"{PACKAGE_INFO_SAMPLE_KEYWORD_2}/";
            if (!path.StartsWith(sample_folder_prefix, StringComparison.OrdinalIgnoreCase))
                return false;
            string development_path = $"{PACKAGE_INFO_SAMPLE_KEYWORD_1}/{path.Substring(sample_folder_prefix.Length)}";
            return Directory.Exists(Path.Combine(folder ?? string.Empty, development_path));
        }
    }
}
#endif
