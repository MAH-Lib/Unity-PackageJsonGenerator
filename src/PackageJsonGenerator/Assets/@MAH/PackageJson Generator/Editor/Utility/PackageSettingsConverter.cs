#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 入力状態と PackageJsonInfo の相互変換（警告は Console に出さず warnings に追加する） </summary>
    internal static class PackageSettingsConverter
    {
        //====定数=========================================
        private static readonly Regex VersionVectorRegex = new(@"^(\d+)(?:\.(\d+))?(?:\.(\d+))?(.*)$", RegexOptions.Compiled);

        //====入力状態 → JSON================================
        /// <summary> 入力状態から PackageJsonInfo に変換 </summary>
        internal static PackageJsonInfo ConvertToPackageJsonInfo(PackageRequiredSettings required_settings, PackageOptionalSettings optional_settings, List<string> warnings)
        {
            PackageJsonInfo info = new()
            {
                Name = required_settings.Name,
                Version = ConvertToVersionString(required_settings.Version),
                Description = required_settings.Description,
                DisplayName = required_settings.DisplayName,
                Unity = ConvertToUnityVersionString(required_settings.MajorVer, required_settings.MinorVer),
                UnityRelease = optional_settings.UnityRelease,
                License = required_settings.License?.Trim(),
                LicensesURL = optional_settings.LicensesURL,
                ChangelogURL = optional_settings.ChangelogURL,
                DocumentationURL = optional_settings.DocumentationURL,
                HideInEditor = optional_settings.HideInEditor,
                Author = required_settings.AuthorInfo,
                Keywords = optional_settings.Keywords != null && optional_settings.Keywords.Any() ? optional_settings.Keywords : null,
                Dependencies = ConvertToDependencies(optional_settings.Dependencies, warnings),
                Samples = optional_settings.SamplesInfo != null && optional_settings.SamplesInfo.Any() ? optional_settings.SamplesInfo : null
            };
            return info;
        }

        //====JSON → 入力状態================================
        /// <summary> PackageJsonInfo を入力状態に書き込む（読めない値は補って warnings に追加する） </summary>
        internal static void ConvertToPackageSettings(PackageJsonInfo package_json_info, PackageRequiredSettings required_settings, PackageOptionalSettings optional_settings, List<string> warnings)
        {
            //不正な名前でも読み込んだ値は残し、警告だけ出す（生成前チェックで止める）
            required_settings.Name = (package_json_info.Name ?? string.Empty).ToLowerInvariant();
            if (!PackageJsonValidator.IsValidPackageName(required_settings.Name))
                warnings.Add($"読み込んだパッケージ名の形式が正しくありません : {required_settings.Name}");
            if (!TryConvertToVersionVector(package_json_info.Version, out required_settings.Version))
                warnings.Add($"version の値 \"{package_json_info.Version}\" を {ConvertToVersionString(required_settings.Version)} として読み込みました");
            required_settings.Description = package_json_info.Description ?? string.Empty;
            required_settings.DisplayName = package_json_info.DisplayName ?? string.Empty;
            required_settings.License = package_json_info.License ?? string.Empty;
            ConvertToUnityVersion(package_json_info.Unity, out required_settings.MajorVer, out required_settings.MinorVer, warnings);
            required_settings.AuthorInfo = package_json_info.Author ?? new();

            optional_settings.ChangelogURL = package_json_info.ChangelogURL ?? string.Empty;
            optional_settings.DocumentationURL = package_json_info.DocumentationURL ?? string.Empty;
            optional_settings.LicensesURL = package_json_info.LicensesURL ?? string.Empty;
            optional_settings.UnityRelease = package_json_info.UnityRelease ?? string.Empty;
            optional_settings.HideInEditor = package_json_info.HideInEditor;
            optional_settings.Keywords = package_json_info.Keywords ?? new();
            optional_settings.Dependencies = ConvertToDependenciesInfo(package_json_info.Dependencies, warnings);
            optional_settings.SamplesInfo = package_json_info.Samples ?? new();
            //以前のバージョンが Windows で出力した "\" 区切りのパスを "/" 区切りに読み替える
            foreach (SampleInfo sample in optional_settings.SamplesInfo)
            {
                if (sample != null && sample.Path != null)
                    sample.Path = sample.Path.Replace('\\', '/');
            }
        }

        //====バージョン======================================
        /// <summary> バージョンを "X.Y.Z" 形式の文字列に変換 </summary>
        internal static string ConvertToVersionString(Vector3Int version_vec)
        {
            return $"{version_vec.x}.{version_vec.y}.{version_vec.z}";
        }

        /// <summary> バージョン文字列を変換（要素の不足・余り・プレビュー表記・数値以外を補った場合は false） </summary>
        internal static bool TryConvertToVersionVector(string version, out Vector3Int result)
        {
            result = Vector3Int.zero;
            if (string.IsNullOrWhiteSpace(version))
                return false;

            Match match = VersionVectorRegex.Match(version.Trim());
            if (!match.Success)
                return false;

            bool is_exact = match.Groups[2].Success && match.Groups[3].Success && string.IsNullOrEmpty(match.Groups[4].Value);
            for (int index = 0; index < 3; index++)
            {
                Group group = match.Groups[index + 1];
                if (!group.Success)
                    continue;
                //int に収まらない数値は読めないものとして 0.0.0 にする
                if (!int.TryParse(group.Value, out int parse))
                {
                    result = Vector3Int.zero;
                    return false;
                }
                result[index] = parse;
            }
            return is_exact;
        }

        /// <summary> 対応Unityバージョンを "Major.Minor" 形式の文字列に変換 </summary>
        internal static string ConvertToUnityVersionString(int major, int minor)
        {
            return $"{major}.{minor}";
        }

        /// <summary> Load した unity の値を Major / Minor に変換（読めない値は初期値にして warnings に追加する） </summary>
        internal static void ConvertToUnityVersion(string unity, out int major, out int minor, List<string> warnings)
        {
            if (!UnityVersionConverter.TryParseUnityVersion(unity, out major, out minor, out bool has_minor))
            {
                warnings.Add($"unity の値 \"{unity}\" を読み込めないため {major}.{minor} にしました");
                return;
            }
            if (!has_minor)
            {
                warnings.Add($"unity の値 \"{unity}\" に Minor が無いため {major}.{minor} にしました");
                return;
            }
            if (!UnityVersionConverter.IsKnownVersion(major, minor))
                warnings.Add($"unity の値 \"{unity}\" は一覧に無いバージョンです。そのまま使います");
        }

        //====外部パッケージ===================================
        /// <summary> 外部パッケージの入力状態を dependencies に変換（不正な名前の行は除外して warnings に追加する） </summary>
        internal static Dictionary<string, string> ConvertToDependencies(List<DependenciesInfo> dependencies_infos, List<string> warnings)
        {
            if (dependencies_infos == null || !dependencies_infos.Any())
                return null;
            Dictionary<string, string> dependencies = new();
            foreach (DependenciesInfo info in dependencies_infos)
            {
                if (info == null)
                    continue;

                //初期値 "com." のままの行や不正な名前の行は出力しない
                if (!PackageJsonValidator.IsValidPackageName(info.PacckageName))
                {
                    warnings.Add($"外部パッケージ名が正しくないため出力しません : {info.PacckageName}");
                    continue;
                }

                //UPM は完全一致のバージョンのみ対応（重複は生成前チェックで止めているが、念のため例外を出さない）
                dependencies.TryAdd(info.PacckageName, ConvertToVersionString(info.Version));
            }
            return dependencies.Any() ? dependencies : null;
        }

        /// <summary> dependencies を外部パッケージの入力状態に変換（演算子付き・範囲は完全一致に読み替えて warnings に追加する） </summary>
        internal static List<DependenciesInfo> ConvertToDependenciesInfo(Dictionary<string, string> dependencies, List<string> warnings)
        {
            if (dependencies == null || !dependencies.Any())
                return new();

            List<DependenciesInfo> dependencies_infos = new();

            foreach (KeyValuePair<string, string> key_value_pair in dependencies)
            {
                if (string.IsNullOrEmpty(key_value_pair.Key) || string.IsNullOrEmpty(key_value_pair.Value))
                    continue;

                if (!VersionOperatorConverter.TryExtractVersion(key_value_pair.Value, out string version, out bool is_converted))
                {
                    warnings.Add($"外部パッケージ {key_value_pair.Key} のバージョン \"{key_value_pair.Value}\" を読み込めないため除外しました");
                    continue;
                }
                if (is_converted)
                    warnings.Add($"外部パッケージ {key_value_pair.Key} のバージョン \"{key_value_pair.Value}\" を \"{version}\" に変換しました（UPM は完全一致のバージョンのみ対応）");

                //TryExtractVersion が "X.Y.Z" 形式を返すので、ここでは補完が起きない
                TryConvertToVersionVector(version, out Vector3Int version_vec);
                DependenciesInfo info = new()
                {
                    PacckageName = key_value_pair.Key,
                    Version = version_vec,
                    SelectPackageIndex = FindPackageIndex(key_value_pair.Key),
                };

                dependencies_infos.Add(info);
            }

            return dependencies_infos;
        }

        /// <summary> 登録済みパッケージ一覧から名前の選択配列番号を探す（見つからなければ 0） </summary>
        internal static int FindPackageIndex(string name)
        {
            int index = Array.IndexOf(UnityPackageHelper.AccPackageNames, name);
            return index > 0 ? index : 0;
        }
    }
}
#endif
