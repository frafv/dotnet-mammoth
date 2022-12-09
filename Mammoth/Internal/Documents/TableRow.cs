using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class TableRow : DocumentElement, IHasChildren
	{
		internal TableRow(IEnumerable<DocumentElement> children, bool isHeader = false)
		{
			Children = children;
			IsHeader = isHeader;
		}
		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
		public bool IsHeader { get; }
	}
}

