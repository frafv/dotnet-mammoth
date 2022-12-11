using System;
using Mammoth.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class RelationshipsTests
	{
		[TestMethod()]
		[ExpectedException(typeof(Exception))]
		public void ExceptionIsThrownIfRelationshipCannotBeFound()
		{
			var relationships = Relationships.EMPTY;
			ExceptionAssert.ThrowMessage("Could not find relationship 'rId5'",
				() => relationships.FindTargetByRelationshipId("rId5"));
		}
	}
}