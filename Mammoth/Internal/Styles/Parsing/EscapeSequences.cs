using System.Text;
using System.Text.RegularExpressions;

namespace Mammoth.Internal.Styles.Parsing
{
	internal static class EscapeSequences
	{
		readonly static Regex PATTERN = new Regex(@"\\(.)", RegexOptions.Compiled);
		public static string Decode(string value)
		{
			var decoded = new StringBuilder();
			int lastIndex = 0;
			for (var matcher = PATTERN.Match(value); matcher.Success; matcher = matcher.NextMatch())
			{
				decoded.Append(value.Substring(lastIndex, matcher.Index - lastIndex));
				decoded.Append(EscapeSequence(matcher.Groups[1].Value));
				lastIndex = matcher.Index + matcher.Length;
			}
			decoded.Append(value.Substring(lastIndex, value.Length - lastIndex));
			return decoded.ToString();
		}
		static char EscapeSequence(string code)
		{
			switch (code)
			{
				case "n":
					return '\n';
				case "r":
					return '\r';
				case "t":
					return '\t';
				default:
					return code[0];
			}
		}
	}
}

