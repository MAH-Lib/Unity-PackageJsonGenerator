#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    public class PackageJsonCreateWindow : EditorWindow
    {
        //====定数======================================
        private const string PACKAGE_JSON_CREATE_WINDOW_NAME = "PackageJson Generator";
        private const string PACKAGE_JSON_MENU_PATH = "MAH/Generator/PackageJson Generator Window";

        private const string PACKAGE_JSON_OUTPUT_DEFAULT_PATH = "Assets";

        private const int LANGUAGE_POPUP_SIZE = 100;
        private const int JSON_PREVIEW_MAX_HEIGHT = 200;
        //文言は LocalizedText の表にある

        //====変数======================================
        //ドメインリロード後も入力を残すため、入力状態と画面の状態は SerializeField にする
        [SerializeField] private Vector3 _scrollPos;

        [SerializeField] private PackageRequiredSettings _requiredSettings = new();
        [SerializeField] private PackageOptionalSettings _optionalSettings = new();

        [SerializeField, FormerlySerializedAs("_pacckageJsonPath")] private string _packageJsonPath = PACKAGE_JSON_OUTPUT_DEFAULT_PATH;

        [SerializeField] private RequiredSettingsDrawer _requiredDrawer = new();
        [SerializeField] private OptionalSettingsDrawer _optionalDrawer = new();

        /// <summary> 初期化済み判定（ドメインリロード時は初期化しない） </summary>
        [SerializeField] private bool _isInitialized;

        /// <summary> JSON プレビューの Foldout 開閉とスクロール位置 </summary>
        [SerializeField] private bool _isShowJsonPreview;
        [SerializeField] private Vector2 _jsonPreviewScrollPos;
        /// <summary> JSON プレビューに表示する文字列（Layout のイベントのときに作り直す） </summary>
        private string _jsonPreview = string.Empty;

        /// <summary> 欄ごとの入力エラー </summary>
        private readonly FieldErrorStore _fieldErrors = new();
        /// <summary> 最後の操作の結果 </summary>
        private string _resultMessage;
        private MessageType _resultMessageType;
        /// <summary> 最後の操作中に出た警告の件数 </summary>
        private int _warningCount;
        /// <summary> 次の OnGUI で入力欄のフォーカスを外すか </summary>
        private bool _isFocusClearRequested;
        /// <summary> 記憶したパッケージの一覧（SessionState から読んだもの。毎回読むと重いのでここに持つ） </summary>
        private List<PackageSnapshot> _rememberedPackages = new();

        //====Unity====================================
        void OnEnable()
        {
            if (!_isInitialized)
            {
                _requiredSettings = new();
                _optionalSettings = new();
                _requiredDrawer = new();
                _optionalDrawer = new();
                _requiredDrawer.ResetLicense(_requiredSettings);

                _packageJsonPath = PACKAGE_JSON_OUTPUT_DEFAULT_PATH;

                _isInitialized = true;
            }

            //登録済みパッケージの一覧は static のためドメインリロードで消える。毎回読み込む
            UnityPackageHelper.LoadPackageInfo();
            RefreshRememberedPackages();
        }

        void OnDisable()
        {
            //ウィンドウを閉じても保存前の編集を残すため、記憶済みのパッケージなら今の入力を記憶し直す
            if (IsCurrentPackageRemembered())
                RememberCurrentPackage();
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

            DrawLanguageSettings();
            DrawRememberedPackages();

            using (EditorGUILayout.ScrollViewScope scroll_view = new EditorGUILayout.ScrollViewScope(_scrollPos))
            {
                //（確認ダイアログを OnGUI のレイアウト途中で出さないよう、delayCall で OnGUI の外で処理する）
                _requiredDrawer.Draw(_requiredSettings, _fieldErrors, () => EditorApplication.delayCall += CreateLicenseFile, ApplyCurrentEditorVersion);
                _optionalDrawer.Draw(_optionalSettings, _fieldErrors);

                _scrollPos = scroll_view.scrollPosition;
            }
            DrawPackageFileOperations();
        }

        //====言語設定================================
        /// <summary> 表示言語の選択（右上に置く） </summary>
        private void DrawLanguageSettings()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.Language), GUILayout.Width(EditorStyles.label.CalcSize(new GUIContent(LocalizedText.Get(TextId.Language))).x + 4));
                LanguageType language = (LanguageType)EditorGUILayout.Popup((int)LocalizedText.AccCurrentLanguage, LocalizedText.LanguageNames, GUILayout.Width(LANGUAGE_POPUP_SIZE));
                if (language != LocalizedText.AccCurrentLanguage)
                {
                    LocalizedText.AccCurrentLanguage = language;
                    //表示中のエラーと操作結果は前の言語の文言なので、言語が混ざらないよう消す
                    _fieldErrors.ClearAll();
                    SetResultMessage(MessageType.None, string.Empty);
                }
            }
        }

        //====読み込んだパッケージの記憶================================
        /// <summary> 記憶したパッケージの一覧と「一覧から外す」ボタンの描画（一覧が空なら描画しない） </summary>
        private void DrawRememberedPackages()
        {
            if (!_rememberedPackages.Any())
                return;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.RememberedPackages), GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_SIZE));

                //今のフォルダが記憶されていなければ、先頭に「記憶していないパッケージ」を出して選んだ状態にする
                List<string> labels = _rememberedPackages.Select(snapshot => snapshot.AccDisplayName).ToList();
                int index = _rememberedPackages.FindIndex(snapshot => PackageSessionStore.IsSameFolder(snapshot.FolderPath, _packageJsonPath));
                bool is_remembered = index >= 0;
                if (!is_remembered)
                {
                    labels.Insert(0, LocalizedText.Get(TextId.NotRememberedPackage));
                    index = 0;
                }

                int selected = EditorGUILayout.Popup(index, labels.ToArray(), GUILayout.ExpandWidth(true));
                if (selected != index)
                {
                    PackageSnapshot snapshot = _rememberedPackages[is_remembered ? selected : selected - 1];
                    //入力状態を丸ごと入れ替えるため、OnGUI の途中ではなく delayCall で行う
                    EditorApplication.delayCall += () => SwitchPackage(snapshot.FolderPath);
                }

                EditorGUI.BeginDisabledGroup(!is_remembered);
                if (GUILayout.Button(LocalizedText.Get(TextId.RemoveFromListButton), GUILayout.ExpandWidth(false)))
                {
                    PackageSessionStore.Remove(_packageJsonPath);
                    RefreshRememberedPackages();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        /// <summary> 今の入力を記憶したパッケージから選んだものに切り替える（今のパッケージが記憶済みなら、今の入力を記憶し直してから切り替える） </summary>
        private void SwitchPackage(string folder)
        {
            PackageSnapshot snapshot = PackageSessionStore.Find(folder);
            if (snapshot == null)
                return;

            if (IsCurrentPackageRemembered())
                RememberCurrentPackage();

            _requiredSettings = snapshot.Required ?? new();
            _optionalSettings = snapshot.Optional ?? new();
            _packageJsonPath = snapshot.FolderPath;
            _requiredDrawer.SyncLicense(_requiredSettings);
            _fieldErrors.ClearAll();
            _isFocusClearRequested = true;

            RefreshRememberedPackages();
            SetResultMessage(MessageType.Info, LocalizedText.Format(TextId.PackageSwitched, snapshot.AccDisplayName));
            Repaint();
        }

        /// <summary> 今の入力を今のフォルダの分として記憶する </summary>
        private void RememberCurrentPackage()
        {
            PackageSessionStore.Save(_packageJsonPath, _requiredSettings, _optionalSettings);
            RefreshRememberedPackages();
        }

        /// <summary> 今のフォルダが記憶済みか判定 </summary>
        private bool IsCurrentPackageRemembered()
        {
            return _rememberedPackages.Exists(snapshot => PackageSessionStore.IsSameFolder(snapshot.FolderPath, _packageJsonPath));
        }

        /// <summary> 記憶したパッケージの一覧を読み直す </summary>
        private void RefreshRememberedPackages()
        {
            _rememberedPackages = PackageSessionStore.GetAll();
        }

        //====ファイル操作================================
        private void DrawPackageFileOperations()
        {
            GUILayoutOption label_layout = GUILayout.MaxWidth(DrawerUtility.INFO_LABEL_SIZE);
            GUILayoutOption button_layout = GUILayout.MaxWidth(DrawerUtility.INFO_BUTTON_SIZE);

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField(LocalizedText.Get(TextId.FileSection), EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space();

                DrawFileOutputSettings(label_layout, button_layout);

                EditorGUILayout.Space();

                DrawJsonPreview();

                DrawResultMessage();

                DrawFileOptionButton();

                EditorGUILayout.Space(20);
            }
        }
        /// <summary> JSON プレビュー（Create したときに出力される内容）の描画 </summary>
        private void DrawJsonPreview()
        {
            _isShowJsonPreview = EditorGUILayout.Foldout(_isShowJsonPreview, LocalizedText.Get(TextId.JsonPreview), true);
            if (!_isShowJsonPreview)
                return;

            //Layout と Repaint で表示する内容が変わらないよう、Layout のときだけ作り直す（警告は Create のときに出すので、ここでは捨てる）
            if (Event.current.type == EventType.Layout)
            {
                PackageJsonInfo info = PackageSettingsConverter.ConvertToPackageJsonInfo(_requiredSettings, _optionalSettings, new List<string>());
                _jsonPreview = PackageJsonFileUtility.SerializePackageJson(info);
            }

            using (EditorGUILayout.ScrollViewScope scroll_view = new EditorGUILayout.ScrollViewScope(_jsonPreviewScrollPos, GUILayout.MaxHeight(JSON_PREVIEW_MAX_HEIGHT)))
            {
                float height = EditorStyles.textArea.CalcHeight(new GUIContent(_jsonPreview), position.width);
                EditorGUILayout.SelectableLabel(_jsonPreview, EditorStyles.textArea, GUILayout.Height(height));
                _jsonPreviewScrollPos = scroll_view.scrollPosition;
            }
        }

        //ファイルパス設定
        private void DrawFileOutputSettings(GUILayoutOption label_layout, GUILayoutOption button_layout)
        {
            //生成パス入力
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(LocalizedText.Get(TextId.FolderPath), label_layout);
                _packageJsonPath = EditorGUILayout.TextField(_packageJsonPath, GUILayout.ExpandWidth(true));
                //ファイル設定ボタン
                if (GUILayout.Button(LocalizedText.Get(TextId.BrowseButton), button_layout))
                {
                    string selected_path = EditorUtility.OpenFolderPanel(LocalizedText.Get(TextId.FolderDialogTitle), Application.dataPath, string.Empty);
                    if (!string.IsNullOrEmpty(selected_path))
                    {
                        // 相対パスに変換（例: "Assets/..." 形式に）
                        if (selected_path.StartsWith(Application.dataPath))
                            selected_path = $"{PACKAGE_JSON_OUTPUT_DEFAULT_PATH}{selected_path.Substring(Application.dataPath.Length)}";
                        _packageJsonPath = selected_path;
                    }
                }
                //パスリセットボタン
                if (GUILayout.Button(LocalizedText.Get(TextId.ResetButton), button_layout))
                {
                    _packageJsonPath = PACKAGE_JSON_OUTPUT_DEFAULT_PATH;
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
                if (GUILayout.Button(LocalizedText.Get(TextId.CreateButton), GUILayout.ExpandWidth(this)))
                    EditorApplication.delayCall += CreatePackageJson;
                //読み込み
                if (GUILayout.Button(LocalizedText.Get(TextId.LoadButton), GUILayout.ExpandWidth(this)))
                    EditorApplication.delayCall += LoadPackageJson;
            }
        }

        /// <summary> package.json 生成 </summary>
        private void CreatePackageJson()
        {
            _warningCount = 0;
            if (!PackageJsonValidator.CanCreatePackageJson(_requiredSettings, _optionalSettings, out string check_error_message))
            {
                LogError(check_error_message);
                Repaint();
                return;
            }
            //サンプルのフォルダの有無と unityRelease の形式は、警告だけ出して作成は続ける（サンプルのフォルダを後から作る手順もあるため）
            LogWarnings(PackageJsonValidator.GetCreateWarnings(_optionalSettings, _packageJsonPath));

            string file_path = PackageJsonFileUtility.GetPackageJsonPath(_packageJsonPath);
            if (File.Exists(file_path) &&
                !EditorUtility.DisplayDialog(LocalizedText.Get(TextId.OverwriteDialogTitle), LocalizedText.Format(TextId.OverwriteDialogMessage, file_path),
                    LocalizedText.Get(TextId.OverwriteDialogOK), LocalizedText.Get(TextId.OverwriteDialogCancel)))
            {
                SetResultMessage(MessageType.Info, LocalizedText.Get(TextId.OverwriteCanceled));
                Repaint();
                return;
            }

            List<string> warnings = new();
            PackageJsonInfo info = PackageSettingsConverter.ConvertToPackageJsonInfo(_requiredSettings, _optionalSettings, warnings);
            LogWarnings(warnings);
            if (!PackageJsonFileUtility.TrySavePackageJson(_packageJsonPath, info, out string error_message))
                LogError(error_message);
            else
            {
                SetSucceededMessage(LocalizedText.Format(TextId.CreateSucceeded, file_path));
                RememberCurrentPackage();
            }
            Repaint();
        }

        /// <summary> package.json 読み込み（失敗した場合は入力内容を変えない） </summary>
        private void LoadPackageJson()
        {
            _warningCount = 0;
            if (!PackageJsonFileUtility.TryLoadPackageJson(_packageJsonPath, out PackageJsonInfo info, out string error_message))
            {
                LogError(error_message);
                Repaint();
                return;
            }

            _fieldErrors.ClearAll();
            //入力欄にフォーカスがあると、読み込んだ値ではなく編集中の値が表示されるため、次の OnGUI で外す
            _isFocusClearRequested = true;

            List<string> warnings = new();
            PackageSettingsConverter.ConvertToPackageSettings(info, _requiredSettings, _optionalSettings, warnings);
            LogWarnings(warnings);
            _requiredDrawer.SyncLicense(_requiredSettings);
            RememberCurrentPackage();

            SetSucceededMessage(LocalizedText.Format(TextId.LoadSucceeded, PackageJsonFileUtility.GetPackageJsonPath(_packageJsonPath)));
            Repaint();
        }

        /// <summary> 今の Editor のバージョンを unity と unityRelease に入れる </summary>
        private void ApplyCurrentEditorVersion()
        {
            string unity_version = Application.unityVersion;
            if (!UnityVersionConverter.TryParseEditorVersion(unity_version, out int major, out int minor, out string release))
            {
                LogError(LocalizedText.Format(TextId.EditorVersionUnreadableError, unity_version));
                return;
            }

            _requiredSettings.MajorVer = major;
            _requiredSettings.MinorVer = minor;
            _optionalSettings.UnityRelease = release;
            //リリースバージョンの欄にフォーカスがあると、書き換えた値が表示されないため外す
            _isFocusClearRequested = true;
            SetResultMessage(MessageType.Info, LocalizedText.Format(TextId.CurrentEditorApplied,
                PackageSettingsConverter.ConvertToUnityVersionString(major, minor), release, unity_version));
        }

        /// <summary> 独自ライセンスのひな形の LICENSE.md をフォルダーパスのフォルダに作成 </summary>
        private void CreateLicenseFile()
        {
            _warningCount = 0;
            string author_name = _requiredSettings.AuthorInfo?.Name;
            if (string.IsNullOrWhiteSpace(author_name))
            {
                LogError(LocalizedText.Get(TextId.LicenseAuthorEmptyError));
                Repaint();
                return;
            }

            string file_path = LicenseFileUtility.GetLicenseFilePath(_packageJsonPath);
            if (File.Exists(file_path) &&
                !EditorUtility.DisplayDialog(LocalizedText.Get(TextId.LicenseOverwriteDialogTitle), LocalizedText.Format(TextId.OverwriteDialogMessage, file_path),
                    LocalizedText.Get(TextId.OverwriteDialogOK), LocalizedText.Get(TextId.OverwriteDialogCancel)))
            {
                SetResultMessage(MessageType.Info, LocalizedText.Get(TextId.OverwriteCanceled));
                Repaint();
                return;
            }

            string text = LicenseFileUtility.GetProprietaryLicenseText(author_name, DateTime.Now.Year);
            if (!LicenseFileUtility.TryCreateLicenseFile(_packageJsonPath, text, out string error_message))
                LogError(error_message);
            else
                SetSucceededMessage(LocalizedText.Format(TextId.LicenseCreated, file_path));
            Repaint();
        }

        //====メッセージ==================================
        /// <summary> 警告を Console に出し、件数を数える </summary>
        private void LogWarnings(List<string> messages)
        {
            foreach (string message in messages)
            {
                Debug.LogWarning(message);
                _warningCount++;
            }
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
                SetResultMessage(MessageType.Warning, $"{message} / {LocalizedText.Format(TextId.WarningCount, _warningCount)}");
            else
                SetResultMessage(MessageType.Info, message);
        }

        /// <summary> 操作結果の描画 </summary>
        private void DrawResultMessage()
        {
            if (!string.IsNullOrEmpty(_resultMessage))
                EditorGUILayout.HelpBox(_resultMessage, _resultMessageType);
        }
    }
}
#endif
