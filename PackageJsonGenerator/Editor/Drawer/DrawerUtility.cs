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
        //文言は LocalizedText の表にある

        internal const int INFO_LABEL_SIZE = 130;
        internal const int INFO_LABEL_LOW_INCIDENT_SIZE = 110;
        internal const int INFO_BUTTON_SIZE = 80;
        //英語の見出しは日本語より長いため、余裕を持たせる
        private const int SECTION_TITLE_MAX_WIDTH = 200;
        private const int PRE_RELEASE_LABEL_SIZE = 80;
        private const int PRE_RELEASE_INPUT_MIN_SIZE = 60;

        //====描画======================================
        /// <summary> 中央寄せの太字の見出し描画 </summary>
        internal static void DrawSectionTitle(string title)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel, GUILayout.MaxWidth(SECTION_TITLE_MAX_WIDTH));
                GUILayout.FlexibleSpace();
            }
        }

        /// <summary> 検証付きの入力欄描画（確定した値が変わったときだけ検証し、不正ならエラーを登録して今の値を返す） </summary>
        internal static string DrawValidatedTextField(FieldErrorStore field_errors, object key, string current, Func<string, bool> is_valid, TextId error_id, params GUILayoutOption[] options)
        {
            //レイアウトの指定が無ければ横幅いっぱいにする
            string input = EditorGUILayout.DelayedTextField(current, options.Length > 0 ? options : new[] { GUILayout.ExpandWidth(true) });
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

            field_errors.Set(key, LocalizedText.Format(error_id, input));
            return current;
        }

        /// <summary> バージョン（Major / Minor / Patch とプレリリースの表記）の入力欄描画（負の値は 0 にする。表記の入力エラーは pre_release_key で登録する） </summary>
        internal static void DrawVersionFields(ref Vector3Int version, ref string pre_release, FieldErrorStore field_errors, object pre_release_key, GUILayoutOption label_layout, GUILayoutOption input_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.Major), label_layout);
            version.x = Mathf.Max(0, EditorGUILayout.IntField(version.x, input_layout));
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.Minor), label_layout);
            version.y = Mathf.Max(0, EditorGUILayout.IntField(version.y, input_layout));
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.Patch), label_layout);
            version.z = Mathf.Max(0, EditorGUILayout.IntField(version.z, input_layout));
            //プレリリースの表記（"X.Y.Z-表記" の "表記" の部分）
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.PreRelease), GUILayout.MaxWidth(PRE_RELEASE_LABEL_SIZE));
            pre_release = DrawValidatedTextField(field_errors, pre_release_key, pre_release, PackageJsonValidator.IsValidPreRelease, TextId.PreReleaseError,
                GUILayout.MinWidth(PRE_RELEASE_INPUT_MIN_SIZE), GUILayout.ExpandWidth(true));
        }
    }
}
#endif
