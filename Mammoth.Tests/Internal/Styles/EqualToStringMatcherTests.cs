using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Tests
{
	[TestClass()]
	public class EqualToStringMatcherTests
	{
		[TestMethod()]
		public void IsCaseInsensitive()
		{
			var matcher = new EqualToStringMatcher("Heading 1");
			Assert.IsTrue(matcher.Matches("heaDING 1"));
			Assert.IsFalse(matcher.Matches("heaDING 2"));
		}
	}
}