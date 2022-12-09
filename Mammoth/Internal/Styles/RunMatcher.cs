using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class RunMatcher : DocumentElementMatcher<Run>
	{
		internal readonly static RunMatcher ANY = new RunMatcher();

		internal RunMatcher(string styleId = null, StringMatcher styleName = null)
		{
			this.StyleId = styleId;
			this.StyleName = styleName;
		}

		internal string StyleId { get; }
		internal StringMatcher StyleName { get; }

		public static RunMatcher ByStyleId(string styleId)
		{
			return new RunMatcher(styleId: styleId);
		}
		public static RunMatcher ByStyleName(string styleName)
		{
			return new RunMatcher(styleName: new EqualToStringMatcher(styleName));
		}

		public override bool Matches(Run run)
		{
			return DocumentElementMatching.MatchesStyle(StyleId, StyleName, run.Style);
		}
	}
}

