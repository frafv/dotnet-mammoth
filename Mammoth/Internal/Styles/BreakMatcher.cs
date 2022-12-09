using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class BreakMatcher : DocumentElementMatcher<Break>
	{
		internal readonly static BreakMatcher LINE_BREAK = new BreakMatcher(Break.BreakType.LINE);
		internal readonly static BreakMatcher PAGE_BREAK = new BreakMatcher(Break.BreakType.PAGE);
		internal readonly static BreakMatcher COLUMN_BREAK = new BreakMatcher(Break.BreakType.COLUMN);

		internal BreakMatcher(Break.BreakType breakType)
		{
			this.Type = breakType;
		}

		internal Break.BreakType Type { get; }

		public override bool Matches(Break element)
		{
			return element.Type == Type;
		}
	}
}

