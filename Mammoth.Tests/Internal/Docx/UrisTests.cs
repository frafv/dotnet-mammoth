using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Docx.Uris;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class UrisTests
	{
		[TestMethod()]
		public void WhenPathDoesNotHaveLeadingSlashThenPathIsResolvedRelativeToBase()
		{
			Assert.AreEqual("one/two/three/four",
				UriToZipEntryName("one/two", "three/four"));
		}

		[TestMethod()]
		public void WhenPathHasLeadingSlashThenBaseIsIgnored()
		{
			Assert.AreEqual("three/four",
				UriToZipEntryName("one/two", "/three/four"));
		}
	}
}