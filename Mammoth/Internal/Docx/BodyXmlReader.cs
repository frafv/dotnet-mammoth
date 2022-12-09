using System.Collections.Generic;
using System.IO.Compression;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal class BodyXmlReader
	{
		readonly Styles styles;
		readonly Numbering numbering;
		readonly Relationships relationships;
		readonly ContentTypes contentTypes;
		readonly ZipArchive file;
		readonly IFileReader fileReader;
		internal BodyXmlReader(
			Styles styles,
			Numbering numbering,
			Relationships relationships,
			ContentTypes contentTypes,
			ZipArchive file,
			IFileReader fileReader)
		{
			this.styles = styles;
			this.numbering = numbering;
			this.relationships = relationships;
			this.contentTypes = contentTypes;
			this.file = file;
			this.fileReader = fileReader;
		}
		public ReadResult ReadElements(IEnumerable<XmlNode> nodes)
		{
			return new StatefulBodyXmlReader(
				styles: styles,
				numbering: numbering,
				relationships: relationships,
				contentTypes: contentTypes,
				file: file,
				fileReader: fileReader
			).ReadElements(nodes);
		}
		public ReadResult ReadElement(XmlElement element)
		{
			return new StatefulBodyXmlReader(
				styles: styles,
				numbering: numbering,
				relationships: relationships,
				contentTypes: contentTypes,
				file: file,
				fileReader: fileReader
			).ReadElement(element);
		}
	}
}

