namespace Mammoth.Internal.Documents
{
	internal class Break : DocumentElement
	{
		internal enum BreakType
		{
			LINE, PAGE, COLUMN
		}

		internal readonly static Break LINE_BREAK = new Break(BreakType.LINE);
		internal readonly static Break PAGE_BREAK = new Break(BreakType.PAGE);
		internal readonly static Break COLUMN_BREAK = new Break(BreakType.COLUMN);

		Break(BreakType type)
		{
			Type = type;
		}
		public BreakType Type { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

