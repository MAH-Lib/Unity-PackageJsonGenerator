#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class VersionOperatorConverter
    {
        //====変数=========================================
        //"X.Y.Z" と、後ろに付くプレリリースの表記（"-preview.1" など。規則に合うかは PackageSettingsConverter で確かめる）
        private const string VERSION_PATTERN = @"\d+\.\d+\.\d+(?:-[0-9A-Za-z.\-]+)?";

        private static readonly Regex VersionOnlyRegex = new($@"^\s*({VERSION_PATTERN})\s*$");
        private static readonly Regex OperatorMatchRegex = new($@"^\s*(>=|<=|>|<)\s*({VERSION_PATTERN})\s*$");
        //範囲は前後に空白がある " - " だけを認識する（空白の無い "1.0.0-2.0.0" は SemVer のプレリリースとして上で読まれる）
        private static readonly Regex RangeMatchRegex = new($@"^\s*({VERSION_PATTERN})\s+-\s+({VERSION_PATTERN})\s*$");

        //====関数=========================================
        /// <summary> バージョン制約文字列から完全一致のバージョンを取り出す（演算子付き・範囲は変換扱い） </summary>
        internal static bool TryExtractVersion(string constraint, out string version, out bool is_converted)
        {
            version = string.Empty;
            is_converted = false;

            if (string.IsNullOrWhiteSpace(constraint))
                return false;

            //完全一致形式
            Match version_only_match = VersionOnlyRegex.Match(constraint);
            if (version_only_match.Success)
            {
                version = version_only_match.Groups[1].Value;
                return true;
            }

            //比較演算子形式
            Match operator_match = OperatorMatchRegex.Match(constraint);
            if (operator_match.Success)
            {
                version = operator_match.Groups[2].Value;
                is_converted = true;
                return true;
            }

            //範囲形式（下限を採用）
            Match range_match = RangeMatchRegex.Match(constraint);
            if (range_match.Success)
            {
                version = range_match.Groups[1].Value;
                is_converted = true;
                return true;
            }

            return false;
        }
    }
}
#endif
