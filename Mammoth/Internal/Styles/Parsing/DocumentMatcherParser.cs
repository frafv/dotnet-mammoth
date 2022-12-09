using System;
using System.Globalization;
using Mammoth.Internal.Documents;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class DocumentMatcherParser
	{
		public static Action<StyleMapBuilder, HtmlPath> Parse(TokenIterator tokens)
		{
			var identifier = tokens.Next(TokenType.IDENTIFIER);
			switch (identifier.Value)
			{
				case "p":
					var paragraph = ParseParagraphMatcher(tokens);
					return (builder, path) => builder.MapParagraph(paragraph, path);
				case "r":
					var run = ParseRunMatcher(tokens);
					return (builder, path) => builder.MapRun(run, path);
				case "table":
					var table = ParseTableMatcher(tokens);
					return (builder, path) => builder.MapTable(table, path);
				case "b":
					return (builder, path) => builder.Bold(path);
				case "i":
					return (builder, path) => builder.Italic(path);
				case "u":
					return (builder, path) => builder.Underline(path);
				case "strike":
					return (builder, path) => builder.Strikethrough(path);
				case "all-caps":
					return (builder, path) => builder.AllCaps(path);
				case "small-caps":
					return (builder, path) => builder.SmallCaps(path);
				case "comment-reference":
					return (builder, path) => builder.CommentReference(path);
				case "br":
					var breakMatcher = ParseBreakMatcher(tokens);
					return (builder, path) => builder.MapBreak(breakMatcher, path);
				default:
					throw new LineParseException(identifier, $"Unrecognised document element: {identifier}");
			}
		}
		static ParagraphMatcher ParseParagraphMatcher(TokenIterator tokens)
		{
			string styleId = ParseStyleId(tokens);
			var styleName = ParseStyleName(tokens);
			var numbering = ParseNumbering(tokens);
			return new ParagraphMatcher(styleId, styleName, numbering);
		}
		static RunMatcher ParseRunMatcher(TokenIterator tokens)
		{
			string styleId = ParseStyleId(tokens);
			var styleName = ParseStyleName(tokens);
			return new RunMatcher(styleId, styleName);
		}
		static TableMatcher ParseTableMatcher(TokenIterator tokens)
		{
			string styleId = ParseStyleId(tokens);
			StringMatcher styleName = ParseStyleName(tokens);
			return new TableMatcher(styleId, styleName);
		}
		static string ParseStyleId(TokenIterator tokens)
		{
			return TokenParser.ParseClassName(tokens);
		}
		static StringMatcher ParseStyleName(TokenIterator tokens)
		{
			if (!tokens.TrySkip(TokenType.SYMBOL, "["))
			{
				return null;
			}

			tokens.Skip(TokenType.IDENTIFIER, "style-name");
			var stringMatcher = ParseStringMatcher(tokens);
			tokens.Skip(TokenType.SYMBOL, "]");
			return stringMatcher;
		}
		static StringMatcher ParseStringMatcher(TokenIterator tokens)
		{
			if (tokens.TrySkip(TokenType.SYMBOL, "="))
			{
				return new EqualToStringMatcher(TokenParser.ParseString(tokens));
			}
			else if (tokens.TrySkip(TokenType.SYMBOL, "^="))
			{
				return new StartsWithStringMatcher(TokenParser.ParseString(tokens));
			}
			else
			{
				throw new LineParseException(tokens.Next(), "Expected string matcher but got token " + tokens.Next().Value);
			}
		}
		static NumberingLevel ParseNumbering(TokenIterator tokens)
		{
			if (!tokens.TrySkip(TokenType.SYMBOL, ":"))
			{
				return null;
			}

			bool isOrdered = ParseListType(tokens);
			tokens.Skip(TokenType.SYMBOL, "(");
			string level = (Int64.Parse(tokens.NextValue(TokenType.INTEGER), CultureInfo.InvariantCulture) - 1L).ToString(CultureInfo.InvariantCulture);
			tokens.Skip(TokenType.SYMBOL, ")");
			return new NumberingLevel(level, isOrdered);
		}
		static bool ParseListType(TokenIterator tokens)
		{
			var listType = tokens.Next(TokenType.IDENTIFIER);
			switch (listType.Value)
			{
				case "ordered-list":
					return true;
				case "unordered-list":
					return false;
				default:
					throw new LineParseException(listType, $"Unrecognised list type: {listType}");
			}
		}
		static BreakMatcher ParseBreakMatcher(TokenIterator tokens)
		{
			tokens.Skip(TokenType.SYMBOL, "[");
			tokens.Skip(TokenType.IDENTIFIER, "type");
			tokens.Skip(TokenType.SYMBOL, "=");
			var stringToken = tokens.Next(TokenType.STRING);
			tokens.Skip(TokenType.SYMBOL, "]");
			string typeName = TokenParser.ParseStringToken(stringToken);
			switch (typeName)
			{
				case "line":
					return BreakMatcher.LINE_BREAK;
				case "page":
					return BreakMatcher.PAGE_BREAK;
				case "column":
					return BreakMatcher.COLUMN_BREAK;
				default:
					throw new LineParseException(stringToken, $"Unrecognised break type: {typeName}");
			}
		}
	}
}

