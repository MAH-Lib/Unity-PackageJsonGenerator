#if UNITY_EDITOR
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal class PackageRequiredSettings
    {
        //====定数=======================
        /// <summary> パッケージのデフォルト名 </summary>
        internal const string PACKAGE_DEFAULT_NAME = "com.mypackage";

        //====変数=======================
        /// <summary> パッケージ名 </summary>
        internal string Name;
        /// <summary> パッケージバージョン </summary>
        internal Vector3Int Version;
        /// <summary> パッケージ概要 </summary>
        internal string Description;
        /// <summary> パッケージ表示名 </summary>
        internal string DisplayName;
        /// <summary> 対応Unityバージョン </summary>
        internal MajorVersion MajorVer;
        /// <summary> 対応Unityバージョン </summary>
        internal int MinorVer;
        /// <summary> ライセンス名 </summary>
        internal string License;
        /// <summary> 著者情報 </summary>
        internal AuthorInfo AuthorInfo;

        //====コンストラクタ===============
        internal PackageRequiredSettings()
        {
            Name = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            Description = string.Empty;
            DisplayName = string.Empty;
            MajorVer = MajorVersion.Unity_6;
            MinorVer = 1;
            License = string.Empty;
            AuthorInfo = new();
        }

        //====リセット====================
        /// <summary> リセット </summary>
        internal void Reset()
        {
            Name = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            Description = string.Empty;
            DisplayName = string.Empty;
            MajorVer = MajorVersion.Unity_6;
            MinorVer = 1;
            License = string.Empty;
            AuthorInfo = new();
        }
    }
}
#endif