using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Mammoth.Internal.Conversion;
using Mammoth.Internal.Docx;
using Mammoth.Internal.Results;
using Mammoth.Internal.Styles.Parsing;
using static Mammoth.Internal.Html.Html;

namespace Mammoth.Internal
{
	internal class InternalDocumentConverter
	{
		readonly DocumentToHtmlOptions options;
		internal InternalDocumentConverter(DocumentToHtmlOptions options)
		{
			this.options = options;
		}
		public InternalResult<string> ConvertToHtml(Stream stream)
		{
			string path = stream is FileStream fs ? fs.Name : null;
			return WithDocxFile(stream, zipFile => ConvertToHtml(path, zipFile));
		}
		public InternalResult<string> ConvertToHtml(string file)
		{
			return WithDocxFile(file, zipFile => ConvertToHtml(file, zipFile));
		}
		InternalResult<string> ConvertToHtml(string path, ZipArchive zipFile)
		{
			var styleMap = ReadEmbeddedStyleMap(zipFile) is string map ? StyleMapParser.Parse(map) : null;
			var conversionOptions = styleMap != null ? options.AddEmbeddedStyleMap(styleMap) : options;

			return DocumentReader.ReadDocument(path, zipFile)
				.FlatMap(nodes => DocumentToHtml.ConvertToHtml(nodes, conversionOptions))
				.Map(nodes => StripEmpty(nodes).ToArray())
				.Map(nodes => Collapse(nodes).ToArray())
				.Map(nodes => Write(nodes));
		}
		string ReadEmbeddedStyleMap(ZipArchive zipFile)
		{
			return EmbeddedStyleMap.ReadStyleMap(zipFile);
		}
		public InternalResult<string> ExtractRawText(Stream stream)
		{
			string path = stream is FileStream fs ? fs.Name : null;
			return WithDocxFile(stream, zipFile =>
				ExtractRawText(path, zipFile));
		}
		public InternalResult<string> ExtractRawText(string file)
		{
			return WithDocxFile(file, zipFile =>
				ExtractRawText(file, zipFile));
		}
		InternalResult<string> ExtractRawText(string path, ZipArchive zipFile)
		{
			return DocumentReader.ReadDocument(path, zipFile)
				.Map(document => RawText.ExtractRawText(document));
		}
		static T WithDocxFile<T>(string file, Func<ZipArchive, T> function)
		{
			using (var stream = File.OpenRead(file))
			using (var zipFile = new ZipArchive(stream, ZipArchiveMode.Read))
				return function(zipFile);
		}
		T WithDocxFile<T>(Stream stream, Func<ZipArchive, T> function)
		{
			using (var zipFile = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true))
				return function(zipFile);
		}
	}
}
