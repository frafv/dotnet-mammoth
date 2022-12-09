using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class StyleMapping<TElement>
		where TElement : DocumentElement
	{
		internal StyleMapping(DocumentElementMatcher<TElement> matcher, HtmlPath htmlPath)
		{
			this.Matcher = matcher;
			this.HtmlPath = htmlPath;
		}
		public bool Matches(TElement element)
		{
			return Matcher.Matches(element);
		}

		public HtmlPath HtmlPath { get; }
		internal DocumentElementMatcher<TElement> Matcher { get; }
	}
}

