#if UNITY_EDITOR
using System;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    [Serializable]
    internal class PackageRequiredSettings
    {
        //====定数=======================
        /// <summary> パッケージのデフォルト名 </summary>
        internal const string PACKAGE_DEFAULT_NAME = "com.mypackage";

        //====変数=======================
        /// <summary> パッケージ名 </summary>
        [SerializeField] internal string Name;
        /// <summary> パッケージバージョン </summary>
        [SerializeField] internal Vector3Int Version;
        /// <summary> パッケージバージョンのプレリリースの表記（例 preview.1。空ならプレリリースではない） </summary>
        [SerializeField] internal string PreRelease;
        /// <summary> パッケージ概要 </summary>
        [SerializeField] internal string Description;
        /// <summary> パッケージ表示名 </summary>
        [SerializeField] internal string DisplayName;
        /// <summary> 対応Unityバージョン（Major。例 6000） </summary>
        [SerializeField] internal int MajorVer;
        /// <summary> 対応Unityバージョン（Minor） </summary>
        [SerializeField] internal int MinorVer;
        /// <summary> ライセンス名 </summary>
        [SerializeField] internal string License;
        /// <summary> 著者情報 </summary>
        [SerializeField] internal AuthorInfo AuthorInfo;

        //====コンストラクタ===============
        internal PackageRequiredSettings()
        {
            Name = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            PreRelease = string.Empty;
            Description = string.Empty;
            DisplayName = string.Empty;
            MajorVer = UnityVersionConverter.DEFAULT_MAJOR_VERSION;
            MinorVer = UnityVersionConverter.DEFAULT_MINOR_VERSION;
            License = string.Empty;
            AuthorInfo = new();
        }

        //====リセット====================
        /// <summary> リセット </summary>
        internal void Reset()
        {
            Name = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            PreRelease = string.Empty;
            Description = string.Empty;
            DisplayName = string.Empty;
            MajorVer = UnityVersionConverter.DEFAULT_MAJOR_VERSION;
            MinorVer = UnityVersionConverter.DEFAULT_MINOR_VERSION;
            License = string.Empty;
            AuthorInfo = new();
        }
    }
}
#endif