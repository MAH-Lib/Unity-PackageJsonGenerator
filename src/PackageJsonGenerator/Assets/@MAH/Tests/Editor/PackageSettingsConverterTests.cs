#if UNITY_EDITOR
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class PackageSettingsConverterTests
    {
        //====バージョン======================================
        [TestCase("1.2.3", true, 1, 2, 3)]
        [TestCase(" 1.2.3 ", true, 1, 2, 3)]
        [TestCase("1.2", false, 1, 2, 0)]
        [TestCase("1.2.3.4", false, 1, 2, 3)]
        [TestCase("1.2.3-preview.1", false, 1, 2, 3)]
        [TestCase("a.b.c", false, 0, 0, 0)]
        [TestCase("99999999999.0.0", false, 0, 0, 0)]
        [TestCase("", false, 0, 0, 0)]
        [TestCase(null, false, 0, 0, 0)]
        public void TryConvertToVersionVector(string version, bool expected, int x, int y, int z)
        {
            bool result = PackageSettingsConverter.TryConvertToVersionVector(version, out Vector3Int version_vec);

            Assert.AreEqual(expected, result);
            Assert.AreEqual(new Vector3Int(x, y, z), version_vec);
        }

        [Test]
        public void ConvertToVersionString()
        {
            Assert.AreEqual("1.20.3", PackageSettingsConverter.ConvertToVersionString(new Vector3Int(1, 20, 3)));
        }

        [TestCase("2022.3", 2022, 3, 0)]
        [TestCase("6000.6", 6000, 6, 0)]
        [TestCase("6000", 6000, 0, 1)]
        [TestCase("6000.7", 6000, 7, 1)]
        [TestCase("2024.1", 2024, 1, 1)]
        [TestCase("abc", 6000, 0, 1)]
        [TestCase("6000.x", 6000, 0, 1)]
        [TestCase("", 6000, 0, 1)]
        [TestCase(null, 6000, 0, 1)]
        public void ConvertToUnityVersion(string unity, int expected_major, int expected_minor, int expected_warning_count)
        {
            List<string> warnings = new();

            PackageSettingsConverter.ConvertToUnityVersion(unity, out int major, out int minor, warnings);

            Assert.AreEqual(expected_major, major);
            Assert.AreEqual(expected_minor, minor);
            Assert.AreEqual(expected_warning_count, warnings.Count);
        }

        //====外部パッケージ===================================
        [Test]
        public void ConvertToDependenciesInfo_ConvertsConstraints()
        {
            Dictionary<string, string> dependencies = new()
            {
                { "com.a.b", "1.2.3" },
                { "com.c.d", ">=1.2.0" },
                { "com.e.f", "1.0.0 - 2.0.0" },
                { "com.g.h", "1.0.0-preview.1" },
            };
            List<string> warnings = new();

            List<DependenciesInfo> infos = PackageSettingsConverter.ConvertToDependenciesInfo(dependencies, warnings);

            Assert.AreEqual(3, infos.Count);
            Assert.AreEqual("com.a.b", infos[0].PacckageName);
            Assert.AreEqual(new Vector3Int(1, 2, 3), infos[0].Version);
            Assert.AreEqual("com.c.d", infos[1].PacckageName);
            Assert.AreEqual(new Vector3Int(1, 2, 0), infos[1].Version);
            Assert.AreEqual("com.e.f", infos[2].PacckageName);
            Assert.AreEqual(new Vector3Int(1, 0, 0), infos[2].Version);
            //変換 2 件（com.c.d、com.e.f）と除外 1 件（com.g.h）
            Assert.AreEqual(3, warnings.Count);
        }

        [Test]
        public void ConvertToDependencies_SkipsInvalidName()
        {
            List<DependenciesInfo> infos = new()
            {
                new() { PacckageName = "com.example.dep", Version = new Vector3Int(1, 0, 2) },
                new(),
            };
            List<string> warnings = new();

            Dictionary<string, string> dependencies = PackageSettingsConverter.ConvertToDependencies(infos, warnings);

            Assert.AreEqual(1, dependencies.Count);
            Assert.AreEqual("1.0.2", dependencies["com.example.dep"]);
            Assert.AreEqual(1, warnings.Count);
        }

        [Test]
        public void ConvertToDependencies_ReturnsNullWhenNoValidRow()
        {
            List<DependenciesInfo> infos = new() { new(), new() };
            List<string> warnings = new();

            Assert.IsNull(PackageSettingsConverter.ConvertToDependencies(infos, warnings));
            Assert.IsNull(PackageSettingsConverter.ConvertToDependencies(new List<DependenciesInfo>(), warnings));
        }

        //====往復======================================
        [Test]
        public void RoundTrip_KeepsAllValues()
        {
            PackageRequiredSettings required_settings = new()
            {
                Name = "com.example.tool",
                Version = new Vector3Int(1, 2, 3),
                Description = "Example package",
                DisplayName = "Example Tool",
                MajorVer = 2022,
                MinorVer = 3,
                License = "MIT",
                AuthorInfo = new() { Name = "Example", Email = "dev@example.com", URL = "https://example.com" },
            };
            PackageOptionalSettings optional_settings = new()
            {
                ChangelogURL = "https://example.com/changelog",
                DocumentationURL = "https://example.com/docs",
                LicensesURL = "https://example.com/license",
                UnityRelease = "0f1",
                HideInEditor = true,
                Keywords = new() { "tool", "editor" },
                Dependencies = new() { new() { PacckageName = "com.example.dep", Version = new Vector3Int(3, 2, 1) } },
                SamplesInfo = new() { new() { DisplayName = "Basic", Description = "Basic sample", Path = "Samples~/Basic" } },
            };
            List<string> warnings = new();

            PackageJsonInfo info = PackageSettingsConverter.ConvertToPackageJsonInfo(required_settings, optional_settings, warnings);
            PackageRequiredSettings loaded_required = new();
            PackageOptionalSettings loaded_optional = new();
            PackageSettingsConverter.ConvertToPackageSettings(info, loaded_required, loaded_optional, warnings);

            Assert.IsEmpty(warnings);
            Assert.AreEqual(required_settings.Name, loaded_required.Name);
            Assert.AreEqual(required_settings.Version, loaded_required.Version);
            Assert.AreEqual(required_settings.Description, loaded_required.Description);
            Assert.AreEqual(required_settings.DisplayName, loaded_required.DisplayName);
            Assert.AreEqual(required_settings.MajorVer, loaded_required.MajorVer);
            Assert.AreEqual(required_settings.MinorVer, loaded_required.MinorVer);
            Assert.AreEqual(required_settings.License, loaded_required.License);
            Assert.AreEqual("Example", loaded_required.AuthorInfo.Name);
            Assert.AreEqual("dev@example.com", loaded_required.AuthorInfo.Email);
            Assert.AreEqual("https://example.com", loaded_required.AuthorInfo.URL);

            Assert.AreEqual(optional_settings.ChangelogURL, loaded_optional.ChangelogURL);
            Assert.AreEqual(optional_settings.DocumentationURL, loaded_optional.DocumentationURL);
            Assert.AreEqual(optional_settings.LicensesURL, loaded_optional.LicensesURL);
            Assert.AreEqual(optional_settings.UnityRelease, loaded_optional.UnityRelease);
            Assert.AreEqual(optional_settings.HideInEditor, loaded_optional.HideInEditor);
            CollectionAssert.AreEqual(new[] { "tool", "editor" }, loaded_optional.Keywords);
            Assert.AreEqual(1, loaded_optional.Dependencies.Count);
            Assert.AreEqual("com.example.dep", loaded_optional.Dependencies[0].PacckageName);
            Assert.AreEqual(new Vector3Int(3, 2, 1), loaded_optional.Dependencies[0].Version);
            Assert.AreEqual(1, loaded_optional.SamplesInfo.Count);
            Assert.AreEqual("Basic", loaded_optional.SamplesInfo[0].DisplayName);
            Assert.AreEqual("Basic sample", loaded_optional.SamplesInfo[0].Description);
            Assert.AreEqual("Samples~/Basic", loaded_optional.SamplesInfo[0].Path);
        }

        [Test]
        public void ConvertToPackageSettings_ConvertsOldSamplePathSeparator()
        {
            PackageJsonInfo info = new()
            {
                Name = "com.example.tool",
                Version = "1.0.0",
                Unity = "2022.3",
                Samples = new() { new() { Path = @"Samples~\Basic" } },
            };
            PackageRequiredSettings required_settings = new();
            PackageOptionalSettings optional_settings = new();
            List<string> warnings = new();

            PackageSettingsConverter.ConvertToPackageSettings(info, required_settings, optional_settings, warnings);

            Assert.AreEqual("Samples~/Basic", optional_settings.SamplesInfo[0].Path);
        }

        [Test]
        public void ConvertToPackageSettings_KeepsInvalidNameWithWarning()
        {
            PackageJsonInfo info = new() { Name = "My Package", Version = "1.0.0", Unity = "2022.3" };
            PackageRequiredSettings required_settings = new();
            PackageOptionalSettings optional_settings = new();
            List<string> warnings = new();

            PackageSettingsConverter.ConvertToPackageSettings(info, required_settings, optional_settings, warnings);

            Assert.AreEqual("my package", required_settings.Name);
            Assert.AreEqual(1, warnings.Count);
        }
    }
}
#endif
