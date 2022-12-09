namespace Mammoth.Internal.Styles.Parsing
{
	internal class Token
	{
		internal enum TokenType
		{
			WHITESPACE, IDENTIFIER, SYMBOL, STRING, UNTERMINATED_STRING, INTEGER, EOF, UNKNOWN
		}

		internal Token(int characterIndex, TokenType tokenType, string value)
		{
			this.CharacterIndex = characterIndex;
			this.Type = tokenType;
			this.Value = value;
		}
		public int CharacterIndex { get; }

		public TokenType Type { get; }

		public string Value { get; }

		public override string ToString()
		{
			return $"Token(tokenType={Type}, value={Value})";
		}
	}
}

