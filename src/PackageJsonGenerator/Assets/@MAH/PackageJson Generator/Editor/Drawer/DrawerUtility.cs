#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 各 Drawer が共通で使うラベル・レイアウト・描画処理 </summary>
    internal static class DrawerUtility
    {
        //====定数======================================
        internal const string PACKAGE_INFO_NAME_LABEL = "パッケージ名";
        internal const string PACKAGE_INFO_VERSION_LABEL = "バージョン";
        internal const string PACKAGE_INFO_DESCRIPTION_LABEL = "概要";
        internal const string PACKAGE_INFO_DISPLAY_NAME_LABEL = "UPM表示名";
        internal const string PACKAGE_INFO_MAJOR_LABEL = "Major ";
        internal const string PACKAGE_INFO_MINOR_LABEL = "Minor";
        internal const string PACKAGE_INFO_PATCH_LABEL = "Patch";
        internal const string PACKAGE_INFO_PATH_SETTING_BUTTON_NAME = "Browse";
        internal const string PACKAGE_INFO_RESET_BUTTON_NAME = "Reset";
        internal const string PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON = "Open URL";

        internal const string LIST_EMPTY_LABEL = "None Data ...";
        internal const string LIST_ADD_BUTTON = "Add";
        internal const string LIST_REMOVE_BUTTON = "Remove";

        internal const string PACKAGE_NAME_ERROR_MESSAGE = "「{0}」はパッケージ名の形式ではありません（例：com.company.my-tool。小文字の英数字とハイフンをドットで区切ります）";
        internal const string EMAIL_ERROR_MESSAGE = "「{0}」は E-mail の形式ではありません（例：name@example.com）";
        internal const string URL_ERROR_MESSAGE = "「{0}」は URL の形式ではありません（http:// か https:// で始めてください）";
        internal const string SAMPLE_PATH_ERROR_MESSAGE = "「{0}」からサンプルのパスを作れません。Samples か Samples~ フォルダの下のフォルダを指定してください（例：Samples~/Basic）";

        internal const int INFO_LABEL_SIZE = 130;
        internal const int INFO_LABEL_LOW_INCIDENT_SIZE = 110;
        internal const int INFO_BUTTON_SIZE = 80;

        //====描画======================================
        /// <summary> 中央寄せの太字の見出し描画 </summary>
        internal static void DrawSectionTitle(string title)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary> 検証付きの入力欄描画（確定した値が変わったときだけ検証し、不正ならエラーを登録して今の値を返す） </summary>
        internal static string DrawValidatedTextField(FieldErrorStore field_errors, object key, string current, Func<string, bool> is_valid, string error_format)
        {
            string input = EditorGUILayout.DelayedTextField(current, GUILayout.ExpandWidth(true));
            if (input == current)
                return current;

            if (string.IsNullOrEmpty(input))
            {
                field_errors.Clear(key);
                return string.Empty;
            }
            if (is_valid(input))
            {
                field_errors.Clear(key);
                return input;
            }

            field_errors.Set(key, string.Format(error_format, input));
            return current;
        }

        /// <summary> バージョン（Major / Minor / Patch）の入力欄描画（負の値は 0 にする） </summary>
        internal static void DrawVersionFields(ref Vector3Int version, GUILayoutOption label_layout, GUILayoutOption input_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, label_layout);
            version.x = Mathf.Max(0, EditorGUILayout.IntField(version.x, input_layout));
            EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, label_layout);
            version.y = Mathf.Max(0, EditorGUILayout.IntField(version.y, input_layout));
            EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, label_layout);
            version.z = Mathf.Max(0, EditorGUILayout.IntField(version.z, input_layout));
        }
    }
}
#endif
