#if UNITY_EDITOR
using NUnit.Framework;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class UnityVersionConverterTests
    {
        //====Editor のバージョン======================================
        [TestCase("6000.3.26f1", true, 6000, 3, "26f1")]
        [TestCase("6000.0.0b5", true, 6000, 0, "0b5")]
        [TestCase("2022.3.10f1c1", true, 2022, 3, "10f1")]
        [TestCase("2021.3.45f1", true, 2021, 3, "45f1")]
        [TestCase("6000.3", false, 6000, 0, "")]
        [TestCase("abc", false, 6000, 0, "")]
        [TestCase("", false, 6000, 0, "")]
        [TestCase(null, false, 6000, 0, "")]
        public void TryParseEditorVersion(string unity_version, bool expected, int expected_major, int expected_minor, string expected_release)
        {
            bool result = UnityVersionConverter.TryParseEditorVersion(unity_version, out int major, out int minor, out string release);

            Assert.AreEqual(expected, result);
            Assert.AreEqual(expected_major, major);
            Assert.AreEqual(expected_minor, minor);
            Assert.AreEqual(expected_release, release);
        }

        [Test]
        public void TryParseEditorVersion_ReadsRunningEditor()
        {
            //実際に動いている Editor のバージョンも読めること
            Assert.IsTrue(UnityVersionConverter.TryParseEditorVersion(UnityEngine.Application.unityVersion, out _, out _, out _));
        }
    }
}
#endif
