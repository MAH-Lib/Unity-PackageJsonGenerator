#if UNITY_EDITOR
using System;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace MAH.Tools.PackageJsonGenerator.Editor.Tests
{
    public class LocalizedTextTests
    {
        //====定数======================================
        private static readonly Regex PlaceholderRegex = new(@"\{(\d+)\}");

        //====後片付け==================================
        [TearDown]
        public void TearDown()
        {
            LocalizedText.OverrideLanguage(null);
        }

        //====文言の表==================================
        [Test]
        public void AllTextIds_HaveTextsForAllLanguages()
        {
            int language_count = Enum.GetValues(typeof(LanguageType)).Length;
            foreach (TextId id in Enum.GetValues(typeof(TextId)))
            {
                Assert.IsTrue(LocalizedText.TryGetTexts(id, out string[] texts), $"文言が登録されていない : {id}");
                Assert.AreEqual(language_count, texts.Length, $"言語の数が合わない : {id}");
                Assert.IsTrue(texts.All(text => !string.IsNullOrEmpty(text)), $"空の文言がある : {id}");
            }
        }

        [Test]
        public void AllTextIds_HaveSamePlaceholdersInAllLanguages()
        {
            foreach (TextId id in Enum.GetValues(typeof(TextId)))
            {
                LocalizedText.TryGetTexts(id, out string[] texts);
                string[] expected = GetPlaceholders(texts[0]);
                foreach (string text in texts.Skip(1))
                    CollectionAssert.AreEquivalent(expected, GetPlaceholders(text), $"{{0}} などの数が言語で違う : {id}");
            }
        }

        [Test]
        public void LanguageNames_MatchLanguageType()
        {
            Assert.AreEqual(Enum.GetValues(typeof(LanguageType)).Length, LocalizedText.LanguageNames.Length);
        }

        //====言語の切り替え==================================
        [Test]
        public void OverrideLanguage_ChangesText()
        {
            LocalizedText.OverrideLanguage(LanguageType.English);
            Assert.AreEqual("Package name", LocalizedText.Get(TextId.PackageName));

            LocalizedText.OverrideLanguage(LanguageType.Japanese);
            Assert.AreEqual("パッケージ名", LocalizedText.Get(TextId.PackageName));
        }

        [Test]
        public void Format_InsertsValues()
        {
            LocalizedText.OverrideLanguage(LanguageType.English);
            Assert.AreEqual("Created package.json : Assets/package.json", LocalizedText.Format(TextId.CreateSucceeded, "Assets/package.json"));
        }

        //====テスト用の関数==================================
        /// <summary> 文言に含まれる {0} などの番号の一覧取得（重複は除く） </summary>
        private static string[] GetPlaceholders(string text)
        {
            return PlaceholderRegex.Matches(text).Cast<Match>().Select(match => match.Groups[1].Value).Distinct().ToArray();
        }
    }
}
#endif
