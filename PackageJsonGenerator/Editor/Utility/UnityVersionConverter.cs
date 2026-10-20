#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class UnityVersionConverter
    {
        //====定数===========================================
        /// <summary> 対応Unityバージョンの初期値（Major） </summary>
        internal const int DEFAULT_MAJOR_VERSION = 6000;
        /// <summary> 対応Unityバージョンの初期値（Minor） </summary>
        internal const int DEFAULT_MINOR_VERSION = 0;

        /// <summary> 選択肢に並べる Major の一覧（新しい Major が出たらここに追加する） </summary>
        private static readonly int[] MajorVersions = { 6000, 2023, 2022, 2021, 2020, 2019, 2018, 2017, 5 };

        /// <summary> Major ごとの Minor の一覧（新しい Minor が出たらここに追加する） </summary>
        private static readonly Dictionary<int, int[]> MinorVersionMap = new()
        {
            { 6000, new[] { 0, 1, 2, 3, 4, 5, 6 } },
            { 2023, new[] { 1, 2, 3 } },
            { 2022, new[] { 1, 2, 3 } },
            { 2021, new[] { 1, 2, 3 } },
            { 2020, new[] { 1, 2, 3 } },
            { 2019, new[] { 1, 2, 3, 4 } },
            { 2018, new[] { 1, 2, 3, 4 } },
            { 2017, new[] { 1, 2, 3, 4 } },
            { 5, new[] { 0, 1, 2, 3, 4, 5, 6 } },
        };

        //====関数===========================================
        /// <summary> Major の一覧取得 </summary>
        internal static int[] GetMajorVersions()
        {
            return MajorVersions;
        }

        /// <summary> Major に対応した Minor の一覧取得（一覧に無い Major は空） </summary>
        internal static int[] GetMinorVersions(int major)
        {
            return MinorVersionMap.TryGetValue(major, out int[] minors) ? minors : Array.Empty<int>();
        }

        /// <summary> 一覧にあるバージョンか判定 </summary>
        internal static bool IsKnownVersion(int major, int minor)
        {
            return GetMinorVersions(major).Contains(minor);
        }

        /// <summary> Major の表示名取得 </summary>
        internal static string GetMajorVersionLabel(int major)
        {
            return major == 6000 ? "6000 (Unity 6)" : major.ToString();
        }

        /// <summary> "Major.Minor" 形式の文字列を解析（読めない場合は初期値を入れて false） </summary>
        internal static bool TryParseUnityVersion(string unity, out int major, out int minor, out bool has_minor)
        {
            major = DEFAULT_MAJOR_VERSION;
            minor = DEFAULT_MINOR_VERSION;
            has_minor = false;

            if (string.IsNullOrWhiteSpace(unity))
                return false;

            string[] unity_str = unity.Trim().Split('.');
            if (!int.TryParse(unity_str[0], out int parse_major))
                return false;

            //Minor が無い場合は、その Major の一覧の先頭を使う
            if (unity_str.Length < 2)
            {
                int[] minors = GetMinorVersions(parse_major);
                major = parse_major;
                minor = minors.Any() ? minors[0] : 0;
                return true;
            }

            if (!int.TryParse(unity_str[1], out int parse_minor))
                return false;

            major = parse_major;
            minor = parse_minor;
            has_minor = true;
            return true;
        }
    }
}
#endif
