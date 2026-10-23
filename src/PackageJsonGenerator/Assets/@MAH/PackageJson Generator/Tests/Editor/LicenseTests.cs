#if UNITY_EDITOR
using System;
using System.IO;
using NUnit.Framework;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class LicenseTests
    {
        //====変数======================================
        /// <summary> テストごとに作る一時フォルダ（Assets の外に作る） </summary>
        private string _tempFolder;

        //====準備・後片付け==================================
        [SetUp]
        public void SetUp()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), $"PackageJsonGeneratorTests_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_tempFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempFolder))
                Directory.Delete(_tempFolder, true);
        }

        //====ライセンス None====================================
        [Test]
        public void None_ConvertsToUNLICENSED()
        {
            Assert.AreEqual("UNLICENSED", SPDXLicenseConverter.LicenseTypeToSPDXLicense(LicenseType.None));
            Assert.AreEqual(LicenseType.None, SPDXLicenseConverter.SPDXLicenseToLicenseType("UNLICENSED"));
        }

        [Test]
        public void ExistingLicenseValues_AreUnchanged()
        {
            //None を末尾に足しても、既存の選択肢の値は変わらない
            Assert.AreEqual(0, (int)LicenseType.Unlicense);
            Assert.AreEqual(1, (int)LicenseType.Proprietary);
            Assert.AreEqual(10, (int)LicenseType.Other);
            Assert.AreEqual(11, (int)LicenseType.None);
        }

        //====LICENSE.md====================================
        [Test]
        public void GetProprietaryLicenseText_ContainsAuthorAndYear()
        {
            //前後の空白は取り除く
            string text = LicenseFileUtility.GetProprietaryLicenseText(" Example ", 2026);

            StringAssert.Contains("Copyright (c) 2026 Example. All rights reserved.", text);
        }

        [Test]
        public void TryCreateLicenseFile_CreatesFile()
        {
            Assert.IsTrue(LicenseFileUtility.TryCreateLicenseFile(_tempFolder, "license text", out string error_message));
            Assert.IsEmpty(error_message);
            Assert.AreEqual("license text", File.ReadAllText(LicenseFileUtility.GetLicenseFilePath(_tempFolder)));
        }

        [Test]
        public void TryCreateLicenseFile_FailsWhenFolderDoesNotExist()
        {
            string folder = Path.Combine(_tempFolder, "NotExist");

            Assert.IsFalse(LicenseFileUtility.TryCreateLicenseFile(folder, "license text", out string error_message));
            Assert.AreEqual(LocalizedText.Format(TextId.FolderNotFoundError, folder), error_message);
        }

        [Test]
        public void TryCreateLicenseFile_FailsWhenFolderIsEmpty()
        {
            Assert.IsFalse(LicenseFileUtility.TryCreateLicenseFile(string.Empty, "license text", out string error_message));
            Assert.AreEqual(LocalizedText.Get(TextId.FolderEmptyError), error_message);
        }
    }
}
#endif
