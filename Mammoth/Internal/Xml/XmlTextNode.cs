namespace Mammoth.Internal.Xml
{
	internal class XmlTextNode : XmlNode
	{
		internal XmlTextNode(string value)
		{
			Value = value;
		}
		public string Value { get; }

		public override string InnerText => Value;

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
		public override string ToString()
		{
			return $"XmlTextNode(value={Value})";
		}
	}
}

