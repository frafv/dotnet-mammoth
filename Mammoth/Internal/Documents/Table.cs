using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Table : DocumentElement, IHasChildren
	{
		internal Table(Style style, IEnumerable<DocumentElement> children)
		{
			Style = style;
			Children = children;
		}
		public Style Style { get; }

		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

