using System;
using System.Collections.Generic;

namespace Mammoth.Internal.Xml
{
	internal sealed class XmlAttributes : Dictionary<string, string>
	{
		internal XmlAttributes()
			: base(StringComparer.InvariantCulture) { }
		XmlAttributes(IDictionary<string, string> attributes)
			: base(attributes, StringComparer.InvariantCulture) { }

		public static XmlAttributes Create(IDictionary<string, string> attributes)
		{
			return attributes != null ? new XmlAttributes(attributes) : new XmlAttributes();
		}
	}
}
