namespace Mammoth.Internal.Html
{
	internal class HtmlForceWrite : HtmlNode
	{
		internal static readonly HtmlForceWrite FORCE_WRITE = new HtmlForceWrite();
		HtmlForceWrite() { }
		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

