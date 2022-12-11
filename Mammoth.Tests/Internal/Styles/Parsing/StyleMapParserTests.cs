using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	[TestClass()]
	public class StyleMapParserTests
	{
		[TestMethod()]
		public void EmptyStringIsParsedAsEmptyStyleMap()
		{
			var styleMap = StyleMapParser.Parse("");
			HtmlPathAssert.AreEqual(StyleMap.Builder().Build(), styleMap);
		}

		[TestMethod()]
		public void MapParagraphs()
		{
			var styleMap = StyleMapParser.Parse("p => p");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.MapParagraph(ParagraphMatcher.ANY, HtmlPath.CollapsibleElement("p"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapRuns()
		{
			var styleMap = StyleMapParser.Parse("r => p");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.MapRun(RunMatcher.ANY, HtmlPath.CollapsibleElement("p"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapBold()
		{
			StyleMap styleMap = StyleMapParser.Parse("b => em");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.Bold(HtmlPath.CollapsibleElement("em"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapItalic()
		{
			StyleMap styleMap = StyleMapParser.Parse("i => strong");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.Italic(HtmlPath.CollapsibleElement("strong"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapUnderline()
		{
			StyleMap styleMap = StyleMapParser.Parse("u => em");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.Underline(HtmlPath.CollapsibleElement("em"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapStrikethrough()
		{
			StyleMap styleMap = StyleMapParser.Parse("strike => del");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.Strikethrough(HtmlPath.CollapsibleElement("del"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapAllCaps()
		{
			StyleMap styleMap = StyleMapParser.Parse("all-caps => span");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.AllCaps(HtmlPath.CollapsibleElement("span"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapSmallCaps()
		{
			StyleMap styleMap = StyleMapParser.Parse("small-caps => span");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.SmallCaps(HtmlPath.CollapsibleElement("span"))
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void MapCommentReference()
		{
			StyleMap styleMap = StyleMapParser.Parse("comment-reference =>");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.CommentReference(HtmlPath.EMPTY)
				.Build(),
				(styleMap));
		}

		[TestMethod()]
		public void MapLineBreaks()
		{
			var styleMap = StyleMapParser.Parse("br[type='line'] => div");
			var expectedStyleMap = StyleMap.Builder()
				.MapBreak(BreakMatcher.LINE_BREAK, HtmlPath.CollapsibleElement("div"))
				.Build();
			HtmlPathAssert.AreEqual(expectedStyleMap, styleMap);
		}

		[TestMethod()]
		public void MapPageBreaks()
		{
			StyleMap styleMap = StyleMapParser.Parse("br[type='page'] => div");
			var expectedStyleMap = StyleMap.Builder()
				.MapBreak(BreakMatcher.PAGE_BREAK, HtmlPath.CollapsibleElement("div"))
				.Build();
			HtmlPathAssert.AreEqual(expectedStyleMap, styleMap);
		}

		[TestMethod()]
		public void MapColumnBreaks()
		{
			StyleMap styleMap = StyleMapParser.Parse("br[type='column'] => div");
			var expectedStyleMap = StyleMap.Builder()
				.MapBreak(BreakMatcher.COLUMN_BREAK, HtmlPath.CollapsibleElement("div"))
				.Build();
			HtmlPathAssert.AreEqual(expectedStyleMap, styleMap);
		}

		[TestMethod()]
		public void BlankLinesAreIgnored()
		{
			StyleMap styleMap = StyleMapParser.Parse("\n\n  \n\np =>\n\r\n");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.MapParagraph(ParagraphMatcher.ANY, HtmlPath.EMPTY)
				.Build(),
				styleMap);
		}

		[TestMethod()]
		public void LineStartingWithHashIsIgnored()
		{
			StyleMap styleMap = StyleMapParser.Parse("#p => p");
			HtmlPathAssert.AreEqual(StyleMap.Builder().Build(), styleMap);
		}

		[TestMethod()]
		public void ParseMultipleMappings()
		{
			StyleMap styleMap = StyleMapParser.Parse("p =>\nr =>");
			HtmlPathAssert.AreEqual(StyleMap.Builder()
				.MapParagraph(ParagraphMatcher.ANY, HtmlPath.EMPTY)
				.MapRun(RunMatcher.ANY, HtmlPath.EMPTY)
				.Build(),
				styleMap);
		}
	}
}