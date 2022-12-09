namespace Mammoth.Internal.Html
{
	internal abstract class HtmlNode
	{
		public abstract void Accept(IVisitor visitor);

		internal interface IVisitor
		{
			void Visit(HtmlElement element);
			void Visit(HtmlTextNode node);
			void Visit(HtmlForceWrite forceWrite);
		}
	}
}

