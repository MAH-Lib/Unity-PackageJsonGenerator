#if UNITY_EDITOR
using System;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    [Serializable]
    internal class DependenciesInfo
    {
        //====定数======================
        /// <summary> パッケージのデフォルト名 </summary>
        internal const string PACKAGE_DEFAULT_NAME = "com.";

        //====変数======================
        /// <summary> パッケージ名 </summary>
        public string PacckageName;
        /// <summary> バージョン </summary>
        public Vector3Int Version;
        /// <summary> パッケージ選択配列番号 </summary>
        public int SelectPackageIndex;

        //====コンストラクタ==============
        public DependenciesInfo()
        {
            PacckageName = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            SelectPackageIndex = 0;
        }

        //====リセット==================
        /// <summary> リセット </summary>
        public void Reset()
        {
            PacckageName = PACKAGE_DEFAULT_NAME;
            Version = Vector3Int.zero;
            SelectPackageIndex = 0;
        }
    }
}
#endif