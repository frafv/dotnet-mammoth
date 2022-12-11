using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Html.Tests
{
	[TestClass()]
	public class HtmlStripEmptyTests
	{
		[TestMethod()]
		public void TextNodesWithTextAreNotStripped()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Text("H")),
				Html.Text("H"));
		}

		[TestMethod()]
		public void EmptyTextNodesAreStripped()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Text("")));
		}

		[TestMethod()]
		public void ElementsWithNonEmptyChildrenAreNotStripped()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Element("p", Html.Text("H"))),
				Html.Element("p", Html.Text("H")));
		}

		[TestMethod()]
		public void ElementsWithNoChildrenAreStripped()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Element("p")));
		}

		[TestMethod()]
		public void ElementsWithOnlyEmptyChildrenAreStripped()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Element("p", Html.Text(""))));
		}

		[TestMethod()]
		public void EmptyChildrenAreRemoved()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(
					Html.Element("ul",
						Html.Element("li", Html.Text("")),
						Html.Element("li", Html.Text("H")))),

				Html.Element("ul",
				Html.Element("li", Html.Text("H"))));
		}

		[TestMethod()]
		public void VoidElementsAreNeverEmpty()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.Element("br")),
				Html.Element("br"));
		}

		[TestMethod()]
		public void ForceWritesAreNeverEmpty()
		{
			HtmlNodeAssert.AreEqual(
				StripEmpty(Html.FORCE_WRITE),
				Html.FORCE_WRITE);
		}

		IList<HtmlNode> StripEmpty(params HtmlNode[] nodes)
		{
			var list = Html.StripEmpty(nodes);
			Assert.IsNotNull(list);
			return list.ToList();
		}
	}
}