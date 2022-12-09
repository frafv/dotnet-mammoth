using System;

namespace Mammoth.Internal.Styles
{
	internal sealed class StartsWithStringMatcher : StringMatcher
	{
		internal StartsWithStringMatcher(string prefix)
		{
			this.Prefix = prefix;
		}

		internal string Prefix { get; }

		public override bool Matches(string value)
		{
			return value.StartsWith(Prefix, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}

