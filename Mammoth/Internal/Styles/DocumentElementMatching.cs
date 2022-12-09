using System;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal static class DocumentElementMatching
	{
		public static bool MatchesStyle(string styleId, StringMatcher styleName, Style style)
		{
			return MatchesStyleId(styleId, style) && MatchesStyleName(styleName, style);
		}
		static bool MatchesStyleId(string styleId, Style style)
		{
			return Matches(styleId, style?.StyleId, (required, actual) =>
				String.Equals(required, actual, StringComparison.Ordinal));
		}
		static bool MatchesStyleName(StringMatcher styleName, Style style)
		{
			return Matches(styleName, style?.Name, (required, actual) => required.Matches(actual));
		}
		public static bool Matches<T, U>(T required, U actual, Func<T, U, bool> areEqual)
			where T : class
			where U : class
		{
			return required == null || actual != null && areEqual(required, actual);
		}
	}
}

