using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Html;
using Mammoth.Internal.Results;
using Mammoth.Internal.Styles;
using static Mammoth.Internal.Html.Html;
using NoteType = Mammoth.Internal.Documents.Note.NoteType;

namespace Mammoth.Internal.Conversion
{
	internal class DocumentToHtml
	{
		class ReferencedComment
		{
			public ReferencedComment(string label, Comment comment)
			{
				Label = label;
				Comment = comment;
			}

			internal string Label { get; }
			internal Comment Comment { get; }
		}

		class Context
		{
			public Context(bool isHeader)
			{
				IsHeader = isHeader;
			}

			public bool IsHeader { get; }

			public Context AddIsHeader(bool isHeader)
			{
				return new Context(isHeader);
			}
		}

		class ElementConverterVisitor : DocumentElement.IVisitor
		{
			readonly DocumentToHtml owner;
			readonly Context context;
			IEnumerable<HtmlNode> result;
			ElementConverterVisitor(DocumentToHtml owner, Context context)
			{
				this.owner = owner;
				this.context = context;
			}
			public static IEnumerable<HtmlNode> Visit(DocumentToHtml owner, DocumentElement element, Context context)
			{
				var visitor = new ElementConverterVisitor(owner, context);
				element.Accept(visitor);
				return visitor.result;
			}

			IEnumerable<HtmlNode> Map(bool forceWrite)
			{
				if (forceWrite)
					yield return FORCE_WRITE;
			}

			void DocumentElement.IVisitor.Visit(Paragraph paragraph)
			{
				var content = owner.ConvertChildrenToHtml(paragraph, context);
				var children = Map(owner.preserveEmptyParagraphs).Concat(content);
				if (!(owner.styleMap.GetParagraphHtmlPath(paragraph) is HtmlPath mapping))
				{
					if (paragraph.Style is Style style)
						owner.warnings.Add($"Unrecognised paragraph style: {style.Describe()}");
					mapping = HtmlPath.Element("p");
				}
				result = mapping.Wrap(children);
			}

			void DocumentElement.IVisitor.Visit(Run run)
			{
				var nodes = owner.ConvertChildrenToHtml(run, context);
				if (run.IsSmallCaps)
					nodes = (owner.styleMap.SmallCaps ?? HtmlPath.EMPTY).Wrap(nodes);
				if (run.IsAllCaps)
					nodes = (owner.styleMap.AllCaps ?? HtmlPath.EMPTY).Wrap(nodes);
				if (run.IsStrikethrough)
					nodes = (owner.styleMap.Strikethrough ?? HtmlPath.CollapsibleElement("s")).Wrap(nodes);
				if (run.IsUnderline)
					nodes = (owner.styleMap.Underline ?? HtmlPath.EMPTY).Wrap(nodes);
				if (run.VerticalAlignment == Run.RunVerticalAlignment.SUBSCRIPT)
					nodes = HtmlPath.CollapsibleElement("sub").Wrap(nodes);
				if (run.VerticalAlignment == Run.RunVerticalAlignment.SUPERSCRIPT)
					nodes = HtmlPath.CollapsibleElement("sup").Wrap(nodes);
				if (run.IsItalic)
					nodes = (owner.styleMap.Italic ?? HtmlPath.CollapsibleElement("em")).Wrap(nodes);
				if (run.IsBold)
					nodes = (owner.styleMap.Bold ?? HtmlPath.CollapsibleElement("strong")).Wrap(nodes);
				if (owner.styleMap.GetRunHtmlPath(run) is HtmlPath mapping)
					nodes = mapping.Wrap(nodes);
				else if (run.Style is Style style)
					owner.warnings.Add($"Unrecognised run style: {style.Describe()}");
				result = nodes;
			}

			void DocumentElement.IVisitor.Visit(Text text) => result = Map(text.Value);
			IEnumerable<HtmlNode> Map(string text)
			{
				if (text != null)
					yield return Text(text);
			}

