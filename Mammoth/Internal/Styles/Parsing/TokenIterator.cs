using System;
using System.Collections.Generic;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal class TokenIterator
	{
		readonly IList<Token> tokens;
		readonly Token end;
		int index;
		internal TokenIterator(IList<Token> tokens, Token end)
		{
			this.tokens = tokens;
			this.end = end;
			this.index = 0;
		}
		public bool IsNext(int offset, TokenType tokenType, string value)
		{
			int tokenIndex = index + offset;
			var token = this[tokenIndex];
			return token.Type == tokenType && token.Value == value;
		}
		public bool IsNext(TokenType tokenType, string value)
		{
			return IsNext(0, tokenType, value);
		}
		public bool TrySkip(TokenType tokenType, string value)
		{
			if (!IsNext(tokenType, value))
				return false;
			Skip();
			return true;
		}
		public TokenType PeekTokenType()
		{
			return this[index].Type;
		}
		public Token Next()
		{
			var token = this[index];
			index++;
			return token;
		}
		public Token Next(TokenType type)
		{
			var token = this[index];
			if (token.Type != type)
			{
				throw UnexpectedTokenType(type, token);
			}
			index++;
			return token;
		}
		public string NextValue(TokenType type)
		{
			return Next(type).Value;
		}
		public void Skip()
		{
			index++;
		}
		public void Skip(TokenType tokenType)
		{
			var token = this[index];
			if (token.Type != tokenType)
			{
				throw UnexpectedTokenType(tokenType, token);
			}
			index++;
		}
		public void Skip(TokenType tokenType, string tokenValue)
		{
			var token = this[index];
			if (token.Type != tokenType)
			{
				throw UnexpectedTokenType(tokenType, token);
			}
			string actualValue = token.Value;
			if (actualValue != tokenValue)
			{
				throw new LineParseException(token, $"Expected {tokenType} token with value {tokenValue} but value was {actualValue}");
			}
			index++;
		}
		LineParseException UnexpectedTokenType(TokenType expected, Token actual)
		{
			return new LineParseException(actual, $"Expected token of type {expected} but was of type {actual.Type}");
		}
		public bool TryParse(Action action)
		{
			int originalIndex = this.index;
			try
			{
				action();
				return true;
			}
			catch (LineParseException)
			{
				index = originalIndex;
				return false;
			}
		}
		Token this[int index] => index < tokens.Count ? tokens[index] : end;
	}
}

