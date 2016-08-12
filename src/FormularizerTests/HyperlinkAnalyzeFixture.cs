using System.Linq;
using Formularizer.Core.Common;
using Formularizer.Core.HyperlinkAnalyze;
using NUnit.Framework;

namespace FormularizerTests
{
	[TestFixture]
	[Ignore] // not actual, need to update sample files to use base64 encoded data
	public class HyperlinkAnalyzeFixture
	{
		[TestCase(TestConstants.TestFileOneSheet)]
		public void Check_Hyperlinks_Are_Fetched(string path)
		{
			using (var helper = new FileRunHelper(path))
			{
				var doc = helper.Document;
				Assert.That(doc, Is.Not.Null);

				var results = HyperlinkAnalyzer.Analyze(doc);
				Assert.That(results.Success, Is.True);

				var sheetInfos = results.Value.Sheets;
				Assert.That(sheetInfos.Count, Is.GreaterThan(0));

				var links = sheetInfos.SelectMany(s => s.Value.Formulas).ToList();
				Assert.That(links.Count, Is.GreaterThan(0));
			}
		}

		[TestCase(TestConstants.TestFileTwoSheets)]
		public void Check_Hyperlinks_Are_Fetched_For_All_Sheets(string path)
		{
			using (var helper = new FileRunHelper(path))
			{
				var doc = helper.Document;
				Assert.That(doc, Is.Not.Null);

				var results = HyperlinkAnalyzer.Analyze(doc);
				Assert.That(results.Success, Is.True);

				var sheetInfos = results.Value.Sheets;
				Assert.That(sheetInfos.Count, Is.EqualTo(2));

				foreach (var sheetInfo in sheetInfos)
				{
					Assert.That(sheetInfo.Value.Formulas.Count, Is.GreaterThan(0));
				}
			}
		}

		[TestCase(TestConstants.TestFileOneSheet)]
		public void Check_Hyperlinks_Have_Custom_Scheme_Removed(string path)
		{
			using (var helper = new FileRunHelper(path))
			{
				var doc = helper.Document;
				Assert.That(doc, Is.Not.Null);

				var results = HyperlinkAnalyzer.Analyze(doc);
				Assert.That(results.Success, Is.True);

				var sheetInfos = results.Value.Sheets;
				Assert.That(sheetInfos.Count, Is.EqualTo(1));

				var links = sheetInfos.SelectMany(s => s.Value.Formulas).ToList();
				Assert.That(links.Any(l => l.Value.StartsWith(Constants.FormulaScheme)), Is.False);
			}
		}
	}
}