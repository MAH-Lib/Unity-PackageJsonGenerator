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
        private const string UNITY_VERSION_UNLISTED_LABEL = "（一覧外）";
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

        private const string OVERWRITE_DIALOG_TITLE = "package.json の上書き";
        private const string OVERWRITE_DIALOG_MESSAGE = "は既に存在します。上書きしますか？";
        private const string OVERWRITE_DIALOG_OK = "上書き";
        private const string OVERWRITE_DIALOG_CANCEL = "キャンセル";
        private const string OVERWRITE_CANCELED_MESSAGE = "上書きをキャンセルしました";
        private const string CREATE_SUCCEEDED_MESSAGE = "package.json を作成しました";
        private const string LOAD_SUCCEEDED_MESSAGE = "package.json を読み込みました";
        private const string WARNING_COUNT_MESSAGE = "警告 {0} 件（詳しくは Console を参照）";

        private const string PACKAGE_NAME_ERROR_MESSAGE = "「{0}」はパッケージ名の形式ではありません（例：com.company.my-tool。小文字の英数字とハイフンをドットで区切ります）";
        private const string EMAIL_ERROR_MESSAGE = "「{0}」は E-mail の形式ではありません（例：name@example.com）";
        private const string URL_ERROR_MESSAGE = "「{0}」は URL の形式ではありません（http:// か https:// で始めてください）";
        private const string SAMPLE_PATH_ERROR_MESSAGE = "「{0}」からサンプルのパスを作れません。Samples か Samples~ フォルダの下のフォルダを指定してください（例：Samples~/Basic）";

        //入力エラーの辞書のキー（固定の欄）
        private const string FIELD_KEY_PACKAGE_NAME = "PackageName";
        private const string FIELD_KEY_AUTHOR_EMAIL = "AuthorEmail";
        private const string FIELD_KEY_AUTHOR_URL = "AuthorURL";
        private const string FIELD_KEY_CHANGELOG_URL = "ChangelogURL";
        private const string FIELD_KEY_DOCUMENTATION_URL = "DocumentationURL";
        private const string FIELD_KEY_LICENSES_URL = "LicensesURL";

        private const string LIST_EMPTY_LABEL = "None Data ...";
        private const string LIST_ADD_BUTTON = "Add";
        private const string LIST_REMOVE_BUTTON = "Remove";

        private const int INFO_LABEL_SIZE = 130;
        private const int INFO_LABEL_LOW_INCIDENT_SIZE = 110;
        private const int INFO_BUTTON_SIZE = 80;

        private static readonly Regex PackageNameRegex = new(@"^[a-z0-9\-]+(\.[a-z0-9\-]+)+$", RegexOptions.Compiled);
        private static readonly Regex PackageEmailRegex = new(@"^[^@\s]+@[^@\s\.]+\.[^@\s\.]+(\.[^@\s\.]+)*$", RegexOptions.Compiled);
        private static readonly Regex VersionVectorRegex = new(@"^(\d+)(?:\.(\d+))?(?:\.(\d+))?(.*)$", RegexOptions.Compiled);
        private static readonly Regex PackageURLRegex = new(@"^https?:\/\/[\w\-]+(\.[\w\-]+)+([\/\w\-.~:?#[\]@!$&'()*+,;=]*)?$", RegexOptions.Compiled);

        //====変数======================================
        //ドメインリロード後も入力を残すため、入力状態は SerializeField にする
        [SerializeField] private Vector3 _scrollPos;

        [SerializeField] private PackageRequiredSettings _requiredSettings = new();
        [SerializeField] private PackageOptionalSettings _optionalSettings = new();
        [SerializeField] private LicenseType _licenseType;
        [SerializeField] private string _licenseName;

        [SerializeField] private string _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;

        [SerializeField] private bool _isShowKeyword;
        [SerializeField] private bool _isShowSamples;
        [SerializeField] private bool _isShowDependencies;

        /// <summary> 初期化済み判定（ドメインリロード時は初期化しない） </summary>
        [SerializeField] private bool _isInitialized;

        /// <summary> 欄ごとの入力エラー（キーは欄の定数、またはリストの行のオブジェクト） </summary>
        private readonly Dictionary<object, string> _fieldErrors = new();
        /// <summary> 最後の操作の結果 </summary>
        private string _resultMessage;
        private MessageType _resultMessageType;
        /// <summary> 最後の操作中に出た警告の件数 </summary>
        private int _warningCount;
        /// <summary> 次の OnGUI で入力欄のフォーカスを外すか </summary>
        private bool _isFocusClearRequested;

        //====Unity====================================
        void OnEnable()
        {
            if (!_isInitialized)
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

                _isInitialized = true;
            }

            //登録済みパッケージの一覧は static のためドメインリロードで消える。毎回読み込む
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
            if (_isFocusClearRequested)
            {
                GUI.FocusControl(null);
                _isFocusClearRequested = false;
            }

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
                //確定した値が変わったときだけ検証する（毎フレーム検証するとエラー表示がすぐ消えるため）
                if (name != _requiredSettings.Name)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        _requiredSettings.Name = PackageRequiredSettings.PACKAGE_DEFAULT_NAME;
                        ClearFieldError(FIELD_KEY_PACKAGE_NAME);
                    }
                    else if (IsValidPackageName(name.ToLowerInvariant()))
                    {
                        _requiredSettings.Name = name.ToLowerInvariant();
                        ClearFieldError(FIELD_KEY_PACKAGE_NAME);
                    }
                    else
                        SetFieldError(FIELD_KEY_PACKAGE_NAME, string.Format(PACKAGE_NAME_ERROR_MESSAGE, name));
                }
            }
            DrawFieldError(FIELD_KEY_PACKAGE_NAME);

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
                int major = DrawVersionPopup(_requiredSettings.MajorVer, UnityVersionConverter.GetMajorVersions(), UnityVersionConverter.GetMajorVersionLabel);
                if (_requiredSettings.MajorVer != major)
                {
                    //Major を変えたら Minor はその Major の一覧の先頭（最も古いもの）にする
                    int[] minors = UnityVersionConverter.GetMinorVersions(major);
                    _requiredSettings.MajorVer = major;
                    _requiredSettings.MinorVer = minors.Any() ? minors[0] : UnityVersionConverter.DEFAULT_MINOR_VERSION;
                }
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, label_layout);
                _requiredSettings.MinorVer = DrawVersionPopup(_requiredSettings.MinorVer, UnityVersionConverter.GetMinorVersions(_requiredSettings.MajorVer), minor => minor.ToString());
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
                _requiredSettings.AuthorInfo.Email = DrawValidatedTextField(FIELD_KEY_AUTHOR_EMAIL, _requiredSettings.AuthorInfo.Email, IsValidEmail, EMAIL_ERROR_MESSAGE);
            }
            DrawFieldError(FIELD_KEY_AUTHOR_EMAIL);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_AUTHOR_URL_LABEL, label_layout);
                _requiredSettings.AuthorInfo.URL = DrawValidatedTextField(FIELD_KEY_AUTHOR_URL, _requiredSettings.AuthorInfo.URL, IsValidURL, URL_ERROR_MESSAGE);
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_requiredSettings.AuthorInfo.URL))
                        Application.OpenURL(_requiredSettings.AuthorInfo.URL);
                }
            }
            DrawFieldError(FIELD_KEY_AUTHOR_URL);
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
                    _requiredSettings.License = EditorGUILayout.TextField(_requiredSettings.License, GUILayout.ExpandWidth(true));
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
                _optionalSettings.ChangelogURL = DrawValidatedTextField(FIELD_KEY_CHANGELOG_URL, _optionalSettings.ChangelogURL, IsValidURL, URL_ERROR_MESSAGE);
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.ChangelogURL))
                        Application.OpenURL(_optionalSettings.ChangelogURL);
                }
            }
            DrawFieldError(FIELD_KEY_CHANGELOG_URL);

            //ドキュメントRUL
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_DOCUMENTATION_URL_LABEL, label_layout);
                _optionalSettings.DocumentationURL = DrawValidatedTextField(FIELD_KEY_DOCUMENTATION_URL, _optionalSettings.DocumentationURL, IsValidURL, URL_ERROR_MESSAGE);
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.DocumentationURL))
                        Application.OpenURL(_optionalSettings.DocumentationURL);
                }
            }
            DrawFieldError(FIELD_KEY_DOCUMENTATION_URL);

            //ライセンスURL
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(PACKAGE_INFO_LICENSES_URL_LABEL, label_layout);
                _optionalSettings.LicensesURL = DrawValidatedTextField(FIELD_KEY_LICENSES_URL, _optionalSettings.LicensesURL, IsValidURL, URL_ERROR_MESSAGE);
                if (GUILayout.Button(PACKAGE_INFO_AUTHOR_URL_OPEN_BUTTON, button_layout))
                {
                    if (!string.IsNullOrWhiteSpace(_optionalSettings.LicensesURL))
                        Application.OpenURL(_optionalSettings.LicensesURL);
                }
            }
            DrawFieldError(FIELD_KEY_LICENSES_URL);
            EditorGUI.indentLevel--;
        }

        /// <summary> 検証付きの入力欄描画（確定した値が変わったときだけ検証し、不正ならエラーを登録して今の値を返す） </summary>
        private string DrawValidatedTextField(object key, string current, Func<string, bool> is_valid, string error_format)
        {
            string input = EditorGUILayout.DelayedTextField(current, GUILayout.ExpandWidth(true));
            if (input == current)
                return current;

            if (string.IsNullOrEmpty(input))
            {
                ClearFieldError(key);
                return string.Empty;
            }
            if (is_valid(input))
            {
                ClearFieldError(key);
                return input;
            }

            SetFieldError(key, string.Format(error_format, input));
            return current;
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
                                        //確定時のみ検証するため DelayedTextField を使う（1文字ずつ検証すると入力できない）
                                        string selected_path = EditorGUILayout.DelayedTextField(sample.Path, GUILayout.ExpandWidth(true));
                                        //ファイル設定ボタン
                                        if (GUILayout.Button(PACKAGE_INFO_PATH_SETTING_BUTTON_NAME, apth_button_layout))
                                        {
                                            selected_path = EditorUtility.OpenFolderPanel(PACKAGE_INFO_SAMPLE_PATH_HEADER, Application.dataPath, string.Empty);
                                        }
                                        //パスリセットボタン（入力欄の古い値で上書きされないよう、比較対象もリセット値にする）
                                        if (GUILayout.Button(PACKAGE_INFO_RESET_BUTTON_NAME, apth_button_layout))
                                        {
                                            sample.Path = PACKAGE_INFO_SAMPLE_KEYWORD_2;
                                            selected_path = sample.Path;
                                            ClearFieldError(sample);
                                        }
                                        //空はフォルダ選択のキャンセルなので何もしない
                                        if (!string.IsNullOrEmpty(selected_path) && sample.Path != selected_path)
                                        {
                                            if (IsValidSampleFolderpath(selected_path, out string sample_path))
                                            {
                                                sample.Path = sample_path;
                                                ClearFieldError(sample);
                                            }
                                            else
                                                SetFieldError(sample, string.Format(SAMPLE_PATH_ERROR_MESSAGE, selected_path));
                                        }
                                    }
                                    DrawFieldError(sample);
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LIST_REMOVE_BUTTON, button_layout))
                                {
                                    int count = _optionalSettings.SamplesInfo.Count;
                                    ClearFieldError(_optionalSettings.SamplesInfo[count - 1]);
                                    _optionalSettings.SamplesInfo.RemoveAt(--count);
                                }
                                if (GUILayout.Button(LIST_ADD_BUTTON, button_layout))
                                {
                                    _optionalSettings.SamplesInfo.Add(new() { Path = PACKAGE_INFO_SAMPLE_KEYWORD_2 });
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
                                        //確定時のみ検証するため DelayedTextField を使う（1文字ずつ検証すると "." を入力できない）
                                        string name = EditorGUILayout.DelayedTextField(dependencies.PacckageName, GUILayout.ExpandWidth(true));
                                        name = name.ToLowerInvariant();
                                        if (name != dependencies.PacckageName)
                                        {
                                            if (string.IsNullOrEmpty(name))
                                            {
                                                dependencies.PacckageName = DependenciesInfo.PACKAGE_DEFAULT_NAME;
                                                ClearFieldError(dependencies);
                                            }
                                            else if (IsValidPackageName(name))
                                            {
                                                dependencies.PacckageName = name;
                                                ClearFieldError(dependencies);
                                            }
                                            else
                                                SetFieldError(dependencies, string.Format(PACKAGE_NAME_ERROR_MESSAGE, name));
                                            dependencies.SelectPackageIndex = FindPackageIndex(dependencies.PacckageName);
                                        }

                                        EditorGUI.BeginDisabledGroup(!UnityPackageHelper.AccIsLoaded);
                                        int index = EditorGUILayout.Popup(dependencies.SelectPackageIndex, package_names, package_popup_layout);
                                        EditorGUI.EndDisabledGroup();

                                        if (dependencies.SelectPackageIndex != index)
                                        {
                                            SetDependenciesInfo(dependencies, index, package_names, package_versions);
                                            ClearFieldError(dependencies);
                                        }
                                    }
                                    DrawFieldError(dependencies);

                                    //パッケージバージョン（UPM は完全一致のバージョンのみ対応）
                                    using (new EditorGUILayout.HorizontalScope())
                                    {
                                        EditorGUILayout.LabelField(PACKAGE_INFO_VERSION_LABEL, depensencies_layout);
                                        EditorGUILayout.LabelField(PACKAGE_INFO_MAJOR_LABEL, version_layout);
                                        dependencies.Version.x = EditorGUILayout.IntField(dependencies.Version.x, version_input_layout);
                                        EditorGUILayout.LabelField(PACKAGE_INFO_MINOR_LABEL, version_layout);
                                        dependencies.Version.y = EditorGUILayout.IntField(dependencies.Version.y, version_input_layout);
                                        EditorGUILayout.LabelField(PACKAGE_INFO_PATCH_LABEL, version_layout);
                                        dependencies.Version.z = EditorGUILayout.IntField(dependencies.Version.z, version_input_layout);
                                    }
                                }
                            }
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                GUILayout.FlexibleSpace();
                                if (GUILayout.Button(LIST_REMOVE_BUTTON, button_layout))
                                {
                                    int count = _optionalSettings.Dependencies.Count;
                                    ClearFieldError(_optionalSettings.Dependencies[count - 1]);
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
            if (!TryConvertToVersionVector(package_versions[index], out info.Version))
                LogWarning($"外部パッケージ {name} のバージョン \"{package_versions[index]}\" を {ConvertToVersionString(info.Version)} として設定しました");
            info.PacckageName = name;
            info.SelectPackageIndex = index;
        }

        /// <summary> 登録済みパッケージ一覧から名前の選択配列番号を探す（見つからなければ 0） </summary>
        private int FindPackageIndex(string name)
        {
            int index = Array.IndexOf(UnityPackageHelper.AccPackageNames, name);
            return index > 0 ? index : 0;
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

                DrawResultMessage();

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
                //（確認ダイアログを OnGUI のレイアウト途中で出さないよう、delayCall で OnGUI の外で処理する）
                if (GUILayout.Button(PACKAGE_INFO_JSON_FILE_CREATE_BUTTON, GUILayout.ExpandWidth(this)))
                    EditorApplication.delayCall += CreatePackageJson;
                //読み込み
                if (GUILayout.Button(PACKAGE_INFO_JSON_FILE_LOAD_BUTTON, GUILayout.ExpandWidth(this)))
                    EditorApplication.delayCall += LoadPackageJson;
            }
        }

        /// <summary> package.json 生成 </summary>
        private void CreatePackageJson()
        {
            _warningCount = 0;
            if (!IsCanCreatePackageJson())
            {
                Repaint();
                return;
            }

            string file_path = PackageJsonFileUtility.GetPackageJsonPath(_pacckageJsonPath);
            if (File.Exists(file_path) &&
                !EditorUtility.DisplayDialog(OVERWRITE_DIALOG_TITLE, $"{file_path} {OVERWRITE_DIALOG_MESSAGE}", OVERWRITE_DIALOG_OK, OVERWRITE_DIALOG_CANCEL))
            {
                SetResultMessage(MessageType.Info, OVERWRITE_CANCELED_MESSAGE);
                Repaint();
                return;
            }

            PackageJsonInfo info = ConvertToPackageJsonInfo();
            if (!PackageJsonFileUtility.TrySavePackageJson(_pacckageJsonPath, info, out string error_message))
                LogError(error_message);
            else
                SetSucceededMessage($"{CREATE_SUCCEEDED_MESSAGE} : {file_path}");
            Repaint();
        }

        /// <summary> package.json 読み込み（失敗した場合は入力内容を変えない） </summary>
        private void LoadPackageJson()
        {
            _warningCount = 0;
            if (!PackageJsonFileUtility.TryLoadPackageJson(_pacckageJsonPath, out PackageJsonInfo info, out string error_message))
            {
                LogError(error_message);
                Repaint();
                return;
            }

            _fieldErrors.Clear();
            //入力欄にフォーカスがあると、読み込んだ値ではなく編集中の値が表示されるため、次の OnGUI で外す
            _isFocusClearRequested = true;
            ConvertToPackageSettings(info);
            SetSucceededMessage($"{LOAD_SUCCEEDED_MESSAGE} : {PackageJsonFileUtility.GetPackageJsonPath(_pacckageJsonPath)}");
            Repaint();
        }

        //====メッセージ==================================
        /// <summary> 欄の入力エラー登録 </summary>
        private void SetFieldError(object key, string message)
        {
            _fieldErrors[key] = message;
        }

        /// <summary> 欄の入力エラー解除 </summary>
        private void ClearFieldError(object key)
        {
            _fieldErrors.Remove(key);
        }

        /// <summary> 欄の入力エラー描画 </summary>
        private void DrawFieldError(object key)
        {
            if (_fieldErrors.TryGetValue(key, out string message))
                EditorGUILayout.HelpBox(message, MessageType.Error);
        }

        /// <summary> 警告を Console に出し、件数を数える </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning(message);
            _warningCount++;
        }

        /// <summary> エラーを Console に出し、操作結果にも表示する </summary>
        private void LogError(string message)
        {
            Debug.LogError(message);
            SetResultMessage(MessageType.Error, message);
        }

        /// <summary> 操作結果の設定 </summary>
        private void SetResultMessage(MessageType type, string message)
        {
            _resultMessageType = type;
            _resultMessage = message;
        }

        /// <summary> 成功時の操作結果の設定（警告があれば件数を付ける） </summary>
        private void SetSucceededMessage(string message)
        {
            if (_warningCount > 0)
                SetResultMessage(MessageType.Warning, $"{message} / {string.Format(WARNING_COUNT_MESSAGE, _warningCount)}");
            else
                SetResultMessage(MessageType.Info, message);
        }

        /// <summary> 操作結果の描画 </summary>
        private void DrawResultMessage()
        {
            if (!string.IsNullOrEmpty(_resultMessage))
                EditorGUILayout.HelpBox(_resultMessage, _resultMessageType);
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
                License = _requiredSettings.License?.Trim(),
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

            //不正な名前でも読み込んだ値は残し、警告だけ出す（生成前チェックで止める）
            _requiredSettings.Name = (package_json_info.Name ?? string.Empty).ToLowerInvariant();
            if (!IsValidPackageName(_requiredSettings.Name))
                LogWarning($"読み込んだパッケージ名の形式が正しくありません : {_requiredSettings.Name}");
            if (!TryConvertToVersionVector(package_json_info.Version, out _requiredSettings.Version))
                LogWarning($"version の値 \"{package_json_info.Version}\" を {ConvertToVersionString(_requiredSettings.Version)} として読み込みました");
            _requiredSettings.Description = package_json_info.Description ?? string.Empty;
            _requiredSettings.DisplayName = package_json_info.DisplayName ?? string.Empty;
            _requiredSettings.License = package_json_info.License ?? string.Empty;
            ConvertToUnityVersion(package_json_info.Unity, out _requiredSettings.MajorVer, out _requiredSettings.MinorVer);
            _requiredSettings.AuthorInfo = package_json_info.Author ?? new();

            _optionalSettings.ChangelogURL = package_json_info.ChangelogURL ?? string.Empty;
            _optionalSettings.DocumentationURL = package_json_info.DocumentationURL ?? string.Empty;
            _optionalSettings.LicensesURL = package_json_info.LicensesURL ?? string.Empty;
            _optionalSettings.UnityRelease = package_json_info.UnityRelease ?? string.Empty;
            _optionalSettings.HideInEditor = package_json_info.HideInEditor;
            _optionalSettings.Keywords = package_json_info.Keywords ?? new();
            _optionalSettings.Dependencies = ConvertToDependenciesInfo(package_json_info.Dependencies);
            _optionalSettings.SamplesInfo = package_json_info.Samples ?? new();
            //以前のバージョンが Windows で出力した "\" 区切りのパスを "/" 区切りに読み替える
            foreach (SampleInfo sample in _optionalSettings.SamplesInfo)
            {
                if (sample != null && sample.Path != null)
                    sample.Path = sample.Path.Replace('\\', '/');
            }

            _licenseType = SPDXLicenseConverter.SPDXLicenseToLicenseType(_requiredSettings.License);
            _licenseName = SPDXLicenseConverter.LicenseTypeToLicenseFullName(_licenseType);
        }

        private string ConvertToVersionString(Vector3Int version_vec)
        {
            return $"{version_vec.x}.{version_vec.y}.{version_vec.z}";
        }

        /// <summary> バージョン文字列を変換（要素の不足・余り・プレビュー表記・数値以外を補った場合は false） </summary>
        private bool TryConvertToVersionVector(string version, out Vector3Int result)
        {
            result = Vector3Int.zero;
            if (string.IsNullOrWhiteSpace(version))
                return false;

            Match match = VersionVectorRegex.Match(version.Trim());
            if (!match.Success)
                return false;

            bool is_exact = match.Groups[2].Success && match.Groups[3].Success && string.IsNullOrEmpty(match.Groups[4].Value);
            for (int index = 0; index < 3; index++)
            {
                Group group = match.Groups[index + 1];
                if (!group.Success)
                    continue;
                //int に収まらない数値は読めないものとして 0.0.0 にする
                if (!int.TryParse(group.Value, out int parse))
                {
                    result = Vector3Int.zero;
                    return false;
                }
                result[index] = parse;
            }
            return is_exact;
        }

        private string ConvertToUnityVersionString(int major, int minor)
        {
            return $"{major}.{minor}";
        }

        /// <summary> Load した unity の値を Major / Minor に変換（読めない値は初期値にして警告） </summary>
        private void ConvertToUnityVersion(string unity, out int major, out int minor)
        {
            if (!UnityVersionConverter.TryParseUnityVersion(unity, out major, out minor, out bool has_minor))
            {
                LogWarning($"unity の値 \"{unity}\" を読み込めないため {major}.{minor} にしました");
                return;
            }
            if (!has_minor)
            {
                LogWarning($"unity の値 \"{unity}\" に Minor が無いため {major}.{minor} にしました");
                return;
            }
            if (!UnityVersionConverter.IsKnownVersion(major, minor))
                LogWarning($"unity の値 \"{unity}\" は一覧に無いバージョンです。そのまま使います");
        }

        private Dictionary<string, string> ConvertToDependencies(List<DependenciesInfo> dependencies_infos)
        {
            if (dependencies_infos == null || !dependencies_infos.Any())
                return null;
            Dictionary<string, string> dependencies = new();
            foreach (DependenciesInfo info in dependencies_infos)
            {
                if (info == null)
                    continue;

                //初期値 "com." のままの行や不正な名前の行は出力しない
                if (!IsValidPackageName(info.PacckageName))
                {
                    LogWarning($"外部パッケージ名が正しくないため出力しません : {info.PacckageName}");
                    continue;
                }

                //UPM は完全一致のバージョンのみ対応（重複は生成前チェックで止めているが、念のため例外を出さない）
                dependencies.TryAdd(info.PacckageName, ConvertToVersionString(info.Version));
            }
            return dependencies.Any() ? dependencies : null;
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

                if (!VersionOperatorConverter.TryExtractVersion(key_value_pair.Value, out string version, out bool is_converted))
                {
                    LogWarning($"外部パッケージ {key_value_pair.Key} のバージョン \"{key_value_pair.Value}\" を読み込めないため除外しました");
                    continue;
                }
                if (is_converted)
                    LogWarning($"外部パッケージ {key_value_pair.Key} のバージョン \"{key_value_pair.Value}\" を \"{version}\" に変換しました（UPM は完全一致のバージョンのみ対応）");

                //TryExtractVersion が "X.Y.Z" 形式を返すので、ここでは補完が起きない
                TryConvertToVersionVector(version, out Vector3Int version_vec);
                DependenciesInfo info = new()
                {
                    PacckageName = key_value_pair.Key,
                    Version = version_vec,
                    SelectPackageIndex = FindPackageIndex(key_value_pair.Key),
                };

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

            //OS に関係なく "/" 区切りで扱い、フォルダ名単位で "Samples" / "Samples~" を探す（部分一致はさせない）
            string[] folders = path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries);
            int index = Array.FindLastIndex(folders, folder =>
                string.Equals(folder, PACKAGE_INFO_SAMPLE_KEYWORD_1, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(folder, PACKAGE_INFO_SAMPLE_KEYWORD_2, StringComparison.OrdinalIgnoreCase));
            if (index == -1 || index == folders.Length - 1)
                return false;

            sample_path = $"{PACKAGE_INFO_SAMPLE_KEYWORD_2}/{string.Join("/", folders.Skip(index + 1))}";
            return true;
        }

        private bool IsCanCreatePackageJson()
        {
            if (string.IsNullOrWhiteSpace(_requiredSettings.Name))
            {
                LogError("パッケージ名が入力されていません");
                return false;
            }
            if (!IsValidPackageName(_requiredSettings.Name))
            {
                LogError($"パッケージ名の形式が正しくありません : {_requiredSettings.Name}");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.Description))
            {
                LogError("パッケージ概要が入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.DisplayName))
            {
                LogError("UPM表示名が入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.License))
            {
                LogError("ライセンスが入力されていません");
                return false;
            }
            if (string.IsNullOrWhiteSpace(_requiredSettings.AuthorInfo.Name))
            {
                LogError("著者名が入力されていません");
                return false;
            }

            //外部パッケージ名の重複（不正な名前の行は出力されないので対象外）
            string[] duplicate_names = _optionalSettings.Dependencies
                .Where(info => info != null && IsValidPackageName(info.PacckageName))
                .GroupBy(info => info.PacckageName)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();
            if (duplicate_names.Any())
            {
                LogError($"外部パッケージ名が重複しています : {string.Join(", ", duplicate_names)}");
                return false;
            }

            return true;
        }
    }
}
#endif