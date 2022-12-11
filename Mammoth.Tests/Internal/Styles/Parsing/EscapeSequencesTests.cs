using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	[TestClass()]
	public class EscapeSequencesTests
	{
		[TestMethod()]
		public void LineFeedsAreDecoded()
		{
			Assert.AreEqual("\n", EscapeSequences.Decode(@"\n"));
		}

		[TestMethod()]
		public void CarriageReturnsAreDecoded()
		{
			Assert.AreEqual("\r", EscapeSequences.Decode(@"\r"));
		}

		[TestMethod()]
		public void TabsAreDecoded()
		{
			Assert.AreEqual("\t", EscapeSequences.Decode(@"\t"));
		}

		[TestMethod()]
		public void BackslashesAreDecoded()
		{
			Assert.AreEqual("\\", EscapeSequences.Decode(@"\\"));
		}

		[TestMethod()]
		public void ColonsAreDecoded()
		{
			Assert.AreEqual(":", EscapeSequences.Decode(@"\:"));
		}
	}
}