using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Mammoth.Internal.Xml;
using Mammoth.Internal.Xml.Parsing;

namespace Mammoth.Internal.Docx
{
	internal class EmbeddedStyleMap
	{
		readonly static string STYLE_MAP_PATH = "mammoth/style-map";
		readonly static string ABSOLUTE_STYLE_MAP_PATH = "/" + STYLE_MAP_PATH;
		readonly static string RELATIONSHIPS_PATH = "word/_rels/document.xml.rels";
		readonly static string CONTENT_TYPES_PATH = "[Content_Types].xml";
		internal readonly static NamespacePrefixes RELATIONSHIPS_NAMESPACES = NamespacePrefixes.Builder()
			.DefaultPrefix("http://schemas.openxmlformats.org/package/2006/relationships")
			.Build();
		internal static readonly NamespacePrefixes CONTENT_TYPES_NAMESPACES = NamespacePrefixes.Builder()
			.DefaultPrefix("http://schemas.openxmlformats.org/package/2006/content-types")
			.Build();
		public static string ReadStyleMap(ZipArchive file)
		{
			var entry = file.GetEntry(STYLE_MAP_PATH);
			if (entry == null) return null;
			using (var reader = new StreamReader(entry.Open()))
				return reader.ReadToEnd();
		}
		public static void EmbedStyleMap(ZipArchive archive, string styleMap)
		{
			var entry = archive.GetEntry(STYLE_MAP_PATH) ?? archive.CreateEntry(STYLE_MAP_PATH);
			using (var writer = new StreamWriter(entry.Open()))
				writer.Write(styleMap);
			UpdateRelationships(archive);
			UpdateContentTypes(archive);
		}
		static void UpdateRelationships(ZipArchive archive)
		{
			var parser = new XmlParser(RELATIONSHIPS_NAMESPACES);
			XmlElement relationships;
			var entry = archive.GetEntry(RELATIONSHIPS_PATH) ??
				throw new IOException(RELATIONSHIPS_PATH + " not found");
			using (var stream = entry.Open())
				relationships = parser.ParseStream(stream);
			var relationship = XmlNodes.Element("Relationship", new XmlAttributes
			{
				{ "Id", "rMammothStyleMap" },
				{ "Type", "http://schemas.zwobble.org/mammoth/style-map" },
				{ "Target", ABSOLUTE_STYLE_MAP_PATH }
			});
			var updatedRelationships = UpdateOrAddElement(relationships, relationship, "Id");
			using (var writer = new StreamWriter(entry.Open()))
				XmlWriter.Write(writer, updatedRelationships, RELATIONSHIPS_NAMESPACES);
		}
		static void UpdateContentTypes(ZipArchive archive)
		{
			var parser = new XmlParser(CONTENT_TYPES_NAMESPACES);
			XmlElement contentTypes;
			var entry = archive.GetEntry(CONTENT_TYPES_PATH) ??
				throw new IOException(CONTENT_TYPES_PATH + " not found");
			using (var stream = entry.Open())
				contentTypes = parser.ParseStream(stream);
			var @override = XmlNodes.Element("Override", new XmlAttributes
			{
				{ "PartName", ABSOLUTE_STYLE_MAP_PATH },
				{ "ContentType", "text/prs.mammoth.style-map" }
			});
			var updatedRelationships = UpdateOrAddElement(contentTypes, @override, "PartName");
			using (var writer = new StreamWriter(entry.Open()))
				XmlWriter.Write(writer, updatedRelationships, CONTENT_TYPES_NAMESPACES);
		}
		static XmlElement UpdateOrAddElement(XmlElement parent, XmlElement element, string identifyingAttribute)
		{
			var dup = parent.Children
				.OfType<XmlElement>()
				.FirstOrDefault(
					child => child.Name == element.Name &&
					child.GetAttributeOrNone(identifyingAttribute) == element.GetAttributeOrNone(identifyingAttribute));

			var children = new List<XmlNode>(parent.Children);
			if (dup != null)
			{
				int index = children.IndexOf(dup);
				children[index] = element;
			}
			else
			{
				children.Add(element);
			}
			return new XmlElement(parent.Name, parent.Attributes, children);
		}
	}
}

