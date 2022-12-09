namespace Mammoth.Internal.Html
{
	internal sealed class HtmlTextNode : HtmlNode
	{
		internal HtmlTextNode(string value)
		{
			Value = value;
		}
		public string Value { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

