using System.Collections.Generic;
using System.Linq;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class StyleMappingTokeniser
	{
		public static TokenIterator Tokenise(string line)
		{
			return new TokenIterator(
				TokeniseToList(line),
				new Token(line.Length, TokenType.EOF, ""));
		}
		public static IList<Token> TokeniseToList(string line)
		{
			// language=regex
			string stringPrefix = @"'(?:(?:\\.|[^'])*)";
			// language=regex
			string identifierCharacter = @"(?:[a-zA-Z\-_]|\\.)";

			var tokeniser = new RegexTokeniser(
				TokenType.UNKNOWN,
				RegexTokeniser.Rule(TokenType.IDENTIFIER, $"{identifierCharacter}(?:{identifierCharacter}|[0-9])*"),
				RegexTokeniser.Rule(TokenType.SYMBOL, /* language=regex */ @":|>|=>|\^=|=|\(|\)|\[|\]|\||!|\."),
				RegexTokeniser.Rule(TokenType.WHITESPACE, /* language=regex */ "\\s+"),
				RegexTokeniser.Rule(TokenType.STRING, stringPrefix + "'"),
				RegexTokeniser.Rule(TokenType.UNTERMINATED_STRING, stringPrefix),
				RegexTokeniser.Rule(TokenType.INTEGER, /* language=regex */ @"[0-9]+"));
			return tokeniser.Tokenise(line).ToList();
		}
	}
}

