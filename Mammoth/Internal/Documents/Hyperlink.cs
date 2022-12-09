using System.Collections.Generic;

namespace Mammoth.Internal.Documents
{
	internal class Hyperlink : DocumentElement, IHasChildren
	{
		internal Hyperlink(string href, string anchor, string targetFrame, IEnumerable<DocumentElement> children)
		{
			Href = href;
			Anchor = anchor;
			TargetFrame = targetFrame;
			Children = children;
		}
		public static Hyperlink CreateHref(string href, IEnumerable<DocumentElement> children) =>
			CreateHref(href, null, children);
		public static Hyperlink CreateHref(string href, string targetFrame, IEnumerable<DocumentElement> children)
		{
			return new Hyperlink(href, null, targetFrame, children);
		}
		public static Hyperlink CreateAnchor(string anchor, IEnumerable<DocumentElement> children) =>
			CreateAnchor(anchor, null, children);
		public static Hyperlink CreateAnchor(string anchor, string targetFrame, IEnumerable<DocumentElement> children)
		{
			return new Hyperlink(null, anchor, targetFrame, children);
		}
		public string Href { get; }

		public string Anchor { get; }

		public string TargetFrame { get; }

		public IEnumerable<DocumentElement> Children { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

