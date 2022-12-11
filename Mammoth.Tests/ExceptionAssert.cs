using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Tests
{
	class ExceptionAssert
	{
		public static void ThrowMessage(string message, Action assert)
		{
			try
			{
				assert();
			}
			catch (Exception ex)
			{
				Assert.AreEqual(message, ex.Message);
				throw;
			}
		}
	}
}
