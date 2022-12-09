using System.Collections.Generic;

namespace Mammoth.Internal.Xml.Parsing
{
	internal interface ISimpleSaxHandler
	{
		void StartElement(ElementName name, IDictionary<ElementName, string> attributes);
		void EndElement();
		void Characters(string @string);
	}
}

