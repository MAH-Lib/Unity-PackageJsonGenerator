#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class PackageJsonFileUtility
    {
        //====定数=======================
        private const string PACKGAGE_JSON_OUTPUT_FILE_NAME = "package.json";
        private const string SAVE_ERORR_MESSAGE = "保存パスが指定されていません";
        private const string LOAD_ERORR_MESSAGE = "指定したパスにファイルが存在しません";

        //====保存=======================
        /// <summary> Package.json生成 </summary>
        internal static void SavePackageJson(string path, PackageJsonInfo package_json_info)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.Log(SAVE_ERORR_MESSAGE);
                return;
            }

            string json = JsonConvert.SerializeObject(package_json_info, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreEmptyAndNullResolver(),
                Formatting = Formatting.Indented
            });

            string create_path = Path.Combine(path, PACKGAGE_JSON_OUTPUT_FILE_NAME);
            File.WriteAllText(create_path, json);
            AssetDatabase.Refresh();
        }

        //====読み込み=======================
        /// <summary> Package.json読み込み </summary>
        internal static PackageJsonInfo LoadPackageJson(string path)
        {
            string full_path = Path.Combine(path, PACKGAGE_JSON_OUTPUT_FILE_NAME);

            if (!File.Exists(full_path))
            {
                Debug.Log($"{LOAD_ERORR_MESSAGE} : {full_path}");
                return null;
            }
            
            string json = File.ReadAllText(full_path);
            return JsonConvert.DeserializeObject<PackageJsonInfo>(json);
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