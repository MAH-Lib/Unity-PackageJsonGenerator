#if UNITY_EDITOR
using System;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 記憶したパッケージ 1 つ分の入力状態（JsonUtility で SessionState に保存する） </summary>
    [Serializable]
    internal class PackageSnapshot
    {
        //====変数=======================
        /// <summary> package.json のフォルダーパス（パッケージを区別するキー） </summary>
        [SerializeField] internal string FolderPath;
        /// <summary> 必須項目の入力状態 </summary>
        [SerializeField] internal PackageRequiredSettings Required;
        /// <summary> 任意項目の入力状態 </summary>
        [SerializeField] internal PackageOptionalSettings Optional;

        //====アクセサ=========================
        /// <summary> 一覧に表示する名前（パッケージ名（フォルダーパス）） </summary>
        internal string AccDisplayName => $"{Required?.Name} ({FolderPath})";
    }
}
#endif
