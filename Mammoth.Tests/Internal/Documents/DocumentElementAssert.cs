using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Documents.Tests
{
	static class DocumentElementAssert
	{
		public static bool AreEqual(Document expected, Document actual)
		{
			if (!AreObjects(expected, actual)) return true;
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool IsParagraph(DocumentElement element, Paragraph expected)
		{
			if (!IsDocumentElement(element, expected)) return false;
			return AreEqual(expected, (Paragraph)element);
		}

		public static bool IsParagraph(DocumentElement element)
		{
			return IsDocumentElement<Paragraph>(element);
		}

		public static bool AreEqual(Paragraph expected, Paragraph actual)
		{
			if (!AreObjects(expected, actual)) return true;
			AreEqual(expected.Style, actual.Style);
			AreEqual(expected.Numbering, actual.Numbering);
			AreEqual(expected.Indent, actual.Indent);
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool IsRun(DocumentElement element, Run expected)
		{
			if (!IsDocumentElement(element, expected)) return false;
			return AreEqual(expected, (Run)element);
		}

		public static bool IsEmptyRun(DocumentElement element)
		{
			if (!IsRun(element)) return false;
			HasChildren(element, any: false);
			return true;
		}

		public static bool IsRun(DocumentElement element)
		{
			return IsDocumentElement<Run>(element);
		}

		public static bool AreEqual(Run expected, Run actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.VerticalAlignment, actual.VerticalAlignment);
			Assert.AreEqual(expected.IsBold, actual.IsBold);
			Assert.AreEqual(expected.IsItalic, actual.IsItalic);
			Assert.AreEqual(expected.IsUnderline, actual.IsUnderline);
			Assert.AreEqual(expected.IsStrikethrough, actual.IsStrikethrough);
			Assert.AreEqual(expected.IsAllCaps, actual.IsAllCaps);
			Assert.AreEqual(expected.IsSmallCaps, actual.IsSmallCaps);
			AreEqual(expected.Style, actual.Style);
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool IsTextElement(DocumentElement element, string text)
		{
			switch (element)
			{
				case Text textElement:
					Assert.AreEqual(text, textElement.Value);
					return true;
				default:
					return IsDocumentElement<Text>(element);
			}
		}

		public static bool IsHyperlink(DocumentElement element)
		{
			return IsDocumentElement<Hyperlink>(element);
		}

		public static bool IsTable(DocumentElement element, Table expected)
		{
			if (!IsDocumentElement<Table>(element)) return false;
			return AreEqual(expected, (Table)element);
		}

		public static bool IsTable(DocumentElement element)
		{
			return IsDocumentElement<Table>(element);
		}

		public static bool AreEqual(Table expected, Table actual)
		{
			if (!AreObjects(expected, actual)) return true;
			AreEqual(expected.Style, actual.Style);
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool IsTableRow(DocumentElement element, bool expectedHeader)
		{
			if (!IsDocumentElement<TableRow>(element)) return false;
			var row = element as TableRow;
			Assert.AreEqual(expectedHeader, row.IsHeader);
			return true;
		}

		public static bool IsTableRow(DocumentElement element)
		{
			return IsDocumentElement<TableRow>(element);
		}

		public static bool AreEqual(TableRow expected, TableRow actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.IsHeader, actual.IsHeader);
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool IsTableCell(DocumentElement element, TableCell expected)
		{
			if (!IsDocumentElement<TableCell>(element)) return false;
			return AreEqual(expected, (TableCell)element);
		}

		public static bool IsTableCell(DocumentElement element)
		{
			return IsDocumentElement<TableCell>(element);
		}

		public static bool AreEqual(TableCell expected, TableCell actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.Rowspan, actual.Rowspan);
			Assert.AreEqual(expected.Colspan, actual.Colspan);
			SequenceEqual(expected.Children, actual.Children);
			return true;
		}

		public static bool AreEqual(Bookmark expected, Bookmark actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.Name, actual.Name);
			return true;
		}

		public static bool AreEqual(NoteReference expected, NoteReference actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.NoteId, actual.NoteId);
			Assert.AreEqual(expected.NoteType, actual.NoteType);
			return true;
		}

		public static bool AreEqual(CommentReference expected, CommentReference actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.CommentId, actual.CommentId);
			return true;
		}

		public static bool AreEqual(Note expected, Note actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.Type, actual.Type);
			Assert.AreEqual(expected.Id, actual.Id);
			SequenceEqual(expected.Body, actual.Body);
			return true;
		}

		public static bool HasChildren(DocumentElement element, bool any)
		{
			Assert.IsInstanceOfType(element, typeof(IHasChildren));
			var parent = (IHasChildren)element;
			Assert.AreEqual(any, parent.Children.Any());
			return any;
		}

		public static bool HasStyle(DocumentElement element, Style expected)
		{
			switch (element)
			{
				case Paragraph paragraph:
					return AreEqual(expected, paragraph.Style);
				case Run run:
					return AreEqual(expected, run.Style);
				case Table table:
					return AreEqual(expected, table.Style);
				default:
					Assert.Fail("Unknown document element " + element.GetType());
					return false;
			}
		}

		public static bool AreEqual(Style expected, Style actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.StyleId, actual.StyleId);
			Assert.AreEqual(expected.Name, actual.Name);
			return true;
		}

		public static bool HasNumbering(DocumentElement element, NumberingLevel expected)
		{
			switch (element)
			{
				case Paragraph paragraph:
					return AreEqual(expected, paragraph.Numbering);
				default:
					return IsParagraph(element);
			}
		}

		public static bool AreEqual(NumberingLevel expected, NumberingLevel actual)
		{
			if (!AreObjects(expected, actual)) return true;
			Assert.AreEqual(expected.LevelIndex, actual.LevelIndex);
			Assert.AreEqual(expected.IsOrdered, actual.IsOrdered);
			return true;
		}

		public static bool HasIndent(DocumentElement element,
			string expectedStart = null, string expectedEnd = null, string expectedFirstLine = null, string expectedHanging = null)
		{
			switch (element)
			{
				case Paragraph paragraph:
					return AreEqual(paragraph.Indent,
						expectedStart: expectedStart,
						expectedEnd: expectedEnd,
						expectedFirstLine: expectedFirstLine,
						expectedHanging: expectedHanging);
				default:
					return IsParagraph(element);
			}
		}

		public static bool AreEqual(ParagraphIndent indent,
			string expectedStart = null, string expectedEnd = null, string expectedFirstLine = null, string expectedHanging = null)
		{
			Assert.IsNotNull(indent);

			if (expectedStart == null)
				Assert.IsNull(indent.Start);
			else
				Assert.AreEqual(expectedStart, indent.Start);

			if (expectedEnd == null)
				Assert.IsNull(indent.End);
			else
				Assert.AreEqual(expectedEnd, indent.End);

			if (expectedFirstLine == null)
				Assert.IsNull(indent.FirstLine);
			else
				Assert.AreEqual(expectedFirstLine, indent.FirstLine);

			if (expectedHanging == null)
				Assert.IsNull(indent.Hanging);
			else
				Assert.AreEqual(expectedHanging, indent.Hanging);
			return true;
		}

		static bool AreEqual(ParagraphIndent expected, ParagraphIndent actual)
		{
			return AreEqual(actual,
				expectedStart: expected.Start, expectedEnd: expected.End, expectedFirstLine: expected.FirstLine, expectedHanging: expected.Hanging);
		}

		public static bool HasHref(DocumentElement element, string href)
		{
			switch (element)
			{
				case Hyperlink hyperlink:
					Assert.AreEqual(href, hyperlink.Href);
					return true;
				default:
					return IsHyperlink(element);
			}
		}

		public static bool HasAnchor(DocumentElement element, string anchor)
		{
			switch (element)
			{
				case Hyperlink hyperlink:
					Assert.AreEqual(anchor, hyperlink.Anchor);
					return true;
				default:
					return IsHyperlink(element);
			}
		}

		public static bool HasTargetFrame(DocumentElement element, string targetFrame)
		{
			switch (element)
			{
				case Hyperlink hyperlink:
					Assert.AreEqual(targetFrame, hyperlink.TargetFrame);
					return true;
				default:
					return IsHyperlink(element);
			}
		}

		static bool IsDocumentElement<TElement>(DocumentElement element)
			where TElement : DocumentElement
		{
			Assert.IsNotNull(element);
			Assert.IsInstanceOfType(element, typeof(TElement), $"Should be {typeof(TElement).Name}");
			return true;
		}

		static bool IsDocumentElement<TElement>(DocumentElement element, TElement expected)
			where TElement : DocumentElement
		{
			if (expected == null)
			{
				Assert.IsNull(expected);
				return false;
			}
			IsDocumentElement<TElement>(element);
			return true;
		}

		static bool AreObjects(object expected, object actual)
		{
			if (expected == null)
			{
				Assert.IsNull(actual);
				return false;
			}
			else
			{
				Assert.IsNotNull(actual);
				return true;
			}
		}

		public static bool SequenceEqual(IEnumerable<DocumentElement> expected, IEnumerable<DocumentElement> actual)
		{
			var expectedList = expected.ToList();
			var actualList = actual.ToList();
			Assert.AreEqual(expectedList.Count, actualList.Count);
			for (int k = 0; k < expectedList.Count; k++)
			{
				var expectedChild = expectedList[k];
				var actualChild = actualList[k];
				Assert.AreEqual(expectedChild.GetType(), actualChild.GetType());
				if (!AreEqual(expectedChild, actualChild)) return false;
			}
			return true;
		}

		static bool AreEqual(DocumentElement expected, DocumentElement actual)
		{
			switch (expected)
			{
				case Paragraph paragraph:
					return AreEqual(paragraph, actual as Paragraph);
				case Run run:
					return AreEqual(run, actual as Run);
				case Text text:
					var actualText = actual as Text;
					Assert.AreEqual(text.Value, actualText.Value);
					return true;
				case Table table:
					return AreEqual(table, actual as Table);
				case TableRow tableRow:
					return AreEqual(tableRow, actual as TableRow);
				case TableCell tableCell:
					return AreEqual(tableCell, actual as TableCell);
				default:
					Assert.Fail("Unknown document element " + actual.GetType());
					return false;
			}
		}
	}
}
