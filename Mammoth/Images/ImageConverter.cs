using System.Collections.Generic;

namespace Mammoth.Images
{
	internal static class ImageConverter
	{
		internal delegate IDictionary<string, string> ImgElementConvert(IImage image);
	}
}

