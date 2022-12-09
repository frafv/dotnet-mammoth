using System.Collections.Generic;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Docx
{
	internal class Styles
	{
		internal static readonly Styles EMPTY = new Styles();
		readonly IDictionary<string, Style> paragraphStyles;
		readonly IDictionary<string, Style> characterStyles;
		readonly IDictionary<string, Style> tableStyles;
		readonly IDictionary<string, NumberingStyle> numberingStyles;
		private Styles()
			: this(
				new Dictionary<string, Style>(),
				new Dictionary<string, Style>(),
				new Dictionary<string, Style>(),
				new Dictionary<string, NumberingStyle>())
		{ }
		internal Styles(
			IDictionary<string, Style> paragraphStyles,
			IDictionary<string, Style> characterStyles,
			IDictionary<string, Style> tableStyles,
			IDictionary<string, NumberingStyle> numberingStyles)
		{
			this.paragraphStyles = paragraphStyles;
			this.characterStyles = characterStyles;
			this.tableStyles = tableStyles;
			this.numberingStyles = numberingStyles;
		}
		public Style FindParagraphStyleById(string id)
		{
			return paragraphStyles.TryGetValue(id, out var style) ? style : null;
		}
		public Style FindCharacterStyleById(string id)
		{
			return characterStyles.TryGetValue(id, out var style) ? style : null;
		}
		public Style FindTableStyleById(string id)
		{
			return tableStyles.TryGetValue(id, out var style) ? style : null;
		}
		public NumberingStyle FindNumberingStyleById(string id)
		{
			return numberingStyles.TryGetValue(id, out var style) ? style : null;
		}
	}
}

