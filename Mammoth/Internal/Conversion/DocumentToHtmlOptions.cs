using System;
using System.Collections.Generic;
using System.IO;
using Mammoth.Internal.Styles;
using Mammoth.Internal.Styles.Parsing;

namespace Mammoth.Internal.Conversion
{
	internal class DocumentToHtmlOptions
	{
		internal static DocumentToHtmlOptions DEFAULT = new DocumentToHtmlOptions(
			InternalImageConverter.ImgElement(image =>
			{
				byte[] data;
				using (var stream = image.Open())
				using (var mem = new MemoryStream())
				{
					stream.CopyTo(mem);
					data = mem.ToArray();
				}
				string base64 = Convert.ToBase64String(data);
				string src = $"data:{image.ContentType};base64,{base64}";
				return new Dictionary<string, string> { ["src"] = src };
			}));
		readonly StyleMap styleMap;
		readonly StyleMap embeddedStyleMap;
		readonly bool disableDefaultStyleMap;
		readonly bool disableEmbeddedStyleMap;

		internal DocumentToHtmlOptions(
			string idPrefix,
			bool preserveEmptyParagraphs,
			StyleMap styleMap,
			StyleMap embeddedStyleMap,
			bool disableDefaultStyleMap,
			bool disableEmbeddedStyleMap,
			InternalImageConverter imageConverter)
		{
			IdPrefix = idPrefix;
			PreserveEmptyParagraphs = preserveEmptyParagraphs;
			this.styleMap = styleMap;
			this.embeddedStyleMap = embeddedStyleMap;
			this.disableDefaultStyleMap = disableDefaultStyleMap;
			this.disableEmbeddedStyleMap = disableEmbeddedStyleMap;
			ImageConverter = imageConverter;
		}
		private DocumentToHtmlOptions(InternalImageConverter imageConverter)
			: this(
				idPrefix: "",
				preserveEmptyParagraphs: false,
				styleMap: StyleMap.EMPTY,
				embeddedStyleMap: StyleMap.EMPTY,
				disableDefaultStyleMap: false,
				disableEmbeddedStyleMap: false,
				imageConverter: imageConverter)
		{ }
		public DocumentToHtmlOptions AddIdPrefix(string prefix)
		{
			return new DocumentToHtmlOptions(prefix, PreserveEmptyParagraphs, styleMap, embeddedStyleMap, disableDefaultStyleMap, disableEmbeddedStyleMap, ImageConverter);
		}
		public DocumentToHtmlOptions AddPreserveEmptyParagraphs()
		{
			return new DocumentToHtmlOptions(IdPrefix, true, styleMap, embeddedStyleMap, disableDefaultStyleMap, disableEmbeddedStyleMap, ImageConverter);
		}
		public DocumentToHtmlOptions AddStyleMap(string styleMap)
		{
			return AddStyleMap(StyleMapParser.Parse(styleMap));
		}
		public DocumentToHtmlOptions AddStyleMap(StyleMap styleMap)
		{
			return new DocumentToHtmlOptions(IdPrefix, PreserveEmptyParagraphs, this.styleMap.Update(styleMap), embeddedStyleMap, disableDefaultStyleMap, disableEmbeddedStyleMap, ImageConverter);
		}
		public DocumentToHtmlOptions AddDisableDefaultStyleMap()
		{
			return new DocumentToHtmlOptions(IdPrefix, PreserveEmptyParagraphs, styleMap, embeddedStyleMap, true, disableEmbeddedStyleMap, ImageConverter);
		}
		public DocumentToHtmlOptions AddDisableEmbeddedStyleMap()
		{
			return new DocumentToHtmlOptions(IdPrefix, PreserveEmptyParagraphs, styleMap, embeddedStyleMap, disableDefaultStyleMap, true, ImageConverter);
		}
		public DocumentToHtmlOptions AddEmbeddedStyleMap(StyleMap embeddedStyleMap)
		{
			return new DocumentToHtmlOptions(IdPrefix, PreserveEmptyParagraphs, styleMap, embeddedStyleMap, disableDefaultStyleMap, disableEmbeddedStyleMap, ImageConverter);
		}
		public DocumentToHtmlOptions AddImageConverter(InternalImageConverter imageConverter)
		{
			return new DocumentToHtmlOptions(IdPrefix, PreserveEmptyParagraphs, styleMap, embeddedStyleMap, disableDefaultStyleMap, disableEmbeddedStyleMap, imageConverter);
		}
		public string IdPrefix { get; }

		public bool PreserveEmptyParagraphs { get; }

		public StyleMap StyleMap
		{
			get
			{
				var styleMap = StyleMap.EMPTY;
				if (!disableDefaultStyleMap)
					styleMap = styleMap.Update(DefaultStyles.DEFAULT_STYLE_MAP);
				if (!disableEmbeddedStyleMap)
					styleMap = styleMap.Update(embeddedStyleMap);
				styleMap = styleMap.Update(this.styleMap);
				return styleMap;
			}
		}

		public InternalImageConverter ImageConverter { get; }
	}
}

