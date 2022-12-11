using Mammoth.Internal.Html;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	class HtmlElementBuilder
	{
		public static HtmlElementBuilder Collapsible(string tagName)
		{
			return new HtmlElementBuilder(tagName, isCollapsible: true);
		}

		public static HtmlElementBuilder Fresh(string tagName)
		{
			return new HtmlElementBuilder(tagName);
		}

		readonly string tagName;
		readonly bool isCollapsible;
		readonly string separator;

		public HtmlElementBuilder(string tagName, bool isCollapsible = false, string separator = "")
		{
			this.tagName = tagName;
			this.isCollapsible = isCollapsible;
			this.separator = separator;
		}

		public HtmlElementBuilder Separator(string separator)
		{
			return new HtmlElementBuilder(tagName, isCollapsible, separator);
		}

		public HtmlPathElement PathElement()
		{
			return new HtmlPathElement(Tag());
		}

		public HtmlElement Element(params HtmlNode[] children)
		{
			return new HtmlElement(Tag(), children);
		}

		HtmlTag Tag()
		{
			return new HtmlTag(tagName, isCollapsible: isCollapsible, separator: separator);
		}
	}
}
