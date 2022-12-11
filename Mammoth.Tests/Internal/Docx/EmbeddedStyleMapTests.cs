using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Mammoth.Internal.Xml;
using Mammoth.Internal.Xml.Parsing;
using Mammoth.Internal.Xml.Parsing.Tests;
using Mammoth.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class EmbeddedStyleMapTests
	{
		static readonly string ORIGINAL_RELATIONSHIPS_XML = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
			"<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">" +
			"<Relationship Id=\"rId3\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings\" Target=\"settings.xml\"/>" +
			"</Relationships>";

		static readonly XmlElement EXPECTED_RELATIONSHIPS_XML = XmlNodes.Element("Relationships",
			XmlNodes.Element("Relationship", new XmlAttributes
			{
				["Id"] = "rId3",
				["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings",
				["Target"] = "settings.xml"
			}),
			XmlNodes.Element("Relationship", new XmlAttributes
			{
				["Id"] = "rMammothStyleMap",
				["Type"] = "http://schemas.zwobble.org/mammoth/style-map",
				["Target"] = "/mammoth/style-map"
			}));

		static readonly string ORIGINAL_CONTENT_TYPES_XML = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
			"<Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">" +
			"<Default Extension=\"png\" ContentType=\"image/png\"/>" +
			"</Types>";

		static readonly XmlElement EXPECTED_CONTENT_TYPES_XML = XmlNodes.Element("Types",
			XmlNodes.Element("Default", new XmlAttributes { ["Extension"] = "png", ["ContentType"] = "image/png" }),
			XmlNodes.Element("Override", new XmlAttributes { ["PartName"] = "/mammoth/style-map", ["ContentType"] = "text/prs.mammoth.style-map" }));

		[TestMethod()]
		public void EmbeddedStyleMapPreservesUnrelatedFiles()
		{
			var archive = NormalDocx();
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			Assert.AreEqual("placeholder text", ReadString(archive, "placeholder"));
		}

		[TestMethod()]
		public void EmbeddedStyleMapCanBeReadAfterBeingEmbedded()
		{
			var archive = NormalDocx();
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			Assert.AreEqual("p => h1", EmbeddedStyleMap.ReadStyleMap(archive));
		}

		[TestMethod()]
		public void EmbeddedStyleMapIsReferencedInRelationships()
		{
			var archive = NormalDocx();
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			XmlNodeAssert.AreEqual(EXPECTED_RELATIONSHIPS_XML, ReadRelationships(archive));
		}

		[TestMethod()]
		public void EmbeddedStyleMapHasOverrideContentTypeInContentTypesXml()
		{
			var archive = NormalDocx();
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			XmlNodeAssert.AreEqual(EXPECTED_CONTENT_TYPES_XML, ReadContentTypes(archive));
		}

		[TestMethod()]
		public void OverwriteExistingStyleMap()
		{
			var archive = NormalDocx();
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			EmbeddedStyleMap.EmbedStyleMap(archive, "p => h1");
			Assert.AreEqual("p => h1", EmbeddedStyleMap.ReadStyleMap(archive));
			XmlNodeAssert.AreEqual(EXPECTED_RELATIONSHIPS_XML, ReadRelationships(archive));
			XmlNodeAssert.AreEqual(EXPECTED_CONTENT_TYPES_XML, ReadContentTypes(archive));
		}

		XmlNode ReadRelationships(ZipArchive archive)
		{
			return new XmlParser(EmbeddedStyleMap.RELATIONSHIPS_NAMESPACES)
				.ParseString(ReadString(archive, "word/_rels/document.xml.rels"));
		}

		XmlNode ReadContentTypes(ZipArchive archive)
		{
			return new XmlParser(EmbeddedStyleMap.CONTENT_TYPES_NAMESPACES)
				.ParseString(ReadString(archive, "[Content_Types].xml"));
		}

		static ZipArchive NormalDocx()
		{
			return InMemoryArchive.FromStrings(new Dictionary<string, string>
			{
				["placeholder"] = "placeholder text",
				["word/_rels/document.xml.rels"] = ORIGINAL_RELATIONSHIPS_XML,
				["[Content_Types].xml"] = ORIGINAL_CONTENT_TYPES_XML
			}, ZipArchiveMode.Update);
		}

		string ReadString(ZipArchive archive, string path)
		{
			var entry = archive.GetEntry(path);
			if (entry == null) return null;
			var reader = new StreamReader(entry.Open(), Encoding.UTF8);
			return reader.ReadToEnd();
		}
	}
}