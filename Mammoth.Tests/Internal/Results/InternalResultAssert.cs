using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Results.Tests
{
	static class InternalResultAssert
	{
		public static void IsResult<T>(InternalResult<T> actual, params string[] warnings)
			where T : class
		{
			CollectionAssert.AreEqual(warnings, actual.Warnings);
		}
	}
}