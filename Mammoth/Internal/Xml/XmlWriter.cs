using System;
using System.Collections.Generic;
using System.IO;

namespace Mammoth.Internal.Xml
{
	internal static class XmlWriter
	{
		public static void Write(TextWriter writer, XmlElement xml, NamespacePrefixes namespaces)
		{
			using (var doc = System.Xml.XmlWriter.Create(writer, new System.Xml.XmlWriterSettings { Indent = false, OmitXmlDeclaration = false }))
			{
				doc.WriteStartDocument(true);
				var visitor = new XmlNodeWriter(doc, namespaces);
				visitor.WriteStartElement(xml.Name);
				visitor.WriteNamespaces();
				visitor.WriteAttributes(xml.Attributes);
				visitor.WriteNodes(xml.Children);
				doc.WriteEndElement();
				doc.WriteEndDocument();
			}
		}

		public static string ToString(XmlElement xml, NamespacePrefixes namespaces)
		{
			var writer = new StringWriter();
			Write(writer, xml, namespaces);
			return writer.ToString();
		}

		class XmlNodeWriter : XmlNode.IVisitor
		{
			readonly System.Xml.XmlWriter writer;
			readonly NamespacePrefixes namespaces;

			public XmlNodeWriter(System.Xml.XmlWriter writer, NamespacePrefixes namespaces)
			{
				this.writer = writer;
				this.namespaces = namespaces;
			}

			public void Visit(XmlElement element)
			{
				WriteStartElement(element.Name);
				WriteAttributes(element.Attributes);
				WriteNodes(element.Children);
				writer.WriteEndElement();
			}

			public void Visit(XmlTextNode textNode)
			{
				writer.WriteString(textNode.Value);
			}

			public void WriteNamespaces()
			{
				foreach (var prefix in namespaces)
					writer.WriteAttributeString("xmlns", prefix.Prefix, null, prefix.Uri);
			}

			public void WriteAttributes(IDictionary<string, string> attributes)
			{
				foreach (var attr in attributes)
				{
					var name = ReadName(attr.Key);
					if (name.Prefix != null)
						writer.WriteAttributeString(name.Prefix, name.LocalName, name.Uri, attr.Value);
					else
						writer.WriteAttributeString(name.LocalName, attr.Value);
				}
			}

			public void WriteNodes(IEnumerable<XmlNode> children)
			{
				foreach (var child in children)
					child.Accept(this);
			}

			public void WriteStartElement(string qname)
			{
				var name = ReadName(qname);
				if (name.Prefix != null)
					writer.WriteStartElement(name.Prefix, name.LocalName, name.Uri);
				else
					writer.WriteStartElement(name.LocalName, namespaces.DefaultNamespace?.Uri);
			}

			XmlName ReadName(string name)
			{
				string[] parts = name.Split(new[] { ':' }, 2);
				if (parts.Length == 1)
				{
					return new XmlName(namespaces.DefaultNamespace, parts[0]);
				}
				else
				{
					string prefix = parts[0];
					var ns = namespaces.LookupPrefix(prefix) ??
						throw new Exception("Could not find namespace for prefix: " + prefix);
					return new XmlName(ns, parts[1]);
				}
			}
		}

		readonly struct XmlName
		{
			public readonly string Prefix;
			public readonly string Uri;
			public readonly string LocalName;

			public XmlName(NamespacePrefix ns, string localName)
			{
				Prefix = ns?.Prefix;
				Uri = ns?.Uri;
				LocalName = localName;
			}
		}
	}
}
