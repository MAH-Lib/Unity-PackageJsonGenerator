#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    public class PackageJsonCreateWindow : EditorWindow
    {
        //====定数======================================
        private const string PACKAGE_JSON_CREATE_WINDOW_NAME = "PackageJson Generator";
        private const string PACKAGE_JSON_MENU_PATH = "MAH/Generator/PackageJson Generator Window";

        private const string PACKGAGE_JSON_OUTPUT_DEFAULT_PATH = "Assets";

        private const string PACKAGE_JSON_OUTPUT_SETTING_LABEL = "package.json ファイル操作";
        private const string PACKAGE_JSON_OUTPUT_PATH_SETTING_WINDOW_LABEL = "package.json の保存先を選択";
        private const string PACKAGE_INFO_PATH_SETTING_BUTTON_NAME = "Browse";
        private const string PACKAGE_INFO_RESET_BUTTON_NAME = "Reset";
        private const string PACKAGE_INFO_JSON_FILE_PATH_BUTTON = "フォルダーパス";
        private const string PACKAGE_INFO_JSON_FILE_CREATE_BUTTON = "Create package.json";
        private const string PACKAGE_INFO_JSON_FILE_LOAD_BUTTON = "Load package.json";

        private const string REQUIRED_PACKAGE_INFO_LABEL = "必須設定項目";
        private const string PACKAGE_INFO_LABEL = "パッケージ情報設定";
        private const string PACKAGE_INFO_NAME_LABEL = "パッケージ名";
        private const string PACKAGE_INFO_VERSION_LABEL = "バージョン";
        private const string PACKAGE_INFO_DESCRIPTION_LABEL = "概要";
        private const string PACKAGE_INFO_DISPLAY_NAME_LABEL = "UPM表示名";
        private const string PACKAGE_INFO_UNITY_VERSION_LABEL = "対応 Unity バージョン設定";
        private const string PACKAGE_INFO_MAJOR_LABEL = "Major ";
        private const string PACKAGE_INFO_MINOR_LABEL = "Minor";
        private const string PACKAGE_INFO_PATCH_LABEL = "Patch";
        private const string PACKAGE_INFO_AUTHOR_LABEL = "著者情報設定";
        private const string PACKAGE_INFO_AUTHOR_NAME_LABEL = "著者名";
        private const string PACKAGE_INFO_AUTHOR_EMAIL_LABEL = "E-mail";
        private const string PACKAGE_INFO_AUTHOR_URL_LABEL = "URL";
        private const string PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON = "Open URL";

        private const string PACKAGE_INFO_LICENSES_LABEL = "ライセンス情報設定";
        private const string PACKAGE_INFO_LICENSES_NAME_LABEL = "ライセンス";

        private const string OPTIONAL_PACKAGE_INFO_LABEL = "任意設定項目";
        private const string PACKAGE_INFO_UNITY_LABEL = "Unity 情報設定";
        private const string PACKAGE_INFO_UNITY_RELEASE_VERSITON_LABEL = "リリースバージョン";
        private const string PACKAGE_INFO_UPM_HIDE_LABEL = "UMP除外フラグ";
        private const string PACKAGE_INFO_DOCS_LABEL = "ドキュメント情報設定";
        private const string PACKAGE_INFO_CHANGELOG_URL_LABEL = "変更ログURL";
        private const string PACKAGE_INFO_DOCUMENTATION_URL_LABEL = "ドキュメントURL";
        private const string PACKAGE_INFO_LICENSES_URL_LABEL = "ライセンスURL";

        private const string PACKAGE_INFO_KEYWORD_LABEL = "キーワード情報設定";
        private const string PACKAGE_INFO_KEYWORD_LIST_LABEL = "キーワード一覧";
        private const string PACKAGE_INFO_KEYWORD_INPUT_LABEL = "キーワード";

        private const string PACKAGE_INFO_UNITY_SAMPLE_LABEL = "パッケージサンプル情報設定";
        private const string PACKAGE_INFO_SAMPLE_PATH_LABEL = "パス";
        private const string PACKAGE_INFO_SAMPLE_LIST_LABEL = "サンプル一覧";
        private const string PACKAGE_INFO_SAMPLE_INPUT_LABEL = "サンプル";
        private const string PACKAGE_INFO_SAMPLE_PATH_HEADER = "Samples フォルダーパス設定";
        private const string PACKAGE_INFO_SAMPLE_KEYWORD_1 = "Samples";
        private const string PACKAGE_INFO_SAMPLE_KEYWORD_2 = "Samples~";

        private const string PACKAGE_INFO_DEPENDENCIES_LABEL = "外部パッケージ情報設定";
        private const string PACKAGE_INFO_DEPENDENCIES_LIST_LABEL = "外部パッケージ一覧";
        private const string PACKAGE_INFO_DEPENDENCIES_INPUT_LABEL = "外部パッケージ";
        private const string PACKAGE_INFO_DEPENDENCIES_VERSION_OPERATER_LABEL = "条件";
        private const string PACKAGE_INFO_DEPENDENCIES_VERSION_FORM_LABEL = "以上 >=";
        private const string PACKAGE_INFO_DEPENDENCIES_VERSION_TO_LABEL = "以下 <=";

        private const string LIST_EMPTY_LABEL = "None Data ...";
        private const string LIST_ADD_BUTTON = "Add";
        private const string LIST_REMOVE_BUTTON = "Remove";

        private const int INFO_LABEL_SIZE = 130;
        private const int INFO_LABEL_LOW_INCIDENT_SIZE = 110;
        private const int INFO_BUTTON_SIZE = 80;

        private static readonly Regex PackageNameRegex = new(@"^[a-z0-9\-]+(\.[a-z0-9\-]+)+$", RegexOptions.Compiled);
        private static readonly Regex PackageEmailRegex = new(@"^[^@\s]+@[^@\s\.]+\.[^@\s\.]+(\.[^@\s\.]+)*$", RegexOptions.Compiled);
        private static readonly Regex PackageURLRegex = new(@"^https?:\/\/[\w\-]+(\.[\w\-]+)+([\/\w\-.~:?#[\]@!$&'()*+,;=]*)?$", RegexOptions.Compiled);

        //====変数======================================
        private Vector3 _scrollPos;

        private PackageRequiredSettings _requiredSettings = new();
        private PackageOptionalSettings _optionalSettings = new();
        private LicenseType _licenseType;
        private string _licenseName;

        private string _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;

        private bool _isShowKeyword;
        private bool _isShowSamples;
        private bool _isShowDependencies;

        //====Unity====================================
        void OnEnable()
        {
            _requiredSettings = new();
            _optionalSettings = new();
            _licenseType = LicenseType.Unlicense;
            _requiredSettings.License = SPDXLicenseConverter.LicenseTypeToSPDXLicense(_licenseType);
            _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(_licenseType);

            _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;

            _isShowKeyword = true;
            _isShowSamples = true;
            _isShowDependencies = true;

            UnityPackageHelper.LoadPackageInfo();
        }

        //====ウィンドウ=================================
        [MenuItem(PACKAGE_JSON_MENU_PATH)]
        private static void ShowWindow()
        {
            //Editor Window表示
            if (Application.isEditor)
                GetWindow<PackageJsonCreateWindow>(PACKAGE_JSON_CREATE_WINDOW_NAME);
        }

        //====GUI=======================================
        void OnGUI()
        {
            using (EditorGUILayout.ScrollViewScope scroll_view = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                DrawRequiredSettings();
                DrawOptionalSettings();

                _scrollPos = scroll_view.scrollPosition;
            }
            DrawPackageFileOperations();
        }

        //====必須項目設定================================
        private void DrawRequiredSettings()
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(INFO_BUTTON_SIZE);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                //必須設定項目ラベル
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(REQUIRED_PACKAGE_INFO_LABEL, EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();

                //パッケージ情報設定
                DrawPackageInfoSettings(label_layout);

                EditorGUILayout.Space();

                //Unityバージョン設定
                DrawUnityVersionSettings(label_layout);

                EditorGUILayout.Space();

                //ライセンス情報
                DrawLicenseInfoSettings(label_layout);

                EditorGUILayout.Space();

                //パッケージ著者情報設定
                DrawAuthorSettings(label_layout, button_layout);

                EditorGUILayout.Space();
            }
        }

        //必須パッケージ情報設定
        private void DrawPackageInfoSettings(GUILayoutOption label_layout)
        {
            GUILayoutOption version_layout = GUILayout.MaxWidth(50);
            GUILayoutOption version_input_layout = GUILayout.MaxWidth(60);

            EditorGUILayout.LabelField(PACKAGE_INFO_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //パッケージ名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_NAME_LABEL, label_layout);
                string name = EditorGUILayout.DelayedTextField(_requiredSettings.Name, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(name))
                    _requiredSettings.Name = PackageRequiredSettings.PACKAGE_DEFAULT_NAME;
                else
                {
                    name = name.ToLowerInvariant();
                    if (IsValidPackageName(name))
                        _requiredSettings.Name = name;
                }
            }

            //パッケージバージョン設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_VERSION_LABEL, label_layout);
                EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, version_layout);
                _requiredSettings.Version.x = EditorGUILayout.IntField(_requiredSettings.Version.x, version_input_layout);
                EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, version_layout);
                _requiredSettings.Version.y = EditorGUILayout.IntField(_requiredSettings.Version.y, version_input_layout);
                EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, version_layout);
                _requiredSettings.Version.z = EditorGUILayout.IntField(_requiredSettings.Version.z, version_input_layout);
            }

            //パッケージ表示名設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_DISPLAY_NAME_LABEL, label_layout);
                _requiredSettings.DisplayName = EditorGUILayout.TextField(_requiredSettings.DisplayName, GUILayout.ExpandWidth(true));
            }

            //パッケージ概要設定
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_DESCRIPTION_LABEL, label_layout);
                _requiredSettings.Description = EditorGUILayout.TextArea(_requiredSettings.Description, GUILayout.ExpandWidth(true));
            }
            EditorGUI.indentLevel--;
        }

        //Unityバージョン設定
        private void DrawUnityVersionSettings(GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_VERSION_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, label_layout);
                MajorVersion major = (MajorVersion)EditorGUILayout.EnumPopup(_requiredSettings.MajorVer, GUILayout.ExpandWidth(true));
                if (_requiredSettings.MajorVer != major)
                {
                    _requiredSettings.MajorVer = major;
                    _requiredSettings.MinorVer = UnityVersionConverter.DEFAULT_MINOR_VERSION;
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, label_layout);
                Type minor_type = UnityVersionConverter.GetMinorVersionForMajorVersion(_requiredSettings.MajorVer);
                Enum currentValue = (Enum)Enum.ToObject(minor_type, _requiredSettings.MinorVer);
                Enum newValue = EditorGUILayout.EnumPopup(currentValue, GUILayout.ExpandWidth(true));
                _requiredSettings.MinorVer = Convert.ToInt32(newValue);
            }
            EditorGUI.indentLevel--;
        }

        //パッケージ著者情報設定
        private void DrawAuthorSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_LABEL, EditorStyles.boldLabel, label_layout);
            EditorGUI.indentLevel++;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_NAME_LABEL, label_layout);
                _requiredSettings.AuthorInfo.Name = EditorGUILayout.TextField(_requiredSettings.AuthorInfo.Name, GUILayout.ExpandWidth(true));
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_EMAIL_LABEL, label_layout);
                string e_mail = EditorGUILayout.DelayedTextField(_requiredSettings.AuthorInfo.Email, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(e_mail))
                    _requiredSettings.AuthorInfo.Email = string.Empty;
                else if (IsValidEmail(e_mail))
                    _requiredSettings.AuthorInfo.Email = e_mail;
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_URL_LABEL, label_layout);
                string url = EditorGUILayout.DelayedTextField(_requiredSettings.AuthorInfo.URL, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(url))
                    _requiredSettings.AuthorInfo.URL = string.Empty;
                else if (IsValidURL(url))
                    _requiredSettings.AuthorInfo.URL = url;
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_requiredSettings.AuthorInfo.URL))
                        Application.OpenURL(_requiredSettings.AuthorInfo.URL);
                }
            }
            EditorGUI.indentLevel--;
        }

        //ライセンス情報設定
        private void DrawLicenseInfoSettings(GUILayoutOption label_layout)
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
                    _requiredSettings.License = SPDXLicenseConverter.LicenseTypeToSPDXLicense(license_type);
                    _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(license_type);
                    _licenseType = license_type;
                }
                if (_licenseType == LicenseType.Other)
                {
                    EditorGUILayout.TextField(_requiredSettings.License, GUILayout.ExpandWidth(true));
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

        //====任意項目設定================================
        private void DrawOptionalSettings()
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(INFO_BUTTON_SIZE);

            EditorGUILayout.Space();

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                //任意設定項目ラベル
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(OPTIONAL_PACKAGE_INFO_LABEL, EditorStyles.boldLabel, GUILayout.MaxWidth(100));
                    GUILayout.FlexibleSpace();
                }

                //Unity情報設定
                DrawUnityInfoSettings(label_layout);

                EditorGUILayout.Space();

                //ドキュメント情報設定
                DrawDocumentationSettings(label_layout, button_layout);

                EditorGUILayout.Space();

                //パッケージ情報設定
                //DrawSubPackageInfoSettings(label_layout, button_layout);

                //キーワード設定
                DrawKeyWordList(label_layout, button_layout);

                EditorGUILayout.Space();

                //サンプル情報設定
                DrawSamplesInfoSettings(label_layout, button_layout);

                EditorGUILayout.Space();

                //外部パッケージ情報設定
                DrawDependenciesSettings(label_layout, button_layout);

                EditorGUILayout.Space();
            }
        }

        //Unity情報設定
        private void DrawUnityInfoSettings(GUILayoutOption label_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //リリースバージョン
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_RELEASE_VERSITON_LABEL, label_layout);
                _optionalSettings.UnityRelease = EditorGUILayout.TextField(_optionalSettings.UnityRelease, GUILayout.ExpandWidth(true));
            }

            //UPMウィンドウ表示フラグ
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_UPM_HIDE_LABEL, label_layout);
                _optionalSettings.HideInEditor = EditorGUILayout.Toggle(_optionalSettings.HideInEditor);
            }
            EditorGUI.indentLevel--;
        }

        //ドキュメント情報設定
        private void DrawDocumentationSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_DOCS_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //更新ログURL
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_CHANGELOG_URL_LABEL, label_layout);
                string url = EditorGUILayout.DelayedTextField(_optionalSettings.ChangelogURL, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(url))
                    _optionalSettings.ChangelogURL = string.Empty;
                else if (IsValidURL(url))
                    _optionalSettings.ChangelogURL = url;
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.ChangelogURL))
                        Application.OpenURL(_optionalSettings.ChangelogURL);
                }
            }

            //ドキュメントRUL
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_DOCUMENTATION_URL_LABEL, label_layout);
                string url = EditorGUILayout.DelayedTextField(_optionalSettings.DocumentationURL, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(url))
                    _optionalSettings.DocumentationURL = string.Empty;
                else if (IsValidURL(url))
                    _optionalSettings.DocumentationURL = url;
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.DocumentationURL))
                        Application.OpenURL(_optionalSettings.DocumentationURL);
                }
            }

            //ライセンスURL
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_LICENSES_URL_LABEL, label_layout);
                string url = EditorGUILayout.DelayedTextField(_optionalSettings.LicensesURL, GUILayout.ExpandWidth(true));
                if (string.IsNullOrEmpty(url))
                    _optionalSettings.LicensesURL = string.Empty;
                else if (IsValidURL(url))
                    _optionalSettings.LicensesURL = url;
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.LicensesURL))
                        Application.OpenURL(_optionalSettings.LicensesURL);
                }
            }
            EditorGUI.indentLevel--;
        }

        //キーワード設定
        private void DrawKeyWordList(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            if (!_optionalSettings.Keywords.Any())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_INPUT_LABEL, label_layout);
                    EditorGUILayout.LabelField(LIST_EMPTY_LABEL, GUILayout.ExpandWidth(true));
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                    {
                        _optionalSettings.Keywords.Add(string.Empty);
                    }
                }
            }
            else
            {
                _isShowKeyword = EditorGUILayout.Foldout(_isShowKeyword, PACKAGE_INFO_KEYWORD_LIST_LABEL);
                if (_isShowKeyword)
                {
                    GUILayoutOption keyword_layout = GUILayout.MaxWidth(INFO_LABEL_LOW_INCIDENT_SIZE);
                    EditorGUI.indentLevel++;
                    for (int index = 0; index < _optionalSettings.Keywords.Count; index++)
                    {
                        using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
                        {
                            EditorGUILayout.LabelField(PACKAGE_INFO_KEYWORD_INPUT_LABEL, keyword_layout);
                            _optionalSettings.Keywords[index] = EditorGUILayout.TextField(_optionalSettings.Keywords[index], GUILayout.ExpandWidth(true));
                            if (GUILayout.Button(LIST_REMOVE_BUTTON, button_layout))
                            {
                                _optionalSettings.Keywords.RemoveAt(index);
                                index--;
                            }
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                        {
                            _optionalSettings.Keywords.Add(string.Empty);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        //パッケージサンプル情報設定
        private void DrawSamplesInfoSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_UNITY_SAMPLE_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //サンプル情報
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!_optionalSettings.SamplesInfo.Any())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(PACKAGE_INFO_SAMPLE_INPUT_LABEL, label_layout);
                        EditorGUILayout.LabelField(LIST_EMPTY_LABEL, GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                        {
                            _optionalSettings.SamplesInfo.Add(new() { Path = PACKAGE_INFO_SAMPLE_KEYWORD_2 });
                        }
                    }
                }
                else
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        _isShowSamples = EditorGUILayout.Foldout(_isShowSamples, PACKAGE_INFO_SAMPLE_LIST_LABEL);
                        if (_isShowSamples)
                        {
                            GUILayoutOption sample_layout = GUILayout.MaxWidth(INFO_LABEL_LOW_INCIDENT_SIZE);
                            GUILayoutOption apth_button_layout = GUILayout.MaxWidth(65);
                            EditorGUI.indentLevel++;

                            foreach (SampleInfo sample in _optionalSettings.SamplesInfo)
                            {
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    //表示名
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(PACKAGE_INFO_DISPLAY_NAME_LABEL, sample_layout);
                                        sample.DisplayName = EditorGUILayout.TextField(sample.DisplayName, GUILayout.ExpandWidth(true));
                                    }
                                    //概要
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(PACKAGE_INFO_DESCRIPTION_LABEL, sample_layout);
                                        sample.Description = EditorGUILayout.TextField(sample.Description, GUILayout.ExpandWidth(true));
                                    }
                                    //フォルダーパス
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(PACKAGE_INFO_SAMPLE_PATH_LABEL, sample_layout);
                                        string selected_path = EditorGUILayout.TextField(sample.Path, GUILayout.ExpandWidth(true));
                                        //ファイル設定ボタン
                                        if (GUILayout.Button(PACKAGE_INFO_PATH_SETTING_BUTTON_NAME, apth_button_layout))
                                        {
                                            selected_path = EditorUtility.OpenFolderPanel(PACKAGE_INFO_SAMPLE_PATH_HEADER, Application.dataPath, string.Empty);
                                        }
                                        //パスリセットボタン
                                        if (GUILayout.Button(PACKAGE_INFO_RESET_BUTTON_NAME, apth_button_layout))
                                        {
                                            sample.Path = PACKAGE_INFO_SAMPLE_KEYWORD_1;
                                        }
                                        if (sample.Path != selected_path)
                                        {
                                            if (IsValidSampleFolderpath(selected_path, out string sample_path))
                                                sample.Path = sample_path;
                                        }
                                    }
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LIST_REMOVE_BUTTON, button_layout))
                                {
                                    int count = _optionalSettings.SamplesInfo.Count;
                                    _optionalSettings.SamplesInfo.RemoveAt(--count);
                                }
                                if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                                {
                                    _optionalSettings.SamplesInfo.Add(new());
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

        //外部パッケージ情報
        private void DrawDependenciesSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_LABEL, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            //外部パッケージ情報
            using (new EditorGUILayout.HorizontalScope())
            {
                if (!_optionalSettings.Dependencies.Any())
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_INPUT_LABEL, label_layout);
                        EditorGUILayout.LabelField(LIST_EMPTY_LABEL, GUILayout.ExpandWidth(true));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                        {
                            _optionalSettings.Dependencies.Add(new());
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
                            GUILayoutOption depensencies_layout = GUILayout.MaxWidth(INFO_LABEL_LOW_INCIDENT_SIZE);
                            GUILayoutOption package_popup_layout = GUILayout.MaxWidth(200);
                            GUILayoutOption version_layout = GUILayout.MaxWidth(65);
                            GUILayoutOption version_input_layout = GUILayout.MaxWidth(65);
                            GUILayoutOption operator_layout = GUILayout.MaxWidth(150);
                            GUILayoutOption range_layout = GUILayout.MaxWidth(95);
                            GUILayoutOption range_version_layout = GUILayout.MaxWidth(80);
                            GUILayoutOption range_version_input_layout = GUILayout.MaxWidth(80);

                            EditorGUI.indentLevel++;

                            string[] package_names = UnityPackageHelper.AccPackageNames;
                            string[] package_versions = UnityPackageHelper.AccPackageVersions;

                            foreach (DependenciesInfo dependencies in _optionalSettings.Dependencies)
                            {
                                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                                {
                                    //パッケージ名
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(PACKAGE_INFO_NAME_LABEL, depensencies_layout);
                                        string name = EditorGUILayout.TextField(dependencies.PacckageName, GUILayout.ExpandWidth(true));
                                        name = name.ToLowerInvariant();
                                        if (string.IsNullOrEmpty(name))
                                            dependencies.PacckageName = "com.";
                                        else if (IsValidPackageName(name))
                                            dependencies.PacckageName = name;

                                        EditorGUI.BeginDisabledGroup(!UnityPackageHelper.AccIsLoaded);
                                        int index = EditorGUILayout.Popup(dependencies.SelectPackageIndex, package_names, package_popup_layout);
                                        EditorGUI.EndDisabledGroup();

                                        if (dependencies.SelectPackageIndex != index)
                                            SetDependenciesInfo(dependencies, index, package_names, package_versions);
                                    }

                                    if (dependencies.Operator == VersionOperator.Range)
                                    {
                                        //バージョン条件
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.LabelField(PACKAGE_INFO_VERSION_LABEL, depensencies_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_VERSION_OPERATER_LABEL, version_layout);
                                            dependencies.Operator = (VersionOperator)EditorGUILayout.EnumPopup(dependencies.Operator, GUILayout.ExpandWidth(true));
                                        }
                                        EditorGUI.indentLevel++;
                                        //最低バージョン
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_VERSION_FORM_LABEL, range_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, range_version_layout);
                                            dependencies.Version.x = EditorGUILayout.IntField(dependencies.Version.x, range_version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, range_version_layout);
                                            dependencies.Version.y = EditorGUILayout.IntField(dependencies.Version.y, range_version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, range_version_layout);
                                            dependencies.Version.z = EditorGUILayout.IntField(dependencies.Version.z, range_version_input_layout);
                                        }
                                        //最高バージョン
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_VERSION_TO_LABEL, range_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, range_version_layout);
                                            dependencies.OperatorVersion.x = EditorGUILayout.IntField(dependencies.OperatorVersion.x, range_version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, range_version_layout);
                                            dependencies.OperatorVersion.y = EditorGUILayout.IntField(dependencies.OperatorVersion.y, range_version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, range_version_layout);
                                            dependencies.OperatorVersion.z = EditorGUILayout.IntField(dependencies.OperatorVersion.z, range_version_input_layout);
                                        }
                                        EditorGUI.indentLevel--;
                                    }
                                    else
                                    {
                                        //パッケージバージョン
                                        using (new EditorGUILayout.HorizontalScope())
                                        {
                                            EditorGUILayout.LabelField(PACKAGE_INFO_VERSION_LABEL, depensencies_layout);

                                            //バージョン条件
                                            EditorGUILayout.LabelField(PACKAGE_INFO_DEPENDENCIES_VERSION_OPERATER_LABEL, version_layout);
                                            dependencies.Operator = (VersionOperator)EditorGUILayout.EnumPopup(dependencies.Operator, operator_layout);

                                            //バージョン情報
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, version_layout);
                                            dependencies.Version.x = EditorGUILayout.IntField(dependencies.Version.x, version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, version_layout);
                                            dependencies.Version.y = EditorGUILayout.IntField(dependencies.Version.y, version_input_layout);
                                            EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, version_layout);
                                            dependencies.Version.z = EditorGUILayout.IntField(dependencies.Version.z, version_input_layout);
                                        }
                                    }
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LIST_REMOVE_BUTTON, button_layout))
                                {
                                    int count = _optionalSettings.Dependencies.Count;
                                    _optionalSettings.Dependencies.RemoveAt(--count);
                                }
                                if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                                {
                                    _optionalSettings.Dependencies.Add(new());
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                }
            }

            EditorGUI.indentLevel--;
        }

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

            if (!IsValidPackageName(name))
                return;
            info.Version = ConvertToVersionVector(package_versions[index]);
            info.PacckageName = name;
            info.SelectPackageIndex = index;
        }

        //====ファイル操作================================
        private void DrawPackageFileOperations()
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(INFO_BUTTON_SIZE);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(PACKAGE_JSON_OUTPUT_SETTING_LABEL, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();

                DrawFileOutputSettings(label_layout, button_layout);

                EditorGUILayout.Space();

                DrawFileOptionButton();

                EditorGUILayout.Space(20);
            }
        }
        //ファイルパス設定
        private void DrawFileOutputSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            //生成パス入力
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_JSON_FILE_PATH_BUTTON, label_layout);
                _pacckageJsonPath = EditorGUILayout.TextField(_pacckageJsonPath, GUILayout.ExpandWidth(true));
                //ファイル設定ボタン
                if (GUILayout.Button(PACKAGE_INFO_PATH_SETTING_BUTTON_NAME, button_layout))
                {
                    string selected_path = EditorUtility.OpenFolderPanel(PACKAGE_JSON_OUTPUT_PATH_SETTING_WINDOW_LABEL, Application.dataPath, string.Empty);
                    if (!string.IsNullOrEmpty(selected_path))
                    {
                        // 相対パスに変換（例: "Assets/..." 形式に）
                        if (selected_path.StartsWith(Application.dataPath))
                            selected_path = $"{PACKGAGE_JSON_OUTPUT_DEFAULT_PATH}{selected_path.Substring(Application.dataPath.Length)}";
                        _pacckageJsonPath = selected_path;
                    }
                }
                //パスリセットボタン
                if (GUILayout.Button(PACKAGE_INFO_RESET_BUTTON_NAME, button_layout))
                {
                    _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;
                }
            }
        }

        //Package.json操作
        private void DrawFileOptionButton()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                //生成
                if (GUILayout.Button(PACKAGE_INFO_JSON_FILE_CREATE_BUTTON, GUILayout.ExpandWidth(this)))
                {
                    if (!IsCanCreatePackageJson())
                        return;
                    PackageJsonInfo info = ConvertToPackageJsonInfo();
                    PackageJsonFileUtility.SavePackageJson(_pacckageJsonPath, info);
                }
                //読み込み
                if (GUILayout.Button(PACKAGE_INFO_JSON_FILE_LOAD_BUTTON, GUILayout.ExpandWidth(this)))
                {
                    PackageJsonInfo info = PackageJsonFileUtility.LoadPackageJson(_pacckageJsonPath);
                    ConvertToPackageSettings(info);
                }
            }
        }

        //====Package.json==============================
        private PackageJsonInfo ConvertToPackageJsonInfo()
        {
            PackageJsonInfo info = new()
            {
                Name = _requiredSettings.Name,
                Version = ConvertToVersionString(_requiredSettings.Version),
                Description = _requiredSettings.Description,
                DisplayName = _requiredSettings.DisplayName,
                Unity = ConvertToUnityVersionString(_requiredSettings.MajorVer, _requiredSettings.MinorVer),
                UnityRelease = _optionalSettings.UnityRelease,
                License = _requiredSettings.License,
                LicensesURL = _optionalSettings.LicensesURL,
                ChangelogURL = _optionalSettings.ChangelogURL,
                DocumentationURL = _optionalSettings.DocumentationURL,
                HideInEditor = _optionalSettings.HideInEditor,
                Author = _requiredSettings.AuthorInfo,
                Keywords = _optionalSettings.Keywords != null && _optionalSettings.Keywords.Any() ? _optionalSettings.Keywords : null,
                Dependencies = ConvertToDependencies(_optionalSettings.Dependencies),
                Samples = _optionalSettings.SamplesInfo != null && _optionalSettings.SamplesInfo.Any() ? _optionalSettings.SamplesInfo : null
            };
            return info;
        }

        private void ConvertToPackageSettings(PackageJsonInfo package_json_info)
        {
            if (package_json_info == null)
            {
                _requiredSettings.Reset();
                _optionalSettings.Reset();
                return;
            }

            _requiredSettings.Name = package_json_info.Name ?? string.Empty;
            _requiredSettings.Version = ConvertToVersionVector(package_json_info.Version);
            _requiredSettings.Description = package_json_info.Description ?? string.Empty;
            _requiredSettings.DisplayName = package_json_info.DisplayName ?? string.Empty;
            _requiredSettings.License = package_json_info.License ?? string.Empty;
            _requiredSettings.MajorVer = ConvertToMajorVersion(package_json_info.Unity);
            _requiredSettings.MinorVer = ConvertToMinorVersion(package_json_info.Unity);
            _requiredSettings.AuthorInfo = package_json_info.Author ?? new();

            _optionalSettings.ChangelogURL = package_json_info.ChangelogURL ?? string.Empty;
            _optionalSettings.DocumentationURL = package_json_info.DocumentationURL ?? string.Empty;
            _optionalSettings.LicensesURL = package_json_info.LicensesURL ?? string.Empty;
            _optionalSettings.UnityRelease = package_json_info.UnityRelease ?? string.Empty;
            _optionalSettings.HideInEditor = package_json_info.HideInEditor;
            _optionalSettings.Keywords = package_json_info.Keywords ?? new();
            _optionalSettings.Dependencies = ConvertToDependenciesInfo(package_json_info.Dependencies);
            _optionalSettings.SamplesInfo = package_json_info.Samples ?? new();

            _licenseType = SPDXLicenseConverter.SPDXLicenseToLicenseType(_requiredSettings.License);
            _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(_licenseType);
        }

        private string ConvertToVersionString(Vector3Int version_vec)
        {
            return $"{version_vec.x}.{version_vec.y}.{version_vec.z}";
        }

        private Vector3Int ConvertToVersionVector(string version)
        {
            Vector3Int result = default;
            string[] version_str = version.Split('.');
            for (int index = 0; index < version_str.Length; index++)
            {
                if (!int.TryParse(version_str[index], out int parse))
                    continue;
                result[index] = parse;
            }
            return result;
        }

        private string ConvertToUnityVersionString(MajorVersion major, int minor)
        {
            return $"{UnityVersionConverter.MajorVersionToString(major)}.{minor}";
        }

        private MajorVersion ConvertToMajorVersion(string unity)
        {
            if (string.IsNullOrEmpty(unity))
                throw new NotImplementedException();
            string[] unity_str = unity.Split('.');

            if (unity_str == null || !unity_str.Any())
                throw new NotImplementedException();
            return UnityVersionConverter.StringToMajorVersion(unity_str[0]);
        }

        private int ConvertToMinorVersion(string unity)
        {
            if (string.IsNullOrEmpty(unity))
                throw new NotImplementedException();

            string[] unity_str = unity.Split('.');
            if (unity_str == null || unity_str.Length < 1)
                throw new NotImplementedException();

            if (!int.TryParse(unity_str[1], out int result))
                throw new NotImplementedException();

            return result;
        }

        private Dictionary<string, string> ConvertToDependencies(List<DependenciesInfo> dependencies_infos)
        {
            if (dependencies_infos == null || !dependencies_infos.Any())
                return null;
            Dictionary<string, string> dependencies = new();
            foreach (DependenciesInfo info in dependencies_infos)
            {
                if (info == null || string.IsNullOrWhiteSpace(info.PacckageName))
                    continue;

                string @operator = VersionOperatorConverter.VersionOperatorToOperatorString(info.Operator);
                string version = ConvertToVersionString(info.Version);

                if (info.Operator == VersionOperator.Range)
                {
                    string version_less = ConvertToVersionString(info.OperatorVersion);
                    dependencies.Add(info.PacckageName, $"{version}{@operator}{version_less}");
                }
                else
                {
                    dependencies.Add(info.PacckageName, $"{@operator}{version}");
                }
            }
            return dependencies;
        }

        private List<DependenciesInfo> ConvertToDependenciesInfo(Dictionary<string, string> dependencies)
        {
            if (dependencies == null || !dependencies.Any())
                return new();

            List<DependenciesInfo> dependencies_infos = new();

            foreach (KeyValuePair<string, string> key_value_pair in dependencies)
            {
                if (string.IsNullOrEmpty(key_value_pair.Key) || string.IsNullOrEmpty(key_value_pair.Value))
                    continue;

                string[] str_val = VersionOperatorConverter.SplitVersionConstraint(key_value_pair.Value);
                if (str_val == null || str_val.Length < 2)
                    continue;
                DependenciesInfo info = new()
                {
                    PacckageName = key_value_pair.Key,
                    Operator = VersionOperatorConverter.OperatorStringToVersionOperator(str_val[0]),
                    Version = ConvertToVersionVector(str_val[1]),
                };

                if (str_val.Length > 2 && !string.IsNullOrEmpty(str_val[2]))
                {
                    info.OperatorVersion = ConvertToVersionVector(str_val[2]);
                }

                dependencies_infos.Add(info);
            }

            return dependencies_infos;
        }

        //====入力判定===================================
        private bool IsValidPackageName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return PackageNameRegex.IsMatch(name);
        }

        private bool IsValidEmail(string e_mail)
        {
            if (string.IsNullOrWhiteSpace(e_mail))
                return false;

            return PackageEmailRegex.IsMatch(e_mail);
        }

        private bool IsValidURL(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return PackageURLRegex.IsMatch(url);
        }

        private bool IsValidSampleFolderpath(string path, out string sample_path)
        {
            sample_path = string.Empty;

            if (string.IsNullOrEmpty(path))
                return false;

            string sample_file_name = PACKAGE_INFO_SAMPLE_KEYWORD_1;
            int index = path.LastIndexOf(PACKAGE_INFO_SAMPLE_KEYWORD_1, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
            {
                index = path.LastIndexOf(PACKAGE_INFO_SAMPLE_KEYWORD_2, StringComparison.OrdinalIgnoreCase);
                if (index == -1)
                    return false;
                sample_file_name = PACKAGE_INFO_SAMPLE_KEYWORD_2;
            }

            string folidr_path = path.Substring(index + sample_file_name.Length).TrimStart(Path.DirectorySeparatorChar);
            if (string.IsNullOrEmpty(folidr_path))
                return false;

            sample_path = Path.Combine(PACKAGE_INFO_SAMPLE_KEYWORD_2, folidr_path);
            return true;
        }

        private bool IsCanCreatePackageJson()
        {
            if (string.IsNullOrWhiteSpace(_requiredSettings.Name))
            {
                Debug.LogError("パッケージ名が入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.Description))
            {
                Debug.LogError("パッケージ概要が入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.DisplayName))
            {
                Debug.LogError("UPM表示名が入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.License))
            {
                Debug.LogError("ライセンスが入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.AuthorInfo.Name))
            {
                Debug.LogError("著者名が入力されていません");
                return false;
            }

            return true;
        }
    }
}
#endif