using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal abstract class DocumentElementMatcher<TElement>
		where TElement : DocumentElement
	{
		public abstract bool Matches(TElement element);
	}
}

