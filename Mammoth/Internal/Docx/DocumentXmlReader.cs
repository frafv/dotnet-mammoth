using System.Collections.Generic;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal class DocumentXmlReader
	{
		internal BodyXmlReader bodyReader;
		internal Notes notes;
		internal IEnumerable<Comment> comments;
		internal DocumentXmlReader(BodyXmlReader bodyReader, Notes notes, IEnumerable<Comment> comments)
		{
			this.bodyReader = bodyReader;
			this.notes = notes;
			this.comments = comments;
		}
		public InternalResult<Document> ReadElement(XmlElement element)
		{
			var body = element.FindChildOrEmpty("w:body");
			return bodyReader.ReadElements(body.Children)
				.ToResult()
				.Map(children => new Document(children, notes, comments));
		}
	}
}

