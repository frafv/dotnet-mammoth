using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Tests
{
	[TestClass()]
	public class StartsWithStringMatcherTests
	{
		[TestMethod()]
		public void MatchesStringWithPrefix()
		{
			var matcher = new StartsWithStringMatcher("Heading");
			Assert.IsTrue(matcher.Matches("Heading"));
			Assert.IsTrue(matcher.Matches("Heading 1"));
			Assert.IsFalse(matcher.Matches("Custom Heading"));
			Assert.IsFalse(matcher.Matches("Head"));
			Assert.IsFalse(matcher.Matches("Header 2"));
		}

		[TestMethod()]
		public void IsCaseInsensitive()
		{
			var matcher = new StartsWithStringMatcher("Heading");
			Assert.IsTrue(matcher.Matches("heaDING"));
		}
	}
}