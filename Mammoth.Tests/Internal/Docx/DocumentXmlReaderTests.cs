using Mammoth.Internal.Documents;
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
	public class DocumentXmlReaderTests
	{
		[TestMethod()]
		public void ReadTextWithinDocument()
		{
			var documentElement = Element("w:document",
				Element("w:body",
					Element("w:p",
						Element("w:r",
							Element("w:t",
								XmlNodes.Text("Hello!"))))));

			var reader = new DocumentXmlReader(BodyReader(), Notes.EMPTY, new Comment[0]);

			var result = reader.ReadElement(documentElement);
			InternalResultAssert.IsResult(result);
			DocumentElementAssert.AreEqual(
				Document(
					WithChildren(ParagraphWithText("Hello!"))),
				result.Value);
		}

		[TestMethod()]
		public void NotesOfDocumentAreIncludedInDocument()
		{
			var note = new Note(Note.NoteType.FOOTNOTE, "4", new[] { ParagraphWithText("Hello") });
			var notes = new Notes(new[] { note });
			var reader = new DocumentXmlReader(BodyReader(), notes, new Comment[0]);

			var documentElement = Element("w:document", Element("w:body"));
			var document = reader.ReadElement(documentElement);

			DocumentElementAssert.AreEqual(note,
				document.Value.Notes.FindNote(Note.NoteType.FOOTNOTE, "4"));
		}
	}
}