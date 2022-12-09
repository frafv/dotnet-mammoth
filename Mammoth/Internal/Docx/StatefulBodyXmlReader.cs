using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Xml;
using NoteType = Mammoth.Internal.Documents.Note.NoteType;
using VerticalAlignment = Mammoth.Internal.Documents.Run.RunVerticalAlignment;

namespace Mammoth.Internal.Docx
{
	internal class StatefulBodyXmlReader
	{
		class ComplexField
		{
			public static readonly ComplexField UNKNOWN = new ComplexField();

			public static ComplexField HyperlinkHref(string href)
			{
				return new HyperlinkHrefComplexField(href);
			}

			public static ComplexField HyperlinkAnchor(string anchor)
			{
				return new HyperlinkAnchorComplexField(anchor);
			}
		}

		abstract class HyperlinkComplexField : ComplexField
		{
			public abstract Hyperlink Hyperlink(IEnumerable<DocumentElement> children);
		}

		class HyperlinkHrefComplexField : HyperlinkComplexField
		{
			readonly string href;
			public HyperlinkHrefComplexField(string href)
			{
				this.href = href;
			}
			public override Hyperlink Hyperlink(IEnumerable<DocumentElement> children)
			{
				return Documents.Hyperlink.CreateHref(href, children);
			}
		}

		class HyperlinkAnchorComplexField : HyperlinkComplexField
		{
			readonly string anchor;
			public HyperlinkAnchorComplexField(string anchor)
			{
				this.anchor = anchor;
			}
			public override Hyperlink Hyperlink(IEnumerable<DocumentElement> children)
			{
				return Documents.Hyperlink.CreateAnchor(anchor, children);
			}
		}

		static readonly ISet<string> IMAGE_TYPES_SUPPORTED_BY_BROWSERS = new HashSet<string>
		{ "image/png", "image/gif", "image/jpeg", "image/svg+xml", "image/tiff" };

		readonly Styles styles;
		readonly Numbering numbering;
		readonly Relationships relationships;
		readonly ContentTypes contentTypes;
		readonly ZipArchive file;
		readonly IFileReader fileReader;
		readonly StringBuilder currentInstrText = new StringBuilder();
		readonly Stack<ComplexField> complexFieldStack = new Stack<ComplexField>();

		internal StatefulBodyXmlReader(
			Styles styles,
			Numbering numbering,
			Relationships relationships,
			ContentTypes contentTypes,
			ZipArchive file,
			IFileReader fileReader)
		{
			this.styles = styles;
			this.numbering = numbering;
			this.relationships = relationships;
			this.contentTypes = contentTypes;
			this.file = file;
			this.fileReader = fileReader;
		}

		public ReadResult ReadElement(XmlElement element)
		{
			switch (element.Name)
			{
				case "w:t":
					return ReadResult.Success(new Text(element.InnerText));
				case "w:r":
					return ReadRun(element);
				case "w:p":
					return ReadParagraph(element);

				case "w:fldChar":
					return ReadFieldChar(element);
				case "w:instrText":
					return ReadInstrText(element);

				case "w:tab":
					return ReadResult.Success(Tab.TAB);
				case "w:noBreakHyphen":
					return ReadResult.Success(new Text("\u2011"));
				case "w:softHyphen":
					return ReadResult.Success(new Text("\u00ad"));
				case "w:sym":
					return ReadSymbol(element);
				case "w:br":
					return ReadBreak(element);

				case "w:tbl":
					return ReadTable(element);
				case "w:tr":
					return ReadTableRow(element);
				case "w:tc":
					return ReadTableCell(element);

				case "w:hyperlink":
					return ReadHyperlink(element);
				case "w:bookmarkStart":
					return ReadBookmark(element);
				case "w:footnoteReference":
					return ReadNoteReference(NoteType.FOOTNOTE, element);
				case "w:endnoteReference":
					return ReadNoteReference(NoteType.ENDNOTE, element);
				case "w:commentReference":
					return ReadCommentReference(element);

				case "w:pict":
					return ReadPict(element);

				case "v:imagedata":
					return ReadImagedata(element);

				case "wp:inline":
				case "wp:anchor":
					return ReadInline(element);

				case "w:sdt":
					return ReadSdt(element);

				case "w:ins":
				case "w:object":
				case "w:smartTag":
				case "w:drawing":
				case "v:group":
				case "v:rect":
				case "v:roundrect":
				case "v:shape":
				case "v:textbox":
				case "w:txbxContent":
					return ReadElements(element.Children);

				case "office-word:wrap":
				case "v:shadow":
				case "v:shapetype":
				case "w:bookmarkEnd":
				case "w:sectPr":
				case "w:proofErr":
				case "w:lastRenderedPageBreak":
				case "w:commentRangeStart":
				case "w:commentRangeEnd":
				case "w:del":
				case "w:footnoteRef":
				case "w:endnoteRef":
				case "w:annotationRef":
				case "w:pPr":
				case "w:rPr":
				case "w:tblPr":
				case "w:tblGrid":
				case "w:trPr":
				case "w:tcPr":
					return ReadResult.EMPTY_SUCCESS;

				default:
					string warning = $"An unrecognised element was ignored: {element.Name}";
					return ReadResult.EmptyWithWarning(warning);
			}
		}
		ReadResult ReadRun(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("w:rPr");
			return ReadResult.Map(
				ReadRunStyle(properties),
				ReadElements(element.Children),
				(style, children) =>
				{
					if (CurrentHyperlinkComplexField != null)
						children = new[] { CurrentHyperlinkComplexField.Hyperlink(children) };

					return new Run(
						isBold: IsBold(properties),
						isItalic: IsItalic(properties),
						isUnderline: IsUnderline(properties),
						isStrikethrough: IsStrikethrough(properties),
						isAllCaps: IsAllCaps(properties),
						isSmallCaps: IsSmallCaps(properties),
						verticalAlignment: ReadVerticalAlignment(properties),
						style: style,
						children: children);
				});
		}
		HyperlinkComplexField CurrentHyperlinkComplexField =>
			complexFieldStack.OfType<HyperlinkComplexField>().LastOrDefault();

