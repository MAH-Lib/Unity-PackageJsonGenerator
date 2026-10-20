#if UNITY_EDITOR
using System.Collections.Generic;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal class PackageOptionalSettings
    {
        /// <summary> 更新ログURL </summary>
        internal string ChangelogURL;
        /// <summary> ドキュメントURL </summary>
        internal string DocumentationURL;
        /// <summary> ライセンスURL </summary>
        internal string LicensesURL;
        /// <summary> リリースバージョン </summary>
        internal string UnityRelease;
        /// <summary> UPMウィンドウ表示判定 </summary>
        internal bool HideInEditor;
        /// <summary> パッケージキーワード </summary>
        internal List<string> Keywords;
        /// <summary> 外部パッケージ </summary>
        internal List<DependenciesInfo> Dependencies;
        /// <summary> サンプル </summary>
        internal List<SampleInfo> SamplesInfo;

        //====コンストラクタ===============
        internal PackageOptionalSettings()
        {
            ChangelogURL = string.Empty;
            DocumentationURL = string.Empty;
            LicensesURL = string.Empty;
            UnityRelease = string.Empty;
            HideInEditor = false;
            Keywords = new();
            Dependencies = new();
            SamplesInfo = new();
        }

        //====リセット====================
        /// <summary> リセット </summary>
        internal void Reset()
        {
            ChangelogURL = string.Empty;
            DocumentationURL = string.Empty;
            LicensesURL = string.Empty;
            UnityRelease = string.Empty;
            Keywords.Clear();
            Dependencies.Clear();
            SamplesInfo.Clear();
        }
    }
}
#endif