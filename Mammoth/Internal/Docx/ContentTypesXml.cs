using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal static class ContentTypesXml
	{
		public static ContentTypes ReadContentTypesXmlElement(XmlElement element)
		{
			return new ContentTypes(
				ReadDefaults(element.FindChildren("content-types:Default")),
				ReadOverrides(element.FindChildren("content-types:Override")));
		}
		static IDictionary<string, string> ReadDefaults(XmlElementList children)
		{
			return children.Select(c => ReadDefault(c)).ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, string> ReadDefault(XmlElement element)
		{
			return new KeyValuePair<string, string>(
				element.GetAttribute("Extension"),
				element.GetAttribute("ContentType"));
		}
		static IDictionary<string, string> ReadOverrides(XmlElementList children)
		{
			return children.Select(c => ReadOverride(c)).ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, string> ReadOverride(XmlElement element)
		{
			return new KeyValuePair<string, string>(
				element.GetAttribute("PartName").TrimStart('/'),
				element.GetAttribute("ContentType"));
		}
	}
}

