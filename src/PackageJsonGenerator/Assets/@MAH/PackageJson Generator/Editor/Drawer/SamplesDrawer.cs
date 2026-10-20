#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> パッケージサンプルの描画 </summary>
    [Serializable]
    internal class SamplesDrawer
    {
        //====定数======================================

        //====変数======================================
        [SerializeField] private bool _isShowSamples = true;

        //====描画======================================
        /// <summary> パッケージサンプルの描画 </summary>
        internal void Draw(PackageOptionalSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.SampleSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //サンプル情報
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!settings.SamplesInfo.Any())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.Sample), label_layout);
                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.ListEmpty), GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
                        {
                            settings.SamplesInfo.Add(new() { Path = PackageJsonValidator.PACKAGE_INFO_SAMPLE_KEYWORD_2 });
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _isShowSamples = EditorGUILayout.Foldout(_isShowSamples, LocalizedText.Get(TextId.SampleList));
                        if (_isShowSamples)
                        {
                            GUILayoutOption sample_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_LOW_INCIDENT_SIZE);
                            GUILayoutOption path_button_layout = GUILayout.MaxWidth(65);
                            EditorGUI.indentLevel++;

                            foreach (SampleInfo sample in settings.SamplesInfo)
                            {
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    //表示名
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.DisplayName), sample_layout);
                                        sample.DisplayName = EditorGUILayout.TextField(sample.DisplayName, GUILayout.ExpandWidth(true));
                                    }
                                    //概要
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.Description), sample_layout);
                                        sample.Description = EditorGUILayout.TextField(sample.Description, GUILayout.ExpandWidth(true));
                                    }
                                    //フォルダーパス
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.SamplePath), sample_layout);
                                        //確定時のみ検証するため DelayedTextField を使う（1文字ずつ検証すると入力できない）
                                        string selected_path = EditorGUILayout.DelayedTextField(sample.Path, GUILayout.ExpandWidth(true));
                                        //ファイル設定ボタン
                                        if (GUILayout.Button(LocalizedText.Get(TextId.BrowseButton), path_button_layout))
                                        {
                                            selected_path = EditorUtility.OpenFolderPanel(LocalizedText.Get(TextId.SamplePathDialogTitle), Application.dataPath, string.Empty);
                                        }
                                        //パスリセットボタン（入力欄の古い値で上書きされないよう、比較対象もリセット値にする）
                                        if (GUILayout.Button(LocalizedText.Get(TextId.ResetButton), path_button_layout))
                                        {
                                            sample.Path = PackageJsonValidator.PACKAGE_INFO_SAMPLE_KEYWORD_2;
                                            selected_path = sample.Path;
                                            field_errors.Clear(sample);
                                        }
                                        //空はフォルダ選択のキャンセルなので何もしない
                                        if (!string.IsNullOrEmpty(selected_path) && sample.Path != selected_path)
                                        {
                                            if (PackageJsonValidator.IsValidSampleFolderPath(selected_path, out string sample_path))
                                            {
                                                sample.Path = sample_path;
                                                field_errors.Clear(sample);
                                            }
                                            else
                                                field_errors.Set(sample, LocalizedText.Format(TextId.SamplePathError, selected_path));
                                        }
                                    }
                                    field_errors.Draw(sample);
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LocalizedText.Get(TextId.ListRemoveButton), button_layout))
                                {
                                    int count = settings.SamplesInfo.Count;
                                    field_errors.Clear(settings.SamplesInfo[count - 1]);
                                    settings.SamplesInfo.RemoveAt(--count);
                                }
                                if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
                                {
                                    settings.SamplesInfo.Add(new() { Path = PackageJsonValidator.PACKAGE_INFO_SAMPLE_KEYWORD_2 });
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
