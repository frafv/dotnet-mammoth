using System.IO;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Html;
using Mammoth.Internal.Html.Tests;
using Mammoth.Internal.Results;
using Mammoth.Internal.Results.Tests;
using Mammoth.Internal.Styles;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Note;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;
using static Mammoth.Internal.Html.Html;
using VerticalAlignment = Mammoth.Internal.Documents.Run.RunVerticalAlignment;

namespace Mammoth.Internal.Conversion.Tests
{
	[TestClass()]
	public class DocumentToHtmlTests
	{
		[TestMethod()]
		public void PlainParagraphIsConvertedToPlainParagraph()
		{
			AssertSequenceEqual(
				ConvertToHtml(Paragraph(WithChildren(RunWithText("Hello")))),
				new[] { Element("p", Text("Hello")) });
		}

		[TestMethod()]
		public void ForceWriteIsInsertedIntoParagraphIfEmptyParagraphsShouldBePreserved()
		{
			var options = DocumentToHtmlOptions.DEFAULT.AddPreserveEmptyParagraphs();
			AssertIsSuccess(
				DocumentToHtml.ConvertToHtml(Paragraph(), options),
				Element("p", FORCE_WRITE));
		}

		[TestMethod()]
		public void MultipleParagraphsInDocumentAreConvertedToMultipleParagraphs()
		{
			AssertSequenceEqual(
				ConvertToHtml(Document(
					WithChildren(
						Paragraph(WithChildren(RunWithText("Hello"))),
						Paragraph(WithChildren(RunWithText("there")))))),

				Element("p", Text("Hello")),
				Element("p", Text("there")));
		}

		[TestMethod()]
		public void ParagraphStyleMappingsCanBeUsedToMapParagraphs()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Paragraph(WithStyle(new Style("TipsParagraph"))),
					StyleMap.Builder()
						.MapParagraph(
							ParagraphMatcher.ByStyleId("TipsParagraph"),
							HtmlPath.Element("p", new HtmlAttributes { ["class"] = "tip" }))
						.Build()),

