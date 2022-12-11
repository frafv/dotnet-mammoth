using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Mammoth.Tests
{
	static class InMemoryArchive
	{
		public static ZipArchive FromStrings(IDictionary<string, string> entries, ZipArchiveMode mode = ZipArchiveMode.Read)
		{
			var mem = new MemoryStream();
			using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, leaveOpen: true))
			{
				foreach (var entry in entries)
				{
					var file = zip.CreateEntry(entry.Key);
					using (var writer = new StreamWriter(file.Open(), Encoding.UTF8))
						writer.Write(entry.Value);
				}
			}
			mem.Position = 0;
			return new ZipArchive(mem, mode);
		}
	}
}
