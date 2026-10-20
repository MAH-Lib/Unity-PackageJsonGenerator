#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 外部パッケージ（dependencies）の描画 </summary>
    [Serializable]
    internal class DependenciesDrawer
    {
        //====定数======================================
        //入力エラーの辞書のキー（行のオブジェクトと組にして使う）
        private const string FIELD_KEY_PRE_RELEASE = "PreRelease";

        //====変数======================================
        [SerializeField] private bool _isShowDependencies = true;

        //====描画======================================
        /// <summary> 外部パッケージの描画 </summary>
        internal void Draw(PackageOptionalSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.DependenciesSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //外部パッケージ情報
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!settings.Dependencies.Any())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.Dependency), label_layout);
                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.ListEmpty), GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
                        {
                            settings.Dependencies.Add(new());
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _isShowDependencies = EditorGUILayout.Foldout(_isShowDependencies, LocalizedText.Get(TextId.DependenciesList));
                        if (_isShowDependencies)
                        {
                            GUILayoutOption dependencies_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_LOW_INCIDENT_SIZE);
                            GUILayoutOption package_popup_layout = GUILayout.MaxWidth(200);
                            GUILayoutOption version_layout = GUILayout.MaxWidth(65);
                            GUILayoutOption version_input_layout = GUILayout.MaxWidth(65);

                            EditorGUI.indentLevel++;

                            string[] package_names = UnityPackageHelper.AccPackageNames;
                            string[] package_versions = UnityPackageHelper.AccPackageVersions;
                            //先頭の「選択なし」の項目は、今の表示言語の文言にする
                            string[] package_popup_names = (string[])package_names.Clone();
                            if (package_popup_names.Length > 0)
                                package_popup_names[0] = LocalizedText.Get(TextId.SelectPackage);

                            foreach (DependenciesInfo dependencies in settings.Dependencies)
                            {
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    //パッケージ名
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.PackageName), dependencies_layout);
                                        //確定時のみ検証するため DelayedTextField を使う（1文字ずつ検証すると "." を入力できない）
                                        string name = EditorGUILayout.DelayedTextField(dependencies.PackageName, GUILayout.ExpandWidth(true));
                                        name = name.ToLowerInvariant();
                                        if (name != dependencies.PackageName)
                                        {
                                            if (string.IsNullOrEmpty(name))
                                            {
                                                dependencies.PackageName = DependenciesInfo.PACKAGE_DEFAULT_NAME;
                                                field_errors.Clear(dependencies);
                                            }
                                            else if (PackageJsonValidator.IsValidPackageName(name))
                                            {
                                                dependencies.PackageName = name;
                                                field_errors.Clear(dependencies);
                                            }
                                            else
                                                field_errors.Set(dependencies, LocalizedText.Format(TextId.PackageNameError, name));
                                            dependencies.SelectPackageIndex = PackageSettingsConverter.FindPackageIndex(dependencies.PackageName);
                                        }

                                        EditorGUI.BeginDisabledGroup(!UnityPackageHelper.AccIsLoaded);
                                        int index = EditorGUILayout.Popup(dependencies.SelectPackageIndex, package_popup_names, package_popup_layout);
                                        EditorGUI.EndDisabledGroup();

                                        if (dependencies.SelectPackageIndex != index)
                                        {
                                            SetDependenciesInfo(dependencies, index, package_names, package_versions);
                                            field_errors.Clear(dependencies);
                                        }
                                    }
                                    field_errors.Draw(dependencies);

                                    //パッケージバージョン（UPM は完全一致のバージョンのみ対応）
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(LocalizedText.Get(TextId.Version), dependencies_layout);
                                        DrawerUtility.DrawVersionFields(ref dependencies.Version, ref dependencies.PreRelease, field_errors, GetPreReleaseKey(dependencies), version_layout, version_input_layout);
                                    }
                                    field_errors.Draw(GetPreReleaseKey(dependencies));
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LocalizedText.Get(TextId.ListRemoveButton), button_layout))
                                {
                                    int count = settings.Dependencies.Count;
                                    field_errors.Clear(settings.Dependencies[count - 1]);
                                    field_errors.Clear(GetPreReleaseKey(settings.Dependencies[count - 1]));
                                    settings.Dependencies.RemoveAt(--count);
                                }
                                if (GUILayout.Button(LocalizedText.Get(TextId.ListAddButton), button_layout))
                                {
                                    settings.Dependencies.Add(new());
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        /// <summary> ポップアップで選んだ登録済みパッケージの名前とバージョンを設定 </summary>
        private void SetDependenciesInfo(DependenciesInfo info, int index, string[] package_names, string[] package_versions)
        {
            if (index >= package_names.Length && index >= package_versions.Length)
                return;
            if (index == 0)
            {
                info.Reset();
                return;
            }
            string name = package_names[index];

            if (!PackageJsonValidator.IsValidPackageName(name))
                return;
            if (!PackageSettingsConverter.TryConvertToVersionVector(package_versions[index], out info.Version, out info.PreRelease))
                Debug.LogWarning(LocalizedText.Format(TextId.DependencyVersionSetWarning, name, package_versions[index], PackageSettingsConverter.ConvertToVersionString(info.Version, info.PreRelease)));
            info.PackageName = name;
            info.SelectPackageIndex = index;
        }

        /// <summary> 行のプレリリースの表記の入力エラーのキー取得（名前の入力エラーのキー＝行のオブジェクトと区別する） </summary>
        private static object GetPreReleaseKey(DependenciesInfo info)
        {
            return (info, FIELD_KEY_PRE_RELEASE);
        }
    }
}
#endif
