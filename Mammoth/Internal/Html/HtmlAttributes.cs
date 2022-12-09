using System;
using System.Collections.Generic;

namespace Mammoth.Internal.Html
{
	internal sealed class HtmlAttributes : Dictionary<string, string>, IEquatable<HtmlAttributes>
	{
		internal HtmlAttributes()
			: base(StringComparer.InvariantCultureIgnoreCase) { }
		HtmlAttributes(IDictionary<string, string> attributes)
			: base(attributes, StringComparer.InvariantCultureIgnoreCase) { }

		public static HtmlAttributes Create(IDictionary<string, string> attributes)
		{
			return attributes != null ? new HtmlAttributes(attributes) : new HtmlAttributes();
		}

		public bool Equals(HtmlAttributes other)
		{
			if (Count != other.Count) return false;
			foreach (var attr in other)
			{
				if (!ContainsKey(attr.Key) ||
					!String.Equals(this[attr.Key], other[attr.Key], StringComparison.InvariantCulture))
					return false;
			}
			return true;
		}
	}
}
