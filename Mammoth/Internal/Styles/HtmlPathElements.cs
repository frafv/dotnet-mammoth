using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Html;

namespace Mammoth.Internal.Styles
{
	internal sealed class HtmlPathElements : HtmlPath
	{
		internal HtmlPathElements(IEnumerable<HtmlPathElement> elements)
		{
			this.Elements = elements.ToList();
		}
		internal HtmlPathElements(params HtmlPathElement[] elements)
			: this((IEnumerable<HtmlPathElement>)elements) { }

		internal new IList<HtmlPathElement> Elements { get; }

		public override IEnumerable<HtmlNode> Wrap(IEnumerable<HtmlNode> nodes)
		{
			foreach (var element in Elements.Reverse())
				nodes = element.Wrap(nodes);
			return nodes;
		}
	}
}

