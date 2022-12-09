using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Xml;
using NoteType = Mammoth.Internal.Documents.Note.NoteType;

namespace Mammoth.Internal.Docx
{
	internal class NotesXmlReader
	{
		readonly BodyXmlReader bodyReader;
		readonly string tagName;
		readonly NoteType noteType;
		NotesXmlReader(BodyXmlReader bodyReader, string tagName, NoteType noteType)
		{
			this.bodyReader = bodyReader;
			this.tagName = tagName;
			this.noteType = noteType;
		}
		public static NotesXmlReader Footnote(BodyXmlReader bodyReader)
		{
			return new NotesXmlReader(bodyReader, "footnote", NoteType.FOOTNOTE);
		}
		public static NotesXmlReader Endnote(BodyXmlReader bodyReader)
		{
			return new NotesXmlReader(bodyReader, "endnote", NoteType.ENDNOTE);
		}
		public InternalResult<Note[]> ReadElement(XmlElement element)
		{
			var elements = element.FindChildren("w:" + this.tagName).Where(elem => IsNoteElement(elem));
			return InternalResult.Join(elements.Select(elem => ReadNoteElement(elem)));
		}
		public bool IsNoteElement(XmlElement element)
		{
#pragma warning disable IDE0075 // Simplify conditional expression
			return element.GetAttributeOrNone("w:type") is string type ? !IsSeparatorType(type) : true;
#pragma warning restore IDE0075 // Simplify conditional expression
		}
		public bool IsSeparatorType(string type)
		{
			return type == "continuationSeparator" || type == "separator";
		}
		public InternalResult<Note> ReadNoteElement(XmlElement element)
		{
			return bodyReader.ReadElements(element.Children)
				.ToResult()
				.Map(children => new Note(noteType, element.GetAttribute("w:id"), children));
		}
	}
}

