using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Docx.StylesXml;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class StylesXmlTests
	{
		[TestMethod()]
		public void ParagraphStyleIsNoneIfNoStyleWithThatIdExists()
		{
			var element = Element("w:styles");

			var styles = ReadStylesXmlElement(element);

			Assert.IsNull(styles.FindParagraphStyleById("Heading1"));
		}

		[TestMethod()]
		public void ParagraphStyleCanBeFoundById()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "paragraph", ["w:styleId"] = "Heading1" },
					NameElement("Heading 1")));

			var styles = ReadStylesXmlElement(element);

			Assert.AreEqual("Heading 1", styles.FindParagraphStyleById("Heading1")?.Name);
		}

		[TestMethod()]
		public void CharacterStyleCanBeFoundById()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "character", ["w:styleId"] = "Heading1Char" },
					NameElement("Heading 1 Char")));

			var styles = ReadStylesXmlElement(element);

			Assert.AreEqual("Heading 1 Char", styles.FindCharacterStyleById("Heading1Char")?.Name);
		}

		[TestMethod()]
		public void TableStyleCanBeFoundById()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "table", ["w:styleId"] = "TableNormal" },
					NameElement("Normal Table")));

			var styles = ReadStylesXmlElement(element);

			Assert.AreEqual("Normal Table", styles.FindTableStyleById("TableNormal")?.Name);
		}

		[TestMethod()]
		public void ParagraphAndCharacterStylesAreDistinct()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "paragraph", ["w:styleId"] = "Heading1" },
					NameElement("Heading 1")),
				Element("w:style", new XmlAttributes { ["w:type"] = "character", ["w:styleId"] = "Heading1Char" },
					NameElement("Heading 1 Char")));

			var styles = ReadStylesXmlElement(element);

			Assert.IsNull(styles.FindCharacterStyleById("Heading1"));
			Assert.IsNull(styles.FindParagraphStyleById("Heading1Char"));
		}

		[TestMethod()]
		public void StyleNameIsNoneIfNameElementDoesNotExist()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "paragraph", ["w:styleId"] = "Heading1" }),
				Element("w:style", new XmlAttributes { ["w:type"] = "character", ["w:styleId"] = "Heading1Char" }));

			var styles = ReadStylesXmlElement(element);

			Assert.IsNull(styles.FindParagraphStyleById("Heading1")?.Name);
			Assert.IsNull(styles.FindCharacterStyleById("Heading1Char")?.Name);
		}

		[TestMethod()]
		public void NumberingStyleIsNoneIfNoStyleWithThatIdExists()
		{
			var element = Element("w:styles");

			var styles = ReadStylesXmlElement(element);

			Assert.IsNull(styles.FindNumberingStyleById("List1"));
		}

		[TestMethod()]
		public void NumberingStyleHasNoneNumIdIfStyleHasNoParagraphProperties()
		{
			var element = Element("w:-styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "numbering", ["w:styleId"] = "List1" }));

			var styles = ReadStylesXmlElement(element);

			Assert.IsNull(styles.FindNumberingStyleById("List1")?.NumId);
		}

		[TestMethod()]
		public void NumberingStyleHasNumIdReadFromParagraphProperties()
		{
			var element = Element("w:styles",
				Element("w:style", new XmlAttributes { ["w:type"] = "numbering", ["w:styleId"] = "List1" },
					Element("w:pPr",
						Element("w:numPr",
							Element("w:numId", new XmlAttributes { ["w:val"] = "42" })))));

			var styles = ReadStylesXmlElement(element);

			Assert.AreEqual("42", styles.FindNumberingStyleById("List1")?.NumId);
		}

		private XmlElement NameElement(string name)
		{
			return Element("w:name", new XmlAttributes { ["w:val"] = name });
		}
	}
}