using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Html
{
	internal sealed class HtmlTag
	{
		internal HtmlTag(IEnumerable<string> tagNames, IDictionary<string, string> attributes = null, bool isCollapsible = false, string separator = "")
		{
			TagNames = tagNames.ToArray();
			Attributes = HtmlAttributes.Create(attributes);
			IsCollapsible = isCollapsible;
			Separator = separator;
		}
		internal HtmlTag(string tagName, IDictionary<string, string> attributes = null, bool isCollapsible = false, string separator = "")
			: this(new[] { tagName }, attributes, isCollapsible, separator)
		{ }

		public IEnumerable<string> TagNames { get; }

		public HtmlAttributes Attributes { get; }

		public bool IsCollapsible { get; }

		public string Separator { get; }
	}
}

