using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Xml.Parsing.Tests
{
	static class XmlNodeAssert
	{
		public static void IsTextNode(XmlNode node, string text)
		{
			Assert.IsNotNull(node);
			Assert.IsInstanceOfType(node, typeof(XmlTextNode), "Should be text node");
			var textNode = (XmlTextNode)node;
			Assert.AreEqual(text, textNode.Value, "Should equal text value");
		}

		public static void IsElement(XmlNode node, string name, params Action<XmlNode>[] children) =>
			IsElement(node, name, new XmlAttributes(), children);
		public static void IsElement(XmlNode node, string name, XmlAttributes attributes, params Action<XmlNode>[] children)
		{
			Assert.IsNotNull(node);
			Assert.IsInstanceOfType(node, typeof(XmlElement), "Should be element");
			var elem = (XmlElement)node;
			Assert.AreEqual(name, elem.Name, "Should equal name");
			Assert.IsTrue(attributes.Count <= elem.Attributes.Count,
				$"Expected at least {attributes.Count} attributes actual is {elem.Attributes.Count}");
			foreach (var attr in attributes)
			{
				Assert.IsTrue(elem.Attributes.ContainsKey(attr.Key),
					$"Expected attributes {String.Join(", ", attributes.Keys)} actual attributes {String.Join(", ", elem.Attributes.Keys)}");
				Assert.AreEqual(attr.Value, elem.Attributes[attr.Key], "Should equal attribute value");
			}
			Assert.AreEqual(children.Length, elem.Children.Count, "Should equal children count");
			for (int k = 0; k < children.Length; k++)
			{
				children[k](elem.Children[k]);
			}
		}

		public static void AreEqual(XmlNode expected, XmlNode actual)
		{
			Assert.IsNotNull(actual);
			Assert.AreEqual(expected.GetType(), actual.GetType(), "Should be " + expected.GetType().Name);
			switch (expected)
			{
				case XmlElement element:
					var actualElement = actual as XmlElement;
					Assert.AreEqual(element.Name, actualElement.Name, "Should equal name");
					foreach (var attr in element.Attributes)
					{
						Assert.IsTrue(actualElement.Attributes.ContainsKey(attr.Key),
							$"Expected attributes {String.Join(", ", element.Attributes.Keys)} actual attributes {String.Join(", ", actualElement.Attributes.Keys)}");
						Assert.AreEqual(attr.Value, actualElement.Attributes[attr.Key], "Should equal attribute value");
					}
					SequenceEqual(element.Children, actualElement.Children);
					break;
				case XmlTextNode text:
					var actualText = actual as XmlTextNode;
					Assert.AreEqual(text.Value, actualText.Value, "Should equal text value");
					break;
				default:
					Assert.Fail("Unknown XML node " + actual.GetType().Name);
					break;
			}
		}

		public static void SequenceEqual(IList<XmlNode> expected, IList<XmlNode> actual)
		{
			Assert.AreEqual(expected.Count, actual.Count, "Should equal children count");
			for (int k = 0; k < expected.Count; k++)
			{
				var expectedChild = expected[k];
				var actualChild = actual[k];
				AreEqual(expectedChild, actualChild);
			}
		}
	}
}
