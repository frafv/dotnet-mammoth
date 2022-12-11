using Mammoth.Internal.Html;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	[TestClass()]
	public class HtmlPathParserTests
	{
		[TestMethod()]
		public void ParseEmptyPath()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath(""),
				HtmlPath.EMPTY);
		}

		[TestMethod()]
		public void ReadSingleElement()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("p"),
				HtmlPath.CollapsibleElement("p"));
		}

		[TestMethod()]
		public void ReadElementWithChoiceOfTagNames()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("ul|ol"),
				HtmlPath.CollapsibleElement(new[] { "ul", "ol" }));

			HtmlPathAssert.AreEqual(
				ParseHtmlPath("ul|ol|p"),
				HtmlPath.CollapsibleElement(new[] { "ul", "ol", "p" }));
		}

		[TestMethod()]
		public void ReadNestedElements()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("ul > li"),
				HtmlPath.Elements(
					HtmlElementBuilder.Collapsible("ul").PathElement(),
					HtmlElementBuilder.Collapsible("li").PathElement()));
		}

		[TestMethod()]
		public void ReadClassOnElement()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("p.tip"),
				HtmlPath.CollapsibleElement("p", new HtmlAttributes { ["class"] = "tip" }));
		}

		[TestMethod()]
		public void ReadMultipleClassesOnElement()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("p.tip.help"),
				HtmlPath.CollapsibleElement("p", new HtmlAttributes { ["class"] = "tip help" }));
		}

		[TestMethod()]
		public void ReadWhenElementMustBeFresh()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("p:fresh"),
				HtmlPath.Element("p"));
		}

		[TestMethod()]
		public void ReadSeparatorForElements()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("p:separator('x')"),
				HtmlPath.Elements(HtmlElementBuilder.Collapsible("p").Separator("x").PathElement()));
		}

		[TestMethod()]
		public void ReadIgnoreElement()
		{
			HtmlPathAssert.AreEqual(
				ParseHtmlPath("!"),
				HtmlPath.IGNORE);
		}

		HtmlPath ParseHtmlPath(string input)
		{
			return HtmlPathParser.Parse(StyleMappingTokeniser.Tokenise(input));
		}
	}
}