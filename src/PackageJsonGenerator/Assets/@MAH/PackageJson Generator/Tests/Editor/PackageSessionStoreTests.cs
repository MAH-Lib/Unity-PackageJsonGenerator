#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class PackageSessionStoreTests
    {
        //====定数======================================
        /// <summary> テスト専用の SessionState のキー（利用者の記憶を壊さないため） </summary>
        private const string TEST_SESSION_KEY = "MAH.PackageJsonGenerator.Packages.Tests";

        //====準備・後片付け==================================
        [SetUp]
        public void SetUp()
        {
            PackageSessionStore.OverrideSessionKey(TEST_SESSION_KEY);
            PackageSessionStore.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            PackageSessionStore.ClearAll();
            PackageSessionStore.OverrideSessionKey(null);
        }

        //====保存・取得==================================
        [Test]
        public void Save_ThenFind_ReturnsSameValues()
        {
            PackageRequiredSettings required_settings = new() { Name = "com.example.tool", Version = new Vector3Int(1, 2, 3), PreRelease = "pre.1" };
            PackageOptionalSettings optional_settings = new() { Keywords = new() { "tool" } };
            optional_settings.Dependencies.Add(new() { PackageName = "com.example.dep", Version = new Vector3Int(3, 2, 1) });

            PackageSessionStore.Save("Assets/MyPackage", required_settings, optional_settings);
            PackageSnapshot snapshot = PackageSessionStore.Find("Assets/MyPackage");

            Assert.IsNotNull(snapshot);
            Assert.AreEqual("Assets/MyPackage", snapshot.FolderPath);
            Assert.AreEqual("com.example.tool", snapshot.Required.Name);
            Assert.AreEqual(new Vector3Int(1, 2, 3), snapshot.Required.Version);
            Assert.AreEqual("pre.1", snapshot.Required.PreRelease);
            CollectionAssert.AreEqual(new[] { "tool" }, snapshot.Optional.Keywords);
            Assert.AreEqual("com.example.dep", snapshot.Optional.Dependencies[0].PackageName);
        }

        [Test]
        public void Find_ReturnsCopy()
        {
            PackageRequiredSettings required_settings = new() { Name = "com.example.tool" };
            PackageSessionStore.Save("Assets/MyPackage", required_settings, new());

            //保存したあとに元の入力を変えても、記憶した内容は変わらない
            required_settings.Name = "com.example.changed";

            Assert.AreEqual("com.example.tool", PackageSessionStore.Find("Assets/MyPackage").Required.Name);
        }

        [Test]
        public void Save_SameFolder_Overwrites()
        {
            PackageSessionStore.Save("Assets/MyPackage", new() { Name = "com.example.first" }, new());
            PackageSessionStore.Save("Assets/MyPackage/", new() { Name = "com.example.second" }, new());

            List<PackageSnapshot> snapshots = PackageSessionStore.GetAll();

            Assert.AreEqual(1, snapshots.Count);
            Assert.AreEqual("com.example.second", snapshots[0].Required.Name);
        }

        [Test]
        public void Remove_RemovesOnlyThatFolder()
        {
            PackageSessionStore.Save("Assets/A", new() { Name = "com.example.a" }, new());
            PackageSessionStore.Save("Assets/B", new() { Name = "com.example.b" }, new());

            PackageSessionStore.Remove("Assets/A");

            Assert.IsNull(PackageSessionStore.Find("Assets/A"));
            Assert.IsNotNull(PackageSessionStore.Find("Assets/B"));
        }

        [Test]
        public void GetAll_ReturnsEmptyWhenNothingSaved()
        {
            Assert.IsEmpty(PackageSessionStore.GetAll());
        }

        //====フォルダーパス==================================
        [TestCase("Assets/MyPackage", "Assets/MyPackage", true)]
        [TestCase(@"Assets\MyPackage", "Assets/MyPackage", true)]
        [TestCase("Assets/MyPackage/", "Assets/MyPackage", true)]
        [TestCase("assets/mypackage", "Assets/MyPackage", true)]
        [TestCase("Assets/MyPackage2", "Assets/MyPackage", false)]
        public void IsSameFolder(string folder_a, string folder_b, bool expected)
        {
            Assert.AreEqual(expected, PackageSessionStore.IsSameFolder(folder_a, folder_b));
        }
    }
}
#endif
