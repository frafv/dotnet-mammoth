using System;

namespace Mammoth.Internal.Xml.Parsing
{
	internal sealed class ElementName : IEquatable<ElementName>
	{
		internal ElementName(string uri, string localName)
		{
			Uri = uri;
			LocalName = localName;
		}
		public string Uri { get; }
		public string LocalName { get; }

		public override bool Equals(object obj) => obj is ElementName other && Equals(other);
		public bool Equals(ElementName other)
		{
			return other != null &&
				String.Equals(Uri, other.Uri, StringComparison.InvariantCultureIgnoreCase) &&
				String.Equals(LocalName, other.LocalName, StringComparison.Ordinal);
		}

		public override int GetHashCode()
		{
			return
				Uri.GetHashCode() ^
				LocalName.GetHashCode();
		}
	}
}