			void DocumentElement.IVisitor.Visit(Tab tab) => result = Map(tab);
			IEnumerable<HtmlNode> Map(Tab _)
			{
				yield return Text("\t");
			}

			void DocumentElement.IVisitor.Visit(Break breakElement)
			{
				var mapping = owner.styleMap.GetBreakHtmlPath(breakElement) ??
					(breakElement.Type == Break.BreakType.LINE ? HtmlPath.Element("br") : HtmlPath.EMPTY);
				result = mapping.Wrap(Enumerable.Empty<HtmlNode>());
			}

			void DocumentElement.IVisitor.Visit(Table table)
			{
				var mapping = owner.styleMap.GetTableHtmlPath(table) ??
					HtmlPath.Element("table");
				result = mapping.Wrap(GenerateTableChildren(table));
			}
			IEnumerable<HtmlNode> GenerateTableChildren(Table table)
			{

				var children = new List<DocumentElement>(table.Children);
				int bodyIndex = children.FindIndex(child => !IsHeader(child));
				if (bodyIndex < 0) bodyIndex = children.Count;
				if (bodyIndex == 0)
				{
					return owner.ConvertToHtml(table.Children, context.AddIsHeader(false));
				}
				else
				{
					var headRows = owner.ConvertToHtml(
						children.GetRange(0, bodyIndex),
						context.AddIsHeader(true));
					var bodyRows = owner.ConvertToHtml(
						children.GetRange(bodyIndex, children.Count - bodyIndex),
						context.AddIsHeader(false)
					);
					return new[]
					{
						Element("thead", headRows),
						Element("tbody", bodyRows)
					};
				}
			}
			bool IsHeader(DocumentElement child)
			{
				return child is TableRow row && row.IsHeader;
			}

			void DocumentElement.IVisitor.Visit(TableRow tableRow) => result = Map(tableRow);
			IEnumerable<HtmlNode> Map(TableRow tableRow)
			{
				yield return Element("tr", Map(forceWrite: true).Concat(owner.ConvertChildrenToHtml(tableRow, context)));
			}

			void DocumentElement.IVisitor.Visit(TableCell tableCell) => result = Map(tableCell);
			IEnumerable<HtmlNode> Map(TableCell tableCell)
			{
				string tagName = context.IsHeader ? "th" : "td";
				var attributes = new HtmlAttributes();
				if (tableCell.Colspan != 1)
					attributes.Add("colspan", tableCell.Colspan.ToString(NumberFormatInfo.InvariantInfo));
				if (tableCell.Rowspan != 1)
					attributes.Add("rowspan", tableCell.Rowspan.ToString(NumberFormatInfo.InvariantInfo));
				yield return Element(tagName, attributes,
					Map(forceWrite: true).Concat(owner.ConvertChildrenToHtml(tableCell, context)));
			}

			void DocumentElement.IVisitor.Visit(Hyperlink hyperlink) => result = Map(hyperlink);
			IEnumerable<HtmlNode> Map(Hyperlink hyperlink)
			{
				var attributes = new HtmlAttributes { ["href"] = GenerateHref(hyperlink) };
				if (hyperlink.TargetFrame is string targetFrame)
					attributes.Add("target", targetFrame);

				yield return CollapsibleElement("a", attributes, owner.ConvertChildrenToHtml(hyperlink, context));
			}
			string GenerateHref(Hyperlink hyperlink)
			{
				switch (hyperlink)
				{
					case var h when h.Href is string href:
						return href;
					case var h when h.Anchor is string anchor:
						return "#" + owner.GenerateId(anchor);
					default:
						return "";
				}
			}

			void DocumentElement.IVisitor.Visit(Bookmark bookmark) => result = Map(bookmark);
			IEnumerable<HtmlNode> Map(Bookmark bookmark)
			{
				yield return Element("a", new HtmlAttributes { ["id"] = owner.GenerateId(bookmark.Name) }, Map(forceWrite: true));
			}

