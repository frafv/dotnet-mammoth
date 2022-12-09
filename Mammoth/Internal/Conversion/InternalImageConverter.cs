using System.Collections.Generic;
using System.IO;
using Mammoth.Images;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Html;
using static Mammoth.Internal.Html.Html;

namespace Mammoth.Internal.Conversion
{
	internal class InternalImageConverter
	{
		class InternalImage : IImage
		{
			readonly Image internalImage;

			public InternalImage(Image internalImage, string contentType)
			{
				this.internalImage = internalImage;
				ContentType = contentType;
			}

			public string AltText => internalImage.AltText;

			public string ContentType { get; }

			public Stream Open()
			{
				return internalImage.Stream;
			}
		}

		readonly ImageConverter.ImgElementConvert imgElement;

		public static InternalImageConverter ImgElement(ImageConverter.ImgElementConvert imgElement)
		{
			return new InternalImageConverter(imgElement);
		}

		InternalImageConverter(ImageConverter.ImgElementConvert imgElement)
		{
			this.imgElement = imgElement;
		}

		public IEnumerable<HtmlNode> Convert(Image internalImage)
		{
			if (!(internalImage.ContentType is string contentType))
				yield break;

			var image = new InternalImage(internalImage, contentType);
			var attributes = new Dictionary<string, string>(imgElement(image));
			if (internalImage.AltText is string altText && !attributes.ContainsKey("alt"))
				attributes.Add("alt", altText);
			yield return Element("img", attributes);
		}
	}
}