		bool IsBold(IXmlElementLike properties) => ReadBooleanElement(properties, "w:b");
		bool IsItalic(IXmlElementLike properties) => ReadBooleanElement(properties, "w:i");
		bool IsUnderline(IXmlElementLike properties)
		{
			return properties.FindChild("w:u") is XmlElement child &&
				child.GetAttributeOrNone("w:val") is string value &&
				value != "false" && value != "0" && value != "none";
		}

		bool IsStrikethrough(IXmlElementLike properties) => ReadBooleanElement(properties, "w:strike");
		bool IsAllCaps(IXmlElementLike properties) => ReadBooleanElement(properties, "w:caps");
		bool IsSmallCaps(IXmlElementLike properties) => ReadBooleanElement(properties, "w:smallCaps");
		bool ReadBooleanElement(IXmlElementLike properties, string tagName)
		{
			return properties.FindChild(tagName) is XmlElement child &&
				(child.GetAttributeOrNone("w:val") is string value ? value != "false" && value != "0" : true);
		}
		VerticalAlignment ReadVerticalAlignment(IXmlElementLike properties)
		{
			string verticalAlignment = ReadVal(properties, "w:vertAlign");
			switch (verticalAlignment)
			{
				case "superscript":
					return VerticalAlignment.SUPERSCRIPT;
				case "subscript":
					return VerticalAlignment.SUBSCRIPT;
				default:
					return VerticalAlignment.BASELINE;
			}
		}
		InternalResult<Style> ReadRunStyle(IXmlElementLike properties)
		{
			return ReadVal(properties, "w:rStyle") is string styleId ?
				FindStyleById("Run", styleId, styles.FindCharacterStyleById(styleId)) : InternalResult<Style>.EMPTY;
		}
		public ReadResult ReadElements(IEnumerable<XmlNode> nodes)
		{
			return ReadResult.Join(nodes.OfType<XmlElement>().Select(elem => ReadElement(elem)));
		}
		ReadResult ReadParagraph(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("w:pPr");
			var indent = ReadParagraphIndent(properties);
			return ReadResult.Map(
				ReadParagraphStyle(properties),
				ReadElements(element.Children),
				(style, children) => new Paragraph(style, ReadNumbering(style, properties), indent, children)).AppendExtra();
		}
		ReadResult ReadFieldChar(XmlElement element)
		{
			string type = element.GetAttributeOrNone("w:fldCharType");
			switch (type)
			{
				case "begin":
					complexFieldStack.Push(ComplexField.UNKNOWN);
					currentInstrText.Length = 0;
					break;
				case "end":
					complexFieldStack.Pop();
					break;
				case "separate":
					string instrText = currentInstrText.ToString();
					var complexField = ParseHyperlinkFieldCode(instrText) ??
						ComplexField.UNKNOWN;
					complexFieldStack.Pop();
					complexFieldStack.Push(complexField);
					break;
			}
			return ReadResult.EMPTY_SUCCESS;
		}
		ReadResult ReadInstrText(XmlElement element)
		{
			currentInstrText.Append(element.InnerText);
			return ReadResult.EMPTY_SUCCESS;
		}
		ComplexField ParseHyperlinkFieldCode(string instrText)
		{
			var externalLinkPattern = new Regex(@"\s*HYPERLINK ""(.*)""");
			var externalLinkMatcher = externalLinkPattern.Match(instrText);
			if (externalLinkMatcher.Success)
			{
				string href = externalLinkMatcher.Groups[1].Value;
				return ComplexField.HyperlinkHref(href);
			}

			var internalLinkPattern = new Regex(@"\s*HYPERLINK\s+\\l\s+""(.*)""");
			var internalLinkMatcher = internalLinkPattern.Match(instrText);
			if (internalLinkMatcher.Success)
			{
				string anchor = internalLinkMatcher.Groups[1].Value;
				return ComplexField.HyperlinkAnchor(anchor);
			}

			return null;
		}
		InternalResult<Style> ReadParagraphStyle(IXmlElementLike properties)
		{
			return ReadVal(properties, "w:pStyle") is string styleId ?
				FindStyleById("Paragraph", styleId, styles.FindParagraphStyleById(styleId)) : InternalResult<Style>.EMPTY;
		}
		InternalResult<Style> FindStyleById(
			string styleType,
			string styleId,
			Style style)
		{
			if (style != null)
			{
				return InternalResult.Success(style);
			}
			else
			{
				return new InternalResult<Style>(new Style(styleId),
					$"{styleType} style with ID {styleId} was referenced but not defined in the document");
			}
		}
		NumberingLevel ReadNumbering(Style style, IXmlElementLike properties)
		{
			if (style != null)
			{
				string styleId = style.StyleId;
				var level = numbering.FindLevelByParagraphStyleId(styleId);
				if (level != null)
					return level;
			}

			var numberingProperties = properties.FindChildOrEmpty("w:numPr");
			return
				ReadVal(numberingProperties, "w:numId") is string numId &&
				ReadVal(numberingProperties, "w:ilvl") is string ilvl ?
				numbering.FindLevel(numId, ilvl) : null;
		}
		ParagraphIndent ReadParagraphIndent(IXmlElementLike properties)
		{
			var indent = properties.FindChildOrEmpty("w:ind");
			return new ParagraphIndent(
				indent.GetAttributeOrNone("w:start") ?? indent.GetAttributeOrNone("w:left"),
				indent.GetAttributeOrNone("w:end") ?? indent.GetAttributeOrNone("w:right"),
				indent.GetAttributeOrNone("w:firstLine"),
				indent.GetAttributeOrNone("w:hanging"));
		}
		ReadResult ReadSymbol(XmlElement element)
		{
			string font = element.GetAttributeOrNone("w:font");
			string charValue = element.GetAttributeOrNone("w:char");
			if (font != null && charValue != null)
			{
				int? dingbat = Dingbats.FindDingbat(font, Int32.Parse(charValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture));

				if (dingbat == null && charValue.StartsWith("F0", StringComparison.Ordinal) && charValue.Length == 4)
					dingbat = Dingbats.FindDingbat(font, Int32.Parse(charValue.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture));

				if (dingbat is int codePoint)
					return ReadResult.Success(new Text(Char.ConvertFromUtf32(codePoint)));
			}
			return ReadResult.EmptyWithWarning(
				$"A w:sym element with an unsupported character was ignored: char {charValue ?? "null"} in font {font ?? "null"}"
			);
		}

