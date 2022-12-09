namespace Mammoth.Internal.Documents
{
	internal class Text : DocumentElement
	{
		internal Text(string value)
		{
			Value = value;
		}
		public string Value { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

