using System;
using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Html;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class HtmlPathParser
	{
		public static HtmlPath Parse(TokenIterator tokens)
		{
			if (tokens.TrySkip(TokenType.SYMBOL, "!"))
				return HtmlPath.IGNORE;
			else
				return new HtmlPathElements(ParseHtmlPathElements(tokens));
		}
		static IEnumerable<HtmlPathElement> ParseHtmlPathElements(TokenIterator tokens)
		{
			if (tokens.PeekTokenType() == TokenType.IDENTIFIER)
			{
				var element = ParseElement(tokens);
				yield return element;
				while (tokens.PeekTokenType() == TokenType.WHITESPACE && tokens.IsNext(1, TokenType.SYMBOL, ">"))
				{
					tokens.Skip(TokenType.WHITESPACE);
					tokens.Skip(TokenType.SYMBOL, ">");
					tokens.Skip(TokenType.WHITESPACE);
					yield return ParseElement(tokens);
				}
			}
		}
		static HtmlPathElement ParseElement(TokenIterator tokens)
		{
			var tagNames = ParseTagNames(tokens).ToList();
			var classNames = ParseClassNames(tokens).ToList();
			var attributes = !classNames.Any() ?
				new HtmlAttributes() :
				new HtmlAttributes { ["class"] = String.Join(" ", classNames) };
			bool isFresh = ParseIsFresh(tokens);
			string separator = ParseSeparator(tokens);
			return new HtmlPathElement(new HtmlTag(tagNames, attributes, !isFresh, separator));
		}
		static IEnumerable<string> ParseTagNames(TokenIterator tokens)
		{
			yield return TokenParser.ParseIdentifier(tokens);
			while (tokens.TrySkip(TokenType.SYMBOL, "|"))
				yield return TokenParser.ParseIdentifier(tokens);
		}
		static IEnumerable<string> ParseClassNames(TokenIterator tokens)
		{
			while (true)
			{
				string className = TokenParser.ParseClassName(tokens);
				if (String.IsNullOrEmpty(className))
					yield break;

				yield return className;
			}
		}
		static bool ParseIsFresh(TokenIterator tokens)
		{
			return tokens.TryParse(() =>
			{
				tokens.Skip(TokenType.SYMBOL, ":");
				tokens.Skip(TokenType.IDENTIFIER, "fresh");
			});
		}
		static string ParseSeparator(TokenIterator tokens)
		{
			bool isSeparator = tokens.TryParse(() =>
			{
				tokens.Skip(TokenType.SYMBOL, ":");
				tokens.Skip(TokenType.IDENTIFIER, "separator");
			});
			if (!isSeparator)
				return "";

			tokens.Skip(TokenType.SYMBOL, "(");
			string value = TokenParser.ParseString(tokens);
			tokens.Skip(TokenType.SYMBOL, ")");
			return value;
		}
	}
}

