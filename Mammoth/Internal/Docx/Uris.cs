using System;

namespace Mammoth.Internal.Docx
{
	internal static class Uris
	{
		public static string UriToZipEntryName(string @base, string uri)
		{
			return uri.StartsWith("/", StringComparison.Ordinal) ? uri.Substring(1) : @base + "/" + uri;
		}
		public static string ReplaceFragment(string uri, string fragment)
		{
			int hashIndex = uri.IndexOf("#");
			if (hashIndex != -1)
			{
				uri = uri.Remove(hashIndex);
			}
			return uri + "#" + fragment;
		}
	}
}

