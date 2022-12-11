using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Documents.Tests
{
	[TestClass()]
	public class StyleTests
	{
		[TestMethod()]
		public void DescriptionOfStyleIncludesBothIdAndNameIfPresent()
		{
			var style = new Style("Heading1", "Heading 1");
			Assert.AreEqual("Heading 1 (Style ID: Heading1)", style.Describe());
		}

		[TestMethod()]
		public void DescriptionOfStyleIsJustStyleIdIfStyleNameIsMissing()
		{
			var style = new Style("Heading1");
			Assert.AreEqual("Style ID: Heading1", style.Describe());
		}
	}
}