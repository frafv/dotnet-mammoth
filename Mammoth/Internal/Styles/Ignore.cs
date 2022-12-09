using System.Collections.Generic;
using Mammoth.Internal.Html;

namespace Mammoth.Internal.Styles
{
	internal class Ignore : HtmlPath
	{
		internal static readonly HtmlPath INSTANCE = new Ignore();
		Ignore() { }
		public override IEnumerable<HtmlNode> Wrap(IEnumerable<HtmlNode> nodes)
		{
			yield break;
		}
	}
}