				Element("p", new HtmlAttributes { ["class"] = "tip" }));
		}

		[TestMethod()]
		public void WarningIfParagraphHasUnrecognisedStyle()
		{
			AssertIsSuccess(
				ConvertToHtmlResult(
					Paragraph(WithStyle(new Style("TipsParagraph", "Tips Paragraph")))),

				new[] { Element("p") },
				"Unrecognised paragraph style: Tips Paragraph (Style ID: TipsParagraph)");
		}

		[TestMethod()]
		public void RunStyleMappingsCanBeUsedToMapRuns()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithStyle(new Style("TipsRun"))),
					StyleMap.Builder()
						.MapRun(
							RunMatcher.ByStyleId("TipsRun"),
							HtmlPath.Element("span", new HtmlAttributes { ["class"] = "tip" }))
						.Build()),

				new[] { Element("span", new HtmlAttributes { ["class"] = "tip" }) });
		}

		[TestMethod()]
		public void WarningIfRunHasUnrecognisedStyle()
		{
			AssertIsSuccess(
				ConvertToHtmlResult(
					Run(
						WithStyle(new Style("TipsRun", "Tips Run")),
						WithChildren(new Text("Hello")))),

				new[] { Text("Hello") },
				"Unrecognised run style: Tips Run (Style ID: TipsRun)");
		}

		[TestMethod()]
		public void BoldRunsAreWrappedInStrongTagsByDefault()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithBold(true), WithChildren(new Text("Hello")))),
				CollapsibleElement("strong", Text("Hello")));
		}

		[TestMethod()]
		public void BoldRunsCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithBold(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().Bold(HtmlPath.Element("em")).Build()),
				Element("em", Text("Hello")));
		}

		[TestMethod()]
		public void ItalicRunsAreWrappedInEmphasisTags()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithItalic(true), WithChildren(new Text("Hello")))),
				CollapsibleElement("em", Text("Hello")));
		}

		[TestMethod()]
		public void ItalicRunsCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithItalic(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().Italic(HtmlPath.Element("strong")).Build()),
				Element("strong", Text("Hello")));
		}

		[TestMethod()]
		public void UnderliningIsIgnoredByDefault()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithUnderline(true), WithChildren(new Text("Hello")))),
				Text("Hello"));
		}

		[TestMethod()]
		public void UnderliningCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithUnderline(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().Underline(HtmlPath.Element("em")).Build()),

				Element("em", Text("Hello")));
		}

		[TestMethod()]
		public void StruckthroughRunsAreWrappedInStrikethroughTagsByDefault()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithStrikethrough(true), WithChildren(new Text("Hello")))),
				CollapsibleElement("s", Text("Hello")));
		}

		[TestMethod()]
		public void StruckthroughRunsCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithStrikethrough(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().Strikethrough(HtmlPath.Element("del")).Build()),
				Element("del", Text("Hello")));
		}

		[TestMethod()]
		public void AllCapsIsIgnoredByDefault()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithAllCaps(true), WithChildren(new Text("Hello")))),
				Text("Hello"));
		}

		[TestMethod()]
		public void AllCapsCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithAllCaps(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().AllCaps(HtmlPath.Element("span")).Build()),

				Element("span", Text("Hello")));
		}

		[TestMethod()]
		public void SmallCapsIsIgnoredByDefault()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithSmallCaps(true), WithChildren(new Text("Hello")))),
				Text("Hello"));
		}

		[TestMethod()]
		public void SmallCapsCanBeMappedUsingStyleMapping()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Run(WithSmallCaps(true), WithChildren(new Text("Hello"))),
					StyleMap.Builder().SmallCaps(HtmlPath.Element("span")).Build()),

				Element("span", Text("Hello")));
		}

		[TestMethod()]
		public void SuperscriptRunsAreWrappedInSuperscriptTags()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithVerticalAlignment(VerticalAlignment.SUPERSCRIPT), WithChildren(new Text("Hello")))),
				CollapsibleElement("sup", Text("Hello")));
		}

		[TestMethod()]
		public void SubscriptRunsAreWrappedInSubscriptTags()
		{
			AssertSequenceEqual(
				ConvertToHtml(Run(WithVerticalAlignment(VerticalAlignment.SUBSCRIPT), WithChildren(new Text("Hello")))),
				CollapsibleElement("sub", Text("Hello")));
		}

		[TestMethod()]
		public void TabIsConvertedToTabInHtmlText()
		{
			AssertSequenceEqual(
				ConvertToHtml(Tab.TAB),
				Text("\t"));
		}

		[TestMethod()]
		public void LineBreakIsConvertedToBreakElement()
		{
			AssertSequenceEqual(
				ConvertToHtml(Break.LINE_BREAK),
				Element("br"));
		}

		[TestMethod()]
		public void BreaksThatAreNotLineBreaksAreIgnored()
		{
			AssertSequenceEqual(
				ConvertToHtml(Break.PAGE_BREAK));
		}

		[TestMethod()]
		public void BreaksCanBeMappedUsingStyleMappings()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Break.PAGE_BREAK,
					StyleMap.Builder().MapBreak(BreakMatcher.PAGE_BREAK, HtmlPath.Element("hr")).Build()),
				Element("hr"));
		}

		[TestMethod()]
		public void TableIsConvertedToHtmlTable()
		{
			AssertSequenceEqual(
				ConvertToHtml(Table(
					TableRow(
						TableCell(WithChildren(ParagraphWithText("Top left"))),
						TableCell(WithChildren(ParagraphWithText("Top right")))),
					TableRow(
						TableCell(WithChildren(ParagraphWithText("Bottom left"))),
						TableCell(WithChildren(ParagraphWithText("Bottom right")))))),

				Element("table",
					Element("tr",
						FORCE_WRITE,
						Element("td", FORCE_WRITE, Element("p", Text("Top left"))),
						Element("td", FORCE_WRITE, Element("p", Text("Top right")))),
					Element("tr",
						FORCE_WRITE,
						Element("td", FORCE_WRITE, Element("p", Text("Bottom left"))),
						Element("td", FORCE_WRITE, Element("p", Text("Bottom right"))))));
		}

		[TestMethod()]
		public void TableStyleMappingsCanBeUsedToMapTables()
		{
			AssertSequenceEqual(
				ConvertToHtml(
					Table(new TableRow[0], WithStyle(new Style("TableNormal", "Normal Table"))),
					StyleMap.Builder()
						.MapTable(
							TableMatcher.ByStyleName("Normal Table"),
							HtmlPath.Element("table", new HtmlAttributes { ["class"] = "normal-table" }))
						.Build()),

				Element("table", new HtmlAttributes { ["class"] = "normal-table" }));
		}

		[TestMethod()]
		public void HeaderRowsAreWrappedInThead()
		{
			AssertSequenceEqual(
				ConvertToHtml(Table(
					TableRow(new[] { TableCell() }, WithIsHeader(true)),
					TableRow(new[] { TableCell() }, WithIsHeader(true)),
					TableRow(new[] { TableCell() }, WithIsHeader(false)))),

				Element("table",
					Element("thead",
						Element("tr", FORCE_WRITE, Element("th", FORCE_WRITE)),
						Element("tr", FORCE_WRITE, Element("th", FORCE_WRITE))),
					Element("tbody",
						Element("tr", FORCE_WRITE, Element("td", FORCE_WRITE)))));
		}

		[TestMethod()]
		public void TbodyIsOmittedIfAllRowsAreHeaders()
		{
			AssertSequenceEqual(
				StripEmpty(ConvertToHtml(Table(
					TableRow(new[] { TableCell() }, WithIsHeader(true))))).ToArray(),

				Element("table",
					Element("thead",
						Element("tr", FORCE_WRITE, Element("th", FORCE_WRITE)))));
		}

		[TestMethod()]
		public void TableCellsAreWrittenWithColspanIfNotEqualToOne()
		{
			AssertSequenceEqual(
				ConvertToHtml(Table(
					TableRow(
						TableCell(
							WithChildren(ParagraphWithText("Top left")),
							WithColspan(2)),
						TableCell(WithChildren(ParagraphWithText("Top right")))))),

				Element("table",
					Element("tr",
						FORCE_WRITE,
						Element("td", new HtmlAttributes { ["colspan"] = "2" }, FORCE_WRITE, Element("p", Text("Top left"))),
						Element("td", FORCE_WRITE, Element("p", Text("Top right"))))));
		}

		[TestMethod()]
		public void TableCellsAreWrittenWithRowspanIfNotEqualToOne()
		{
			AssertSequenceEqual(
				ConvertToHtml(Table(
					TableRow(
						TableCell(WithRowspan(2))))),
				Element("table",
					Element("tr",
						FORCE_WRITE,
						Element("td", new HtmlAttributes { ["rowspan"] = "2" }, FORCE_WRITE))));
		}

		[TestMethod()]
		public void HyperlinkWithHrefIsConvertedToAnchorTag()
		{
			AssertSequenceEqual(
				ConvertToHtml(Hyperlink(
					WithHref("http://example.com"),
					WithChildren(new Text("Hello")))),
				CollapsibleElement("a", new HtmlAttributes { ["href"] = "http://example.com" }, Text("Hello")));
		}

		[TestMethod()]
		public void HyperlinkWithInternalAnchorReferenceIsConvertedToAnchorTag()
		{
			AssertSequenceEqual(
				ConvertToHtml(Hyperlink(
					WithAnchor("start"),
					WithChildren(new Text("Hello")))),
				CollapsibleElement("a", new HtmlAttributes { ["href"] = "#doc-42-start" }, Text("Hello")));
		}

		[TestMethod()]
		public void HyperlinkTargetFrameIsUsedAsAnchorTarget()
		{
			AssertSequenceEqual(
				ConvertToHtml(Hyperlink(
					WithAnchor("start"),
					WithTargetFrame("_blank"),
					WithChildren(new Text("Hello")))),
				CollapsibleElement("a", new HtmlAttributes { ["href"] = "#doc-42-start", ["target"] = "_blank" }, Text("Hello")));
		}

		[TestMethod()]
		public void BookmarksAreConvertedToAnchorsWithIds()
		{
			AssertSequenceEqual(
				ConvertToHtml(new Bookmark("start")),
				Element("a", new HtmlAttributes { ["id"] = "doc-42-start" }, FORCE_WRITE));
		}

		[TestMethod()]
		public void NoteReferencesAreConvertedToLinksToReferenceBodyAfterMainBody()
		{
			var document = Document(
				WithChildren(Paragraph(WithChildren(
					RunWithText("Knock knock"),
					Run(WithChildren(new NoteReference(NoteType.FOOTNOTE, "4")))))),
				WithNotes(new Notes(new[] { new Note(NoteType.FOOTNOTE, "4", new[] { ParagraphWithText("Who's there?") }) })));

			AssertSequenceEqual(
				ConvertToHtml(document),
				Element("p",
					Text("Knock knock"),
					Element("sup",
						Element("a", new HtmlAttributes { ["href"] = "#doc-42-footnote-4", ["id"] = "doc-42-footnote-ref-4" }, Text("[1]")))),
				Element("ol",
					Element("li", new HtmlAttributes { ["id"] = "doc-42-footnote-4" },
						Element("p",
							Text("Who's there?")),
						CollapsibleElement("p",
							Text(" "),
							Element("a", new HtmlAttributes { ["href"] = "#doc-42-footnote-ref-4" }, Text("↑"))))));
		}

		[TestMethod()]
		public void NoteReferencesAreConvertedWithSequentialNumbers()
		{
			var run = Run(WithChildren(
				new NoteReference(NoteType.FOOTNOTE, "4"),
				new NoteReference(NoteType.FOOTNOTE, "7")));

			AssertSequenceEqual(
				ConvertToHtml(run),
				Element("sup",
					Element("a", new HtmlAttributes { ["href"] = "#doc-42-footnote-4", ["id"] = "doc-42-footnote-ref-4" }, Text("[1]"))),
				Element("sup",
					Element("a", new HtmlAttributes { ["href"] = "#doc-42-footnote-7", ["id"] = "doc-42-footnote-ref-7" }, Text("[2]"))));
		}

		[TestMethod()]
		public void CommentsAreIgnoredByDefault()
		{
			var document = Document(
				WithChildren(
					Paragraph(WithChildren(
						RunWithText("Knock knock"),
						Run(WithChildren(new CommentReference("4")))))),
				WithComments(Comment("4", ParagraphWithText("Who's there?"))));

			AssertSequenceEqual(
				ConvertToHtml(document),
				Element("p",
					Text("Knock knock")));
		}

		[TestMethod()]
		public void CommentReferencesAreLinkedToCommentAfterMainBody()
		{
			var reference = new CommentReference("4");
			var comment = new Comment(
				"4",
				new[] { ParagraphWithText("Who's there?") },
				"The Piemaker",
				"TP");
			var document = Document(
				WithChildren(
					Paragraph(WithChildren(
						RunWithText("Knock knock"),
						Run(WithChildren(reference))))),
				WithComments(comment));

			var styleMap = StyleMap.Builder().CommentReference(HtmlPath.Element("sup")).Build();
			AssertSequenceEqual(
				ConvertToHtml(document, styleMap),
				Element("p",
					Text("Knock knock"),
					Element("sup",
						Element("a", new HtmlAttributes { ["href"] = "#doc-42-comment-4", ["id"] = "doc-42-comment-ref-4" }, Text("[TP1]")))),
				Element("dl",
					Element("dt", new HtmlAttributes { ["id"] = "doc-42-comment-4" },
						Text("Comment [TP1]")),
					Element("dd",
						Element("p",
							Text("Who's there?")),
						CollapsibleElement("p",
							Text(" "),
							Element("a", new HtmlAttributes { ["href"] = "#doc-42-comment-ref-4" }, Text("↑"))))));
		}

		[TestMethod()]
		public void ImagesAreConvertedToImageTagsWithDataUriByDefault()
		{
			Image image = new Image(
				null,
				("image/png"),
				new MemoryStream(new byte[] { 97, 98, 99 }));
			AssertSequenceEqual(
				ConvertToHtml(image),
				Element("img", new HtmlAttributes { ["src"] = "data:image/png;base64,YWJj" }));
		}

		[TestMethod()]
		public void ImagesHaveAltTagsIfAvailable()
		{
			Image image = new Image(
				"It's a hat",
				"image/png",
				new MemoryStream(new byte[] { 97, 98, 99 }));
			var result = ConvertToHtml(image);
			Assert.AreEqual(1, result.Length);
			Assert.IsInstanceOfType(result[0], typeof(HtmlElement));
			var element = (HtmlElement)result[0];
			Assert.IsTrue(element.Attributes.ContainsKey("alt"));
			Assert.AreEqual("It's a hat", element.Attributes["alt"]);
		}

		HtmlNode[] ConvertToHtml(Document document)
		{
			return ConvertToHtml(document, StyleMap.EMPTY);
		}

		HtmlNode[] ConvertToHtml(Document document, StyleMap styleMap)
		{
			var options = DocumentToHtmlOptions.DEFAULT
				.AddIdPrefix("doc-42-")
				.AddStyleMap(styleMap);
			var result = DocumentToHtml.ConvertToHtml(document, options);
			InternalResultAssert.IsResult(result);
			return result.Value;
		}

		HtmlNode[] ConvertToHtml(DocumentElement element)
		{
			return ConvertToHtml(element, StyleMap.EMPTY);
		}

		HtmlNode[] ConvertToHtml(DocumentElement element, StyleMap styleMap)
		{
			var result = ConvertToHtmlResult(element, styleMap);
			InternalResultAssert.IsResult(result);
			return result.Value;
		}

		InternalResult<HtmlNode[]> ConvertToHtmlResult(DocumentElement element)
		{
			return ConvertToHtmlResult(element, StyleMap.EMPTY);
		}

		InternalResult<HtmlNode[]> ConvertToHtmlResult(DocumentElement element, StyleMap styleMap)
		{
			var options = DocumentToHtmlOptions.DEFAULT
				.AddIdPrefix("doc-42-")
				.AddStyleMap(styleMap);
			return DocumentToHtml.ConvertToHtml(element, options);
		}

		void AssertSequenceEqual(HtmlNode[] result, params HtmlNode[] expected)
		{
			HtmlNodeAssert.SequenceEqual(expected, result);
		}

		void AssertIsSuccess(InternalResult<HtmlNode[]> result, params HtmlNode[] expected)
		{
			AssertIsSuccess(result, expected, new string[0]);
		}

		void AssertIsSuccess(InternalResult<HtmlNode[]> result, HtmlNode[] expected, params string[] warnings)
		{
			InternalResultAssert.IsResult(result, warnings);
			HtmlNodeAssert.SequenceEqual(expected, result.Value);
		}
	}
}