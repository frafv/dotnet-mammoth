using Mammoth.Internal.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;

namespace Mammoth.Internal.Styles.Tests
{
	[TestClass()]
	public class ParagraphMatcherTests
	{
		[TestMethod()]
		public void MatcherWithoutConstraintsMatchesEverything()
		{
			Assert.IsTrue(ParagraphMatcher.ANY.Matches(Paragraph()));

			var paragraphWithStyleId = Paragraph(WithStyle(new Style("TipsParagraph")));
			Assert.IsTrue(ParagraphMatcher.ANY.Matches(paragraphWithStyleId));

			var paragraphWithStyleName = Paragraph(WithStyle(new Style("TipsParagraph", "Tips Paragraph")));
			Assert.IsTrue(ParagraphMatcher.ANY.Matches(paragraphWithStyleName));
		}

		[TestMethod()]
		public void MatcherWithStyleIdOnlyMatchesParagraphsWithThatStyleId()
		{
			var matcher = ParagraphMatcher.ByStyleId("TipsParagraph");
			Assert.IsFalse(matcher.Matches(Paragraph()));

			var paragraphWithCorrectStyleId = Paragraph(
				WithStyle(new Style("TipsParagraph")));
			Assert.IsTrue(matcher.Matches(paragraphWithCorrectStyleId));

			var paragraphWithIncorrectStyleId = Paragraph(
				WithStyle(new Style("Heading 1")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectStyleId));
		}

		[TestMethod()]
		public void MatcherWithStyleNameOnlyMatchesParagraphsWithThatStyleName()
		{
			var matcher = ParagraphMatcher.ByStyleName("Tips Paragraph");
			Assert.IsFalse(matcher.Matches(Paragraph()));

			var paragraphWithNamelessStyle = Paragraph(
				WithStyle(new Style("TipsParagraph")));
			Assert.IsFalse(matcher.Matches(paragraphWithNamelessStyle));

			var paragraphWithCorrectStyleName = Paragraph(
				WithStyle(new Style("TipsParagraph", "Tips Paragraph")));
			Assert.IsTrue(matcher.Matches(paragraphWithCorrectStyleName));

			var paragraphWithIncorrectStyleName = Paragraph(
				WithStyle(new Style("Heading 1", "Heading 1")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectStyleName));
		}

		[TestMethod()]
		public void StyleNamesAreCaseInsensitive()
		{
			var matcher = ParagraphMatcher.ByStyleName("tips paragraph");
			Assert.IsFalse(matcher.Matches(Paragraph()));

			var paragraphWithCorrectStyleName = Paragraph(
				WithStyle(new Style("TipsParagraph", "Tips Paragraph")));
			Assert.IsTrue(matcher.Matches(paragraphWithCorrectStyleName));
		}

		[TestMethod()]
		public void MatcherWithOrderedListOnlyMatchesParagraphsWithOrderedListAtThatLeve()
		{
			var matcher = ParagraphMatcher.OrderedList("4");
			Assert.IsFalse(matcher.Matches(Paragraph()));

			var paragraphWithCorrectNumbering = Paragraph(
				WithNumbering(NumberingLevel.Ordered("4")));
			Assert.IsTrue(matcher.Matches(paragraphWithCorrectNumbering));

			var paragraphWithIncorrectLevel = Paragraph(
				WithNumbering(NumberingLevel.Ordered("3")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectLevel));

			var paragraphWithIncorrectOrdering = Paragraph(
				WithNumbering(NumberingLevel.Unordered("4")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectOrdering));
		}

		[TestMethod()]
		public void MatcherWithUnorderedListOnlyMatchesParagraphsWithUnrderedListAtThatLeve()
		{
			var matcher = ParagraphMatcher.UnorderedList("4");
			Assert.IsFalse(matcher.Matches(Paragraph()));

			var paragraphWithCorrectNumbering = Paragraph(
				WithNumbering(NumberingLevel.Unordered("4")));
			Assert.IsTrue(matcher.Matches(paragraphWithCorrectNumbering));

			var paragraphWithIncorrectLevel = Paragraph(
				WithNumbering(NumberingLevel.Unordered("3")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectLevel));

			var paragraphWithIncorrectOrdering = Paragraph(
				WithNumbering(NumberingLevel.Ordered("4")));
			Assert.IsFalse(matcher.Matches(paragraphWithIncorrectOrdering));
		}
	}
}