		ReadResult ReadBreak(XmlElement element)
		{
			string breakType = element.GetAttributeOrNone("w:type");
			switch (breakType)
			{
				case "textWrapping":
				case null:
					return ReadResult.Success(Break.LINE_BREAK);
				case "page":
					return ReadResult.Success(Break.PAGE_BREAK);
				case "column":
					return ReadResult.Success(Break.COLUMN_BREAK);
				default:
					return ReadResult.EmptyWithWarning($"Unsupported break type: {breakType}");
			}
		}
		ReadResult ReadTable(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("w:tblPr");
			return ReadResult.Map(
				ReadTableStyle(properties),
				ReadElements(element.Children)
					.FlatMap(children => CalculateRowspans(children)),
				(style, children) => new Table(style, children));
		}
		InternalResult<Style> ReadTableStyle(IXmlElementLike properties)
		{
			return ReadVal(properties, "w:tblStyle") is string styleId ?
				FindStyleById("Table", styleId, styles.FindTableStyleById(styleId)) : InternalResult<Style>.EMPTY;
		}

		readonly struct TablePosition : IEquatable<TablePosition>
		{
			public int Row { get; }
			public int Cell { get; }

			public TablePosition(int row, int cell)
			{
				Row = row;
				Cell = cell;
			}

			public override bool Equals(object obj) => obj is TablePosition other && Equals(other);
			public bool Equals(TablePosition other)
			{
				return Row == other.Row && Cell == other.Cell;
			}

			public override int GetHashCode()
			{
				return Cell << 16 ^ Row;
			}
		}

