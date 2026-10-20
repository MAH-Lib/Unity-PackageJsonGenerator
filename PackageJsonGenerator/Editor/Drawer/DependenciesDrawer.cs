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
        private const string PACKAGE_INFO_DEPENDENCIES_LABEL = "外部パッケージ情報設定";
        private const string PACKAGE_INFO_DEPENDENCIES_LIST_LABEL = "外部パッケージ一覧";
        private const string PACKAGE_INFO_DEPENDENCIES_INPUT_LABEL = "外部パッケージ";

        //====変数======================================
        [SerializeField] private bool _isShowDependencies = true;

        //====描画======================================
        /// <summary> 外部パッケージの描画 </summary>
        internal void Draw(PackageOptionalSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //外部パッケージ情報
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!settings.Dependencies.Any())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_INPUT_LABEL, label_layout);
                        EditorGUILayout.LabelField(DrawerUtility.LIST_EMPTY_LABEL, GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(DrawerUtility.LIST_ADD_BUTTON, button_layout))
                        {
                            settings.Dependencies.Add(new());
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _isShowDependencies = EditorGUILayout.Foldout(_isShowDependencies, PACKAGE_INFO_DEPENDENCIES_LIST_LABEL);
                        if (_isShowDependencies)
                        {
                            GUILayoutOption depensencies_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_LOW_INCIDENT_SIZE);
                            GUILayoutOption package_popup_layout = GUILayout.MaxWidth(200);
                            GUILayoutOption version_layout = GUILayout.MaxWidth(65);
                            GUILayoutOption version_input_layout = GUILayout.MaxWidth(65);

                            EditorGUI.indentLevel++;

                            string[] package_names = UnityPackageHelper.AccPackageNames;
                            string[] package_versions = UnityPackageHelper.AccPackageVersions;

                            foreach (DependenciesInfo dependencies in settings.Dependencies)
                            {
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    //パッケージ名
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_NAME_LABEL, depensencies_layout);
                                        //確定時のみ検証するため DelayedTextField を使う（1文字ずつ検証すると "." を入力できない）
                                        string name = EditorGUILayout.DelayedTextField(dependencies.PacckageName, GUILayout.ExpandWidth(true));
                                        name = name.ToLowerInvariant();
                                        if (name != dependencies.PacckageName)
                                        {
                                            if (string.IsNullOrEmpty(name))
                                            {
                                                dependencies.PacckageName = DependenciesInfo.PACKAGE_DEFAULT_NAME;
                                                field_errors.Clear(dependencies);
                                            }
                                            else if (PackageJsonValidator.IsValidPackageName(name))
                                            {
                                                dependencies.PacckageName = name;
                                                field_errors.Clear(dependencies);
                                            }
                                            else
                                                field_errors.Set(dependencies, string.Format(DrawerUtility.PACKAGE_NAME_ERROR_MESSAGE, name));
                                            dependencies.SelectPackageIndex = PackageSettingsConverter.FindPackageIndex(dependencies.PacckageName);
                                        }

                                        EditorGUI.BeginDisabledGroup(!UnityPackageHelper.AccIsLoaded);
                                        int index = EditorGUILayout.Popup(dependencies.SelectPackageIndex, package_names, package_popup_layout);
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
                                        EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_VERSION_LABEL, depensencies_layout);
                                        DrawerUtility.DrawVersionFields(ref dependencies.Version, version_layout, version_input_layout);
                                    }
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(DrawerUtility.LIST_REMOVE_BUTTON, button_layout))
                                {
                                    int count = settings.Dependencies.Count;
                                    field_errors.Clear(settings.Dependencies[count - 1]);
                                    settings.Dependencies.RemoveAt(--count);
                                }
                                if (GUILayout.Button(DrawerUtility.LIST_ADD_BUTTON, button_layout))
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
            if (!PackageSettingsConverter.TryConvertToVersionVector(package_versions[index], out info.Version))
                Debug.LogWarning($"外部パッケージ {name} のバージョン \"{package_versions[index]}\" を {PackageSettingsConverter.ConvertToVersionString(info.Version)} として設定しました");
            info.PacckageName = name;
            info.SelectPackageIndex = index;
        }
    }
}
#endif
