#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    [Serializable]
    internal class PackageOptionalSettings
    {
        /// <summary> 更新ログURL </summary>
        [SerializeField] internal string ChangelogURL;
        /// <summary> ドキュメントURL </summary>
        [SerializeField] internal string DocumentationURL;
        /// <summary> ライセンスURL </summary>
        [SerializeField] internal string LicensesURL;
        /// <summary> リリースバージョン </summary>
        [SerializeField] internal string UnityRelease;
        /// <summary> UPMウィンドウ表示判定 </summary>
        [SerializeField] internal bool HideInEditor;
        /// <summary> パッケージキーワード </summary>
        [SerializeField] internal List<string> Keywords;
        /// <summary> 外部パッケージ </summary>
        [SerializeField] internal List<DependenciesInfo> Dependencies;
        /// <summary> サンプル </summary>
        [SerializeField] internal List<SampleInfo> SamplesInfo;

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
            HideInEditor = false;
            Keywords.Clear();
            Dependencies.Clear();
            SamplesInfo.Clear();
        }
    }
}
#endif