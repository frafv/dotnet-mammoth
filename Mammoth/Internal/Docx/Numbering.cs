using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Docx
{
	internal class Numbering
	{
		internal class AbstractNum
		{
			public AbstractNum(IDictionary<string, AbstractNumLevel> levels, string numStyleLink = null)
			{
				Levels = levels;
				NumStyleLink = numStyleLink;
			}

			internal IDictionary<string, AbstractNumLevel> Levels { get; }
			internal string NumStyleLink { get; }
		}

		internal class AbstractNumLevel
		{
			public static AbstractNumLevel Ordered(string levelIndex)
			{
				return new AbstractNumLevel(levelIndex, true, null);
			}

			public static AbstractNumLevel Unordered(string levelIndex)
			{
				return new AbstractNumLevel(levelIndex, false, null);
			}

			internal string LevelIndex { get; }
			internal bool IsOrdered { get; }
			internal string ParagraphStyleId { get; }

			public AbstractNumLevel(string levelIndex, bool isOrdered, string paragraphStyleId = null)
			{
				LevelIndex = levelIndex;
				IsOrdered = isOrdered;
				ParagraphStyleId = paragraphStyleId;
			}

			public NumberingLevel ToNumberingLevel()
			{
				return new NumberingLevel(LevelIndex, IsOrdered);
			}
		}

		internal class Num
		{
			internal string AbstractNumId { get; }

			public Num(string abstractNumId = null)
			{
				AbstractNumId = abstractNumId;
			}
		}

		internal static readonly Numbering EMPTY = new Numbering();

		readonly IDictionary<string, AbstractNum> abstractNums;
		readonly IDictionary<string, AbstractNumLevel> levelsByParagraphStyleId;
		readonly IDictionary<string, Num> nums;
		readonly Styles styles;

		private Numbering()
			: this(
				new Dictionary<string, AbstractNum>(),
				new Dictionary<string, Num>(),
				Styles.EMPTY)
		{ }
		public Numbering(
			IDictionary<string, AbstractNum> abstractNums,
			IDictionary<string, Num> nums,
			Styles styles)
		{
			this.abstractNums = abstractNums;
			levelsByParagraphStyleId = abstractNums.Values
				.SelectMany(abstractNum => abstractNum.Levels.Values)
				.Where(level => level.ParagraphStyleId != null)
				.ToDictionary(level => level.ParagraphStyleId);
			this.nums = nums;
			this.styles = styles;
		}

		public NumberingLevel FindLevel(string numId, string level)
		{
			if (nums.TryGetValue(numId, out var num) &&
				num.AbstractNumId is string abstractNumId &&
				abstractNums.TryGetValue(abstractNumId, out var abstractNum))
			{
				if (abstractNum.NumStyleLink is string numStyleLink)
				{
					var style = styles.FindNumberingStyleById(numStyleLink);
					return style?.NumId is string linkedNumId ? FindLevel(linkedNumId, level) : null;
				}
				else
				{
					return abstractNum.Levels.TryGetValue(level, out var value) ? value.ToNumberingLevel() : null;
				}
			}
			return null;
		}

		public NumberingLevel FindLevelByParagraphStyleId(string styleId)
		{
			return levelsByParagraphStyleId.TryGetValue(styleId, out var value) ? value.ToNumberingLevel() : null;
		}
	}
}

