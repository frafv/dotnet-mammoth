using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class TokenParser
	{
		public static string ParseClassName(TokenIterator tokens)
		{
			if (tokens.TrySkip(TokenType.SYMBOL, "."))
				return ParseIdentifier(tokens);
			else
				return null;
		}
		public static string ParseIdentifier(TokenIterator tokens)
		{
			return EscapeSequences.Decode(tokens.NextValue(TokenType.IDENTIFIER));
		}
		public static string ParseString(TokenIterator tokens)
		{
			return ParseStringToken(tokens.Next(TokenType.STRING));
		}
		public static string ParseStringToken(Token token)
		{
			string value = token.Value;
			return EscapeSequences.Decode(value.Substring(1, value.Length - 2));
		}
	}
}

