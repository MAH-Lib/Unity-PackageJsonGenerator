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


        //入力エラーの辞書のキー
        private const string FIELD_KEY_CHANGELOG_URL = "ChangelogURL";
        private const string FIELD_KEY_DOCUMENTATION_URL = "DocumentationURL";
        private const string FIELD_KEY_LICENSES_URL = "LicensesURL";

        //====変数======================================
        [SerializeField] private bool _isShowKeyword = true;
        [SerializeField] private SamplesDrawer _samplesDrawer = new();
        [SerializeField] private DependenciesDrawer _dependenciesDrawer = new();

        /// <summary> 未対応キーの名前の一覧のキャッシュ（ExtraJson が変わったときだけ作り直す） </summary>
        [NonSerialized] private string _cachedExtraJson;
        [NonSerialized] private string[] _cachedExtraKeys = Array.Empty<string>();

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
                DrawerUtility.DrawSectionTitle(LocalizedText.Get(TextId.OptionalSection));

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

                //このツールが扱わないキー
                DrawExtraKeys(settings, label_layout, button_layout);
            }
        }

        //その他の項目（このツールが扱わないキー。Load した値を Create でそのまま書き戻す）
        private void DrawExtraKeys(PackageOptionalSettings settings, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            if (_cachedExtraJson != settings.ExtraJson)
            {
                _cachedExtraJson = settings.ExtraJson;
                _cachedExtraKeys = PackageSettingsConverter.GetExtraKeys(settings.ExtraJson);
            }
            if (!_cachedExtraKeys.Any())
                return;

            EditorGUILayout.LabelField(LocalizedText.Get(TextId.ExtraKeysSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(string.Join(", ", _cachedExtraKeys), EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
                if (GUILayout.Button(LocalizedText.Get(TextId.ClearButton), button_layout))
                    settings.ExtraJson = string.Empty;
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space();
        }

        //Unity情報設定
        private void DrawUnityInfoSettings(PackageOptionalSettings settings, GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.UnityInfoSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //リリースバージョン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.UnityRelease), label_layout);
                settings.UnityRelease = EditorGUILayout.TextField(settings.UnityRelease, GUILayout.ExpandWidth(true));
            }

            //UPMウィンドウ表示フラグ
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.HideInEditor), label_layout);
                settings.HideInEditor = EditorGUILayout.Toggle(settings.HideInEditor);
            }
            EditorGUI.indentLevel--;
        }

        //ドキュメント情報設定
        private void DrawDocumentationSettings(PackageOptionalSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.DocumentationSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //更新ログURL
            settings.ChangelogURL = DrawURLField(LocalizedText.Get(TextId.ChangelogURL), FIELD_KEY_CHANGELOG_URL, settings.ChangelogURL, field_errors, label_layout, button_layout);
            //ドキュメントURL
            settings.DocumentationURL = DrawURLField(LocalizedText.Get(TextId.DocumentationURL), FIELD_KEY_DOCUMENTATION_URL, settings.DocumentationURL, field_errors, label_layout, button_layout);
            //ライセンスURL
            settings.LicensesURL = DrawURLField(LocalizedText.Get(TextId.LicensesURL), FIELD_KEY_LICENSES_URL, settings.LicensesURL, field_errors, label_layout, button_layout);
            EditorGUI.indentLevel--;
        }

        /// <summary> URL の入力欄（検証と Open URL ボタン付き）の描画 </summary>
        private string DrawURLField(string label, string key, string url, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, label_layout);
                url = DrawerUtility.DrawValidatedTextField(field_errors, key, url, PackageJsonValidator.IsValidURL, TextId.URLError);
                if (GUILayout.Button(LocalizedText.Get(TextId.OpenURLButton), button_layout))
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
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.KeywordSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (!settings.Keywords.Any())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(LocalizedText.Get(TextId.Keyword), label_layout);
                    EditorGUILayout.LabelField(LocalizedText.Get(TextId.ListEmpty), GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
                    {
                        settings.Keywords.Add(string.Empty);
                    }
                }
            }
            else
            {
                _isShowKeyword = EditorGUILayout.Foldout(_isShowKeyword, LocalizedText.Get(TextId.KeywordList));
                if (_isShowKeyword)
                {
                    GUILayoutOption keyword_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_LOW_INCIDENT_SIZE);
                    EditorGUI.indentLevel++;
                    for (int index = 0; index < settings.Keywords.Count; index++)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField(LocalizedText.Get(TextId.Keyword), keyword_layout);
                            settings.Keywords[index] = EditorGUILayout.TextField(settings.Keywords[index], GUILayout.ExpandWidth(true));
                            if (GUILayout.Button(LocalizedText.Get(TextId.ListRemoveButton), button_layout))
                            {
                                settings.Keywords.RemoveAt(index);
                                index--;
                            }
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
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
