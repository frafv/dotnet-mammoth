namespace Mammoth.Internal.Xml
{
	internal class NamespacePrefix
	{
		internal NamespacePrefix(string prefix, string uri)
		{
			Prefix = prefix;
			Uri = uri;
		}
		internal NamespacePrefix(string uri)
			: this(null, uri)
		{ }
		public string Prefix { get; }
		public string Uri { get; }
	}
}

