using Mammoth.Internal.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Tests
{
	[TestClass()]
	public class BreakMatcherTests
	{
		[TestMethod()]
		public void WhenBreakHasDifferentTypeToBreakMatcherThenMatchFails()
		{
			Assert.IsFalse(BreakMatcher.LINE_BREAK.Matches(Break.PAGE_BREAK));
			Assert.IsFalse(BreakMatcher.PAGE_BREAK.Matches(Break.LINE_BREAK));
		}

		[TestMethod()]
		public void WhenBreakHasTheSameTypeAsBreakMatcherThenMatchSucceeds()
		{
			Assert.IsTrue(BreakMatcher.LINE_BREAK.Matches(Break.LINE_BREAK));
			Assert.IsTrue(BreakMatcher.PAGE_BREAK.Matches(Break.PAGE_BREAK));
		}
	}
}