#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 任意項目（Unity情報・ドキュメント・キーワード・サンプル・外部パッケージ）の描画 </summary>
    [Serializable]
    internal class OptionalSettingsDrawer
    {
        //====定数======================================
        private const string OPTIONAL_PACKAGE_INFO_LABEL = "任意設定項目";
        private const string PACKAGE_INFO_UNITY_LABEL = "Unity 情報設定";
        private const string PACKAGE_INFO_UNITY_RELEASE_VERSION_LABEL = "リリースバージョン";
        private const string PACKAGE_INFO_UPM_HIDE_LABEL = "UPM除外フラグ";
        private const string PACKAGE_INFO_DOCS_LABEL = "ドキュメント情報設定";
        private const string PACKAGE_INFO_CHANGELOG_URL_LABEL = "変更ログURL";
        private const string PACKAGE_INFO_DOCUMENTATION_URL_LABEL = "ドキュメントURL";
        private const string PACKAGE_INFO_LICENSES_URL_LABEL = "ライセンスURL";

        private const string PACKAGE_INFO_KEYWORD_LABEL = "キーワード情報設定";
        private const string PACKAGE_INFO_KEYWORD_LIST_LABEL = "キーワード一覧";
        private const string PACKAGE_INFO_KEYWORD_INPUT_LABEL = "キーワード";

        //入力エラーの辞書のキー
        private const string FIELD_KEY_CHANGELOG_URL = "ChangelogURL";
        private const string FIELD_KEY_DOCUMENTATION_URL = "DocumentationURL";
        private const string FIELD_KEY_LICENSES_URL = "LicensesURL";

        //====変数======================================
        [SerializeField] private bool _isShowKeyword = true;
        [SerializeField] private SamplesDrawer _samplesDrawer = new();
        [SerializeField] private DependenciesDrawer _dependenciesDrawer = new();

        //====描画======================================
        /// <summary> 任意項目の描画 </summary>
        internal void Draw(PackageOptionalSettings settings, FieldErrorStore field_errors)
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(DrawerUtility.INFO_BUTTON_SIZE);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                //任意設定項目ラベル
                DrawerUtility.DrawSectionTitle(OPTIONAL_PACKAGE_INFO_LABEL);

                //Unity情報設定
                DrawUnityInfoSettings(settings, label_layout);

                EditorGUILayout.Space();

                //ドキュメント情報設定
                DrawDocumentationSettings(settings, field_errors, label_layout, button_layout);

                EditorGUILayout.Space();

                //キーワード設定
                DrawKeyWordList(settings, label_layout, button_layout);

                EditorGUILayout.Space();

                //サンプル情報設定
                _samplesDrawer.Draw(settings, field_errors, label_layout, button_layout);

                EditorGUILayout.Space();

                //外部パッケージ情報設定
                _dependenciesDrawer.Draw(settings, field_errors, label_layout, button_layout);

                EditorGUILayout.Space();
            }
        }

        //Unity情報設定
        private void DrawUnityInfoSettings(PackageOptionalSettings settings, GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //リリースバージョン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_RELEASE_VERSION_LABEL, label_layout);
                settings.UnityRelease = EditorGUILayout.TextField(settings.UnityRelease, GUILayout.ExpandWidth(true));
            }

            //UPMウィンドウ表示フラグ
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_UPM_HIDE_LABEL, label_layout);
                settings.HideInEditor = EditorGUILayout.Toggle(settings.HideInEditor);
            }
            EditorGUI.indentLevel--;
        }

        //ドキュメント情報設定
        private void DrawDocumentationSettings(PackageOptionalSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_DOCS_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //更新ログURL
            settings.ChangelogURL = DrawURLField(PACKAGE_INFO_CHANGELOG_URL_LABEL, FIELD_KEY_CHANGELOG_URL, settings.ChangelogURL, field_errors, label_layout, button_layout);
            //ドキュメントURL
            settings.DocumentationURL = DrawURLField(PACKAGE_INFO_DOCUMENTATION_URL_LABEL, FIELD_KEY_DOCUMENTATION_URL, settings.DocumentationURL, field_errors, label_layout, button_layout);
            //ライセンスURL
            settings.LicensesURL = DrawURLField(PACKAGE_INFO_LICENSES_URL_LABEL, FIELD_KEY_LICENSES_URL, settings.LicensesURL, field_errors, label_layout, button_layout);
            EditorGUI.indentLevel--;
        }

        /// <summary> URL の入力欄（検証と Open URL ボタン付き）の描画 </summary>
        private string DrawURLField(string label, string key, string url, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, label_layout);
                url = DrawerUtility.DrawValidatedTextField(field_errors, key, url, PackageJsonValidator.IsValidURL, DrawerUtility.URL_ERROR_MESSAGE);
                if (GUILayout.Button(DrawerUtility.PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(url))
                        Application.OpenURL(url);
                }
            }
            field_errors.Draw(key);
            return url;
        }

        //キーワード設定
        private void DrawKeyWordList(PackageOptionalSettings settings, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (!settings.Keywords.Any())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_INPUT_LABEL, label_layout);
                    EditorGUILayout.LabelField(DrawerUtility.LIST_EMPTY_LABEL, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(DrawerUtility.LIST_ADD_BUTTON, button_layout))
                    {
                        settings.Keywords.Add(string.Empty);
                    }
                }
            }
            else
            {
                _isShowKeyword = EditorGUILayout.Foldout(_isShowKeyword, PACKAGE_INFO_KEYWORD_LIST_LABEL);
                if (_isShowKeyword)
                {
                    GUILayoutOption keyword_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_LOW_INCIDENT_SIZE);
                    EditorGUI.indentLevel++;
                    for (int index = 0; index < settings.Keywords.Count; index++)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_INPUT_LABEL, keyword_layout);
                            settings.Keywords[index] = EditorGUILayout.TextField(settings.Keywords[index], GUILayout.ExpandWidth(true));
                            if (GUILayout.Button(DrawerUtility.LIST_REMOVE_BUTTON, button_layout))
                            {
                                settings.Keywords.RemoveAt(index);
                                index--;
                            }
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(DrawerUtility.LIST_ADD_BUTTON, button_layout))
                        {
                            settings.Keywords.Add(string.Empty);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }
    }
}
#endif
