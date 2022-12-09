using System.Collections.Generic;

namespace Mammoth.Internal.Xml
{
	internal interface IXmlElementLike
	{
		bool HasChild(string name);
		XmlElement FindChild(string name);
		IXmlElementLike FindChildOrEmpty(string name);
		string GetAttributeOrNone(string name);
		IList<XmlNode> Children { get; }
	}
}