			void DocumentElement.IVisitor.Visit(NoteReference noteReference) => result = Map(noteReference);
			IEnumerable<HtmlNode> Map(NoteReference noteReference)
			{
				owner.noteReferences.Add(noteReference);
				string noteAnchor = owner.GenerateNoteHtmlId(noteReference.NoteType, noteReference.NoteId);
				string noteReferenceAnchor = owner.GenerateNoteRefHtmlId(noteReference.NoteType, noteReference.NoteId);
				yield return Element("sup",
					Element("a", new HtmlAttributes { ["href"] = "#" + noteAnchor, ["id"] = noteReferenceAnchor },
						Text($"[{owner.noteReferences.Count}]")));
			}

			void DocumentElement.IVisitor.Visit(CommentReference commentReference)
			{
				var nodes = Map(commentReference);
				if (!(owner.styleMap.CommentReference is HtmlPath mapping))
					mapping = HtmlPath.IGNORE;
				result = mapping.Wrap(nodes);
			}
			IEnumerable<HtmlNode> Map(CommentReference commentReference)
			{
				string commentId = commentReference.CommentId;
				if (!owner.comments.TryGetValue(commentId, out var comment))
					throw new Exception($"Referenced comment could not be found, id: {commentId}");
				string label = $"[{comment.AuthorInitials}{owner.referencedComments.Count + 1}]";
				owner.referencedComments.Add(new ReferencedComment(label, comment));

				// TODO: Remove duplication with note references
				yield return Element(
					"a",
					new HtmlAttributes
					{
						["href"] = "#" + owner.GenerateReferentHtmlId("comment", commentId),
						["id"] = owner.GenerateReferenceHtmlId("comment", commentId),
					},
					Text(label));
			}

			void DocumentElement.IVisitor.Visit(Image image)
			{
				// TODO: custom image handlers
				try
				{
					result = owner.imageConverter.Convert(image);
				}
				catch (IOException exception)
				{
					owner.warnings.Add(exception.Message);
					result = Enumerable.Empty<HtmlNode>();
				}
			}
		}

