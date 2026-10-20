#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 必須項目（パッケージ情報・対応Unityバージョン・ライセンス・著者）の描画 </summary>
    [Serializable]
    internal class RequiredSettingsDrawer
    {
        //====定数======================================
        private const string REQUIRED_PACKAGE_INFO_LABEL = "必須設定項目";
        private const string PACKAGE_INFO_LABEL = "パッケージ情報設定";
        private const string PACKAGE_INFO_UNITY_VERSION_LABEL = "対応 Unity バージョン設定";
        private const string UNITY_VERSION_UNLISTED_LABEL = "（一覧外）";
        private const string PACKAGE_INFO_AUTHOR_LABEL = "著者情報設定";
        private const string PACKAGE_INFO_AUTHOR_NAME_LABEL = "著者名";
        private const string PACKAGE_INFO_AUTHOR_EMAIL_LABEL = "E-mail";
        private const string PACKAGE_INFO_AUTHOR_URL_LABEL = "URL";
        private const string PACKAGE_INFO_LICENSES_LABEL = "ライセンス情報設定";
        private const string PACKAGE_INFO_LICENSES_NAME_LABEL = "ライセンス";

        //入力エラーの辞書のキー
        private const string FIELD_KEY_PACKAGE_NAME = "PackageName";
        private const string FIELD_KEY_AUTHOR_EMAIL = "AuthorEmail";
        private const string FIELD_KEY_AUTHOR_URL = "AuthorURL";

        //====変数======================================
        [SerializeField] private LicenseType _licenseType;
        [SerializeField] private string _licenseName;

        //====ライセンス=================================
        /// <summary> ライセンスを初期値にする </summary>
        internal void ResetLicense(PackageRequiredSettings settings)
        {
            _licenseType = LicenseType.Unlicense;
            settings.License = SPDXLicenseConverter.LicenseTypeToSPDXLicense(_licenseType);
            _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(_licenseType);
        }

        /// <summary> 入力状態のライセンスに選択を合わせる（Load 後に呼ぶ） </summary>
        internal void SyncLicense(PackageRequiredSettings settings)
        {
            _licenseType = SPDXLicenseConverter.SPDXLicenseToLicenseType(settings.License);
            _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(_licenseType);
        }

        //====描画======================================
        /// <summary> 必須項目の描画 </summary>
        internal void Draw(PackageRequiredSettings settings, FieldErrorStore field_errors)
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(DrawerUtility.INFO_BUTTON_SIZE);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                //必須設定項目ラベル
                DrawerUtility.DrawSectionTitle(REQUIRED_PACKAGE_INFO_LABEL);

                EditorGUILayout.Space();

                //パッケージ情報設定
                DrawPackageInfoSettings(settings, field_errors, label_layout);

                EditorGUILayout.Space();

                //Unityバージョン設定
                DrawUnityVersionSettings(settings, label_layout);

                EditorGUILayout.Space();

                //ライセンス情報
                DrawLicenseInfoSettings(settings, label_layout);

                EditorGUILayout.Space();

                //パッケージ著者情報設定
                DrawAuthorSettings(settings, field_errors, label_layout, button_layout);

                EditorGUILayout.Space();
            }
        }

        //必須パッケージ情報設定
        private void DrawPackageInfoSettings(PackageRequiredSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout)
        {
            GUILayoutOption version_layout = GUILayout.MaxWidth(50);
            GUILayoutOption version_input_layout = GUILayout.MaxWidth(60);

            EditorGUILayout.LabelField(PACKAGE_INFO_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //パッケージ名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_NAME_LABEL, label_layout);
                string name = EditorGUILayout.DelayedTextField(settings.Name, GUILayout.ExpandWidth(true));
                //確定した値が変わったときだけ検証する（毎フレーム検証するとエラー表示がすぐ消えるため）
                if (name != settings.Name)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        settings.Name = PackageRequiredSettings.PACKAGE_DEFAULT_NAME;
                        field_errors.Clear(FIELD_KEY_PACKAGE_NAME);
                    }
                    else if (PackageJsonValidator.IsValidPackageName(name.ToLowerInvariant()))
                    {
                        settings.Name = name.ToLowerInvariant();
                        field_errors.Clear(FIELD_KEY_PACKAGE_NAME);
                    }
                    else
                        field_errors.Set(FIELD_KEY_PACKAGE_NAME, string.Format(DrawerUtility.PACKAGE_NAME_ERROR_MESSAGE, name));
                }
            }
            field_errors.Draw(FIELD_KEY_PACKAGE_NAME);

            //パッケージバージョン設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_VERSION_LABEL, label_layout);
                DrawerUtility.DrawVersionFields(ref settings.Version, version_layout, version_input_layout);
            }

            //パッケージ表示名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_DISPLAY_NAME_LABEL, label_layout);
                settings.DisplayName = EditorGUILayout.TextField(settings.DisplayName, GUILayout.ExpandWidth(true));
            }

            //パッケージ概要設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_DESCRIPTION_LABEL, label_layout);
                settings.Description = EditorGUILayout.TextArea(settings.Description, GUILayout.ExpandWidth(true));
            }
            EditorGUI.indentLevel--;
        }

        //Unityバージョン設定
        private void DrawUnityVersionSettings(PackageRequiredSettings settings, GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_VERSION_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_MAJOR_LABEL, label_layout);
                int major = DrawVersionPopup(settings.MajorVer, UnityVersionConverter.GetMajorVersions(), UnityVersionConverter.GetMajorVersionLabel);
                if (settings.MajorVer != major)
                {
                    //Major を変えたら Minor はその Major の一覧の先頭（最も古いもの）にする
                    int[] minors = UnityVersionConverter.GetMinorVersions(major);
                    settings.MajorVer = major;
                    settings.MinorVer = minors.Any() ? minors[0] : UnityVersionConverter.DEFAULT_MINOR_VERSION;
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(DrawerUtility.PACKAGE_INFO_MINOR_LABEL, label_layout);
                settings.MinorVer = DrawVersionPopup(settings.MinorVer, UnityVersionConverter.GetMinorVersions(settings.MajorVer), minor => minor.ToString());
            }
            EditorGUI.indentLevel--;
        }

        /// <summary> バージョン選択ポップアップ描画（一覧に無い現在値は「一覧外」として末尾に追加する） </summary>
        private int DrawVersionPopup(int current, int[] versions, Func<int, string> to_label)
        {
            List<string> labels = versions.Select(to_label).ToList();
            int index = Array.IndexOf(versions, current);
            if (index < 0)
            {
                labels.Add($"{to_label(current)}{UNITY_VERSION_UNLISTED_LABEL}");
                index = labels.Count - 1;
            }

            int selected = EditorGUILayout.Popup(index, labels.ToArray(), GUILayout.ExpandWidth(true));
            return selected < versions.Length ? versions[selected] : current;
        }

        //パッケージ著者情報設定
        private void DrawAuthorSettings(PackageRequiredSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_LABEL, EditorStyles.boldLabel, label_layout);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_NAME_LABEL, label_layout);
                settings.AuthorInfo.Name = EditorGUILayout.TextField(settings.AuthorInfo.Name, GUILayout.ExpandWidth(true));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_EMAIL_LABEL, label_layout);
                settings.AuthorInfo.Email = DrawerUtility.DrawValidatedTextField(field_errors, FIELD_KEY_AUTHOR_EMAIL, settings.AuthorInfo.Email, PackageJsonValidator.IsValidEmail, DrawerUtility.EMAIL_ERROR_MESSAGE);
            }
            field_errors.Draw(FIELD_KEY_AUTHOR_EMAIL);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_URL_LABEL, label_layout);
                settings.AuthorInfo.URL = DrawerUtility.DrawValidatedTextField(field_errors, FIELD_KEY_AUTHOR_URL, settings.AuthorInfo.URL, PackageJsonValidator.IsValidURL, DrawerUtility.URL_ERROR_MESSAGE);
                if (GUILayout.Button(DrawerUtility.PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(settings.AuthorInfo.URL))
                        Application.OpenURL(settings.AuthorInfo.URL);
                }
            }
            field_errors.Draw(FIELD_KEY_AUTHOR_URL);
            EditorGUI.indentLevel--;
        }

        //ライセンス情報設定
        private void DrawLicenseInfoSettings(PackageRequiredSettings settings, GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_LICENSES_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            //ライセンス名
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_LICENSES_NAME_LABEL, label_layout);
                LicenseType license_type = (LicenseType)EditorGUILayout.EnumPopup(_licenseType, GUILayout.MaxWidth(200));
                if (_licenseType != license_type)
                {
                    settings.License = SPDXLicenseConverter.LicenseTypeToSPDXLicense(license_type);
                    _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(license_type);
                    _licenseType = license_type;
                }
                if (_licenseType == LicenseType.Other)
                {
                    settings.License = EditorGUILayout.TextField(settings.License, GUILayout.ExpandWidth(true));
                }
                else
                {
                    GUIStyle boxStyle = new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.UpperLeft,
                    };
                    boxStyle.normal.textColor = Color.white;
                    EditorGUILayout.LabelField(_licenseName, boxStyle, GUILayout.ExpandWidth(true));
                }
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
