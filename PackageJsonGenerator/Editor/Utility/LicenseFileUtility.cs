#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    /// <summary> LICENSE.md の作成（Proprietary 用のひな形） </summary>
    internal static class LicenseFileUtility
    {
        //====定数=======================
        private const string LICENSE_FILE_NAME = "LICENSE.md";

        /// <summary> 独自ライセンスのひな形（{0}：年、{1}：著者名）。利用者向けの文書なので英語にする </summary>
        private const string PROPRIETARY_LICENSE_TEMPLATE =
            "# License\n" +
            "\n" +
            "Copyright (c) {0} {1}. All rights reserved.\n" +
            "\n" +
            "This software and associated documentation files (the \"Software\") are proprietary.\n" +
            "No part of the Software may be copied, modified, distributed, or used in any form\n" +
            "without the prior written permission of the copyright holder.\n";

        //====パス=======================
        /// <summary> フォルダーパスから LICENSE.md のパス取得（"/" 区切り） </summary>
        internal static string GetLicenseFilePath(string folder)
        {
            return Path.Combine(folder ?? string.Empty, LICENSE_FILE_NAME).Replace('\\', '/');
        }

        //====ひな形=======================
        /// <summary> 独自ライセンスのひな形の本文取得 </summary>
        internal static string GetProprietaryLicenseText(string author_name, int year)
        {
            return string.Format(PROPRIETARY_LICENSE_TEMPLATE, year, author_name?.Trim());
        }

        //====保存=======================
        /// <summary> LICENSE.md 作成（失敗した場合は例外を出さずに false とエラー文を返す。既存のファイルは上書きする） </summary>
        internal static bool TryCreateLicenseFile(string folder, string text, out string error_message)
        {
            error_message = string.Empty;
            if (string.IsNullOrWhiteSpace(folder))
            {
                error_message = LocalizedText.Get(TextId.FolderEmptyError);
                return false;
            }
            if (!Directory.Exists(folder))
            {
                error_message = LocalizedText.Format(TextId.FolderNotFoundError, folder);
                return false;
            }

            try
            {
                File.WriteAllText(GetLicenseFilePath(folder), text);
            }
            catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException)
            {
                error_message = LocalizedText.Format(TextId.LicenseSaveFailedError, exception.Message);
                return false;
            }
            AssetDatabase.Refresh();
            return true;
        }
    }
}
#endif
