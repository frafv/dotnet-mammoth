using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Mammoth.Internal.Xml.Parsing
{
	internal static class SimpleSax
	{
		internal static void ParseStream(Stream input, ISimpleSaxHandler handler)
		{
			Parse(new StreamReader(input), handler);
		}
		internal static void ParseString(string xml, ISimpleSaxHandler handler)
		{
			Parse(new StringReader(xml), handler);
		}
		static void Parse(TextReader input, ISimpleSaxHandler handler)
		{
			var reader = XmlReader.Create(input, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit });
			while (reader.Read())
			{
				switch (reader.NodeType)
				{
					case XmlNodeType.Element:
						var name = new ElementName(reader.NamespaceURI, reader.LocalName);
						var attributes = new Dictionary<ElementName, string>();
						bool isEmpty = reader.IsEmptyElement;
						for (int attributeIndex = 0; attributeIndex < reader.AttributeCount; attributeIndex++)
						{
							reader.MoveToAttribute(attributeIndex);
							if (!reader.Name.StartsWith("xml", System.StringComparison.InvariantCulture))
								attributes.Add(new ElementName(reader.NamespaceURI, reader.LocalName), reader.Value);
						}
						handler.StartElement(name, attributes);
						if (isEmpty)
						{
							handler.EndElement();
						}
						break;
					case XmlNodeType.CDATA:
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
						handler.Characters(reader.Value);
						break;
					case XmlNodeType.EntityReference:
						throw new System.NotImplementedException();
					case XmlNodeType.EndElement:
						handler.EndElement();
						break;
				}
			}
		}
	}
}
