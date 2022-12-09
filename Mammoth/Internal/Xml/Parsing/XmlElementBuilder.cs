using System.Collections.Generic;

namespace Mammoth.Internal.Xml.Parsing
{
	internal class XmlElementBuilder
	{
		readonly string name;
		readonly IDictionary<string, string> attributes;
		readonly List<XmlNode> children;
		internal XmlElementBuilder(string name, IDictionary<string, string> attributes)
		{
			this.name = name;
			this.attributes = attributes;
			this.children = new List<XmlNode>();
		}
		public XmlElement Build()
		{
			return new XmlElement(name, attributes, children);
		}
		public void AddChild(XmlNode node)
		{
			children.Add(node);
		}
	}
}

