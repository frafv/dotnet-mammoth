using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TokenType = Mammoth.Internal.Styles.Parsing.Token.TokenType;

namespace Mammoth.Internal.Styles.Parsing
{
	internal class RegexTokeniser
	{
		internal class TokenRule
		{
			internal TokenRule(TokenType type, /* language=regex */ string pattern)
			{
				var regex = new Regex(pattern, RegexOptions.Compiled);
				if (regex.Match("").Groups.Count > 1)
				{
					throw new Exception("regex cannot contain any groups");
				}
				this.Type = type;
				this.Regex = regex;
				this.Pattern = pattern;
			}

			public TokenType Type { get; }
			public Regex Regex { get; }
			public string Pattern { get; }
		}

		public static TokenRule Rule(TokenType type, /* language=regex */ string regex)
		{
			return new TokenRule(type, regex);
		}

		readonly Regex pattern;
		readonly TokenType[] rules;
		internal RegexTokeniser(TokenType unknown, params TokenRule[] rules)
		{
			var allRules = new List<TokenRule>(rules)
			{
				Rule(unknown, ".")
			};
			this.pattern = new Regex(String.Join("|", allRules.Select(rule => $"({rule.Pattern})")), RegexOptions.Compiled);
			this.rules = allRules.Select(rule => rule.Type).ToArray();
		}
		public IEnumerable<Token> Tokenise(string value)
		{
			for (var matcher = pattern.Match(value); matcher.Success; matcher = matcher.NextMatch())
			{
				int? groupIndex = null;
				for (int index = 0; index < rules.Length; index++)
					if (matcher.Groups[index + 1].Success)
					{
						groupIndex = index;
					}
				if (groupIndex == null)
				{
					throw new Exception("Could not find group");
				}
				var tokenType = rules[(int)groupIndex];
				yield return new Token(matcher.Index, tokenType, matcher.Value);
			}
		}

	}
}

