#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class PackageJsonFileUtility
    {
        //====定数=======================
        private const string PACKAGE_JSON_OUTPUT_FILE_NAME = "package.json";

        //====パス=======================
        /// <summary> フォルダーパスから package.json のパス取得（"/" 区切り） </summary>
        internal static string GetPackageJsonPath(string folder)
        {
            return Path.Combine(folder ?? string.Empty, PACKAGE_JSON_OUTPUT_FILE_NAME).Replace('\\', '/');
        }

        //====JSON=======================
        /// <summary> package.json に書き出す JSON の文字列取得（保存と JSON プレビューで同じものを使う。空の値は出力しない） </summary>
        internal static string SerializePackageJson(PackageJsonInfo package_json_info)
        {
            return JsonConvert.SerializeObject(package_json_info, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyAndNullResolver(),
                Formatting = Formatting.Indented
            });
        }

        //====保存=======================
        /// <summary> Package.json生成（失敗した場合は例外を出さずに false とエラー文を返す） </summary>
        internal static bool TrySavePackageJson(string folder, PackageJsonInfo package_json_info, out string error_message)
        {
            error_message = string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                error_message = LocalizedText.Get(TextId.FolderEmptyError);
                return false;
            }
            if (!Directory.Exists(folder))
            {
                error_message = LocalizedText.Format(TextId.FolderNotFoundError, folder);
                return false;
            }

            string json = SerializePackageJson(package_json_info);

            try
            {
                File.WriteAllText(GetPackageJsonPath(folder), json);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error_message = LocalizedText.Format(TextId.SaveFailedError, exception.Message);
                return false;
            }
            AssetDatabase.Refresh();
            return true;
        }

        //====読み込み=======================
        /// <summary> Package.json読み込み（失敗した場合は例外を出さずに false とエラー文を返す） </summary>
        internal static bool TryLoadPackageJson(string folder, out PackageJsonInfo package_json_info, out string error_message)
        {
            package_json_info = null;
            error_message = string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                error_message = LocalizedText.Get(TextId.FolderEmptyError);
                return false;
            }

            string full_path = GetPackageJsonPath(folder);
            if (!File.Exists(full_path))
            {
                error_message = LocalizedText.Format(TextId.FileNotFoundError, full_path);
                return false;
            }

            try
            {
                string json = File.ReadAllText(full_path);
                package_json_info = JsonConvert.DeserializeObject<PackageJsonInfo>(json);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
            {
                error_message = LocalizedText.Format(TextId.LoadFailedError, exception.Message);
                return false;
            }

            //空のファイルは null になる
            if (package_json_info == null)
            {
                error_message = LocalizedText.Format(TextId.LoadFailedError, full_path);
                return false;
            }
            return true;
        }
    }

    public class IgnoreEmptyAndNullResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            prop.ShouldSerialize = instance =>
            {
                var value = prop.ValueProvider.GetValue(instance);

                if (value == null)
                    return false;

                // 空文字列の場合はスキップ
                if (value is string str && string.IsNullOrWhiteSpace(str))
                    return false;

                // 空のリストや辞書もスキップ
                if (value is ICollection coll && coll.Count == 0)
                    return false;

                return true;
            };

            return prop;
        }
    }
}
#endif