		ReadResult CalculateRowspans(IEnumerable<DocumentElement> rows)
		{
			string error = CheckTableRows(rows);
			if (error != null)
				return ReadResult.WithWarning(rows, error);

			var rowspans = new Dictionary<TablePosition, int>();
			var merged = new HashSet<TablePosition>();
			{
				var lastCellForColumn = new Dictionary<int, TablePosition>();
				int rowIndex = 0;
				foreach (TableRow row in rows)
				{
					int columnIndex = 0;
					int cellIndex = 0;
					foreach (UnmergedTableCell cell in row.Children)
					{
						var position = new TablePosition(rowIndex, cellIndex);
						if (cell.Vmerge && lastCellForColumn.TryGetValue(columnIndex, out var spanningCell))
						{
							rowspans[spanningCell] = rowspans[spanningCell] + 1;
							merged.Add(position);
						}
						else
						{
							lastCellForColumn[columnIndex] = position;
							rowspans[position] = 1;
						}
						columnIndex += cell.Colspan;
						cellIndex++;
					}
					rowIndex++;
				}
			}

			return ReadResult.Success(rows.Select((rowElement, rowIndex) =>
			{
				var row = (TableRow)rowElement;

				var mergedCells = new List<DocumentElement>();
				int cellIndex = 0;
				foreach (UnmergedTableCell cell in row.Children)
				{
					var position = new TablePosition(rowIndex, cellIndex);
					if (!merged.Contains(position))
					{
						mergedCells.Add(new TableCell(
							rowspans[position],
							cell.Colspan,
							cell.Children
						));
					}
					cellIndex++;
				}

				return new TableRow(mergedCells, row.IsHeader);
			}).ToArray());
		}
		string CheckTableRows(IEnumerable<DocumentElement> rows)
		{
			foreach (var rowElement in rows)
			{
				if (!(rowElement is TableRow row))
					return "unexpected non-row element in table, cell merging may be incorrect";
				foreach (var cell in row.Children)
				{
					if (!(cell is UnmergedTableCell))
						return "unexpected non-cell element in table row, cell merging may be incorrect";
				}
			}
			return null;
		}
		ReadResult ReadTableRow(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("w:trPr");
			bool isHeader = properties.HasChild("w:tblHeader");
			return ReadElements(element.Children)
				.Map(children => new TableRow(children, isHeader));
		}
		ReadResult ReadTableCell(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("w:tcPr");
			string gridSpan = properties
				.FindChildOrEmpty("w:gridSpan")
				.GetAttributeOrNone("w:val");
			int colspan = Int32.TryParse(gridSpan, NumberStyles.Integer, CultureInfo.InvariantCulture, out int num) ? num : 1;
			return ReadElements(element.Children)
				.Map(children => new UnmergedTableCell(ReadVmerge(properties), colspan, children));
		}
		bool ReadVmerge(IXmlElementLike properties)
		{
			return properties.FindChild("w:vMerge") is XmlElement element &&
				(element.GetAttributeOrNone("w:val") is string val ? val == "continue" : true);
		}

		class UnmergedTableCell : DocumentElement
		{
			internal UnmergedTableCell(bool vmerge, int colspan, IEnumerable<DocumentElement> children)
			{
				Vmerge = vmerge;
				Colspan = colspan;
				Children = children;
			}

			public bool Vmerge { get; }
			public int Colspan { get; }
			public IEnumerable<DocumentElement> Children { get; }

			public override void Accept(IVisitor visitor)
			{
				visitor.Visit(new TableCell(1, Colspan, Children));
			}
		}

