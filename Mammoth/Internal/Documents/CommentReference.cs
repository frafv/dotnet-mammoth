namespace Mammoth.Internal.Documents
{
	internal class CommentReference : DocumentElement
	{
		internal CommentReference(string commentId)
		{
			CommentId = commentId;
		}
		public string CommentId { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