		readonly string idPrefix;
		readonly bool preserveEmptyParagraphs;
		readonly StyleMap styleMap;
		readonly InternalImageConverter imageConverter;
		readonly IDictionary<string, Comment> comments;
		readonly IList<NoteReference> noteReferences;
		readonly IList<ReferencedComment> referencedComments;
		readonly ISet<string> warnings;
		readonly static Context INITIAL_CONTEXT = new Context(false);
		internal DocumentToHtml(DocumentToHtmlOptions options, IEnumerable<Comment> comments)
		{
			noteReferences = new List<NoteReference>();
			referencedComments = new List<ReferencedComment>();
			warnings = new HashSet<string>();
			idPrefix = options.IdPrefix;
			preserveEmptyParagraphs = options.PreserveEmptyParagraphs;
			styleMap = options.StyleMap;
			imageConverter = options.ImageConverter;
			this.comments = comments.ToDictionary(c => c.CommentId);
		}
		internal DocumentToHtml(DocumentToHtmlOptions options)
			: this(options, Enumerable.Empty<Comment>())
		{ }
		public static InternalResult<HtmlNode[]> ConvertToHtml(Document document, DocumentToHtmlOptions options)
		{
			var documentConverter = new DocumentToHtml(options, document.Comments);
			// iterate all elements
			var result = documentConverter.ConvertToHtml(document, INITIAL_CONTEXT).ToArray();
			return new InternalResult<HtmlNode[]>(result,
				documentConverter.warnings.ToArray());
		}
		public static IEnumerable<Note> FindNotes(Document document, IEnumerable<NoteReference> noteReferences)
		{
			// TODO: handle missing notes
			return noteReferences.Select(reference => document.Notes.FindNote(reference.NoteType, reference.NoteId));
		}
		public static InternalResult<HtmlNode[]> ConvertToHtml(DocumentElement element, DocumentToHtmlOptions options)
		{
			var documentConverter = new DocumentToHtml(options);
			// iterate all elements
			var result = documentConverter.ConvertToHtml(element, INITIAL_CONTEXT).ToArray();
			return new InternalResult<HtmlNode[]>(result,
				documentConverter.warnings.ToArray());
		}
		IEnumerable<HtmlNode> ConvertToHtml(Document document, Context context)
		{
			return ConvertChildrenToHtml(document, context)
				.Concat(AppendNotes(document, context));
		}
		IEnumerable<HtmlNode> AppendNotes(Document document, Context context)
		{
			// TODO: can you have note references inside a note?
			var notes = FindNotes(document, noteReferences);

			if (notes.Any())
				yield return Element("ol", notes.Select(note => ConvertToHtml(note, context)));
			if (referencedComments.Any())
				yield return Element("dl", referencedComments.SelectMany(comment => ConvertToHtml(comment, context)));
		}
		HtmlNode ConvertToHtml(Note note, Context context)
		{
			string id = GenerateNoteHtmlId(note.Type, note.Id);
			string referenceId = GenerateNoteRefHtmlId(note.Type, note.Id);
			var noteBody = ConvertToHtml(note.Body, context);
			// TODO: we probably want this to collapse more eagerly than other collapsible elements
			// -- for instance, any paragraph will probably do, regardless of attributes. (Possible other elements will do too.)
			var backLink = CollapsibleElement("p",
				Text(" "),
				Element("a", new HtmlAttributes { ["href"] = "#" + referenceId }, Text("\u2191")));
			return Element("li", new HtmlAttributes { ["id"] = id }, noteBody.Append(backLink));
		}
		IEnumerable<HtmlNode> ConvertToHtml(ReferencedComment referencedComment, Context context)
		{
			// TODO: remove duplication with notes
			string commentId = referencedComment.Comment.CommentId;
			var body = ConvertToHtml(referencedComment.Comment.Body, context);
			// TODO: we probably want this to collapse more eagerly than other collapsible elements
			// -- for instance, any paragraph will probably do, regardless of attributes. (Possible other elements will do too.)
			var backLink = CollapsibleElement("p",
				Text(" "),
				Element("a", new HtmlAttributes { ["href"] = "#" + GenerateReferenceHtmlId("comment", commentId) }, Text("\u2191")));

			yield return Element("dt",
				new HtmlAttributes { ["id"] = GenerateReferentHtmlId("comment", commentId) },
				Text("Comment " + referencedComment.Label));
			yield return Element("dd",
				body.Append(backLink));
		}
		IEnumerable<HtmlNode> ConvertToHtml(IEnumerable<DocumentElement> elements, Context context)
		{
			return elements.SelectMany(element => ConvertToHtml(element, context));
		}
		IEnumerable<HtmlNode> ConvertChildrenToHtml(IHasChildren element, Context context)
		{
			return ConvertToHtml(element.Children, context);
		}
		IEnumerable<HtmlNode> ConvertToHtml(DocumentElement element, Context context)
		{
			return ElementConverterVisitor.Visit(this, element, context);
		}
		string GenerateNoteHtmlId(NoteType noteType, string noteId)
		{
			return GenerateReferentHtmlId(NoteTypeToIdFragment(noteType), noteId);
		}
		string GenerateNoteRefHtmlId(NoteType noteType, string noteId)
		{
			return GenerateReferenceHtmlId(NoteTypeToIdFragment(noteType), noteId);
		}
		string GenerateReferentHtmlId(string referenceType, string referenceId)
		{
			return GenerateId(referenceType + "-" + referenceId);
		}
		string GenerateReferenceHtmlId(string referenceType, string referenceId)
		{
			return GenerateId(referenceType + "-ref-" + referenceId);
		}
		string NoteTypeToIdFragment(NoteType noteType)
		{
			switch (noteType)
			{
				case NoteType.FOOTNOTE:
					return "footnote";
				case NoteType.ENDNOTE:
					return "endnote";
				default:
					throw new InvalidOperationException();
			}
		}
		string GenerateId(string bookmarkName)
		{
			return idPrefix + bookmarkName;
		}
	}
}

