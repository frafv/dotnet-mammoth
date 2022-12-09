using System;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class ParagraphMatcher : DocumentElementMatcher<Paragraph>
	{
		internal readonly static ParagraphMatcher ANY = new ParagraphMatcher();

		internal ParagraphMatcher(string styleId = null, StringMatcher styleName = null, NumberingLevel numbering = null)
		{
			this.StyleId = styleId;
			this.StyleName = styleName;
			this.Numbering = numbering;
		}

		internal string StyleId { get; }
		internal StringMatcher StyleName { get; }
		internal NumberingLevel Numbering { get; }

		public static ParagraphMatcher ByStyleId(string styleId)
		{
			return new ParagraphMatcher(styleId: styleId);
		}
		public static ParagraphMatcher ByStyleName(string styleName)
		{
			return ByStyleName(new EqualToStringMatcher(styleName));
		}
		public static ParagraphMatcher ByStyleName(StringMatcher styleName)
		{
			return new ParagraphMatcher(styleName: styleName);
		}
		public static ParagraphMatcher OrderedList(string level)
		{
			return new ParagraphMatcher(numbering: NumberingLevel.Ordered(level));
		}
		public static ParagraphMatcher UnorderedList(string level)
		{
			return new ParagraphMatcher(numbering: NumberingLevel.Unordered(level));
		}
		public override bool Matches(Paragraph paragraph)
		{
			return MatchesStyle(paragraph) && MatchesNumbering(paragraph);
		}
		public bool MatchesStyle(Paragraph paragraph)
		{
			return DocumentElementMatching.MatchesStyle(StyleId, StyleName, paragraph.Style);
		}
		public bool MatchesNumbering(Paragraph paragraph)
		{
			return DocumentElementMatching.Matches(Numbering, paragraph.Numbering,
				(required, actual) =>
					required.IsOrdered == actual.IsOrdered &&
					String.Equals(required.LevelIndex, actual.LevelIndex, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}

