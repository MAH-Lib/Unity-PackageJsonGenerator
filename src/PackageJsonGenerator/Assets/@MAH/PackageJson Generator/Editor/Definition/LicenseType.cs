#if UNITY_EDITOR

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal enum LicenseType
    {
        //ライセンス無し
        Unlicense = 0,
        //独自ライセンス
        Proprietary,
        // MIT License
        MIT,
        // Apache License 2.0
        Apache_2_0,
        // GNU General Public License v3.0
        GPL_3_0,
        // GNU Lesser General Public License v3.0
        LGPL_3_0,
        // Mozilla Public License 2.0
        MPL_2_0,
        // BSD 3-Clause License
        BSD_3_Clause,
        // Creative Commons Zero v1.0
        CC0_1_0,
        // Eclipse Public License 2.0
        EPL_2_0,
        //その他のライセンス
        Other,
    }
}
#endif