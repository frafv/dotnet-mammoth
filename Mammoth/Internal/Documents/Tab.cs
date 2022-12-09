namespace Mammoth.Internal.Documents
{
	internal class Tab : DocumentElement
	{
		internal static Tab TAB = new Tab();
		Tab() { }
		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

