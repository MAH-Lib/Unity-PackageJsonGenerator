#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    //英語・日本語の両方の表示言語で実行する（引数は LanguageType の値。LanguageType は internal のため int で受け取る）
    [TestFixture(0)]
    [TestFixture(1)]
    public class PackageJsonValidatorTests
    {
        //====変数======================================
        private readonly int _language;

        //====準備・後片付け==================================
        public PackageJsonValidatorTests(int language)
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

        //====入力判定======================================
        [TestCase("com.example.tool", true)]
        [TestCase("com.a", true)]
        [TestCase("com.my-company.tool-1", true)]
        [TestCase("Com.Example", false)]
        [TestCase("abc", false)]
        [TestCase("com.", false)]
        [TestCase("my package", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValidPackageName(string name, bool expected)
        {
            Assert.AreEqual(expected, PackageJsonValidator.IsValidPackageName(name));
        }

        [TestCase("name@example.com", true)]
        [TestCase("a.b@example.co.jp", true)]
        [TestCase("abc", false)]
        [TestCase("a@b", false)]
        [TestCase("a @example.com", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValidEmail(string e_mail, bool expected)
        {
            Assert.AreEqual(expected, PackageJsonValidator.IsValidEmail(e_mail));
        }

        [TestCase("https://example.com", true)]
        [TestCase("http://example.com/path?x=1#top", true)]
        [TestCase("https://github.com/MAH-Lib/Unity-PackageJsonGenerator#readme", true)]
        [TestCase("example.com", false)]
        [TestCase("ftp://example.com", false)]
        [TestCase("", false)]
        [TestCase(null, false)]
        public void IsValidURL(string url, bool expected)
        {
            Assert.AreEqual(expected, PackageJsonValidator.IsValidURL(url));
        }

        [TestCase("", true)]
        [TestCase(null, true)]
        [TestCase("preview.1", true)]
        [TestCase("exp.2", true)]
        [TestCase("pre", true)]
        [TestCase("rc-1.0", true)]
        [TestCase("0", true)]
        [TestCase("01", false)]
        [TestCase("pre.01", false)]
        [TestCase("pre..1", false)]
        [TestCase(".pre", false)]
        [TestCase("pre_1", false)]
        [TestCase("pre 1", false)]
        public void IsValidPreRelease(string pre_release, bool expected)
        {
            Assert.AreEqual(expected, PackageJsonValidator.IsValidPreRelease(pre_release));
        }

        [TestCase("C:/Proj/Assets/MyPkg/Samples/Basic", true, "Samples~/Basic")]
        [TestCase(@"C:\Proj\Packages\MyPkg\Samples~\Basic\Scene", true, "Samples~/Basic/Scene")]
        [TestCase("Samples~/Basic", true, "Samples~/Basic")]
        [TestCase("samples~/Basic/", true, "Samples~/Basic")]
        [TestCase("Samples/A/Samples/B", true, "Samples~/B")]
        [TestCase("MySamples/Basic", false, "")]
        [TestCase("Samples~", false, "")]
        [TestCase("Samples/", false, "")]
        [TestCase("Assets/Foo", false, "")]
        [TestCase("", false, "")]
        public void IsValidSampleFolderPath(string path, bool expected, string expected_path)
        {
            bool result = PackageJsonValidator.IsValidSampleFolderPath(path, out string sample_path);

            Assert.AreEqual(expected, result);
            Assert.AreEqual(expected_path, sample_path);
        }

        //====生成前チェック==================================
        [Test]
        public void CanCreatePackageJson_AllValid()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);

            Assert.IsTrue(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out string error_message));
            Assert.IsEmpty(error_message);
        }

        //期待するメッセージは TextId の名前で指定する（TextId は internal のため文字列で受け取る）
        [TestCase("Name", nameof(TextId.PackageNameEmptyError))]
        [TestCase("Description", nameof(TextId.DescriptionEmptyError))]
        [TestCase("DisplayName", nameof(TextId.DisplayNameEmptyError))]
        [TestCase("License", nameof(TextId.LicenseEmptyError))]
        [TestCase("AuthorName", nameof(TextId.AuthorNameEmptyError))]
        public void CanCreatePackageJson_EmptyField(string field, string expected_text_id)
        {
            string expected_message = LocalizedText.Get((TextId)Enum.Parse(typeof(TextId), expected_text_id));
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            switch (field)
            {
                case "Name": required_settings.Name = " "; break;
                case "Description": required_settings.Description = string.Empty; break;
                case "DisplayName": required_settings.DisplayName = string.Empty; break;
                case "License": required_settings.License = string.Empty; break;
                case "AuthorName": required_settings.AuthorInfo.Name = string.Empty; break;
            }

            Assert.IsFalse(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out string error_message));
            Assert.AreEqual(expected_message, error_message);
        }

        [Test]
        public void CanCreatePackageJson_InvalidName()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            required_settings.Name = "my package";

            Assert.IsFalse(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out string error_message));
            Assert.AreEqual(LocalizedText.Format(TextId.PackageNameInvalidError, "my package"), error_message);
        }

        [Test]
        public void CanCreatePackageJson_DuplicateDependencies()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            optional_settings.Dependencies.Add(new() { PackageName = "com.example.dep" });
            optional_settings.Dependencies.Add(new() { PackageName = "com.example.dep" });
            //不正な名前（初期値 "com."）の重複は出力されないので対象外
            optional_settings.Dependencies.Add(new());
            optional_settings.Dependencies.Add(new());

            Assert.IsFalse(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out string error_message));
            Assert.AreEqual(LocalizedText.Format(TextId.DependenciesDuplicateError, "com.example.dep"), error_message);
        }

        [Test]
        public void CanCreatePackageJson_DefaultDependencyRowsOnly()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            optional_settings.Dependencies.Add(new());
            optional_settings.Dependencies.Add(new());

            Assert.IsTrue(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out _));
        }

        //====Create 前の確認==================================
        [Test]
        public void GetCreateWarnings_SampleFolder()
        {
            string folder = Path.Combine(Path.GetTempPath(), $"PackageJsonGeneratorTests_{Guid.NewGuid():N}");
            try
            {
                Directory.CreateDirectory(Path.Combine(folder, "Samples~", "Basic"));
                Directory.CreateDirectory(Path.Combine(folder, "Samples", "Development"));
                PackageOptionalSettings optional_settings = new();
                optional_settings.SamplesInfo.Add(new() { Path = "Samples~/Basic" });
                optional_settings.SamplesInfo.Add(new() { Path = "Samples~/Development" });
                optional_settings.SamplesInfo.Add(new() { Path = "Samples~/Missing" });

                List<string> warnings = PackageJsonValidator.GetCreateWarnings(optional_settings, folder);

                //Samples~/Basic はあり、Samples~/Development は Samples/Development にあるので警告しない
                CollectionAssert.AreEqual(new[] { LocalizedText.Format(TextId.SampleFolderNotFoundWarning, "Samples~/Missing") }, warnings);
            }
            finally
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);
            }
        }

        [TestCase("", 0)]
        [TestCase("0f1", 0)]
        [TestCase("26f1", 0)]
        [TestCase("0b5", 0)]
        [TestCase(" 3p2 ", 0)]
        [TestCase("f1", 1)]
        [TestCase("0.1", 1)]
        [TestCase("26f1c1", 1)]
        public void GetCreateWarnings_UnityRelease(string unity_release, int expected_warning_count)
        {
            PackageOptionalSettings optional_settings = new() { UnityRelease = unity_release };

            Assert.AreEqual(expected_warning_count, PackageJsonValidator.GetCreateWarnings(optional_settings, Path.GetTempPath()).Count);
        }

        //====テスト用データ==================================
        /// <summary> 生成前チェックを通る入力状態の作成 </summary>
        private static void CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings)
        {
            required_settings = new()
            {
                Name = "com.example.tool",
                Description = "Example package",
                DisplayName = "Example Tool",
                License = "MIT",
                AuthorInfo = new() { Name = "Example" },
            };
            optional_settings = new();
        }
    }
}
#endif
