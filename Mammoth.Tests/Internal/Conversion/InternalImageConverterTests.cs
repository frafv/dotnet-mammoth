using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Html;
using Mammoth.Internal.Html.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Html.Html;

namespace Mammoth.Internal.Conversion.Tests
{
	[TestClass()]
	public class InternalImageConverterTests
	{
		[TestMethod()]
		public void WhenElementDoesNotHaveAltTextThenAltAttributeIsNotSet()
		{
			var internalImage = new Image(
				null,
				"image/jpeg",
				new MemoryStream(new byte[] { 97, 98, 99 }));

			var imageConverter = InternalImageConverter.ImgElement(image =>
				new Dictionary<string, string> { ["src"] = "<src>" });
			var result = imageConverter.Convert(internalImage);

			HtmlNodeAssert.AreEqual(
				Element("img", new HtmlAttributes { ["src"] = "<src>" }),
				result.FirstOrDefault());
		}

		[TestMethod()]
		public void WhenElementHasAltTextThenAltAttributeIsSet()
		{
			var internalImage = new Image(
				"<alt>",
				"image/jpeg",
				new MemoryStream(new byte[] { 97, 98, 99 }));

			var imageConverter = InternalImageConverter.ImgElement(image =>
				new Dictionary<string, string> { ["src"] = "<src>" });
			var result = imageConverter.Convert(internalImage);

			HtmlNodeAssert.AreEqual(
				Element("img", new HtmlAttributes { ["alt"] = "<alt>", ["src"] = "<src>" }),
				result.FirstOrDefault());
		}

		[TestMethod()]
		public void ImageAltTextCanBeOverriddenByAltAttributeReturnedFromFunction()
		{
			var internalImage = new Image(
				"<alt>",
				"image/jpeg",
				new MemoryStream(new byte[] { 97, 98, 99 }));

			var imageConverter = InternalImageConverter.ImgElement(image =>
				new Dictionary<string, string> { ["alt"] = "<alt override>", ["src"] = "<src>" });
			var result = imageConverter.Convert(internalImage);

			HtmlNodeAssert.AreEqual(
				Element("img", new HtmlAttributes { ["alt"] = "<alt override>", ["src"] = "<src>" }),
				result.FirstOrDefault());
		}
	}
}