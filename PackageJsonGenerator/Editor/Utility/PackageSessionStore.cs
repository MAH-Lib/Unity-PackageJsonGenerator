#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> 読み込んだパッケージの記憶（SessionState に保存するので、Unity Editor を閉じるまで残る） </summary>
    internal static class PackageSessionStore
    {
        //====定数=======================
        private const string DEFAULT_SESSION_KEY = "MAH.PackageJsonGenerator.Packages";

        //====変数=======================
        /// <summary> SessionState のキー（テストでは利用者の記憶を壊さないよう差し替える） </summary>
        private static string _sessionKey = DEFAULT_SESSION_KEY;

        //====取得=======================
        /// <summary> 記憶したパッケージの一覧取得（取り出すたびに別のオブジェクトになる） </summary>
        internal static List<PackageSnapshot> GetAll()
        {
            string json = SessionState.GetString(_sessionKey, string.Empty);
            if (string.IsNullOrEmpty(json))
                return new();

            PackageSnapshotList snapshot_list = JsonUtility.FromJson<PackageSnapshotList>(json);
            return snapshot_list?.Snapshots ?? new();
        }

        /// <summary> フォルダーパスで記憶したパッケージを探す（無ければ null） </summary>
        internal static PackageSnapshot Find(string folder)
        {
            return GetAll().Find(snapshot => IsSameFolder(snapshot.FolderPath, folder));
        }

        //====保存・削除=======================
        /// <summary> 入力状態を記憶する（同じフォルダの記憶は上書きする） </summary>
        internal static void Save(string folder, PackageRequiredSettings required_settings, PackageOptionalSettings optional_settings)
        {
            List<PackageSnapshot> snapshots = GetAll();
            PackageSnapshot snapshot = new()
            {
                FolderPath = NormalizeFolder(folder),
                Required = required_settings,
                Optional = optional_settings,
            };

            int index = snapshots.FindIndex(item => IsSameFolder(item.FolderPath, folder));
            if (index >= 0)
                snapshots[index] = snapshot;
            else
                snapshots.Add(snapshot);
            Write(snapshots);
        }

        /// <summary> 記憶したパッケージを一覧から外す </summary>
        internal static void Remove(string folder)
        {
            List<PackageSnapshot> snapshots = GetAll();
            snapshots.RemoveAll(snapshot => IsSameFolder(snapshot.FolderPath, folder));
            Write(snapshots);
        }

        //====フォルダーパス=======================
        /// <summary> フォルダーパスを比べやすい形にする（"\" を "/" に、末尾の "/" を取る） </summary>
        internal static string NormalizeFolder(string folder)
        {
            return (folder ?? string.Empty).Trim().Replace('\\', '/').TrimEnd('/');
        }

        /// <summary> 同じフォルダか判定（Windows のパスは大文字小文字を区別しないので、区別せずに比べる） </summary>
        internal static bool IsSameFolder(string folder_a, string folder_b)
        {
            return string.Equals(NormalizeFolder(folder_a), NormalizeFolder(folder_b), StringComparison.OrdinalIgnoreCase);
        }

        //====テスト用=======================
        /// <summary> テスト用：SessionState のキーを差し替える（null で元に戻す） </summary>
        internal static void OverrideSessionKey(string session_key)
        {
            _sessionKey = session_key ?? DEFAULT_SESSION_KEY;
        }

        /// <summary> テスト用：今のキーの記憶をすべて消す </summary>
        internal static void ClearAll()
        {
            SessionState.EraseString(_sessionKey);
        }

        //====内部=======================
        private static void Write(List<PackageSnapshot> snapshots)
        {
            SessionState.SetString(_sessionKey, JsonUtility.ToJson(new PackageSnapshotList { Snapshots = snapshots }));
        }

        /// <summary> JsonUtility は一覧を直接 JSON にできないため、包むクラス </summary>
        [Serializable]
        private class PackageSnapshotList
        {
            [SerializeField] internal List<PackageSnapshot> Snapshots = new();
        }
    }
}
#endif
