#if UNITY_EDITOR

namespace MAH.Tools.PackageJsonGenerator.Editor
{
    internal enum VersionOperator
    {
        Equals = 0,
        GreaterThan,
        GreaterOrEqual,
        LessThan,
        LessOrEqual,
        Range,
    }
}
#endif