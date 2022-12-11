using System.Linq;
using Mammoth.Internal.Documents.Tests;
using Mammoth.Internal.Results.Tests;
using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;
using static Mammoth.Internal.Docx.Tests.BodyXmlReaderMakers;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class CommentXmlReaderTests
	{
		[TestMethod()]
		public void IdAndBodyOfCommentIsRead()
		{
			var body = Element("w:p");
			var reader = new CommentXmlReader(BodyReader());
			var result = reader.ReadElement(Element("w:comments",
				Element("w:comment", new XmlAttributes { ["w:id"] = "1" }, body)));

			InternalResultAssert.IsResult(result);
			Assert.AreEqual(1, result.Value.Length);
			var comment = result.Value[0];
			Assert.AreEqual("1", comment.CommentId);
			Assert.AreEqual(1, comment.Body.Count());
			DocumentElementAssert.IsParagraph(comment.Body.First(), Paragraph());
		}

		[TestMethod()]
		public void WhenOptionalAttributesOfCommentAreMissingThenTheyAreReadAsNone()
		{
			var reader = new CommentXmlReader(BodyReader());
			var result = reader.ReadElement(Element("w:comments",
				Element("w:comment", new XmlAttributes { ["w:id"] = "1" })));

			InternalResultAssert.IsResult(result);
			Assert.AreEqual(1, result.Value.Length);
			var comment = result.Value[0];
			Assert.IsNull(comment.AuthorName);
			Assert.IsNull(comment.AuthorInitials);
		}

		[TestMethod()]
		public void WhenOptionalAttributesOfCommentAreBlankThenTheyAreReadAsNone()
		{
			var reader = new CommentXmlReader(BodyReader());
			var result = reader.ReadElement(Element("w:comments",
				Element("w:comment", new XmlAttributes { ["w:id"] = "1", ["w:author"] = " ", ["w:initials"] = " " })));

			InternalResultAssert.IsResult(result);
			Assert.AreEqual(1, result.Value.Length);
			var comment = result.Value[0];
			Assert.IsNull(comment.AuthorName);
			Assert.IsNull(comment.AuthorInitials);
		}

		[TestMethod()]
		public void WhenOptionalAttributesOfCommentAreNotBlankThenTheyAreRead()
		{
			var reader = new CommentXmlReader(BodyReader());
			var result = reader.ReadElement(Element("w:comments",
				Element("w:comment", new XmlAttributes { ["w:id"] = "1", ["w:author"] = "The Piemaker", ["w:initials"] = "TP" })));

			InternalResultAssert.IsResult(result);
			Assert.AreEqual(1, result.Value.Length);
			var comment = result.Value[0];
			Assert.AreEqual("The Piemaker", comment.AuthorName);
			Assert.AreEqual("TP", comment.AuthorInitials);
		}
	}
}