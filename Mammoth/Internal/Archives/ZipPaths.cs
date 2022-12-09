using System;
using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Archives
{
	internal class ZipPaths
	{
		internal readonly struct ZipPath
		{
			internal ZipPath(string dirname, string basename)
			{
				Dirname = dirname;
				Basename = basename;
			}
			public string Dirname { get; }

			public string Basename { get; }
		}

		public static ZipPath SplitPath(string path)
		{
			int index = path.LastIndexOf('/');
			if (index == -1)
			{
				return new ZipPath("", path);
			}
			else
			{
				string dirname = path.Remove(index);
				string basename = path.Substring(index + 1);
				return new ZipPath(dirname, basename);
			}
		}
		public static string JoinPath(params string[] paths)
		{
			var nonEmptyPaths = paths.Where(p => !String.IsNullOrEmpty(p));
			var relevantPaths = new List<string>();
			foreach (string path in nonEmptyPaths)
			{
				if (path.StartsWith("/", StringComparison.Ordinal))
					relevantPaths.Clear();
				relevantPaths.Add(path);
			}
			return String.Join("/", relevantPaths);
		}
	}
}

