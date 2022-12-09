namespace Mammoth.Internal.Xml
{
	internal abstract class XmlNode
	{
		public abstract string InnerText { get; }
		public abstract void Accept(IVisitor visitor);

		internal interface IVisitor
		{
			void Visit(XmlElement element);
			void Visit(XmlTextNode textNode);
		}
	}
}

