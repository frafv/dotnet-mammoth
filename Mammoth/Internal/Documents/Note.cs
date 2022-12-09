using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Note
	{
		internal enum NoteType
		{
			FOOTNOTE, ENDNOTE
		}

		internal Note(NoteType noteType, string id, IEnumerable<DocumentElement> body)
		{
			Type = noteType;
			Id = id;
			Body = body;
		}
		public NoteType Type { get; }

		public string Id { get; }

		public IEnumerable<DocumentElement> Body { get; }
	}
}