		ReadResult ReadHyperlink(XmlElement element)
		{
			string relationshipId = element.GetAttributeOrNone("r:id");
			string anchor = element.GetAttributeOrNone("w:anchor");
			string targetFrame = element.GetAttributeOrNone("w:tgtFrame");
			if (String.IsNullOrEmpty(targetFrame)) targetFrame = null;
			var childrenResult = ReadElements(element.Children);

			if (relationshipId != null)
			{
				string targetHref = relationships.FindTargetByRelationshipId(relationshipId);
				string href = anchor != null ? Uris.ReplaceFragment(targetHref, anchor) : targetHref;
				return childrenResult.Map(children =>
					Hyperlink.CreateHref(href, targetFrame, children));
			}
			else if (anchor != null)
			{
				return childrenResult.Map(children =>
					Hyperlink.CreateAnchor(anchor, targetFrame, children));
			}
			else
			{
				return childrenResult;
			}
		}
		ReadResult ReadBookmark(XmlElement element)
		{
			string name = element.GetAttribute("w:name");
			if (name == "_GoBack")
				return ReadResult.EMPTY_SUCCESS;
			else
				return ReadResult.Success(new Bookmark(name));
		}
		ReadResult ReadNoteReference(NoteType noteType, XmlElement element)
		{
			string noteId = element.GetAttribute("w:id");
			return ReadResult.Success(new NoteReference(noteType, noteId));
		}
		ReadResult ReadCommentReference(XmlElement element)
		{
			string commentId = element.GetAttribute("w:id");
			return ReadResult.Success(new CommentReference(commentId));
		}
		ReadResult ReadPict(XmlElement element)
		{
			return ReadElements(element.Children).ToExtra();
		}
		ReadResult ReadImagedata(XmlElement element)
		{
			string relationshipId = element.GetAttributeOrNone("r:id");
			if (relationshipId == null)
				return ReadResult.EmptyWithWarning("A v:imagedata element without a relationship ID was ignored");

			string title = element.GetAttributeOrNone("o:title");
			string imagePath = RelationshipIdToDocxPath(relationshipId);
			var entry = file.GetEntry(imagePath);
			return entry == null ? ReadResult.EmptyWithWarning(imagePath + " not found") :
				ReadImage(imagePath, title, entry.Open());
		}
		ReadResult ReadInline(XmlElement element)
		{
			var properties = element.FindChildOrEmpty("wp:docPr");
			string altText = properties.GetAttributeOrNone("descr");
			if (String.IsNullOrWhiteSpace(altText)) altText = properties.GetAttributeOrNone("title");
			var blips = element.FindChildren("a:graphic")
				.FindChildren("a:graphicData")
				.FindChildren("pic:pic")
				.FindChildren("pic:blipFill")
				.FindChildren("a:blip");
			return ReadBlips(blips, altText);
		}
		ReadResult ReadBlips(XmlElementList blips, string altText)
		{
			return ReadResult.Join(blips.Select(blip => ReadBlip(blip, altText)));
		}
		ReadResult ReadBlip(XmlElement blip, string altText)
		{
			string embedRelationshipId = blip.GetAttributeOrNone("r:embed");
			string linkRelationshipId = blip.GetAttributeOrNone("r:link");
			if (embedRelationshipId != null)
			{
				string imagePath = RelationshipIdToDocxPath(embedRelationshipId);
				var entry = file.GetEntry(imagePath);
				return entry == null ? ReadResult.EmptyWithWarning(imagePath + " not found") :
					ReadImage(imagePath, altText, entry.Open());
			}
			else if (linkRelationshipId != null)
			{
				string imagePath = relationships.FindTargetByRelationshipId(linkRelationshipId);
				return ReadImage(imagePath, altText);
			}
			else
			{
				return ReadResult.EmptyWithWarning("Could not find image file for a:blip element");
			}
		}
		ReadResult ReadImage(string imagePath, string altText)
		{
			Stream stream;
			try
			{
				stream = fileReader.Open(imagePath);
			}
			catch (IOException exception)
			{
				return ReadResult.EmptyWithWarning(exception.Message);
			}
			return ReadImage(imagePath, altText, stream);
		}
		ReadResult ReadImage(string imagePath, string altText, Stream open)
		{
			string contentType = contentTypes.FindContentType(imagePath);
			var image = new Image(altText, contentType, open);

			string contentTypeString = contentType ?? "(unknown)";
			if (IMAGE_TYPES_SUPPORTED_BY_BROWSERS.Contains(contentTypeString))
				return ReadResult.Success(image);
			else
				return ReadResult.WithWarning(image, $"Image of type {contentTypeString} is unlikely to display in web browsers");
		}
		ReadResult ReadSdt(XmlElement element)
		{
			return ReadElements(element.FindChildOrEmpty("w:sdtContent").Children);
		}
		string RelationshipIdToDocxPath(string relationshipId)
		{
			string target = relationships.FindTargetByRelationshipId(relationshipId);
			return Uris.UriToZipEntryName("word", target);
		}
		string ReadVal(IXmlElementLike element, string name)
		{
			return element.FindChildOrEmpty(name).GetAttributeOrNone("w:val");
		}
	}
}

