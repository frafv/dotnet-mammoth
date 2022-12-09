using System;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal class CommentXmlReader
	{
		readonly BodyXmlReader bodyReader;
		internal CommentXmlReader(BodyXmlReader bodyReader)
		{
			this.bodyReader = bodyReader;
		}
		public InternalResult<Comment[]> ReadElement(XmlElement element)
		{
			return InternalResult.Join(
				element.FindChildren("w:comment")
					.Select(child => ReadCommentElement(child)));
		}
		public InternalResult<Comment> ReadCommentElement(XmlElement element)
		{
			string commentId = element.GetAttribute("w:id");
			return bodyReader.ReadElements(element.Children)
				.ToResult()
				.Map(children => new Comment(
					commentId,
					children,
					ReadOptionalAttribute(element, "w:author"),
					ReadOptionalAttribute(element, "w:initials")));
		}
		public string ReadOptionalAttribute(XmlElement element, string name)
		{
			string value = element.GetAttributeOrNone(name);
			return String.IsNullOrWhiteSpace(value) ? null : value;
		}
	}
}

