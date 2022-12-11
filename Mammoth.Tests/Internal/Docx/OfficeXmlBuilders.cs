using Mammoth.Internal.Xml;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	static class OfficeXmlBuilders
	{
		public static XmlElement WTr(params XmlElement[] children)
		{
			return Element("w:tr", children);
		}

		public static XmlElement WTc(params XmlElement[] children)
		{
			return Element("w:tc", children);
		}

		public static XmlElement WTcPr(params XmlElement[] children)
		{
			return Element("w:tcPr", children);
		}

		public static XmlElement WGridspan(string val)
		{
			return Element("w:gridSpan", new XmlAttributes { ["w:val"] = val });
		}

		public static XmlElement WVmerge(string val)
		{
			return Element("w:vMerge", new XmlAttributes { ["w:val"] = val });
		}
	}
}
