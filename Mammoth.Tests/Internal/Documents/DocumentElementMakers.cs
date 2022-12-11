using System.Collections.Generic;
using System.Linq;
using Mammoth.Tests;
using static Mammoth.Tests.Argument;
using VerticalAlignment = Mammoth.Internal.Documents.Run.RunVerticalAlignment;

namespace Mammoth.Internal.Documents.Tests
{
	class DocumentElementMakers
	{
		static readonly ArgumentKey<Style> STYLE = new ArgumentKey<Style>("style");
		static readonly ArgumentKey<NumberingLevel> NUMBERING = new ArgumentKey<NumberingLevel>("numbering");
		static readonly ArgumentKey<bool> BOLD = new ArgumentKey<bool>("bold");
		static readonly ArgumentKey<bool> ITALIC = new ArgumentKey<bool>("italic");
		static readonly ArgumentKey<bool> UNDERLINE = new ArgumentKey<bool>("underline");
		static readonly ArgumentKey<bool> STRIKETHROUGH = new ArgumentKey<bool>("strikethrough");
		static readonly ArgumentKey<bool> ALL_CAPS = new ArgumentKey<bool>("allCaps");
		static readonly ArgumentKey<bool> SMALL_CAPS = new ArgumentKey<bool>("smallCaps");
		static readonly ArgumentKey<VerticalAlignment> VERTICAL_ALIGNMENT = new ArgumentKey<VerticalAlignment>("verticalAlignment");
		static readonly ArgumentKey<DocumentElement[]> CHILDREN = new ArgumentKey<DocumentElement[]>("children");
		static readonly ArgumentKey<bool> IS_HEADER = new ArgumentKey<bool>("isHeader");
		static readonly ArgumentKey<int> COLSPAN = new ArgumentKey<int>("colspan");
		static readonly ArgumentKey<int> ROWSPAN = new ArgumentKey<int>("rowspan");
		static readonly ArgumentKey<Comment[]> COMMENTS = new ArgumentKey<Comment[]>("comments");
		static readonly ArgumentKey<string> HREF = new ArgumentKey<string>("href");
		static readonly ArgumentKey<string> ANCHOR = new ArgumentKey<string>("anchor");
		static readonly ArgumentKey<string> TARGET_FRAME = new ArgumentKey<string>("targetFrame");
		static readonly ArgumentKey<Notes> NOTES = new ArgumentKey<Notes>("notes");

		public static Argument<Style> WithStyle(Style style)
		{
			return Arg(STYLE, style);
		}

		public static Argument<NumberingLevel> WithNumbering(NumberingLevel numbering)
		{
			return Arg(NUMBERING, numbering);
		}

		public static Argument<bool> WithBold(bool bold)
		{
			return Arg(BOLD, bold);
		}

		public static Argument<bool> WithItalic(bool italic)
		{
			return Arg(ITALIC, italic);
		}

		public static Argument<bool> WithUnderline(bool underline)
		{
			return Arg(UNDERLINE, underline);
		}

		public static Argument<bool> WithStrikethrough(bool strikethrough)
		{
			return Arg(STRIKETHROUGH, strikethrough);
		}

		public static Argument<bool> WithAllCaps(bool allCaps)
		{
			return Arg(ALL_CAPS, allCaps);
		}

		public static Argument<bool> WithSmallCaps(bool smallCaps)
		{
			return Arg(SMALL_CAPS, smallCaps);
		}

		public static Argument<VerticalAlignment> WithVerticalAlignment(VerticalAlignment verticalAlignment)
		{
			return Arg(VERTICAL_ALIGNMENT, verticalAlignment);
		}

		public static Argument<DocumentElement[]> WithChildren(params DocumentElement[] children)
		{
			return Arg(CHILDREN, children.ToArray());
		}

		public static Argument<bool> WithIsHeader(bool isHeader)
		{
			return Arg(IS_HEADER, isHeader);
		}

		public static Argument<int> WithRowspan(int rowspan)
		{
			return Arg(ROWSPAN, rowspan);
		}

		public static Argument<int> WithColspan(int colspan)
		{
			return Arg(COLSPAN, colspan);
		}

		public static Argument<Comment[]> WithComments(params Comment[] comments)
		{
			return Arg(COMMENTS, comments.ToArray());
		}

		public static Argument<Notes> WithNotes(Notes notes)
		{
			return Arg(NOTES, notes);
		}

		public static Document Document(params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new Document(
				arguments.Get(CHILDREN, new DocumentElement[0]),
				arguments.Get(NOTES, Notes.EMPTY),
				arguments.Get(COMMENTS, new Comment[0]));
		}

		public static Paragraph Paragraph(params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new Paragraph(
				arguments.Get(STYLE, null),
				arguments.Get(NUMBERING, null),
				new ParagraphIndent(),
				arguments.Get(CHILDREN, new DocumentElement[0]));
		}

		public static Paragraph ParagraphWithText(string text)
		{
			return Paragraph(WithChildren(RunWithText(text)));
		}

		public static Run Run(params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new Run(
				arguments.Get(BOLD, false),
				arguments.Get(ITALIC, false),
				arguments.Get(UNDERLINE, false),
				arguments.Get(STRIKETHROUGH, false),
				arguments.Get(ALL_CAPS, false),
				arguments.Get(SMALL_CAPS, false),
				arguments.Get(VERTICAL_ALIGNMENT, VerticalAlignment.BASELINE),
				arguments.Get(STYLE, null),
				arguments.Get(CHILDREN, new DocumentElement[0]));
		}

		public static Run RunWithText(string text)
		{
			return Run(WithChildren(new Text(text)));
		}

		public static Table Table(params TableRow[] rows) =>
			Table((IEnumerable<DocumentElement>)rows);
		public static Table Table(IEnumerable<DocumentElement> rows, params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new Table(arguments.Get(STYLE, null), rows);
		}

		public static TableRow TableRow(params TableCell[] cells) =>
			TableRow((IEnumerable<DocumentElement>)cells);
		public static TableRow TableRow(IEnumerable<DocumentElement> cells, params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new TableRow(cells, arguments.Get(IS_HEADER, false));
		}

		public static TableCell TableCell(params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new TableCell(
				arguments.Get(ROWSPAN, 1),
				arguments.Get(COLSPAN, 1),
				arguments.Get(CHILDREN, new DocumentElement[0]));
		}

		public static Comment Comment(string commentId, params DocumentElement[] body)
		{
			return new Comment(commentId, body);
		}

		public static Hyperlink Hyperlink(params Argument[] args)
		{
			var arguments = new Arguments(args);
			return new Hyperlink(
				arguments.Get(HREF, null),
				arguments.Get(ANCHOR, null),
				arguments.Get(TARGET_FRAME, null),
				arguments.Get(CHILDREN, new DocumentElement[0]));
		}

		public static Argument<string> WithHref(string href)
		{
			return Arg(HREF, href);
		}

		public static Argument<string> WithAnchor(string anchor)
		{
			return Arg(ANCHOR, anchor);
		}

		public static Argument<string> WithTargetFrame(string targetFrame)
		{
			return Arg(TARGET_FRAME, targetFrame);
		}
	}
}
