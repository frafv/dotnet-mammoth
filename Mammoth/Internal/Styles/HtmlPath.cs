using System.Collections.Generic;
using Mammoth.Internal.Html;

namespace Mammoth.Internal.Styles
{
	internal abstract class HtmlPath
	{
		internal static HtmlPath EMPTY = new HtmlPathElements();
		internal static HtmlPath IGNORE = Ignore.INSTANCE;
		public static HtmlPath Elements(params HtmlPathElement[] elements)
		{
			return new HtmlPathElements(elements);
		}
		public static HtmlPath Element(string tagName, IDictionary<string, string> attributes = null)
		{
			var tag = new HtmlTag(tagName, attributes);
			return new HtmlPathElements(new HtmlPathElement(tag));
		}
		public static HtmlPath CollapsibleElement(string tagName, IDictionary<string, string> attributes = null)
		{
			return CollapsibleElement(new[] { tagName }, attributes);
		}
		public static HtmlPath CollapsibleElement(IEnumerable<string> tagNames, IDictionary<string, string> attributes = null)
		{
			var tag = new HtmlTag(tagNames, attributes, isCollapsible: true);
			return new HtmlPathElements(new HtmlPathElement(tag));
		}

		public abstract IEnumerable<HtmlNode> Wrap(IEnumerable<HtmlNode> nodes);
	}
}

