namespace Mammoth.Internal.Documents
{
	internal class NoteReference : DocumentElement
	{
		internal NoteReference(Note.NoteType noteType, string noteId)
		{
			NoteType = noteType;
			NoteId = noteId;
		}
		public static NoteReference FootnoteReference(string noteId)
		{
			return new NoteReference(Note.NoteType.FOOTNOTE, noteId);
		}
		public static NoteReference EndnoteReference(string noteId)
		{
			return new NoteReference(Note.NoteType.ENDNOTE, noteId);
		}
		public Note.NoteType NoteType { get; }

		public string NoteId { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

