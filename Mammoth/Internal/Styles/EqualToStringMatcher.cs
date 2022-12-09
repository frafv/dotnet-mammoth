using System;

namespace Mammoth.Internal.Styles
{
	internal sealed class EqualToStringMatcher : StringMatcher
	{
		internal EqualToStringMatcher(string value)
		{
			this.Value = value;
		}

		internal string Value { get; }

		public override bool Matches(string value)
		{
			return String.Equals(this.Value, value, StringComparison.InvariantCultureIgnoreCase);
		}
	}
}

