using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal static class NumberingXml
	{
		public static Numbering ReadNumberingXmlElement(XmlElement element, Styles styles)
		{
			var abstractNums = ReadAbstractNums(element.FindChildren("w:abstractNum"));
			var nums = ReadNums(element.FindChildren("w:num"));
			return new Numbering(abstractNums, nums, styles);
		}
		static IDictionary<string, Numbering.AbstractNum> ReadAbstractNums(XmlElementList children)
		{
			return children.Select(child => ReadAbstractNum(child)).ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, Numbering.AbstractNum> ReadAbstractNum(XmlElement element)
		{
			string abstractNumId = element.GetAttribute("w:abstractNumId");
			var abstractNum = new Numbering.AbstractNum(
				ReadAbstractNumLevels(element),
				element.FindChildOrEmpty("w:numStyleLink").GetAttributeOrNone("w:val"));
			return new KeyValuePair<string, Numbering.AbstractNum>(abstractNumId, abstractNum);
		}
		static IDictionary<string, Numbering.AbstractNumLevel> ReadAbstractNumLevels(XmlElement element)
		{
			return element.FindChildren("w:lvl").Select(elem => ReadAbstractNumLevel(elem)).ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, Numbering.AbstractNumLevel> ReadAbstractNumLevel(XmlElement element)
		{
			string levelIndex = element.GetAttribute("w:ilvl");
			string numFmt = element.FindChildOrEmpty("w:numFmt").GetAttributeOrNone("w:val");
			bool isOrdered = numFmt != "bullet";
			string paragraphStyleId = element.FindChildOrEmpty("w:pStyle").GetAttributeOrNone("w:val");
			return new KeyValuePair<string, Numbering.AbstractNumLevel>(levelIndex, new Numbering.AbstractNumLevel(levelIndex, isOrdered, paragraphStyleId));
		}
		static IDictionary<string, Numbering.Num> ReadNums(XmlElementList numElements)
		{
			return numElements.Select(elem => ReadNum(elem)).ToDictionary(p => p.Key, p => p.Value);
		}
		static KeyValuePair<string, Numbering.Num> ReadNum(XmlElement numElement)
		{
			string numId = numElement.GetAttribute("w:numId");
			string abstractNumId = numElement.FindChildOrEmpty("w:abstractNumId").GetAttributeOrNone("w:val");
			return new KeyValuePair<string, Numbering.Num>(numId, new Numbering.Num(abstractNumId));
		}
	}
}

