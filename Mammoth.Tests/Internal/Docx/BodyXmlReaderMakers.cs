using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Mammoth.Tests;

namespace Mammoth.Internal.Docx.Tests
{
	static class BodyXmlReaderMakers
	{
		class InvalidFileReader : IFileReader
		{
			public Stream Open(string uri) => throw new InvalidOperationException();
		}

		static readonly ZipArchive EmptyArchive = InMemoryArchive.FromStrings(new Dictionary<string, string>());
		static readonly IFileReader InvalidReader = new InvalidFileReader();

		public static BodyXmlReader BodyReader(params object[] args)
		{
			var arguments = new ArgumentValues(args);
			return new BodyXmlReader(
				arguments.Get(Styles.EMPTY),
				arguments.Get(Numbering.EMPTY),
				arguments.Get(Relationships.EMPTY),
				arguments.Get(ContentTypes.DEFAULT),
				arguments.Get(EmptyArchive),
				arguments.Get(InvalidReader));
		}
	}
}
