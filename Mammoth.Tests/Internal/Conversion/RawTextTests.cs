using Mammoth.Internal.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;

namespace Mammoth.Internal.Conversion.Tests
{
	[TestClass()]
	public class RawTextTests
	{
		[TestMethod()]
		public void TextElementIsConvertedToTextContent()
		{
			var element = new Text("Hello.");

			string result = RawText.ExtractRawText(element);

			Assert.AreEqual("Hello.", result);
		}

		[TestMethod()]
		public void TabElementIsConvertedToTabCharacter()
		{
			var element = Tab.TAB;

			string result = RawText.ExtractRawText(element);

			Assert.AreEqual("\t", result);
		}

		[TestMethod()]
		public void ParagraphsAreTerminatedWithNewlines()
		{
			var element = Paragraph(
				WithChildren(new Text("Hello "), new Text("world.")));

			string result = RawText.ExtractRawText(element);

			Assert.AreEqual("Hello world.\n\n", result);
		}

		[TestMethod()]
		public void ChildrenAreRecursivelyConvertedToText()
		{
			var element = Document(
				WithChildren(
					Paragraph(
						WithChildren(new Text("Hello "), new Text("world.")))));

			string result = RawText.ExtractRawText(element);

			Assert.AreEqual("Hello world.\n\n", result);
		}

		[TestMethod()]
		public void NonTextElementWithoutChildrenIsConvertedToEmptyString()
		{
			var element = Break.LINE_BREAK;

			string result = RawText.ExtractRawText(element);

			Assert.AreEqual("", result);
		}
	}
}