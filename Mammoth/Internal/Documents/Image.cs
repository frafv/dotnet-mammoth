using System.IO;

namespace Mammoth.Internal.Documents
{
	internal class Image : DocumentElement
	{
		internal Image(string altText, string contentType, Stream stream)
		{
			AltText = altText;
			ContentType = contentType;
			Stream = stream;
		}
		public string AltText { get; }

		public string ContentType { get; }

		public Stream Stream { get; }

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}

