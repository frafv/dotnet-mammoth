using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Documents
{
	internal class Notes
	{
		internal readonly static Notes EMPTY = new Notes();
		internal IDictionary<Note.NoteType, IDictionary<string, Note>> notes;
		internal Notes(IEnumerable<Note> notes)
		{
			this.notes = notes.GroupBy(note => note.Type).ToDictionary(g => g.Key, g => (IDictionary<string, Note>)g.ToDictionary(n => n.Id));
		}
		private Notes()
			: this(Enumerable.Empty<Note>())
		{ }
		public Note FindNote(Note.NoteType noteType, string noteId)
		{
			return notes.TryGetValue(noteType, out var notesOfType) && notesOfType.TryGetValue(noteId, out var note) ? note : null;
		}
	}
}

