namespace Mammoth.Internal.Documents
{
	internal sealed class NumberingLevel
	{
		internal NumberingLevel(string levelIndex, bool isOrdered)
		{
			LevelIndex = levelIndex;
			IsOrdered = isOrdered;
		}
		public static NumberingLevel Ordered(string levelIndex)
		{
			return new NumberingLevel(levelIndex, true);
		}
		public static NumberingLevel Unordered(string levelIndex)
		{
			return new NumberingLevel(levelIndex, false);
		}

		public string LevelIndex { get; }

		public bool IsOrdered { get; }
	}
}

