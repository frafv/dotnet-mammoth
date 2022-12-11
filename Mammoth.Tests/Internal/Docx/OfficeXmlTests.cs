using System.IO;
using System.Text;
using Mammoth.Internal.Xml.Parsing.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class OfficeXmlTests
	{
		[TestMethod()]
		public void AlternateContentIsReplacedByContentsOfFallback()
		{
			string xmlString =
				"<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>" +
				"<numbering xmlns:mc=\"http://schemas.openxmlformats.org/markup-compatibility/2006\">" +
				"<mc:AlternateContent>" +
				"<mc:Choice Requires=\"w14\">" +
				"<choice/>" +
				"</mc:Choice>" +
				"<mc:Fallback>" +
				"<fallback/>" +
				"</mc:Fallback>" +
				"</mc:AlternateContent>" +
				"</numbering>";

			var result = OfficeXml.ParseXml(new MemoryStream(Encoding.UTF8.GetBytes(xmlString)));
			XmlNodeAssert.SequenceEqual(new[] { Element("fallback") }, result.Children);
		}
	}
}