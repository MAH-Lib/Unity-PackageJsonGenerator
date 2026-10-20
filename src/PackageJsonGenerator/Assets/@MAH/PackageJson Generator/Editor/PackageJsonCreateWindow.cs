#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
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
        private const string PACKAGE_INFO_JSON_FILE_PATH_BUTTON = "フォルダーパス";
        private const string PACKAGE_INFO_JSON_FILE_CREATE_BUTTON = "Create package.json";
        private const string PACKAGE_INFO_JSON_FILE_LOAD_BUTTON = "Load package.json";

        private const string OVERWRITE_DIALOG_TITLE = "package.json の上書き";
        private const string OVERWRITE_DIALOG_MESSAGE = "は既に存在します。上書きしますか？";
        private const string OVERWRITE_DIALOG_OK = "上書き";
        private const string OVERWRITE_DIALOG_CANCEL = "キャンセル";
        private const string OVERWRITE_CANCELED_MESSAGE = "上書きをキャンセルしました";
        private const string CREATE_SUCCEEDED_MESSAGE = "package.json を作成しました";
        private const string LOAD_SUCCEEDED_MESSAGE = "package.json を読み込みました";
        private const string WARNING_COUNT_MESSAGE = "警告 {0} 件（詳しくは Console を参照）";

        //====変数======================================
        //ドメインリロード後も入力を残すため、入力状態と画面の状態は SerializeField にする
        [SerializeField] private Vector3 _scrollPos;

        [SerializeField] private PackageRequiredSettings _requiredSettings = new();
        [SerializeField] private PackageOptionalSettings _optionalSettings = new();

        [SerializeField] private string _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;

        [SerializeField] private RequiredSettingsDrawer _requiredDrawer = new();
        [SerializeField] private OptionalSettingsDrawer _optionalDrawer = new();

        /// <summary> 初期化済み判定（ドメインリロード時は初期化しない） </summary>
        [SerializeField] private bool _isInitialized;

        /// <summary> 欄ごとの入力エラー </summary>
        private readonly FieldErrorStore _fieldErrors = new();
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
                _requiredDrawer = new();
                _optionalDrawer = new();
                _requiredDrawer.ResetLicense(_requiredSettings);

                _pacckageJsonPath = PACKGAGE_JSON_OUTPUT_DEFAULT_PATH;

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
                _requiredDrawer.Draw(_requiredSettings, _fieldErrors);
                _optionalDrawer.Draw(_optionalSettings, _fieldErrors);

                _scrollPos = scroll_view.scrollPosition;
            }
            DrawPackageFileOperations();
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
                if (GUILayout.Button(DrawerUtility.PACKAGE_INFO_PATH_SETTING_BUTTON_NAME, button_layout))
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
                if (GUILayout.Button(DrawerUtility.PACKAGE_INFO_RESET_BUTTON_NAME, button_layout))
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
            if (!PackageJsonValidator.CanCreatePackageJson(_requiredSettings, _optionalSettings, out string check_error_message))
            {
                LogError(check_error_message);
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

            List<string> warnings = new();
            PackageJsonInfo info = PackageSettingsConverter.ConvertToPackageJsonInfo(_requiredSettings, _optionalSettings, warnings);
            LogWarnings(warnings);
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

            _fieldErrors.ClearAll();
            //入力欄にフォーカスがあると、読み込んだ値ではなく編集中の値が表示されるため、次の OnGUI で外す
            _isFocusClearRequested = true;

            List<string> warnings = new();
            PackageSettingsConverter.ConvertToPackageSettings(info, _requiredSettings, _optionalSettings, warnings);
            LogWarnings(warnings);
            _requiredDrawer.SyncLicense(_requiredSettings);

            SetSucceededMessage($"{LOAD_SUCCEEDED_MESSAGE} : {PackageJsonFileUtility.GetPackageJsonPath(_pacckageJsonPath)}");
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
    }
}
#endif
