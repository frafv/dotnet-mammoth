using System.Collections.Generic;

namespace Mammoth.Internal.Xml
{
	internal class NullXmlElement : IXmlElementLike
	{
		internal static readonly IXmlElementLike INSTANCE = new NullXmlElement();
		NullXmlElement() { }
		public bool HasChild(string name)
		{
			return false;
		}
		public XmlElement FindChild(string name)
		{
			return null;
		}
		public IXmlElementLike FindChildOrEmpty(string name)
		{
			return this;
		}
		public string GetAttributeOrNone(string name)
		{
			return null;
		}
		public IList<XmlNode> Children => new XmlNode[0];
	}
}

