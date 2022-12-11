using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Docx.RelationshipsXml;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class RelationshipsXmlTests
	{
		[TestMethod()]
		public void RelationshipTargetsCanBeFoundById()
		{
			var element = Element("relationship:Relationships",
				Element("relationships:Relationship", new XmlAttributes
				{
					["Id"] = "rId8",
					["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink",
					["Target"] = "http://example.com"
				}),
				Element("relationships:Relationship", new XmlAttributes
				{
					["Id"] = "rId2",
					["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink",
					["Target"] = "http://example.net"
				}));

			var relationships = ReadRelationshipsXmlElement(element);

			Assert.AreEqual("http://example.com", relationships.FindTargetByRelationshipId("rId8"));
		}

		[TestMethod()]
		public void RelationshipTargetsCanBeFoundByType()
		{
			var element = Element("relationship:Relationships",
				Element("relationships:Relationship", new XmlAttributes
				{
					["Id"] = "rId2",
					["Target"] = "docProps/core.xml",
					["Type"] = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"
				}),
				Element("relationships:Relationship", new XmlAttributes
				{
					["Id"] = "rId1",
					["Target"] = "word/document.xml",
					["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
				}),
				Element("relationships:Relationship", new XmlAttributes
				{
					["Id"] = "rId3",
					["Target"] = "word/document2.xml",
					["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
				}));

			var relationships = ReadRelationshipsXmlElement(element);

			CollectionAssert.AreEqual(
				new[] { "word/document.xml", "word/document2.xml" },
				relationships.FindTargetsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"));
		}

		[TestMethod()]
		public void WhenThereAreNoRelationshipsOfRequestedTypeThenEmptyListIsReturned()
		{
			var element = Element("relationship:Relationships");

			var relationships = ReadRelationshipsXmlElement(element);

			CollectionAssert.AreEqual(
				new string[0],
				relationships.FindTargetsByType("http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"));
		}
	}
}