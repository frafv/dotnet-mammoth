using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class StyleMapParser
	{
		public static StyleMap Parse(string input)
		{
			return ParseStyleMappings(Regex.Split(input, @"\r?\n"));
		}
		public static StyleMap ParseStyleMappings(IEnumerable<string> lines)
		{
			var styleMap = StyleMap.Builder();
			int lineIndex = 0;
			foreach (string line in lines)
			{
				try
				{
					HandleLine(styleMap, line);
				}
				catch (LineParseException exception)
				{
					throw new ParseException(GenerateErrorMessage(line, lineIndex + 1, exception.CharacterIndex, exception.Message));
				}
				lineIndex++;
			}
			return styleMap.Build();
		}
		static void HandleLine(StyleMapBuilder styleMap, string line)
		{
			if (line.StartsWith("#", StringComparison.Ordinal))
				return;
			line = line.Trim();
			if (String.IsNullOrEmpty(line))
				return;

			ParseStyleMapping(line).Invoke(styleMap);
		}
		static Action<StyleMapBuilder> ParseStyleMapping(string line)
		{
			var tokens = StyleMappingTokeniser.Tokenise(line);

			var documentMatcher = DocumentMatcherParser.Parse(tokens);

			tokens.Skip(TokenType.WHITESPACE);
			tokens.Skip(TokenType.SYMBOL, "=>");

			var htmlPath = ParseHtmlPath(tokens);

			tokens.Skip(TokenType.EOF);

			return styleMap => documentMatcher(styleMap, htmlPath);
		}
		static HtmlPath ParseHtmlPath(TokenIterator tokens)
		{
			if (tokens.PeekTokenType() == TokenType.EOF)
			{
				return HtmlPath.EMPTY;
			}
			else
			{
				tokens.Skip(TokenType.WHITESPACE);
				return HtmlPathParser.Parse(tokens);
			}
		}
		static string GenerateErrorMessage(string line, int lineNumber, int characterIndex, string message)
		{
			return $"Error reading style map at line {lineNumber}, character {characterIndex + 1}: {message}\n\n{line}\n{RepeatString(" ", characterIndex)}^";
		}
		static string RepeatString(string value, int times)
		{
			var builder = new StringBuilder();
			for (int i = 0; i < times; i++)
				builder.Append(value);
			return builder.ToString();
		}
	}
}

