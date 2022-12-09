using System.IO;

namespace Mammoth
{
	public interface IImage
	{
		/// <summary>
		/// The alt text of the image, if any.
		/// </summary>
		string AltText { get; }
		/// <summary>
		/// The content type of the image, such as <c>image/png</c>.
		/// </summary>
		string ContentType { get; }
		/// <summary>
		/// Open the image file.
		/// </summary>
		Stream Open();
	}
}

