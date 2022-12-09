using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mammoth.Internal.Xml;
using Mammoth.Internal.Xml.Parsing;

namespace Mammoth.Internal.Docx
{
	internal class OfficeXml
	{
		static readonly NamespacePrefixes XML_NAMESPACES = NamespacePrefixes.Builder()
			.Put("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")
			.Put("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing")
			.Put("a", "http://schemas.openxmlformats.org/drawingml/2006/main")
			.Put("pic", "http://schemas.openxmlformats.org/drawingml/2006/picture")
			.Put("content-types", "http://schemas.openxmlformats.org/package/2006/content-types")
			.Put("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships")
			.Put("relationships", "http://schemas.openxmlformats.org/package/2006/relationships")
			.Put("v", "urn:schemas-microsoft-com:vml")
			.Put("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006")
			.Put("office-word", "urn:schemas-microsoft-com:office:word")
			.Build();

		public static XmlElement ParseXml(Stream inputStream)
		{
			var parser = new XmlParser(XML_NAMESPACES);
			return CollapseVisitor.Visit(parser.ParseStream(inputStream)).OfType<XmlElement>().First();
		}

		class CollapseVisitor : XmlNode.IVisitor
		{
			IEnumerable<XmlNode> result;
			CollapseVisitor() { }
			public static IEnumerable<XmlNode> Visit(XmlNode node)
			{
				var visitor = new CollapseVisitor();
				node.Accept(visitor);
				return visitor.result;
			}
			void XmlNode.IVisitor.Visit(XmlElement element) => result = Map(element);
			IEnumerable<XmlNode> Map(XmlElement element)
			{
				if (element.Name == "mc:AlternateContent")
				{
					return element.FindChildOrEmpty("mc:Fallback").Children;
				}
				else
				{
					var collapsedElement = new XmlElement(
						element.Name,
						element.Attributes,
						element.Children.SelectMany(child => Visit(child)));
					return new[] { collapsedElement };
				}
			}
			void XmlNode.IVisitor.Visit(XmlTextNode textNode) => result = Map(textNode);
			IEnumerable<XmlNode> Map(XmlTextNode textNode)
			{
				yield return textNode;
			}
		}
	}
}

