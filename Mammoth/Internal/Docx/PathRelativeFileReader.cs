using System;
using System.IO;

namespace Mammoth.Internal.Docx
{
	internal class PathRelativeFileReader : IFileReader
	{
		readonly Uri path;
		internal PathRelativeFileReader(Uri path = null)
		{
			this.path = path;
		}
		public Stream Open(string uri)
		{
			try
			{
				if (IsAbsoluteUri(uri))
				{
					return OpenRead(uri);
				}
				else if (path != null)
				{
					return OpenRead(Resolve(path, uri));
				}
				else
				{
					throw new ArgumentException("Path of document is unknown, but is required for relative URI");
				}
			}
			catch (Exception exception)
			{
				throw new IOException($"Could not open external image '{uri}': {exception.Message}");
			}
		}
		static Stream OpenRead(string uri)
		{
			return File.OpenRead(uri);
		}
		static bool IsAbsoluteUri(string uriString)
		{
			return Uri.TryCreate(uriString, UriKind.Absolute, out _);
		}
		static string Resolve(Uri path, string uri)
		{
			if (IsAbsoluteUri(uri))
			{
				return uri;
			}
			else if (path.IsAbsoluteUri)
			{
				return new Uri(path, uri).AbsolutePath;
			}
			else
			{
				string basePath = Path.GetDirectoryName(path.ToString());
				return Path.Combine(basePath, uri);
			}
		}
	}
}

