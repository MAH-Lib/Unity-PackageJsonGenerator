#if UNITY_EDITOR
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal class DependenciesInfo
    {
        //====変数======================
        /// <summary> パッケージ名 </summary>
        public string PacckageName;
        /// <summary> バージョン </summary>
        public Vector3Int Version;
        /// <summary> インストール条件 </summary>
        public VersionOperator Operator;
        /// <summary> 範囲バージョン </summary>
        public Vector3Int OperatorVersion;
        /// <summary> パッケージ選択配列番号 </summary>
        public int SelectPackageIndex;

        //====コンストラクタ==============
        public DependenciesInfo()
        {
            PacckageName = "com.";
            Version = Vector3Int.zero;
            Operator = VersionOperator.Equals;
            OperatorVersion = Vector3Int.zero;
            SelectPackageIndex = 0;
        }

        //====リセット==================
        /// <summary> リセット </summary>
        public void Reset()
        {
            PacckageName = "com.";
            Version = Vector3Int.zero;
            Operator = VersionOperator.Equals;
            OperatorVersion = Vector3Int.zero;
            SelectPackageIndex = 0;
        }
    }
}
#endif