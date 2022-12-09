using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class StyleMap
	{
		internal static readonly StyleMap EMPTY = new StyleMapBuilder().Build();

		internal StyleMap(
			HtmlPath bold,
			HtmlPath italic,
			HtmlPath underline,
			HtmlPath strikethrough,
			HtmlPath allCaps,
			HtmlPath smallCaps,
			HtmlPath commentReference,
			IEnumerable<StyleMapping<Paragraph>> paragraphStyles,
			IEnumerable<StyleMapping<Run>> runStyles,
			IEnumerable<StyleMapping<Table>> tableStyles,
			IEnumerable<StyleMapping<Break>> breakStyles)
		{
			this.Bold = bold;
			this.Italic = italic;
			this.Underline = underline;
			this.Strikethrough = strikethrough;
			this.AllCaps = allCaps;
			this.SmallCaps = smallCaps;
			this.CommentReference = commentReference;
			this.ParagraphStyles = paragraphStyles;
			this.RunStyles = runStyles;
			this.TableStyles = tableStyles;
			this.BreakStyles = breakStyles;
		}
		public static StyleMapBuilder Builder()
		{
			return new StyleMapBuilder();
		}
		public static StyleMap Merge(StyleMap high, StyleMap low)
		{
			// TODO: add appropriate tests
			return new StyleMap(
				bold: high.Bold ?? low.Bold,
				italic: high.Italic ?? low.Italic,
				underline: high.Underline ?? low.Underline,
				strikethrough: high.Strikethrough ?? low.Strikethrough,
				allCaps: high.AllCaps ?? low.AllCaps,
				smallCaps: high.SmallCaps ?? low.SmallCaps,
				commentReference: high.CommentReference ?? low.CommentReference,
				paragraphStyles: high.ParagraphStyles.Concat(low.ParagraphStyles).ToList(),
				runStyles: high.RunStyles.Concat(low.RunStyles).ToList(),
				tableStyles: high.TableStyles.Concat(low.TableStyles).ToList(),
				breakStyles: high.BreakStyles.Concat(low.BreakStyles).ToList());
		}
		public StyleMap Update(StyleMap styleMap)
		{
			return Merge(styleMap, this);
		}
		public HtmlPath Bold { get; }
		public HtmlPath Italic { get; }
		public HtmlPath Underline { get; }
		public HtmlPath Strikethrough { get; }
		public HtmlPath AllCaps { get; }
		public HtmlPath SmallCaps { get; }
		public HtmlPath CommentReference { get; }
		internal IEnumerable<StyleMapping<Paragraph>> ParagraphStyles { get; }
		internal IEnumerable<StyleMapping<Run>> RunStyles { get; }
		internal IEnumerable<StyleMapping<Table>> TableStyles { get; }
		internal IEnumerable<StyleMapping<Break>> BreakStyles { get; }

		public HtmlPath GetParagraphHtmlPath(Paragraph paragraph)
		{
			return ParagraphStyles.FirstOrDefault(styleMapping => styleMapping.Matches(paragraph))?.HtmlPath;
		}
		public HtmlPath GetRunHtmlPath(Run run)
		{
			return RunStyles.FirstOrDefault(styleMapping => styleMapping.Matches(run))?.HtmlPath;
		}
		public HtmlPath GetTableHtmlPath(Table table)
		{
			return TableStyles.FirstOrDefault(styleMapping => styleMapping.Matches(table))?.HtmlPath;
		}
		public HtmlPath GetBreakHtmlPath(Break breakElement)
		{
			return BreakStyles.FirstOrDefault(styleMapping => styleMapping.Matches(breakElement))?.HtmlPath;
		}
	}
}

