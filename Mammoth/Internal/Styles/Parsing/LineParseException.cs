using System;

namespace Mammoth.Internal.Styles.Parsing
{
	internal class LineParseException : Exception
	{
		internal LineParseException(Token token, string message) : this(token.CharacterIndex, message) { }
		internal LineParseException(int characterIndex, string message) : base(message)
		{
			this.CharacterIndex = characterIndex;
		}

		internal int CharacterIndex { get; }
	}
}
