using System;
using System.Collections.Generic;
using System.IO;
using Mammoth.Internal;
using Mammoth.Internal.Conversion;

namespace Mammoth
{
	public class DocumentConverter
	{
		private readonly DocumentToHtmlOptions options;

		public DocumentConverter()
			: this(DocumentToHtmlOptions.DEFAULT)
		{
		}

		private DocumentConverter(DocumentToHtmlOptions options)
		{
			this.options = options;
		}

		/// <summary>
		/// A string to prepend to any generated IDs,
		/// such as those used by bookmarks, footnotes and endnotes.
		/// Defaults to the empty string.
		/// </summary>
		/// <param name="idPrefix"></param>
		public DocumentConverter IdPrefix(string idPrefix)
		{
			if (idPrefix == null) throw new ArgumentNullException(nameof(idPrefix));
			return new DocumentConverter(options.AddIdPrefix(idPrefix));
		}

		/// <summary>
		/// By default, empty paragraphs are ignored.
		/// Call this to preserve empty paragraphs in the output.
		/// </summary>
		public DocumentConverter PreserveEmptyParagraphs()
		{
			return new DocumentConverter(options.AddPreserveEmptyParagraphs());
		}

		/// <summary>
		/// Add a style map to specify the mapping of Word styles to HTML.
		/// </summary>
		/// <param name="styleMap"></param>
		/// <remarks>
		/// The most recently added style map has the greatest precedence.
		/// </remarks>
		public DocumentConverter AddStyleMap(string styleMap)
		{
			if (styleMap == null) throw new ArgumentNullException(nameof(styleMap));
			return new DocumentConverter(options.AddStyleMap(styleMap));
		}

		/// <summary>
		/// By default, any added style maps are combined with the default style map.
		/// Call this to stop using the default style map altogether.
		/// </summary>
		public DocumentConverter DisableDefaultStyleMap()
		{
			return new DocumentConverter(options.AddDisableDefaultStyleMap());
		}

		/// <summary>
		/// By default, if the document contains an embedded style map, then it is combined with the default style map.
		/// Call this to ignore any embedded style maps.
		/// </summary>
		public DocumentConverter DisableEmbeddedStyleMap()
		{
			return new DocumentConverter(options.AddDisableEmbeddedStyleMap());
		}

		/// <summary>
		/// By default, images are converted to <c>&lt;img&gt;</c> elements with the source included inline in the <c>src</c> attribute.
		/// Call this to change how images are converted.
		/// </summary>
		/// <param name="imageConverter"></param>
		public DocumentConverter ImageConverter(Func<IImage, IDictionary<string, string>> imageConverter)
		{
			if (imageConverter == null) throw new ArgumentNullException(nameof(imageConverter));
			var convert = InternalImageConverter.ImgElement(image => imageConverter(image));
			return new DocumentConverter(options.AddImageConverter(convert));
		}

		/// <summary>
		/// Converts <paramref name="stream"/> into an HTML string.
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		/// <remarks>
		/// Note that using this method instead of <see cref="ConvertToHtml(string)"/>
		/// means that relative paths to other files, such as images, cannot be resolved.
		/// </remarks>
		public IResult<string> ConvertToHtml(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			return new InternalDocumentConverter(options)
				.ConvertToHtml(stream)
				.ToResult();
		}

		/// <summary>
		/// Converts the file at <paramref name="path"/> into an HTML string.
		/// </summary>
		/// <param name="path"></param>
		public IResult<string> ConvertToHtml(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			return new InternalDocumentConverter(options)
				.ConvertToHtml(path)
				.ToResult();
		}

		/// <summary>
		/// Extract the raw text of the document.
		/// </summary>
		/// <param name="stream"></param>
		/// <remarks>
		/// This will ignore all formatting in the document.
		/// Each paragraph is followed by two newlines.
		/// </remarks>
		public IResult<string> ExtractRawText(Stream stream)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			return new InternalDocumentConverter(options)
				.ExtractRawText(stream)
				.ToResult();
		}

		/// <summary>
		/// Extract the raw text of the document.
		/// </summary>
		/// <param name="path"></param>
		/// <remarks>
		/// This will ignore all formatting in the document.
		/// Each paragraph is followed by two newlines.
		/// </remarks>
		public IResult<string> ExtractRawText(string path)
		{
			if (path == null) throw new ArgumentNullException(nameof(path));
			return new InternalDocumentConverter(options)
				.ExtractRawText(path)
				.ToResult();
		}
	}
}
