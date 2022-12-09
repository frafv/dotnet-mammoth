using System.Collections.Generic;

namespace Mammoth.Internal.Xml
{
	internal static class XmlNodes
	{
		public static XmlElement Element(string name, params XmlNode[] children)
		{
			return Element(name, null, children);
		}
		public static XmlElement Element(string name, IDictionary<string, string> attributes, params XmlNode[] children)
		{
			return new XmlElement(name, attributes, children);
		}
		public static XmlTextNode Text(string value)
		{
			return new XmlTextNode(value);
		}
	}
}

