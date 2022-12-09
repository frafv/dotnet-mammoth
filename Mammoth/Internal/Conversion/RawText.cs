using System;
using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;

namespace Mammoth.Internal.Conversion
{
	internal static class RawText
	{
		public static string ExtractRawText(Document document)
		{
			return ExtractRawText(document.Children);
		}

		static string ExtractRawText(IEnumerable<DocumentElement> nodes)
		{
			return String.Join("", nodes.Select(node => ExtractRawText(node)));
		}

		public static string ExtractRawText(DocumentElement node)
		{
			switch (node)
			{
				case Text textNode:
					return textNode.Value;
				case Tab _:
					return "\t";
				case Paragraph paragraph:
					return ExtractRawText(paragraph.Children) + "\n\n";
				case IHasChildren parent:
					return ExtractRawText(parent.Children);
				default:
					return "";
			}
		}
	}
}
