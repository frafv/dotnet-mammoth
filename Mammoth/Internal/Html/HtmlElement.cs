using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Html
{
	internal sealed class HtmlElement : HtmlNode
	{
		static readonly ISet<string> VOID_TAG_NAMES = new HashSet<string> { "img", "br", "hr" };
		internal HtmlElement(HtmlTag tag, IEnumerable<HtmlNode> children)
		{
			Tag = tag;
			Children = children.ToList();
		}
		public HtmlTag Tag { get; }

		public IEnumerable<string> TagNames => Tag.TagNames;

		public string TagName => TagNames.FirstOrDefault();

		public HtmlAttributes Attributes => Tag.Attributes;

		public IList<HtmlNode> Children { get; }

		public bool IsCollapsible => Tag.IsCollapsible;

		public string Separator => Tag.Separator;

		public bool IsVoid => !Children.Any() && IsVoidTag(TagName);

		public static bool IsVoidTag(string tagName)
		{
			return VOID_TAG_NAMES.Contains(tagName);
		}
		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

