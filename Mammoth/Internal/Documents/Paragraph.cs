using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Paragraph : DocumentElement, IHasChildren
	{
		internal Paragraph(Style style, NumberingLevel numbering, ParagraphIndent indent, IEnumerable<DocumentElement> children)
		{
			Style = style;
			Numbering = numbering;
			Indent = indent;
			Children = children;
		}
		public Style Style { get; }

		public NumberingLevel Numbering { get; }

		public ParagraphIndent Indent { get; }

		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

