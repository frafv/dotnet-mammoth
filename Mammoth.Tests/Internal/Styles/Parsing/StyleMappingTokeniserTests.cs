using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Styles.Parsing.Token;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	[TestClass()]
	public class StyleMappingTokeniserTests
	{
		[TestMethod()]
		public void UnknownTokensAreTokenised()
		{
			AssertTokens("~", IsToken(TokenType.UNKNOWN, "~"));
		}

		[TestMethod()]
		public void EmptyStringIsTokenisedToNoTokens()
		{
			AssertTokens("");
		}

		[TestMethod()]
		public void WhitespaceIsTokenised()
		{
			AssertTokens(" \t\t  ", IsToken(TokenType.WHITESPACE, " \t\t  "));
		}

		[TestMethod()]
		public void IdentifiersAreTokenised()
		{
			AssertTokens("Overture", IsToken(TokenType.IDENTIFIER, "Overture"));
		}

		[TestMethod()]
		public void EscapeSequencesInIdentifiersAreTokenised()
		{
			AssertTokens("\\:", IsToken(TokenType.IDENTIFIER, "\\:"));
		}

		[TestMethod()]
		public void IntegersAreTokenised()
		{
			AssertTokens("123", IsToken(TokenType.INTEGER, "123"));
		}

		[TestMethod()]
		public void StringsAreTokenised()
		{
			AssertTokens("'Tristan'", IsToken(TokenType.STRING, "'Tristan'"));
		}

		[TestMethod()]
		public void EscapeSequencesInStringsAreTokenised()
		{
			AssertTokens("'Tristan\\''", IsToken(TokenType.STRING, "'Tristan\\''"));
		}

		[TestMethod()]
		public void UgnterminatedStringsAreTokenised()
		{
			AssertTokens("'Tristan", IsToken(TokenType.UNTERMINATED_STRING, "'Tristan"));
		}

		[TestMethod()]
		public void ArrowsAreTokenised()
		{
			AssertTokens("=>=>", IsToken(TokenType.SYMBOL, "=>"), IsToken(TokenType.SYMBOL, "=>"));
		}

		[TestMethod()]
		public void DotsAreTokenised()
		{
			AssertTokens(".", IsToken(TokenType.SYMBOL, "."));
		}

		[TestMethod()]
		public void ColonsAreTokenised()
		{
			AssertTokens("::", IsToken(TokenType.SYMBOL, ":"), IsToken(TokenType.SYMBOL, ":"));
		}

		[TestMethod()]
		public void GreaterThansAreTokenised()
		{
			AssertTokens(">>", IsToken(TokenType.SYMBOL, ">"), IsToken(TokenType.SYMBOL, ">"));
		}

		[TestMethod()]
		public void EqualsAreTokenised()
		{
			AssertTokens("==", IsToken(TokenType.SYMBOL, "="), IsToken(TokenType.SYMBOL, "="));
		}

		[TestMethod()]
		public void StartsWithSymbolsAreTokenised()
		{
			AssertTokens("^=^=", IsToken(TokenType.SYMBOL, "^="), IsToken(TokenType.SYMBOL, "^="));
		}

		[TestMethod()]
		public void OpenParensAreTokenised()
		{
			AssertTokens("((", IsToken(TokenType.SYMBOL, "("), IsToken(TokenType.SYMBOL, "("));
		}

		[TestMethod()]
		public void CloseParensAreTokenised()
		{
			AssertTokens("))", IsToken(TokenType.SYMBOL, ")"), IsToken(TokenType.SYMBOL, ")"));
		}

		[TestMethod()]
		public void OpenSquareBracketsAreTokenised()
		{
			AssertTokens("[[", IsToken(TokenType.SYMBOL, "["), IsToken(TokenType.SYMBOL, "["));
		}

		[TestMethod()]
		public void CloseSquareBracketsAreTokenised()
		{
			AssertTokens("]]", IsToken(TokenType.SYMBOL, "]"), IsToken(TokenType.SYMBOL, "]"));
		}

		[TestMethod()]
		public void ChoicesAreTokenised()
		{
			AssertTokens("||", IsToken(TokenType.SYMBOL, "|"), IsToken(TokenType.SYMBOL, "|"));
		}

		[TestMethod()]
		public void BangsAreTokenised()
		{
			AssertTokens("!!", IsToken(TokenType.SYMBOL, "!"), IsToken(TokenType.SYMBOL, "!"));
		}

		[TestMethod()]
		public void TokeniseMultipleTokens()
		{
			AssertTokens("The Magic Position",
				IsToken(TokenType.IDENTIFIER, "The"),
				IsToken(TokenType.WHITESPACE, " "),
				IsToken(TokenType.IDENTIFIER, "Magic"),
				IsToken(TokenType.WHITESPACE, " "),
				IsToken(TokenType.IDENTIFIER, "Position")
			);
		}

		static void AssertTokens(string input, params Token[] tokens)
		{
			var list = StyleMappingTokeniser.TokeniseToList(input);
			Assert.AreEqual(tokens.Length, list.Count, "Should equal tokens count");
			for (int k = 0; k < tokens.Length; k++)
			{
				var expected = tokens[k];
				var actual = list[k];
				Assert.AreEqual(expected.Type, actual.Type, $"Should equal #{k} token type");
				Assert.AreEqual(expected.Value, actual.Value, $"Should equal #{k} token value");
			}
		}

		static Token IsToken(TokenType tokenType, string value)
		{
			return new Token(0, tokenType, value);
		}
	}
}