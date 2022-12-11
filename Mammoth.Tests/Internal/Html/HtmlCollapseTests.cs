using System.Collections.Generic;
using Mammoth.Internal.Styles.Parsing.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Html.Tests
{
	[TestClass()]
	public class HtmlCollapseTests
	{
		[TestMethod()]
		public void CollapsingDoesNothingToSingleTextNode()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(Html.Text("Bluebells")),
				Html.Text("Bluebells"));
		}

		[TestMethod()]
		public void ConsecutiveFreshElementsAreNotCollapsed()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(Html.Element("p"), Html.Element("p")),
				Html.Element("p"), Html.Element("p"));
		}

		[TestMethod()]
		public void ConsecutiveCollapsibleElementsAreCollapsedIfTheyHaveTheSameTagAndAttributes()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("p", Html.Text("One")),
					Html.CollapsibleElement("p", Html.Text("Two"))),

				Html.CollapsibleElement("p", Html.Text("One"), Html.Text("Two")));
		}

		[TestMethod()]
		public void ElementsWithDifferentTagNamesAreNotCollapsed()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("p", Html.Text("One")),
					Html.CollapsibleElement("div", Html.Text("Two"))),

				Html.CollapsibleElement("p", Html.Text("One")),
				Html.CollapsibleElement("div", Html.Text("Two")));
		}

		[TestMethod()]
		public void ElementsWithDifferentAttributesAreNotCollapsed()
		{
			// TODO: should there be some spacing when block-level elements are collapsed?
			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("p", new HtmlAttributes { ["id"] = "a" }, Html.Text("One")),
					Html.CollapsibleElement("p", Html.Text("Two"))),

				Html.CollapsibleElement("p", new HtmlAttributes { ["id"] = "a" }, Html.Text("One")),
				Html.CollapsibleElement("p", Html.Text("Two")));
		}

		[TestMethod()]
		public void ChildrenOfCollapsedElementCanCollapseWithChildrenOfPreviousElement()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("blockquote",
						Html.CollapsibleElement("p", Html.Text("One"))),
					Html.CollapsibleElement("blockquote",
						Html.CollapsibleElement("p", Html.Text("Two")))),

				Html.CollapsibleElement("blockquote",
					Html.CollapsibleElement("p", Html.Text("One"), Html.Text("Two"))));
		}

		[TestMethod()]
		public void CollapsibleElementCanCollapseIntoPreviousFreshElement()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(Html.Element("p"), Html.CollapsibleElement("p")),
				Html.Element("p"));
		}

		[TestMethod()]
		public void ElementWithChoiceOfTagNamesCanCollapseIntoPreviousElementIfItHasOneOfThoseTagNamesAsItsMainTagName()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("ol"),
					Html.CollapsibleElement("ul", "ol")),
				Html.CollapsibleElement("ol"));

			HtmlNodeAssert.AreEqual(
				Collapse(
					Html.CollapsibleElement("ul", "ol"),
					Html.CollapsibleElement("ol")),

				Html.CollapsibleElement("ul", "ol"),
				Html.CollapsibleElement("ol"));
		}

		[TestMethod()]
		public void WhenSeparatorIsPresentThenSeparatorIsPrependedToCollapsedElement()
		{
			HtmlNodeAssert.AreEqual(
				Collapse(
					HtmlElementBuilder.Fresh("pre").Element(Html.Text("Hello")),
					HtmlElementBuilder.Collapsible("pre").Separator("\n").Element(Html.Text(" the"), Html.Text("re"))),

				HtmlElementBuilder.Fresh("pre").Element(
					Html.Text("Hello"),
					Html.Text("\n"),
					Html.Text(" the"),
					Html.Text("re")));
		}

		IList<HtmlNode> Collapse(params HtmlNode[] nodes)
		{
			return Html.Collapse(nodes);
		}
	}
}