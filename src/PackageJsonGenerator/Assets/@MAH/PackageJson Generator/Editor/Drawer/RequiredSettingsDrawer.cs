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

        //入力エラーの辞書のキー
        private const string FIELD_KEY_PACKAGE_NAME = "PackageName";
        private const string FIELD_KEY_PRE_RELEASE = "PreRelease";
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
        /// <summary> 必須項目の描画（on_create_license：「LICENSE.md を作成」、on_apply_current_editor：「現在の Editor に合わせる」ボタンを押したときの処理） </summary>
        internal void Draw(PackageRequiredSettings settings, FieldErrorStore field_errors, Action on_create_license, Action on_apply_current_editor)
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(DrawerUtility.INFO_BUTTON_SIZE);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                //必須設定項目ラベル
                DrawerUtility.DrawSectionTitle(LocalizedText.Get(TextId.RequiredSection));

                EditorGUILayout.Space();

                //パッケージ情報設定
                DrawPackageInfoSettings(settings, field_errors, label_layout);

                EditorGUILayout.Space();

                //Unityバージョン設定
                DrawUnityVersionSettings(settings, label_layout, on_apply_current_editor);

                EditorGUILayout.Space();

                //ライセンス情報
                DrawLicenseInfoSettings(settings, label_layout, on_create_license);

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

            EditorGUILayout.LabelField(LocalizedText.Get(TextId.PackageInfoSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //パッケージ名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.PackageName), label_layout);
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
                        field_errors.Set(FIELD_KEY_PACKAGE_NAME, LocalizedText.Format(TextId.PackageNameError, name));
                }
            }
            field_errors.Draw(FIELD_KEY_PACKAGE_NAME);

            //パッケージバージョン設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.Version), label_layout);
                DrawerUtility.DrawVersionFields(ref settings.Version, ref settings.PreRelease, field_errors, FIELD_KEY_PRE_RELEASE, version_layout, version_input_layout);
            }
            field_errors.Draw(FIELD_KEY_PRE_RELEASE);

            //パッケージ表示名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.DisplayName), label_layout);
                settings.DisplayName = EditorGUILayout.TextField(settings.DisplayName, GUILayout.ExpandWidth(true));
            }

            //パッケージ概要設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.Description), label_layout);
                settings.Description = EditorGUILayout.TextArea(settings.Description, GUILayout.ExpandWidth(true));
            }
            EditorGUI.indentLevel--;
        }

        //Unityバージョン設定
        private void DrawUnityVersionSettings(PackageRequiredSettings settings, GUILayoutOption label_layout, Action on_apply_current_editor)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.UnityVersionSection), EditorStyles.boldLabel);
                //unityRelease は任意項目にあるので、両方を書き換える処理は Window に任せる
                if (GUILayout.Button(LocalizedText.Get(TextId.ApplyCurrentEditorButton), GUILayout.ExpandWidth(false)))
                    on_apply_current_editor?.Invoke();
            }
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.Major), label_layout);
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
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.Minor), label_layout);
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
                labels.Add($"{to_label(current)}{LocalizedText.Get(TextId.UnityVersionUnlisted)}");
                index = labels.Count - 1;
            }

            int selected = EditorGUILayout.Popup(index, labels.ToArray(), GUILayout.ExpandWidth(true));
            return selected < versions.Length ? versions[selected] : current;
        }

        //パッケージ著者情報設定
        private void DrawAuthorSettings(PackageRequiredSettings settings, FieldErrorStore field_errors, GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.AuthorSection), EditorStyles.boldLabel, label_layout);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.AuthorName), label_layout);
                settings.AuthorInfo.Name = EditorGUILayout.TextField(settings.AuthorInfo.Name, GUILayout.ExpandWidth(true));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.AuthorEmail), label_layout);
                settings.AuthorInfo.Email = DrawerUtility.DrawValidatedTextField(field_errors, FIELD_KEY_AUTHOR_EMAIL, settings.AuthorInfo.Email, PackageJsonValidator.IsValidEmail, TextId.EmailError);
            }
            field_errors.Draw(FIELD_KEY_AUTHOR_EMAIL);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.AuthorURL), label_layout);
                settings.AuthorInfo.URL = DrawerUtility.DrawValidatedTextField(field_errors, FIELD_KEY_AUTHOR_URL, settings.AuthorInfo.URL, PackageJsonValidator.IsValidURL, TextId.URLError);
                if (GUILayout.Button(LocalizedText.Get(TextId.OpenURLButton), button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(settings.AuthorInfo.URL))
                        Application.OpenURL(settings.AuthorInfo.URL);
                }
            }
            field_errors.Draw(FIELD_KEY_AUTHOR_URL);
            EditorGUI.indentLevel--;
        }

        //ライセンス情報設定
        private void DrawLicenseInfoSettings(PackageRequiredSettings settings, GUILayoutOption label_layout, Action on_create_license)
        {
            EditorGUILayout.LabelField(LocalizedText.Get(TextId.LicenseSection), EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            //ライセンス名
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.License), label_layout);
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
                //独自ライセンスは定型文が無いので、ひな形の LICENSE.md を作れるようにする
                if (_licenseType == LicenseType.Proprietary && GUILayout.Button(LocalizedText.Get(TextId.CreateLicenseButton), GUILayout.ExpandWidth(false)))
                    on_create_license?.Invoke();
            }

            EditorGUI.indentLevel--;
        }
    }
}
#endif
