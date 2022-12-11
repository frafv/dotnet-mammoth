using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Xml.Parsing.Tests
{
	[TestClass()]
	public class XmlParserTests
	{
		readonly XmlParser parser = new XmlParser(new NamespacePrefixes(new Dictionary<string, NamespacePrefix>()));

		[TestMethod()]
		[DataRow("body", "<body/>")]
		[DataRow("values", "<values />")]
		public void ParseSelfClosingElement(string tag, string xml)
		{
			var element = parser.ParseString(xml);
			XmlNodeAssert.IsElement(element, tag);
		}

		[TestMethod()]
		[DataRow("body", "<body></body>")]
		[DataRow("values", "<values></values>")]
		public void ParseEmptyElementWithSeparateClosingTag(string tag, string xml)
		{
			var element = parser.ParseString(xml);
			XmlNodeAssert.IsElement(element, tag);
		}

		[TestMethod()]
		[DataRow("body", "name", "bob", "<body name='bob'></body>")]
		[DataRow("values", "key", "value", "<values key=\"value\" />")]
		public void ParseAttributesOfTag(string tag, string attr, string value, string xml)
		{
			var element = parser.ParseString(xml);
			XmlNodeAssert.IsElement(element, tag, new XmlAttributes { [attr] = value });
		}

		[TestMethod()]
		public void ParseElementWithDescendantElements()
		{
			var root = parser.ParseString("<body><a><b/></a><a/></body>");
			XmlNodeAssert.IsElement(root, "body",
				child => XmlNodeAssert.IsElement(child, "a",
					child2 => XmlNodeAssert.IsElement(child2, "b")),
				child => XmlNodeAssert.IsElement(child, "a"));
		}

		[TestMethod()]
		public void ParseTextNodes()
		{
			var element = parser.ParseString("<body>Hello!</body>");
			XmlNodeAssert.IsElement(element, "body",
				child => XmlNodeAssert.IsTextNode(child, "Hello!"));
		}

		[TestMethod()]
		public void UnmappedNamespaceUrisInElementNamesAreIncludedInBracesAsPrefix()
		{
			var element = parser.ParseString("<w:body xmlns:w='urn:word'/>");
			XmlNodeAssert.IsElement(element, "{urn:word}body");
		}

		[TestMethod()]
		public void UnmappedNamespaceUrisInAttributeNamesAreIncludedInBracesAsPrefix()
		{
			var element = parser.ParseString("<body xmlns:w='urn:word' w:name='bob'></body>");
			XmlNodeAssert.IsElement(element, "body", new XmlAttributes { ["{urn:word}name"] = "bob" });
		}

		[TestMethod()]
		public void MappedNamespaceUrisInElementNamesArePrefixedToLocalNameWithColon()
		{
			var parser = new XmlParser(NamespacePrefixes.Builder().Put("x", "urn:word").Build());
			var element = parser.ParseString("<w:body xmlns:w='urn:word'/>");
			XmlNodeAssert.IsElement(element, "x:body");
		}

		[TestMethod()]
		public void MappedNamespaceUrisInAttributeNamesArePrefixedToLocalNameWithColon()
		{
			var parser = new XmlParser(NamespacePrefixes.Builder().Put("x", "urn:word").Build());
			var element = parser.ParseString("<body xmlns:w='urn:word' w:name='bob'/>");
			XmlNodeAssert.IsElement(element, "body", new XmlAttributes { ["x:name"] = "bob" });
		}

		[TestMethod()]
		public void ElementsWithDefaultNamespaceHaveNoPrefix()
		{
			var parser = new XmlParser(NamespacePrefixes.Builder().DefaultPrefix("urn:word").Build());
			var element = parser.ParseString("<w:body xmlns:w='urn:word'/>");
			XmlNodeAssert.IsElement(element, "body");
		}

		[TestMethod()]
		public void ParseInputStream()
		{
			var mem = new MemoryStream();
			using (var writer = new StreamWriter(mem, Encoding.UTF8, 4096, leaveOpen: true))
				writer.Write("<body/>");
			mem.Position = 0;
			var element = parser.ParseStream(mem);
			XmlNodeAssert.IsElement(element, "body");
		}
	}
}