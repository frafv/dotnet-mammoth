namespace Mammoth.Internal.Documents
{
	internal abstract class DocumentElement
	{
		public abstract void Accept(IVisitor visitor);

		internal interface IVisitor
		{
			void Visit(Paragraph paragraph);
			void Visit(Run run);
			void Visit(Text text);
			void Visit(Tab tab);
			void Visit(Break lineBreak);
			void Visit(Table table);
			void Visit(TableRow tableRow);
			void Visit(TableCell tableCell);
			void Visit(Hyperlink hyperlink);
			void Visit(Bookmark bookmark);
			void Visit(NoteReference noteReference);
			void Visit(CommentReference commentReference);
			void Visit(Image image);
		}
	}
}

