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
        private const string FOLDER_EMPTY_ERROR_MESSAGE = "フォルダーパスが指定されていません";
        private const string FOLDER_NOT_FOUND_ERROR_MESSAGE = "フォルダが存在しません";
        private const string FILE_NOT_FOUND_ERROR_MESSAGE = "指定したフォルダに package.json が存在しません";
        private const string SAVE_ERROR_MESSAGE = "package.json を保存できませんでした";
        private const string LOAD_ERROR_MESSAGE = "package.json を読み込めませんでした";

        //====パス=======================
        /// <summary> フォルダーパスから package.json のパス取得（"/" 区切り） </summary>
        internal static string GetPackageJsonPath(string folder)
        {
            return Path.Combine(folder ?? string.Empty, PACKAGE_JSON_OUTPUT_FILE_NAME).Replace('\\', '/');
        }

        //====保存=======================
        /// <summary> Package.json生成（失敗した場合は例外を出さずに false とエラー文を返す） </summary>
        internal static bool TrySavePackageJson(string folder, PackageJsonInfo package_json_info, out string error_message)
        {
            error_message = string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                error_message = FOLDER_EMPTY_ERROR_MESSAGE;
                return false;
            }
            if (!Directory.Exists(folder))
            {
                error_message = $"{FOLDER_NOT_FOUND_ERROR_MESSAGE} : {folder}";
                return false;
            }

            string json = JsonConvert.SerializeObject(package_json_info, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyAndNullResolver(),
                Formatting = Formatting.Indented
            });

            try
            {
                File.WriteAllText(GetPackageJsonPath(folder), json);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error_message = $"{SAVE_ERROR_MESSAGE} : {exception.Message}";
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
                error_message = FOLDER_EMPTY_ERROR_MESSAGE;
                return false;
            }

            string full_path = GetPackageJsonPath(folder);
            if (!File.Exists(full_path))
            {
                error_message = $"{FILE_NOT_FOUND_ERROR_MESSAGE} : {full_path}";
                return false;
            }

            try
            {
                string json = File.ReadAllText(full_path);
                package_json_info = JsonConvert.DeserializeObject<PackageJsonInfo>(json);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is JsonException)
            {
                error_message = $"{LOAD_ERROR_MESSAGE} : {exception.Message}";
                return false;
            }

            //空のファイルは null になる
            if (package_json_info == null)
            {
                error_message = $"{LOAD_ERROR_MESSAGE} : {full_path}";
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