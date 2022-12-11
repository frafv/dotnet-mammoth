using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Docx.ContentTypesXml;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class ContentTypesXmlTests
	{
		[TestMethod()]
		public void ContentTypeIsBasedOnDefaultForExtensionIfThereIsNoOverride()
		{
			var element = Element("content-types:Types",
				Element("content-types:Default", new XmlAttributes
				{
					["Extension"] = "png",
					["ContentType"] = "image/png"
				}));
			var contentTypes = ReadContentTypesXmlElement(element);

			Assert.AreEqual("image/png",
				contentTypes.FindContentType("word/media/hat.png"));
		}

		[TestMethod()]
		public void ContentTypeIsBasedOnOverrideIfPresent()
		{
			var element = Element("content-types:Types",
				Element("content-types:Default", new XmlAttributes
				{
					["Extension"] = "png",
					["ContentType"] = "image/png"
				}),
				Element("content-types:Override", new XmlAttributes
				{
					["PartName"] = "/word/media/hat.png",
					["ContentType"] = "image/hat"
				}));
			var contentTypes = ReadContentTypesXmlElement(element);

			Assert.AreEqual("image/hat",
				contentTypes.FindContentType("word/media/hat.png"));
		}

		[TestMethod()]
		[DataRow("image/png", "word/media/hat.png")]
		[DataRow("image/gif", "word/media/hat.gif")]
		[DataRow("image/jpeg", "word/media/hat.jpg")]
		[DataRow("image/jpeg", "word/media/hat.jpeg")]
		[DataRow("image/bmp", "word/media/hat.bmp")]
		[DataRow("image/tiff", "word/media/hat.tif")]
		[DataRow("image/tiff", "word/media/hat.tiff")]
		public void FallbackContentTypesHaveCommonImageTypes(string expected, string path)
		{
			var element = Element("content-types:Types");
			var contentTypes = ReadContentTypesXmlElement(element);
			Assert.AreEqual(expected,
				contentTypes.FindContentType(path));
		}

		[TestMethod()]
		public void FallbackContentTypesAreCaseInsensitive()
		{
			var element = Element("content-types:Types");
			var contentTypes = ReadContentTypesXmlElement(element);

			Assert.AreEqual("image/png",
				contentTypes.FindContentType("word/media/hat.PnG"));
		}
	}
}