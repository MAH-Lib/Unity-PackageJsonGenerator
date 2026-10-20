#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class UnityPackageHelper
    {
        //====変数============================
        private static readonly string[] EMPTY_PACKAGE_STRING = new string[0];

        //====変数============================
        private static string[] _packageNames = EMPTY_PACKAGE_STRING;
        private static string[] _packageVersions = EMPTY_PACKAGE_STRING;
        private static bool _isLoaded = false;

        //====アクセサ=========================
        /// <summary> 読み込み完了フラグ </summary>
        internal static bool AccIsLoaded => _isLoaded;
        /// <summary> パッケージ名一覧 </summary>
        internal static string[] AccPackageNames => _packageNames;
        /// <summary> パッケージバージョン一覧 </summary>
        internal static string[] AccPackageVersions => _packageVersions;

        //====読み込み========================
        /// <summary> パッケージバージョン読み込み </summary>
        internal static void LoadPackageInfo()
        {
            UnityEditor.PackageManager.PackageInfo[] package_info = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            _packageNames = package_info.Select(info => info.name).Prepend("– Select Package –").ToArray();
            _packageVersions = package_info.Select(info => info.version).Prepend("0.0.0").ToArray();
            _isLoaded = true;
        }

        //====データ========================
        /// <summary> 読み込みデータ初期化 </summary>
        internal static void ClearData()
        {
            _packageNames = EMPTY_PACKAGE_STRING;
            _packageVersions = EMPTY_PACKAGE_STRING;
            _isLoaded = false;
        }
    }
}
#endif