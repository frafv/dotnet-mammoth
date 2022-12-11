using System.Collections.Generic;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Docx.NumberingXml;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class NumberingXmlTests
	{
		readonly static XmlElement SAMPLE_NUMBERING_XML = Element("w:numbering",
			Element("w:abstractNum", new XmlAttributes { ["w:abstractNumId"] = "42" },
				Element("w:lvl", new XmlAttributes { ["w:ilvl"] = "0" },
					Element("w:numFmt", new XmlAttributes { ["w:val"] = "bullet" })),
				Element("w:lvl", new XmlAttributes { ["w:ilvl"] = "1" },
					Element("w:numFmt", new XmlAttributes { ["w:val"] = "decimal" }))),
			Element("w:num", new XmlAttributes { ["w:numId"] = "47" },
				Element("w:abstractNumId", new XmlAttributes { ["w:val"] = "42" })));

		[TestMethod()]
		public void FindLevelReturnsNoneIfNumWithIdCannotBeFound()
		{
			var numbering = ReadNumbering(Element("w:numbering"));
			Assert.IsNull(numbering.FindLevel("47", "0"));
		}

		[TestMethod()]
		public void LevelIncludesLevelIndex()
		{
			var numbering = ReadNumbering(SAMPLE_NUMBERING_XML);
			Assert.AreEqual("0", numbering.FindLevel("47", "0")?.LevelIndex);
			Assert.AreEqual("1", numbering.FindLevel("47", "1")?.LevelIndex);
		}

		[TestMethod()]
		public void ListIsNotOrderedIfFormattedAsBullet()
		{
			var numbering = ReadNumbering(SAMPLE_NUMBERING_XML);
			Assert.AreEqual(false, numbering.FindLevel("47", "0")?.IsOrdered);
		}

		[TestMethod()]
		public void ListIsOrderedIfFormattedAsDecimal()
		{
			var numbering = ReadNumbering(SAMPLE_NUMBERING_XML);
			Assert.AreEqual(true, numbering.FindLevel("47", "1")?.IsOrdered);
		}

		[TestMethod()]
		public void NumReferencingNonExistentAbstractNumIsIgnored()
		{
			var element = Element("w:numbering",
				Element("w:num", new XmlAttributes { ["w:numId"] = "47" },
					Element("w:abstractNumId", new XmlAttributes { ["w:val"] = "42" })));

			var numbering = ReadNumbering(element);

			Assert.IsNull(numbering.FindLevel("47", "0"));
		}

		[TestMethod()]
		public void WhenAbstractNumHasNumStyleLinkThenStyleIsUsedToFindNum()
		{
			var numbering = ReadNumberingXmlElement(
				Element("w:numbering",
					Element("w:abstractNum", new XmlAttributes { ["w:abstractNumId"] = "100" },
						Element("w:lvl", new XmlAttributes { ["w:ilvl"] = "0" },
							Element("w:numFmt", new XmlAttributes { ["w:val"] = "decimal" }))),
					Element("w:abstractNum", new XmlAttributes { ["w:abstractNumId"] = "101" },
						Element("w:numStyleLink", new XmlAttributes { ["w:val"] = "List1" })),
					Element("w:num", new XmlAttributes { ["w:numId"] = "200" },
						Element("w:abstractNumId", new XmlAttributes { ["w:val"] = "100" })),
					Element("w:num", new XmlAttributes { ["w:numId"] = "201" },
						Element("w:abstractNumId", new XmlAttributes { ["w:val"] = "101" }))),
				new Styles(
					new Dictionary<string, Style>(),
					new Dictionary<string, Style>(),
					new Dictionary<string, Style>(),
					new Dictionary<string, NumberingStyle> { ["List1"] = new NumberingStyle("200") }));
			Assert.AreEqual(true, numbering.FindLevel("201", "0")?.IsOrdered);
		}

		// See: 17.9.23 pStyle (Paragraph Style's Associated Numbering Level) in ECMA-376, 4th Edition
		[TestMethod()]
		public void NumberingLevelCanBeFoundByParagraphStyleId()
		{
			var numbering = ReadNumbering(
				Element("w:numbering",
					Element("w:abstractNum", new XmlAttributes { ["w:abstractNumId"] = "42" },
						Element("w:lvl", new XmlAttributes { ["w:ilvl"] = "0" },
							Element("w:numFmt", new XmlAttributes { ["w:val"] = "bullet" }))),
					Element("w:abstractNum", new XmlAttributes { ["w:abstractNumId"] = "43" },
						Element("w:lvl", new XmlAttributes { ["w:ilvl"] = "0" },
							Element("w:pStyle", new XmlAttributes { ["w:val"] = "List" }),
							Element("w:numFmt", new XmlAttributes { ["w:val"] = "decimal" })))));
			Assert.AreEqual(true, numbering.FindLevelByParagraphStyleId("List")?.IsOrdered);
			Assert.IsNull(numbering.FindLevelByParagraphStyleId("Paragraph"));
		}


		private Numbering ReadNumbering(XmlElement element)
		{
			return ReadNumberingXmlElement(element, Styles.EMPTY);
		}
	}
}