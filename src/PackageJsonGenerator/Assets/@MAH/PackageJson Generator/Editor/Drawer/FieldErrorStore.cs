#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 欄ごとの入力エラー（キーは欄の定数、またはリストの行のオブジェクト） </summary>
    internal class FieldErrorStore
    {
        //====変数======================
        private readonly Dictionary<object, string> _fieldErrors = new();

        //====関数======================
        /// <summary> 欄の入力エラー登録 </summary>
        internal void Set(object key, string message)
        {
            _fieldErrors[key] = message;
        }

        /// <summary> 欄の入力エラー解除 </summary>
        internal void Clear(object key)
        {
            _fieldErrors.Remove(key);
        }

        /// <summary> すべての入力エラー解除 </summary>
        internal void ClearAll()
        {
            _fieldErrors.Clear();
        }

        /// <summary> 欄の入力エラー描画 </summary>
        internal void Draw(object key)
        {
            if (_fieldErrors.TryGetValue(key, out string message))
                EditorGUILayout.HelpBox(message, MessageType.Error);
        }
    }
}
#endif
