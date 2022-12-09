using System;
using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal static class StylesXml
	{
		public static Styles ReadStylesXmlElement(XmlElement element)
		{
			var styleElements = element.FindChildren("w:style");
			return new Styles(
				ReadStyles(styleElements, "paragraph"),
				ReadStyles(styleElements, "character"),
				ReadStyles(styleElements, "table"),
				ReadNumberingStyles(styleElements));
		}
		static IDictionary<string, Style> ReadStyles(XmlElementList styleElements, string styleType)
		{
			return StyleElementsOfType(styleElements, styleType)
				.Select(elem => ReadStyle(elem))
				.ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, Style> ReadStyle(XmlElement element)
		{
			string styleId = ReadStyleId(element);
			string styleName = element.FindChildOrEmpty("w:name").GetAttributeOrNone("w:val");
			return new KeyValuePair<string, Style>(styleId, new Style(styleId, styleName));
		}
		static IDictionary<string, NumberingStyle> ReadNumberingStyles(XmlElementList styleElements)
		{
			return StyleElementsOfType(styleElements, "numbering").
				Select(elem => ReadNumberingStyle(elem))
				.ToDictionary(p => p.Key, p => p.Value);
		}

		static KeyValuePair<string, NumberingStyle> ReadNumberingStyle(XmlElement element)
		{
			string styleId = ReadStyleId(element);
			string numId = element
				.FindChildOrEmpty("w:pPr")
				.FindChildOrEmpty("w:numPr")
				.FindChildOrEmpty("w:numId")
				.GetAttributeOrNone("w:val");
			return new KeyValuePair<string, NumberingStyle>(styleId, new NumberingStyle(numId));
		}

		static String ReadStyleId(XmlElement element)
		{
			return element.GetAttribute("w:styleId");
		}

		static IEnumerable<XmlElement> StyleElementsOfType(XmlElementList styleElements, String styleType)
		{
			return styleElements.Where(styleElement => IsStyleType(styleElement, styleType));
		}

		static bool IsStyleType(XmlElement styleElement, String styleType)
		{
			return styleElement.GetAttribute("w:type") == styleType;
		}
	}
}

