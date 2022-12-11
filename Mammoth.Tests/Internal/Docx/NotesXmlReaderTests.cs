using Mammoth.Internal.Documents;
using Mammoth.Internal.Documents.Tests;
using Mammoth.Internal.Results.Tests;
using Mammoth.Internal.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Note;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;
using static Mammoth.Internal.Docx.Tests.BodyXmlReaderMakers;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class NotesXmlReaderTests
	{
		static readonly BodyXmlReader BODY_READER = BodyReader();

		[TestMethod()]
		public void IdAndBodyOfFootnoteAreRead()
		{
			var element = Element("w:footnotes",
				Element("w:footnote", new XmlAttributes { ["w:id"] = "1" },
					Element("w:p")));

			var reader = NotesXmlReader.Footnote(BODY_READER);
			var notes = reader.ReadElement(element);

			InternalResultAssert.IsResult(notes);
			Assert.AreEqual(1, notes.Value.Length);
			DocumentElementAssert.AreEqual(
				new Note(NoteType.FOOTNOTE, "1", new[] { Paragraph() }),
				notes.Value[0]);
		}

		[TestMethod()]
		public void ContinuationSeparatorIsIgnored()
		{
			AssertFootnoteTypeIsIgnored("continuationSeparator");
		}

		[TestMethod()]
		public void SeparatorIsIgnored()
		{
			AssertFootnoteTypeIsIgnored("separator");
		}

		private void AssertFootnoteTypeIsIgnored(string noteType)
		{
			var element = Element("w:footnotes",
				Element("w:footnote", new XmlAttributes { ["w:id"] = "1", ["w:type"] = noteType },
					Element("w:p")));

			var reader = NotesXmlReader.Footnote(BODY_READER);
			var notes = reader.ReadElement(element);

			InternalResultAssert.IsResult(notes);
			Assert.AreEqual(0, notes.Value.Length);
		}
	}
}