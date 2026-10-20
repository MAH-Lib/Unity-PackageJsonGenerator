#if UNITY_EDITOR
using NUnit.Framework;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class PackageJsonValidatorTests
    {
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
        public void IsValidSampleFolderpath(string path, bool expected, string expected_path)
        {
            bool result = PackageJsonValidator.IsValidSampleFolderpath(path, out string sample_path);

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

        [TestCase("Name", "パッケージ名が入力されていません")]
        [TestCase("Description", "パッケージ概要が入力されていません")]
        [TestCase("DisplayName", "UPM表示名が入力されていません")]
        [TestCase("License", "ライセンスが入力されていません")]
        [TestCase("AuthorName", "著者名が入力されていません")]
        public void CanCreatePackageJson_EmptyField(string field, string expected_message)
        {
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
            Assert.AreEqual("パッケージ名の形式が正しくありません : my package", error_message);
        }

        [Test]
        public void CanCreatePackageJson_DuplicateDependencies()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            optional_settings.Dependencies.Add(new() { PacckageName = "com.example.dep" });
            optional_settings.Dependencies.Add(new() { PacckageName = "com.example.dep" });
            //不正な名前（初期値 "com."）の重複は出力されないので対象外
            optional_settings.Dependencies.Add(new());
            optional_settings.Dependencies.Add(new());

            Assert.IsFalse(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out string error_message));
            Assert.AreEqual("外部パッケージ名が重複しています : com.example.dep", error_message);
        }

        [Test]
        public void CanCreatePackageJson_DefaultDependencyRowsOnly()
        {
            CreateValidSettings(out PackageRequiredSettings required_settings, out PackageOptionalSettings optional_settings);
            optional_settings.Dependencies.Add(new());
            optional_settings.Dependencies.Add(new());

            Assert.IsTrue(PackageJsonValidator.CanCreatePackageJson(required_settings, optional_settings, out _));
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
