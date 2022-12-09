using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Run : DocumentElement, IHasChildren
	{
		internal enum RunVerticalAlignment
		{
			SUPERSCRIPT, SUBSCRIPT, BASELINE
		}

		internal Run(
			bool isBold,
			bool isItalic,
			bool isUnderline,
			bool isStrikethrough,
			bool isAllCaps,
			bool isSmallCaps,
			RunVerticalAlignment verticalAlignment,
			Style style,
			IEnumerable<DocumentElement> children)
		{
			IsBold = isBold;
			IsItalic = isItalic;
			IsUnderline = isUnderline;
			IsStrikethrough = isStrikethrough;
			IsAllCaps = isAllCaps;
			IsSmallCaps = isSmallCaps;
			VerticalAlignment = verticalAlignment;
			Style = style;
			Children = children;
		}
		public bool IsBold { get; }

		public bool IsItalic { get; }

		public bool IsUnderline { get; }

		public bool IsStrikethrough { get; }

		public bool IsAllCaps { get; }

		public bool IsSmallCaps { get; }

		public RunVerticalAlignment VerticalAlignment { get; }

		public Style Style { get; }

		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

