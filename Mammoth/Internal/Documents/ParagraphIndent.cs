namespace Mammoth.Internal.Documents
{
	internal class ParagraphIndent
	{
		internal ParagraphIndent(string start = null, string end = null, string firstLine = null, string hanging = null)
		{
			Start = start;
			End = end;
			FirstLine = firstLine;
			Hanging = hanging;
		}
		public string Start { get; }

		public string End { get; }

		public string FirstLine { get; }

		public string Hanging { get; }
	}
}

