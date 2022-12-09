using System.Collections.Generic;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Styles
{
	internal sealed class StyleMapBuilder
	{
		HtmlPath underline;
		HtmlPath strikethrough;
		HtmlPath smallCaps;
		HtmlPath allCaps;
		HtmlPath bold;
		HtmlPath italic;
		HtmlPath commentReference;
		readonly IList<StyleMapping<Paragraph>> paragraphStyles = new List<StyleMapping<Paragraph>>();
		readonly IList<StyleMapping<Run>> runStyles = new List<StyleMapping<Run>>();
		readonly IList<StyleMapping<Table>> tableStyles = new List<StyleMapping<Table>>();
		readonly IList<StyleMapping<Break>> breakStyles = new List<StyleMapping<Break>>();
		public StyleMapBuilder Bold(HtmlPath path)
		{
			bold = path;
			return this;
		}
		public StyleMapBuilder Italic(HtmlPath path)
		{
			italic = path;
			return this;
		}
		public StyleMapBuilder Underline(HtmlPath path)
		{
			underline = path;
			return this;
		}
		public StyleMapBuilder Strikethrough(HtmlPath path)
		{
			strikethrough = path;
			return this;
		}
		public StyleMapBuilder AllCaps(HtmlPath path)
		{
			allCaps = path;
			return this;
		}
		public StyleMapBuilder SmallCaps(HtmlPath path)
		{
			smallCaps = path;
			return this;
		}
		public StyleMapBuilder CommentReference(HtmlPath path)
		{
			commentReference = path;
			return this;
		}
		public StyleMapBuilder MapParagraph(ParagraphMatcher matcher, HtmlPath path)
		{
			paragraphStyles.Add(new StyleMapping<Paragraph>(matcher, path));
			return this;
		}
		public StyleMapBuilder MapRun(RunMatcher matcher, HtmlPath path)
		{
			runStyles.Add(new StyleMapping<Run>(matcher, path));
			return this;
		}
		public StyleMapBuilder MapTable(TableMatcher matcher, HtmlPath path)
		{
			tableStyles.Add(new StyleMapping<Table>(matcher, path));
			return this;
		}
		public StyleMapBuilder MapBreak(BreakMatcher matcher, HtmlPath path)
		{
			breakStyles.Add(new StyleMapping<Break>(matcher, path));
			return this;
		}
		public StyleMap Build()
		{
			return new StyleMap(
				bold: bold,
				italic: italic,
				underline: underline,
				strikethrough: strikethrough,
				allCaps: allCaps,
				smallCaps: smallCaps,
				commentReference: commentReference,
				paragraphStyles: paragraphStyles,
				runStyles: runStyles,
				tableStyles: tableStyles,
				breakStyles: breakStyles);
		}
	}
}

