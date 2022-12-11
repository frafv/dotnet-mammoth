using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Html.Tests
{
	static class HtmlNodeAssert
	{
		public static void AreEqual(IList<HtmlNode> list, params HtmlNode[] expected)
		{
			Assert.IsNotNull(list);
			SequenceEqual(expected, list);
		}

		public static void AreEqual(HtmlTag expected, HtmlTag actual)
		{
			if (!AreObjects(expected, actual)) return;
			CollectionAssert.AreEqual(expected.TagNames.ToList(), actual.TagNames.ToList(),
				$"Expected tag names [{String.Join(",", expected.TagNames)}] actual [{String.Join(",", actual.TagNames)}]");
			Assert.IsTrue(expected.Attributes.Equals(actual.Attributes), "Should equal attributes");
			Assert.AreEqual(expected.IsCollapsible, actual.IsCollapsible, "Should equal collapsible");
			Assert.AreEqual(expected.Separator, actual.Separator, "Should equal separator");
		}

		public static void AreEqual(HtmlElement expected, HtmlElement actual)
		{
			if (!AreObjects(expected, actual)) return;
			AreEqual(expected.Tag, actual.Tag);
			SequenceEqual(expected.Children, actual.Children);
		}

		public static void AreEqual(HtmlNode expected, HtmlNode actual)
		{
			if (!AreObjects(expected, actual)) return;
			switch (expected)
			{
				case HtmlElement element:
					AreEqual(element, actual as HtmlElement);
					break;
				case HtmlTextNode text:
					var actualText = actual as HtmlTextNode;
					Assert.AreEqual(text.Value, actualText.Value);
					break;
				case HtmlForceWrite _:
					break;
				default:
					Assert.Fail("Unknown HTML node " + actual.GetType());
					break;
			}
		}

		static bool AreObjects(object expected, object actual)
		{
			if (expected == null)
			{
				Assert.IsNull(actual);
				return false;
			}
			else
			{
				Assert.IsNotNull(actual);
				return true;
			}
		}

		public static void SequenceEqual(IList<HtmlNode> expected, IList<HtmlNode> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count, "Should equal nodes count");
			for (int k = 0; k < expected.Count; k++)
			{
				var expectedChild = expected[k];
				var actualChild = actual[k];
				Assert.AreEqual(expectedChild.GetType(), actualChild.GetType());
				AreEqual(expectedChild, actualChild);
			}
		}
	}
}
