using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Documents.Tests;
using Mammoth.Internal.Html.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	static class HtmlPathAssert
	{
		public static void AreEqual(HtmlPath expected, HtmlPath actual)
		{
			if (!AreObjects(expected, actual)) return;
			Assert.AreEqual(expected.GetType(), actual.GetType());
			switch (expected)
			{
				case HtmlPathElements elements:
					var actualElements = actual as HtmlPathElements;
					Assert.AreEqual(elements.Elements.Count, actualElements.Elements.Count);
					for (int k = 0; k < elements.Elements.Count; k++)
						HtmlNodeAssert.AreEqual(elements.Elements[k].Tag, actualElements.Elements[k].Tag);
					break;
				case Ignore _:
					break;
				default:
					Assert.Fail("Unknown HTML path " + actual.GetType());
					break;
			}
		}

		static void AreEqual<TElement>(DocumentElementMatcher<TElement> expected, DocumentElementMatcher<TElement> actual)
			where TElement : DocumentElement
		{
			if (!AreObjects(expected, actual)) return;
			switch (expected)
			{
				case BreakMatcher breakMatcher:
					var actualBreak = actual as BreakMatcher;
					Assert.AreEqual(breakMatcher.Type, actualBreak.Type);
					break;
				case ParagraphMatcher paragraphMatcher:
					var actualParagraph = actual as ParagraphMatcher;
					Assert.AreEqual(paragraphMatcher.StyleId, actualParagraph.StyleId);
					AreEqual(paragraphMatcher.StyleName, actualParagraph.StyleName);
					DocumentElementAssert.AreEqual(paragraphMatcher.Numbering, actualParagraph.Numbering);
					break;
				case RunMatcher runMatcher:
					var actualRun = actual as RunMatcher;
					Assert.AreEqual(runMatcher.StyleId, actualRun.StyleId);
					AreEqual(runMatcher.StyleName, actualRun.StyleName);
					break;
				case TableMatcher tableMatcher:
					var actualTable = actual as TableMatcher;
					Assert.AreEqual(tableMatcher.StyleId, actualTable.StyleId);
					AreEqual(tableMatcher.StyleName, actualTable.StyleName);
					break;
				default:
					Assert.Fail("Unknown document element matcher " + actual.GetType());
					break;
			}
		}

		static void AreEqual(StringMatcher expected, StringMatcher actual)
		{
			if (!AreObjects(expected, actual)) return;
			switch (expected)
			{
				case EqualToStringMatcher equalMatcher:
					var actualEqual = actual as EqualToStringMatcher;
					Assert.AreEqual(equalMatcher.Value, actualEqual.Value);
					break;
				case StartsWithStringMatcher startMatcher:
					var actualStart = actual as StartsWithStringMatcher;
					Assert.AreEqual(startMatcher.Prefix, actualStart.Prefix);
					break;
				default:
					Assert.Fail("Unknown string matcher " + actual.GetType());
					break;
			}
		}

		public static void AreEqual(StyleMap expected, StyleMap actual)
		{
			if (!AreObjects(expected, actual)) return;
			AreEqual(expected.Bold, actual.Bold);
			AreEqual(expected.Italic, actual.Italic);
			AreEqual(expected.Underline, actual.Underline);
			AreEqual(expected.Strikethrough, actual.Strikethrough);
			AreEqual(expected.AllCaps, actual.AllCaps);
			AreEqual(expected.SmallCaps, actual.SmallCaps);
			SequenceEqual(expected.ParagraphStyles, actual.ParagraphStyles);
			SequenceEqual(expected.RunStyles, actual.RunStyles);
			SequenceEqual(expected.TableStyles, actual.TableStyles);
			SequenceEqual(expected.BreakStyles, actual.BreakStyles);
		}

		static void AreEqual<TElement>(StyleMapping<TElement> expected, StyleMapping<TElement> actual)
			where TElement : DocumentElement
		{
			if (!AreObjects(expected, actual)) return;
			AreEqual(expected.HtmlPath, actual.HtmlPath);
			AreEqual(expected.Matcher, actual.Matcher);
		}

		static bool AreObjects(object expected, object actual)
		{
			if (expected == null)
			{
				Assert.IsNull(actual);
				return false;
			}
			else
			{
				Assert.IsNotNull(actual);
				return true;
			}
		}

		static void SequenceEqual<TElement>(IEnumerable<StyleMapping<TElement>> expected, IEnumerable<StyleMapping<TElement>> actual)
			where TElement : DocumentElement
		{
			var expectedList = expected.ToList();
			var actualList = actual.ToList();
			Assert.AreEqual(expectedList.Count, actualList.Count);
			for (int k = 0; k < expectedList.Count; k++)
			{
				var expectedChild = expectedList[k];
				var actualChild = actualList[k];
				Assert.AreEqual(expectedChild.GetType(), actualChild.GetType());
				AreEqual(expectedChild, actualChild);
			}
		}
	}
}
