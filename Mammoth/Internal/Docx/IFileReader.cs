using System.IO;

namespace Mammoth.Internal.Docx
{
	internal interface IFileReader
	{
		Stream Open(string uri);
	}
}

