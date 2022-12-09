using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Document : IHasChildren
	{
		internal Document(IEnumerable<DocumentElement> children, Notes notes, IEnumerable<Comment> comments)
		{
			Children = children;
			Notes = notes;
			Comments = comments;
		}
		public IEnumerable<DocumentElement> Children { get; }

		public Notes Notes { get; }

		public IEnumerable<Comment> Comments { get; }
	}
}

