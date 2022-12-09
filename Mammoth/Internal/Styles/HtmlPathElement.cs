using System.Collections.Generic;
using Mammoth.Internal.Html;

namespace Mammoth.Internal.Styles
{
	internal sealed class HtmlPathElement
	{
		internal HtmlPathElement(HtmlTag tag)
		{
			this.Tag = tag;
		}

		internal HtmlTag Tag { get; }

		public IEnumerable<HtmlNode> Wrap(IEnumerable<HtmlNode> nodes)
		{
			yield return new HtmlElement(Tag, nodes);
		}
	}
}

