using System;
using Mammoth.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Xml.Tests
{
	[TestClass()]
	public class XmlElementTests
	{
		[TestMethod()]
		[ExpectedException(typeof(Exception))]
		public void TryingToGetNonExistentAttributeThrowsException()
		{
			var element = XmlNodes.Element("p");
			Assert.IsNotNull(element);
			ExceptionAssert.ThrowMessage("Element has no 'class' attribute",
				() => element.GetAttribute("class"));
		}
	}
}