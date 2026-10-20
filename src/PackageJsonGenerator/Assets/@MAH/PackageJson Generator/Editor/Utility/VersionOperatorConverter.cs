#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class VersionOperatorConverter
    {
        //====変数=========================================
        private static readonly Regex VersionOnlyRegex = new(@"^\s*(\d+\.\d+\.\d+)\s*$");
        private static readonly Regex OperatorMatchRegex = new(@"^\s*(>=|<=|>|<)\s*(\d+\.\d+\.\d+)\s*$");
        private static readonly Regex RangeMatchRegex = new(@"^\s*(\d+\.\d+\.\d+)\s*-\s*(\d+\.\d+\.\d+)\s*$");

        //====関数=========================================
        /// <summary> バージョン演算子を文字列に変換 </summary>
        internal static string VersionOperatorToOperatorString(VersionOperator version_operator)
        {
            return version_operator switch
            {
                VersionOperator.Equals => string.Empty,
                VersionOperator.GreaterThan => "> ",
                VersionOperator.GreaterOrEqual => ">=",
                VersionOperator.LessThan => "<",
                VersionOperator.LessOrEqual => "<=",
                VersionOperator.Range => " - ",
                _ => throw new System.NotImplementedException(),
            };
        }

        /// <summary> バージョン演算子文字列をバージョン演算子に変換 </summary>
        internal static VersionOperator OperatorStringToVersionOperator(string version_operator)
        {
            if (string.IsNullOrWhiteSpace(version_operator))
                return VersionOperator.Equals;

            return version_operator switch
            {
                "" => VersionOperator.Equals,
                ">" => VersionOperator.GreaterThan,
                ">=" => VersionOperator.GreaterOrEqual,
                "<" => VersionOperator.LessThan,
                "<=" => VersionOperator.LessOrEqual,
                " - " => VersionOperator.Range,
                "-" => VersionOperator.Range,
                _ => throw new System.NotImplementedException(),
            };
        }

        /// <summary> バージョン文字列を分割 </summary>
        internal static string[] SplitVersionConstraint(string version)
        {
            version = version.Trim();

            //比較演算子形式
            Match version_only_match = VersionOnlyRegex.Match(version);
            if (version_only_match.Success)
            {
                return new[] {"", version_only_match.Groups[1].Value };
            }

            //比較演算子形式
            Match operator_match = OperatorMatchRegex.Match(version);
            if (operator_match.Success)
            {
                return new[] { operator_match.Groups[1].Value, operator_match.Groups[2].Value };
            }

            //範囲形式
            Match range_match = RangeMatchRegex.Match(version);
            if (range_match.Success)
            {
                return new[] {"-", range_match.Groups[1].Value, range_match.Groups[2].Value };
            }

            return null;
        }
    }
}
#endif