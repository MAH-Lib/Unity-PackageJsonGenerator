#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal static class UnityVersionConverter
    {
        //====定数===========================================
        private static readonly Dictionary<MajorVersion, Type> MinorEnumTypeMap = new()
        {
            { MajorVersion.Unity_6, typeof(MinorVersion_6) },
            { MajorVersion.Unity_2023, typeof(MinorVersion_2023) },
            { MajorVersion.Unity_2022, typeof(MinorVersion_2022) },
            { MajorVersion.Unity_2021, typeof(MinorVersion_2021) },
            { MajorVersion.Unity_2020, typeof(MinorVersion_2020) },
            { MajorVersion.Unity_2019, typeof(MinorVersion_2019) },
            { MajorVersion.Unity_2018, typeof(MinorVersion_2018) },
            { MajorVersion.Unity_2017, typeof(MinorVersion_2017) },
            { MajorVersion.Unity_5,   typeof(MinorVersion_5) },
        };

        private static readonly Dictionary<Type, MajorVersion> MajorEnumTypeMap = new()
        {
            { typeof(MinorVersion_6), MajorVersion.Unity_6 },
            { typeof(MinorVersion_2023), MajorVersion.Unity_2023 },
            { typeof(MinorVersion_2022), MajorVersion.Unity_2022},
            { typeof(MinorVersion_2021), MajorVersion.Unity_2021},
            { typeof(MinorVersion_2020), MajorVersion.Unity_2020},
            { typeof(MinorVersion_2019), MajorVersion.Unity_2019},
            { typeof(MinorVersion_2018), MajorVersion.Unity_2018},
            { typeof(MinorVersion_2017), MajorVersion.Unity_2017},
            { typeof(MinorVersion_5), MajorVersion.Unity_5},
        };

        internal static int DEFAULT_MINOR_VERSION = 1;

        //====関数===========================================
        /// <summary> メジャーバージョンに対応したマイナーバージョン取得 </summary>
        internal static Type GetMinorVersionForMajorVersion(MajorVersion major)
        {
            return MinorEnumTypeMap[major];
        }

        /// <summary> マイナーバージョンに対応したメジャーバージョン取得 </summary>
        internal static MajorVersion GetMajorVersionForMinorVersion(Type minor)
        {
            return MajorEnumTypeMap[minor];
        }

        /// <summary> メジャーバージョン文字列取得 </summary>
        internal static string MajorVersionToString(MajorVersion major)
        {
            return major switch
            {
                MajorVersion.Unity_6 => "6000",
                MajorVersion.Unity_2023 => "2023",
                MajorVersion.Unity_2022 => "2022",
                MajorVersion.Unity_2021 => "2021",
                MajorVersion.Unity_2020 => "2020",
                MajorVersion.Unity_2019 => "2019",
                MajorVersion.Unity_2018 => "2018",
                MajorVersion.Unity_2017 => "2017",
                MajorVersion.Unity_5 => "5",
                _ => throw new NotImplementedException(),
            };
        }

        /// <summary> メジャーバージョン文字列取得 </summary>
        internal static MajorVersion StringToMajorVersion(string major)
        {
            if (string.IsNullOrWhiteSpace(major))
                throw new NotImplementedException();

            return major switch
            {
                "6000" => MajorVersion.Unity_6,
                "2023" => MajorVersion.Unity_2023,
                "2022" => MajorVersion.Unity_2022,
                "2021" => MajorVersion.Unity_2021,
                "2020" => MajorVersion.Unity_2020,
                "2019" => MajorVersion.Unity_2019,
                "2018" => MajorVersion.Unity_2018,
                "2017" => MajorVersion.Unity_2017,
                "5" => MajorVersion.Unity_5,
                _ => throw new NotImplementedException(),
            };
        }
    }
}
#endif