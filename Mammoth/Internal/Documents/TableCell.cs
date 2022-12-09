using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class TableCell : DocumentElement, IHasChildren
	{
		internal TableCell(int rowspan, int colspan, IEnumerable<DocumentElement> children)
		{
			Rowspan = rowspan;
			Children = children;
			Colspan = colspan;
		}
		public int Colspan { get; }

		public int Rowspan { get; }

		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

