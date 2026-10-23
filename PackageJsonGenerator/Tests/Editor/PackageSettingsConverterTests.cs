#if UNITY_EDITOR
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    //英語・日本語の両方の表示言語で実行する（引数は LanguageType の値。LanguageType は internal のため int で受け取る）
    [TestFixture(0)]
    [TestFixture(1)]
    public class PackageSettingsConverterTests
    {
        //====変数======================================
        private readonly int _language;

        //====準備・後片付け==================================
        public PackageSettingsConverterTests(int language)
        {
            _language = language;
        }

        [SetUp]
        public void SetUp()
        {
            LocalizedText.OverrideLanguage((LanguageType)_language);
        }

        [TearDown]
        public void TearDown()
        {
            LocalizedText.OverrideLanguage(null);
        }

        //====バージョン======================================
        [TestCase("1.2.3", true, 1, 2, 3, "")]
        [TestCase(" 1.2.3 ", true, 1, 2, 3, "")]
        [TestCase("1.2", false, 1, 2, 0, "")]
        [TestCase("1.2.3.4", false, 1, 2, 3, "")]
        [TestCase("1.2.3-preview.1", true, 1, 2, 3, "preview.1")]
        [TestCase("1.2.3-exp.2", true, 1, 2, 3, "exp.2")]
        [TestCase("1.0.0-2.0.0", true, 1, 0, 0, "2.0.0")]
        [TestCase("1.2.3-01", false, 1, 2, 3, "")]
        [TestCase("1.2.3+build.5", false, 1, 2, 3, "")]
        [TestCase("1.2.3-pre.1+build.5", false, 1, 2, 3, "pre.1")]
        [TestCase("a.b.c", false, 0, 0, 0, "")]
        [TestCase("99999999999.0.0", false, 0, 0, 0, "")]
        [TestCase("", false, 0, 0, 0, "")]
        [TestCase(null, false, 0, 0, 0, "")]
        public void TryConvertToVersionVector(string version, bool expected, int x, int y, int z, string expected_pre_release)
        {
            bool result = PackageSettingsConverter.TryConvertToVersionVector(version, out Vector3Int version_vec, out string pre_release);

            Assert.AreEqual(expected, result);
            Assert.AreEqual(new Vector3Int(x, y, z), version_vec);
            Assert.AreEqual(expected_pre_release, pre_release);
        }

        [Test]
        public void ConvertToVersionString()
        {
            Assert.AreEqual("1.20.3", PackageSettingsConverter.ConvertToVersionString(new Vector3Int(1, 20, 3)));
            Assert.AreEqual("1.20.3", PackageSettingsConverter.ConvertToVersionString(new Vector3Int(1, 20, 3), string.Empty));
            Assert.AreEqual("1.20.3-preview.1", PackageSettingsConverter.ConvertToVersionString(new Vector3Int(1, 20, 3), "preview.1"));
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
                { "com.i.j", ">=2.0.0-pre.3" },
                { "com.k.l", "file:../foo" },
            };
            List<string> warnings = new();

            List<DependenciesInfo> infos = PackageSettingsConverter.ConvertToDependenciesInfo(dependencies, warnings);

            Assert.AreEqual(5, infos.Count);
            Assert.AreEqual("com.a.b", infos[0].PackageName);
            Assert.AreEqual(new Vector3Int(1, 2, 3), infos[0].Version);
            Assert.AreEqual("com.c.d", infos[1].PackageName);
            Assert.AreEqual(new Vector3Int(1, 2, 0), infos[1].Version);
            Assert.AreEqual("com.e.f", infos[2].PackageName);
            Assert.AreEqual(new Vector3Int(1, 0, 0), infos[2].Version);
            Assert.AreEqual("com.g.h", infos[3].PackageName);
            Assert.AreEqual(new Vector3Int(1, 0, 0), infos[3].Version);
            Assert.AreEqual("preview.1", infos[3].PreRelease);
            Assert.AreEqual("com.i.j", infos[4].PackageName);
            Assert.AreEqual(new Vector3Int(2, 0, 0), infos[4].Version);
            Assert.AreEqual("pre.3", infos[4].PreRelease);
            //変換 3 件（com.c.d、com.e.f、com.i.j）と除外 1 件（com.k.l）
            Assert.AreEqual(4, warnings.Count);
        }

        [Test]
        public void ConvertToDependencies_WritesPreRelease()
        {
            List<DependenciesInfo> infos = new()
            {
                new() { PackageName = "com.example.dep", Version = new Vector3Int(1, 0, 0), PreRelease = "exp.1" },
            };
            List<string> warnings = new();

            Dictionary<string, string> dependencies = PackageSettingsConverter.ConvertToDependencies(infos, warnings);

            Assert.AreEqual("1.0.0-exp.1", dependencies["com.example.dep"]);
            Assert.IsEmpty(warnings);
        }

        [Test]
        public void ConvertToDependencies_SkipsInvalidName()
        {
            List<DependenciesInfo> infos = new()
            {
                new() { PackageName = "com.example.dep", Version = new Vector3Int(1, 0, 2) },
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
                PreRelease = "preview.4",
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
                Dependencies = new() { new() { PackageName = "com.example.dep", Version = new Vector3Int(3, 2, 1), PreRelease = "pre.2" } },
                SamplesInfo = new() { new() { DisplayName = "Basic", Description = "Basic sample", Path = "Samples~/Basic" } },
            };
            List<string> warnings = new();

            PackageJsonInfo info = PackageSettingsConverter.ConvertToPackageJsonInfo(required_settings, optional_settings, warnings);
            PackageRequiredSettings loaded_required = new();
            PackageOptionalSettings loaded_optional = new();
            PackageSettingsConverter.ConvertToPackageSettings(info, loaded_required, loaded_optional, warnings);

            Assert.IsEmpty(warnings);
            Assert.AreEqual(required_settings.Name, loaded_required.Name);
            Assert.AreEqual("1.2.3-preview.4", info.Version);
            Assert.AreEqual(required_settings.Version, loaded_required.Version);
            Assert.AreEqual("preview.4", loaded_required.PreRelease);
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
            Assert.AreEqual("com.example.dep", loaded_optional.Dependencies[0].PackageName);
            Assert.AreEqual(new Vector3Int(3, 2, 1), loaded_optional.Dependencies[0].Version);
            Assert.AreEqual("pre.2", loaded_optional.Dependencies[0].PreRelease);
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

        //====未対応キー======================================
        /// <summary> 未対応キー（文字列・オブジェクト・配列）を含む package.json </summary>
        private const string JSON_WITH_EXTRA_KEYS = @"{
            ""name"": ""com.example.tool"",
            ""version"": ""1.0.0"",
            ""unity"": ""2022.3"",
            ""type"": ""tool"",
            ""repository"": { ""type"": ""git"", ""url"": ""https://example.com/repo.git"" },
            ""testables"": [ ""com.example.dep"" ]
        }";

        [Test]
        public void ExtraKeys_AreKeptThroughLoadAndCreate()
        {
            PackageJsonInfo loaded_info = JsonConvert.DeserializeObject<PackageJsonInfo>(JSON_WITH_EXTRA_KEYS);
            PackageRequiredSettings required_settings = new();
            PackageOptionalSettings optional_settings = new();
            List<string> warnings = new();

            PackageSettingsConverter.ConvertToPackageSettings(loaded_info, required_settings, optional_settings, warnings);
            PackageJsonInfo output_info = PackageSettingsConverter.ConvertToPackageJsonInfo(required_settings, optional_settings, warnings);
            JObject output = JObject.Parse(PackageJsonFileUtility.SerializePackageJson(output_info));
            JObject input = JObject.Parse(JSON_WITH_EXTRA_KEYS);

            CollectionAssert.AreEquivalent(new[] { "type", "repository", "testables" }, PackageSettingsConverter.GetExtraKeys(optional_settings.ExtraJson));
            Assert.IsTrue(JToken.DeepEquals(input["type"], output["type"]));
            Assert.IsTrue(JToken.DeepEquals(input["repository"], output["repository"]));
            Assert.IsTrue(JToken.DeepEquals(input["testables"], output["testables"]));
            //このツールが扱うキーは、未対応キーとして二重に出力されない
            Assert.AreEqual("com.example.tool", (string)output["name"]);
        }

        [Test]
        public void ExtraKeys_AreEmptyWhenNotPresent()
        {
            PackageJsonInfo loaded_info = JsonConvert.DeserializeObject<PackageJsonInfo>(@"{ ""name"": ""com.example.tool"", ""version"": ""1.0.0"", ""unity"": ""2022.3"" }");
            PackageRequiredSettings required_settings = new();
            PackageOptionalSettings optional_settings = new() { ExtraJson = @"{ ""type"": ""old"" }" };
            List<string> warnings = new();

            PackageSettingsConverter.ConvertToPackageSettings(loaded_info, required_settings, optional_settings, warnings);

            //未対応キーが無い package.json を Load すると、前のパッケージの未対応キーは消える
            Assert.IsEmpty(optional_settings.ExtraJson);
            Assert.IsEmpty(PackageSettingsConverter.GetExtraKeys(optional_settings.ExtraJson));
            Assert.IsNull(PackageSettingsConverter.ConvertToPackageJsonInfo(required_settings, optional_settings, warnings).ExtraData);
        }

        [TestCase("")]
        [TestCase(null)]
        [TestCase("not json")]
        public void ConvertToExtraData_ReturnsNullForEmptyOrInvalid(string extra_json)
        {
            Assert.IsNull(PackageSettingsConverter.ConvertToExtraData(extra_json));
        }
    }
}
#endif
