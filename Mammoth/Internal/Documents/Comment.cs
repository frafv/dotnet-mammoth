using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Comment
	{
		internal Comment(string commentId, IEnumerable<DocumentElement> body, string authorName = null, string authorInitials = null)
		{
			CommentId = commentId;
			Body = body;
			AuthorName = authorName;
			AuthorInitials = authorInitials;
		}
		public string CommentId { get; }

		public IEnumerable<DocumentElement> Body { get; }

		public string AuthorInitials { get; }

		public string AuthorName { get; }
	}
}

