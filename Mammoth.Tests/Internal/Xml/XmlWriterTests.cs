using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Xml.Tests
{
	[TestClass()]
	public class XmlWriterTests
	{
		const string XmlDecl = "<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"yes\"?>";
		static readonly NamespacePrefixes EmptyNS = NamespacePrefixes.Builder().Build();

		void AssertWrite(string expected, XmlElement element, NamespacePrefixes namespaces)
		{
			string xml = XmlWriter.ToString(element, namespaces);
			Assert.AreEqual(expected, xml);
		}

		[TestMethod()]
		[DataRow("body", XmlDecl + "<body />")]
		[DataRow("values", XmlDecl + "<values />")]
		public void WriteSelfClosingElement(string tag, string xml)
		{
			var element = Element(tag);
			Assert.IsNotNull(element);
			AssertWrite(xml, element, EmptyNS);
		}

		[TestMethod()]
		[DataRow("body", "name", "bob", XmlDecl + "<body name=\"bob\" />")]
		[DataRow("values", "key", "value", XmlDecl + "<values key=\"value\" />")]
		public void WriteAttributesOfTag(string tag, string attr, string value, string xml)
		{
			var element = Element(tag);
			Assert.IsNotNull(element);
			element.Attributes.Add(attr, value);
			AssertWrite(xml, element, EmptyNS);
		}

		[TestMethod()]
		public void WriteElementWithDescendantElements()
		{
			var root = Element("body");
			var child1 = Element("a");
			var child2 = Element("b");
			var child3 = Element("c");
			root.Children.Add(child1);
			child1.Children.Add(child2);
			root.Children.Add(child3);
			AssertWrite(XmlDecl + "<body><a><b /></a><c /></body>",
				root, EmptyNS);
		}

		[TestMethod()]
		public void WriteTextNodes()
		{
			var element = Element("body");
			element.Children.Add(Text("Hello!"));
			AssertWrite(XmlDecl + "<body>Hello!</body>",
				element, EmptyNS);
		}

		[TestMethod()]
		public void WriteUnmappedNamespaceUrisInElementNames()
		{
			var element = Element("w:body");
			AssertWrite(XmlDecl + "<w:body xmlns:w=\"urn:word\" />",
				element, NamespacePrefixes
					.Builder()
					.Put("w", "urn:word")
					.Build());
		}

		[TestMethod()]
		public void WriteUnmappedNamespaceUrisInAttributeNames()
		{
			var element = Element("body");
			element.Attributes["w:name"] = "bob";
			AssertWrite(XmlDecl + "<body xmlns:w=\"urn:word\" xmlns=\"urn:none\" w:name=\"bob\" />",
				element, NamespacePrefixes
					.Builder()
					.Put("w", "urn:word")
					.DefaultPrefix("urn:none")
					.Build());
		}
	}
}