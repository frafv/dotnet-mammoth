using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mammoth.Internal.Styles.Parsing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Tests
{
	[TestClass]
	public class DocumentConverterTests {
		static string BinDirectory;
		static Assembly CurrentAssembly;

		[ClassInitialize]
		public static void Setup(TestContext context)
		{
			CurrentAssembly = Assembly.GetExecutingAssembly();
			BinDirectory = Directory.GetCurrentDirectory();
		}

		[TestMethod]
		public void DocxContainingOneParagraphIsConvertedToSingleParagraphElement() {
			AssertSuccessfulConversion(
				ConvertToHtml("single-paragraph.docx"),
				"<p>Walking on imported air</p>");
		}

		[TestMethod]
		public void CanReadFilesWithUtf8Bom()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("utf8-bom.docx"),
				"<p>This XML has a byte order mark.</p>");
		}

		[TestMethod]
		public void EmptyParagraphsAreIgnoredByDefault()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("empty.docx"),
				"");
		}

		[TestMethod]
		public void EmptyParagraphsArePreservedIfIgnoreEmptyParagraphsIsFalse()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("empty.docx", converter => converter.PreserveEmptyParagraphs()),
				"<p></p>");
		}

		[TestMethod]
		public void SimpleListIsConvertedToListElements()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("simple-list.docx"),
				"<ul><li>Apple</li><li>Banana</li></ul>");
		}

		[TestMethod]
		public void WordTablesAreConvertedToHtmlTables()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("tables.docx"),
				"<p>Above</p>" +
				"<table>" +
				"<tr><td><p>Top left</p></td><td><p>Top right</p></td></tr>" +
				"<tr><td><p>Bottom left</p></td><td><p>Bottom right</p></td></tr>" +
				"</table>" +
				"<p>Below</p>");
		}

		[TestMethod]
		public void InlineImagesReferencedByPathRelativeToPartAreIncludedInOutput()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("tiny-picture.docx"),
				"<p><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAAAAXNSR0IArs4c6QAAAAlwSFlzAAAOvgAADr4B6kKxwAAAABNJREFUKFNj/M+ADzDhlWUYqdIAQSwBE8U+X40AAAAASUVORK5CYII=\" /></p>");
		}

		[TestMethod]
		public void InlineImagesReferencedByPathRelativeToBaseAreIncludedInOutput()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("tiny-picture-target-base-relative.docx"),
				"<p><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAAAAXNSR0IArs4c6QAAAAlwSFlzAAAOvgAADr4B6kKxwAAAABNJREFUKFNj/M+ADzDhlWUYqdIAQSwBE8U+X40AAAAASUVORK5CYII=\" /></p>");
		}

		[TestMethod]
		public void ImagesStoredOutsideOfDocumentAreIncludedInOutput()
		{
			AssertSuccessfulConversion(
				new DocumentConverter().ConvertToHtml(TestFilePath("external-picture.docx")),
				"<p><img src=\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAAAAXNSR0IArs4c6QAAAAlwSFlzAAAOvgAADr4B6kKxwAAAABNJREFUKFNj/M+ADzDhlWUYqdIAQSwBE8U+X40AAAAASUVORK5CYII=\" /></p>");
		}

		[TestMethod]
		public void WarnIfDocumentHasImagesStoredOutsideOfDocumentWhenPathOfDocumentIsUnknown()
		{
			using (var file = File.OpenRead(TestFilePath("external-picture.docx")))
			{
				var buf = new MemoryStream();
				file.CopyTo(buf);
				buf.Position = 0;
				var result = new DocumentConverter().ConvertToHtml(buf);
				Assert.AreEqual("", result.Value);
				Assert.AreEqual("Could not open external image 'tiny-picture.png': Path of document is unknown, but is required for relative URI",
					result.Warnings.Single());
			}
		}

		[TestMethod]
		public void WarnIfImagesStoredOutsideOfDocumentAreNotFound()
		{
			string tempDirectory = Path.Combine(Path.GetTempPath(), "mammoth-" + Guid.NewGuid());
			Directory.CreateDirectory(tempDirectory);
			try
			{
				string documentPath = Path.Combine(tempDirectory, "external-picture.docx");
				File.Copy(TestFilePath("external-picture.docx"), documentPath);
				var result = new DocumentConverter().ConvertToHtml(documentPath);
				Assert.AreEqual("", result.Value);
				StringAssert.StartsWith(result.Warnings.Single(), "Could not open external image 'tiny-picture.png':");
			}
			finally
			{
				Directory.Delete(tempDirectory, recursive: true);
			}
		}

		[TestMethod]
		public void ImageConversionCanBeCustomised()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("tiny-picture.docx", mammoth => mammoth.ImageConverter(ConvertImage6Bytes)),
				"<p><img src=\"iVBORw0K,image/png\" /></p>"
			);
		}

		private IDictionary<string, string> ConvertImage6Bytes(IImage image)
		{
			using (var stream = image.Open())
			{
				string base64 = StreamToBase64(stream);
				string src = base64.Remove(8) + "," + image.ContentType;
				return new Dictionary<string, string> { ["src"] = src };
			}
		}

		private static string StreamToBase64(Stream stream)
		{
			var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return Convert.ToBase64String(memoryStream.ToArray());
		}

		[TestMethod]
		public void ContentTypesAreRead()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("tiny-picture-custom-content-type.docx"),
				"<p><img src=\"data:image/gif;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAIAAAACUFjqAAAAAXNSR0IArs4c6QAAAAlwSFlzAAAOvgAADr4B6kKxwAAAABNJREFUKFNj/M+ADzDhlWUYqdIAQSwBE8U+X40AAAAASUVORK5CYII=\" /></p>");
		}

		[TestMethod]
		public void FootnotesAreAppendedToText()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("footnotes.docx", mammoth => mammoth.IdPrefix("doc-42-")),
					"<p>Ouch" +
					"<sup><a href=\"#doc-42-footnote-1\" id=\"doc-42-footnote-ref-1\">[1]</a></sup>." +
					"<sup><a href=\"#doc-42-footnote-2\" id=\"doc-42-footnote-ref-2\">[2]</a></sup></p>" +
					"<ol><li id=\"doc-42-footnote-1\"><p> A tachyon walks into a bar. <a href=\"#doc-42-footnote-ref-1\">↑</a></p></li>" +
					"<li id=\"doc-42-footnote-2\"><p> Fin. <a href=\"#doc-42-footnote-ref-2\">↑</a></p></li></ol>");
		}

		[TestMethod]
		public void EndNotesAreAppendedToText()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("endnotes.docx", mammoth => mammoth.IdPrefix("doc-42-")),
					"<p>Ouch" +
					"<sup><a href=\"#doc-42-endnote-2\" id=\"doc-42-endnote-ref-2\">[1]</a></sup>." +
					"<sup><a href=\"#doc-42-endnote-3\" id=\"doc-42-endnote-ref-3\">[2]</a></sup></p>" +
					"<ol><li id=\"doc-42-endnote-2\"><p> A tachyon walks into a bar. <a href=\"#doc-42-endnote-ref-2\">↑</a></p></li>" +
					"<li id=\"doc-42-endnote-3\"><p> Fin. <a href=\"#doc-42-endnote-ref-3\">↑</a></p></li></ol>");
		}

		[TestMethod]
		public void RelationshipsAreReadForEachFileContainingBodyXml()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("footnote-hyperlink.docx", mammoth => mammoth.IdPrefix("doc-42-")),

					"<p><sup><a href=\"#doc-42-footnote-1\" id=\"doc-42-footnote-ref-1\">[1]</a></sup></p>" +
					"<ol><li id=\"doc-42-footnote-1\"><p> <a href=\"http://www.example.com\">Example</a> <a href=\"#doc-42-footnote-ref-1\">↑</a></p></li></ol>");
		}

		[TestMethod]
		public void TextBoxesAreRead()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("text-box.docx"),
				"<p>Datum plane</p>");
		}

		[TestMethod]
		public void CanUseCustomStyleMap()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("underline.docx", mammoth => mammoth.AddStyleMap("u => em")),
				"<p><strong>The <em>Sunset</em> Tree</strong></p>");
		}

		[TestMethod]
		public void MostRecentlyAddedStyleMapTakesPrecedence()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("underline.docx", mammoth => mammoth.AddStyleMap("u => em").AddStyleMap("u => strong")),
				"<p><strong>The <strong>Sunset</strong> Tree</strong></p>");
		}

		[TestMethod]
		public void RulesFromPreviouslyAddedStyleMapsStillTakeEffectIfNotOverridden()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("underline.docx", mammoth => mammoth.AddStyleMap("u => em").AddStyleMap("strike => del")),
				"<p><strong>The <em>Sunset</em> Tree</strong></p>");
		}

		[TestMethod]
		[ExpectedException(typeof(ParseException))]
		public void ErrorIsRaisedIfStyleMapCannotBeParsed()
		{
			ExceptionAssert.ThrowMessage("Error reading style map at line 2, character 14: Expected token of type STRING but was of type SYMBOL\n\n" +
				"p[style-name=] =>\n" +
				"             ^",
				() => new DocumentConverter().AddStyleMap("p =>\np[style-name=] =>"));
		}

		[TestMethod]
		public void CanDisableDefaultStyleMap()
		{
			var result = ConvertToHtml("simple-list.docx", mammoth => mammoth.DisableDefaultStyleMap());
			CollectionAssert.AreEqual(new[] { "Unrecognised paragraph style: List Paragraph (Style ID: ListParagraph)" }, result.Warnings);
			Assert.AreEqual("<p>Apple</p><p>Banana</p>", result.Value);
		}

		[TestMethod]
		public void EmbeddedStyleMapIsUsedIfPresent()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("embedded-style-map.docx"),
				"<h1>Walking on imported air</h1>"
			);
		}

		[TestMethod]
		public void ExplicitStyleMapTakesPrecedenceOverEmbeddedStyleMap()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("embedded-style-map.docx", mammoth => mammoth.AddStyleMap("p => p")),
				"<p>Walking on imported air</p>"
			);
		}

		[TestMethod]
		public void ExplicitStyleMapIsCombinedWithEmbeddedStyleMap()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("embedded-style-map.docx", mammoth => mammoth.AddStyleMap("r => strong")),
				"<h1><strong>Walking on imported air</strong></h1>"
			);
		}

		[TestMethod]
		public void EmbeddedStyleMapsCanBeDisabled()
		{
			AssertSuccessfulConversion(
				ConvertToHtml("embedded-style-map.docx", mammoth => mammoth.DisableEmbeddedStyleMap()),
				"<p>Walking on imported air</p>"
			);
		}

		[TestMethod]
		public void CanExtractRawTextFromFile()
		{
			AssertSuccessfulConversion(
				new DocumentConverter().ExtractRawText(TestFileStream("simple-list.docx")),
				"Apple\n\nBanana\n\n");
		}

		[TestMethod]
		public void CanExtractRawTextFromStream()
		{
			using (var file = TestFileStream("simple-list.docx"))
			{
				AssertSuccessfulConversion(
					new DocumentConverter().ExtractRawText(file),
					"Apple\n\nBanana\n\n");
			}
		}

		private void AssertSuccessfulConversion(IResult<string> result, string expectedValue)
		{
			if (result.Warnings.Length > 0)
			{
				Assert.Fail("Unexpected warnings: " + String.Join(", ", result.Warnings));
			}
			Assert.AreEqual(expectedValue, result.Value);
		}

		private IResult<string> ConvertToHtml(string name)
		{
			return ConvertToHtml(name, converter => converter);
		}

		private IResult<string> ConvertToHtml(string name, Func<DocumentConverter, DocumentConverter> configure)
		{
			return configure(new DocumentConverter()).ConvertToHtml(TestFileStream(name));
		}

		private string TestFilePath(string name)
		{
			return Path.Combine(BinDirectory, "TestData", name);
		}

		private Stream TestFileStream(string name)
		{
			return CurrentAssembly.GetManifestResourceStream("Mammoth.Tests.TestData." + name);
		}
	}
}

