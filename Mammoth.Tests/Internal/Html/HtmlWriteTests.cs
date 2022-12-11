using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Html.Tests
{
	[TestClass()]
	public class HtmlWriteTests
	{
		[TestMethod()]
		public void TextNodesAreWrittenAsPlainText()
		{
			Assert.AreEqual(
				"Dark Blue",
				Write(Html.Text("Dark Blue")));
		}

		[TestMethod()]
		public void TextNodesAreHtmlEscaped()
		{
			Assert.AreEqual(
				"&gt;&lt;&amp;",
				Write(Html.Text("><&")));
		}

		[TestMethod()]
		public void DoubleQuotesInTextNodesArentHtmlEscaped()
		{
			Assert.AreEqual(
				"&gt;&lt;&amp;\"",
				Write(Html.Text("><&\"")));
		}

		[TestMethod()]
		public void VoidElementsWithoutChildrenAreWrittenAsSelfClosing()
		{
			Assert.AreEqual(
				"<br />",
				Write(Html.Element("br")));
		}

		[TestMethod()]
		public void WriteSelfClosingElementWithAttributes()
		{
			Assert.AreEqual(
				"<img class=\"external\" src=\"http://example.com\" />",
				Write(Html.Element("img", new HtmlAttributes { ["class"] = "external", ["src"] = "http://example.com" })));
		}

		[TestMethod()]
		public void WriteElementWithNoChildren()
		{
			Assert.AreEqual(
				"<p></p>",
				Write(Html.Element("p")));
		}

		[TestMethod()]
		public void WriteElementWithChildren()
		{
			Assert.AreEqual(
				"<div><p></p><ul></ul></div>",
				Write(Html.Element("div",
					Html.Element("p"),
					Html.Element("ul"))));
		}

		[TestMethod()]
		public void WriteElementWithAttributes()
		{
			Assert.AreEqual(
				"<a class=\"external\" href=\"http://example.com\"></a>",
				Write(Html.Element("a", new HtmlAttributes { ["class"] = "external", ["href"] = "http://example.com" })));
		}

		[TestMethod()]
		public void AttributeValuesAreEscaped()
		{
			Assert.AreEqual(
				"<a href=\"&gt;&lt;&amp;&quot;\"></a>",
				Write(Html.Element("a", new HtmlAttributes { ["href"] = "><&\"" })));
		}

		string Write(params HtmlNode[] nodes)
		{
			return Html.Write(nodes);
		}
	}
}