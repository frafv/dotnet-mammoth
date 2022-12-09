using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class TableMatcher : DocumentElementMatcher<Table>
	{
		internal static TableMatcher ANY = new TableMatcher();

		internal TableMatcher(string styleId = null, StringMatcher styleName = null)
		{
			this.StyleId = styleId;
			this.StyleName = styleName;
		}

		internal string StyleId { get; }
		internal StringMatcher StyleName { get; }

		public static TableMatcher ByStyleId(string styleId)
		{
			return new TableMatcher(styleId: styleId);
		}
		public static TableMatcher ByStyleName(string styleName)
		{
			return new TableMatcher(styleName: new EqualToStringMatcher(styleName));
		}

		public override bool Matches(Table table)
		{
			return DocumentElementMatching.MatchesStyle(StyleId, StyleName, table.Style);
		}
	}
}

