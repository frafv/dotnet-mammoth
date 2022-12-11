using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Styles.Parsing.Token;

namespace Mammoth.Internal.Styles.Parsing.Tests
{
	[TestClass()]
	public class TokenParserTests
	{
		[TestMethod()]
		public void EscapeSequencesInIdentifiersAreDecoded()
		{
			string value = TokenParser.ParseClassName(new TokenIterator(new[]
			{
				new Token(0, TokenType.SYMBOL, "."),
				new Token(1, TokenType.IDENTIFIER, @"\:")
			}, new Token(2, TokenType.EOF, "")));
			Assert.AreEqual(":", value);
		}

		[TestMethod()]
		public void EscapeSequencesInStringAreDecoded()
		{
			string value = TokenParser.ParseString(new TokenIterator(new[]
			{
				new Token(0, TokenType.STRING, @"'\n'")
			}, new Token(1, TokenType.EOF, "")));
			Assert.AreEqual("\n", value);
		}
	}
}