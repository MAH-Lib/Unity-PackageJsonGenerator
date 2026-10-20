#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 入力状態と PackageJsonInfo の相互変換（警告は Console に出さず warnings に追加する） </summary>
    internal static class PackageSettingsConverter
    {
        //====定数=========================================
        //Major.Minor.Patch とプレリリースの表記（"-" の後ろ）を読む。残り（"+build" など）は 5 番目のグループに入る
        private static readonly Regex VersionVectorRegex = new(@"^(\d+)(?:\.(\d+))?(?:\.(\d+))?(?:-([0-9A-Za-z.\-]+))?(.*)$", RegexOptions.Compiled);

        //====入力状態 → JSON================================
        /// <summary> 入力状態から PackageJsonInfo に変換 </summary>
        internal static PackageJsonInfo ConvertToPackageJsonInfo(PackageRequiredSettings required_settings, PackageOptionalSettings optional_settings, List<string> warnings)
        {
            PackageJsonInfo info = new()
            {
                Name = required_settings.Name,
                Version = ConvertToVersionString(required_settings.Version, required_settings.PreRelease),
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
                Samples = optional_settings.SamplesInfo != null && optional_settings.SamplesInfo.Any() ? optional_settings.SamplesInfo : null,
                ExtraData = ConvertToExtraData(optional_settings.ExtraJson),
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
                warnings.Add(LocalizedText.Format(TextId.LoadedPackageNameInvalidWarning, required_settings.Name));
            if (!TryConvertToVersionVector(package_json_info.Version, out required_settings.Version, out required_settings.PreRelease))
                warnings.Add(LocalizedText.Format(TextId.VersionConvertedWarning, package_json_info.Version, ConvertToVersionString(required_settings.Version, required_settings.PreRelease)));
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
            optional_settings.ExtraJson = ConvertToExtraJson(package_json_info.ExtraData);
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

        /// <summary> バージョンを "X.Y.Z" または "X.Y.Z-表記" 形式の文字列に変換（表記が空ならプレリリースではない） </summary>
        internal static string ConvertToVersionString(Vector3Int version_vec, string pre_release)
        {
            string version = ConvertToVersionString(version_vec);
            return string.IsNullOrEmpty(pre_release) ? version : $"{version}-{pre_release}";
        }

        /// <summary> バージョン文字列を変換（要素の不足・余り・不正なプレリリースの表記・ビルドメタデータ・数値以外を補った場合は false） </summary>
        internal static bool TryConvertToVersionVector(string version, out Vector3Int result, out string pre_release)
        {
            result = Vector3Int.zero;
            pre_release = string.Empty;
            if (string.IsNullOrWhiteSpace(version))
                return false;

            Match match = VersionVectorRegex.Match(version.Trim());
            if (!match.Success)
                return false;

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

            //SemVer の規則に合わないプレリリースの表記は読み捨てる
            bool is_valid_pre_release = !match.Groups[4].Success || PackageJsonValidator.IsValidPreRelease(match.Groups[4].Value);
            if (match.Groups[4].Success && is_valid_pre_release)
                pre_release = match.Groups[4].Value;

            return match.Groups[2].Success && match.Groups[3].Success && is_valid_pre_release && string.IsNullOrEmpty(match.Groups[5].Value);
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
                warnings.Add(LocalizedText.Format(TextId.UnityVersionUnreadableWarning, unity, ConvertToUnityVersionString(major, minor)));
                return;
            }
            if (!has_minor)
            {
                warnings.Add(LocalizedText.Format(TextId.UnityVersionNoMinorWarning, unity, ConvertToUnityVersionString(major, minor)));
                return;
            }
            if (!UnityVersionConverter.IsKnownVersion(major, minor))
                warnings.Add(LocalizedText.Format(TextId.UnityVersionUnlistedWarning, unity));
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
                if (!PackageJsonValidator.IsValidPackageName(info.PackageName))
                {
                    warnings.Add(LocalizedText.Format(TextId.DependencyNameInvalidWarning, info.PackageName));
                    continue;
                }

                //UPM は完全一致のバージョンのみ対応（重複は生成前チェックで止めているが、念のため例外を出さない）
                dependencies.TryAdd(info.PackageName, ConvertToVersionString(info.Version, info.PreRelease));
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
                    warnings.Add(LocalizedText.Format(TextId.DependencyVersionUnreadableWarning, key_value_pair.Key, key_value_pair.Value));
                    continue;
                }
                //TryExtractVersion は "X.Y.Z" または "X.Y.Z-表記" を返す。表記が SemVer の規則に合わない場合は読み捨てて変換扱いにする
                if (!TryConvertToVersionVector(version, out Vector3Int version_vec, out string pre_release))
                {
                    version = ConvertToVersionString(version_vec, pre_release);
                    is_converted = true;
                }
                if (is_converted)
                    warnings.Add(LocalizedText.Format(TextId.DependencyVersionConvertedWarning, key_value_pair.Key, key_value_pair.Value, version));

                DependenciesInfo info = new()
                {
                    PackageName = key_value_pair.Key,
                    Version = version_vec,
                    PreRelease = pre_release,
                    SelectPackageIndex = FindPackageIndex(key_value_pair.Key),
                };

                dependencies_infos.Add(info);
            }

            return dependencies_infos;
        }

        //====未対応キー===================================
        /// <summary> 読み込んだ未対応キーを JSON オブジェクトの文字列に変換（無ければ空文字） </summary>
        internal static string ConvertToExtraJson(IDictionary<string, JToken> extra_data)
        {
            if (extra_data == null || !extra_data.Any())
                return string.Empty;
            return JsonConvert.SerializeObject(extra_data);
        }

        /// <summary> 未対応キーの JSON オブジェクトの文字列を、出力用の辞書に戻す（空や読めない場合は null） </summary>
        internal static IDictionary<string, JToken> ConvertToExtraData(string extra_json)
        {
            if (string.IsNullOrWhiteSpace(extra_json))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, JToken>>(extra_json);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary> 未対応キーの名前の一覧取得（画面の表示用） </summary>
        internal static string[] GetExtraKeys(string extra_json)
        {
            IDictionary<string, JToken> extra_data = ConvertToExtraData(extra_json);
            return extra_data == null ? Array.Empty<string>() : extra_data.Keys.ToArray();
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
