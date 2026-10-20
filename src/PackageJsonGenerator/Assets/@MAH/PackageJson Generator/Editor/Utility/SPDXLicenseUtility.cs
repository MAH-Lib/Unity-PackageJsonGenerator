#if UNITY_EDITOR
using System.Collections.Generic;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class SPDXLicenseConverter
    {
        //====定数=======================================
        private static readonly Dictionary<LicenseType, string> SPDX_LICENSE_LIST = new()
        {
            { LicenseType.Unlicense, "Unlicense" },
            { LicenseType.Proprietary, "Proprietary" },
            { LicenseType.MIT, "MIT" },
            { LicenseType.Apache_2_0, "Apache-2.0" },
            { LicenseType.GPL_3_0, "GPL-3.0" },
            { LicenseType.LGPL_3_0, "LGPL-3.0" },
            { LicenseType.MPL_2_0, "MPL-2.0" },
            { LicenseType.BSD_3_Clause, "BSD-3-Clause" },
            { LicenseType.CC0_1_0, "CC0-1.0" },
            { LicenseType.EPL_2_0, "EPL-2.0" }
        };

        private static readonly Dictionary<LicenseType, string> LICENSE_NAME_LIST = new()
        {
            { LicenseType.Unlicense, "The Unlicense" },
            { LicenseType.Proprietary, "Proprietary" },
            { LicenseType.MIT, "MIT License" },
            { LicenseType.Apache_2_0, "Apache License 2.0" },
            { LicenseType.GPL_3_0, "GNU General Public License v3.0" },
            { LicenseType.LGPL_3_0, "GNU Lesser General Public License v3.0" },
            { LicenseType.MPL_2_0, "Mozilla Public License 2.0" },
            { LicenseType.BSD_3_Clause, "BSD 3-Clause License" },
            { LicenseType.CC0_1_0, "Creative Commons Zero v1.0" },
            { LicenseType.EPL_2_0, "Eclipse Public License 2.0" }
        };

        //====関数=======================================
        /// <summary> SPDX ライセンスコード変換 </summary>
        internal static string LicenseTypeToSPDXLicense(LicenseType license)
        {
            if (!SPDX_LICENSE_LIST.ContainsKey(license))
                return string.Empty;
            return SPDX_LICENSE_LIST[license];
        }

        /// <summary> ライセンス名変換 </summary>
        internal static string LicenseTypeToLicenseFullName(LicenseType license)
        {
            if (!LICENSE_NAME_LIST.ContainsKey(license))
                return string.Empty;
            return LICENSE_NAME_LIST[license];
        }

        /// <summary> ライセンスタイプ変換 </summary>
        internal static LicenseType SPDXLicenseToLicenseType(string license)
        {
            if (!SPDX_LICENSE_LIST.ContainsValue(license))
                return LicenseType.Other;
            foreach (KeyValuePair<LicenseType, string> pair in SPDX_LICENSE_LIST)
            {
                if (pair.Value != license)
                    continue;
                return pair.Key;
            }
            return LicenseType.Other;
        }
    }
}
#endif