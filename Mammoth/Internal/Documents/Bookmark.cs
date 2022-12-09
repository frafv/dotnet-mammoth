namespace Mammoth.Internal.Documents
{
	internal class Bookmark : DocumentElement
	{
		internal Bookmark(string name)
		{
			Name = name;
		}
		public string Name { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

