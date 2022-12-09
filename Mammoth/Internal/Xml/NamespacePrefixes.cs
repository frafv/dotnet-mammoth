using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Xml
{
	internal class NamespacePrefixes : IEnumerable<NamespacePrefix>
	{
		internal class NamespacePrefixesBuilder
		{
			readonly IDictionary<string, NamespacePrefix> uriToPrefix = new Dictionary<string, NamespacePrefix>();
			public NamespacePrefixesBuilder Put(string prefix, string uri)
			{
				uriToPrefix.Add(uri, new NamespacePrefix(prefix, uri));
				return this;
			}
			public NamespacePrefixesBuilder DefaultPrefix(string uri)
			{
				uriToPrefix.Add(uri, new NamespacePrefix(uri: uri));
				return this;
			}
			public NamespacePrefixes Build()
			{
				return new NamespacePrefixes(uriToPrefix);
			}
		}

		readonly IDictionary<string, NamespacePrefix> uriToPrefix;
		internal NamespacePrefixes(IDictionary<string, NamespacePrefix> uriToPrefix)
		{
			this.uriToPrefix = uriToPrefix;
		}

		internal static NamespacePrefixesBuilder Builder()
		{
			return new NamespacePrefixesBuilder();
		}

		public NamespacePrefix LookupUri(string uri)
		{
			return uriToPrefix.TryGetValue(uri, out var prefix) ? prefix : null;
		}
		public NamespacePrefix DefaultNamespace => LookupPrefix(null);
		public NamespacePrefix LookupPrefix(string prefix)
		{
			return uriToPrefix.Values.FirstOrDefault(ns => ns.Prefix == prefix);
		}
		IEnumerator<NamespacePrefix> IEnumerable<NamespacePrefix>.GetEnumerator()
		{
			return uriToPrefix.Values.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return uriToPrefix.Values.GetEnumerator();
		}
	}
}

