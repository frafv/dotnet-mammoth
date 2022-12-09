using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Mammoth.Internal.Xml.Parsing
{
	internal class XmlParser
	{
		class NodeGenerator : ISimpleSaxHandler
		{
			readonly Stack<XmlElementBuilder> elementStack;
			readonly XmlParser parser;
			internal NodeGenerator(XmlParser parser)
			{
				this.parser = parser;
				this.elementStack = new Stack<XmlElementBuilder>();
			}
			public XmlElement Root => elementStack.First().Build();

			public void StartElement(ElementName name, IDictionary<ElementName, string> attributes)
			{
				var simpleAttributes = attributes.ToDictionary(p => ReadName(p.Key), p => p.Value);
				var element = new XmlElementBuilder(ReadName(name), simpleAttributes);
				this.elementStack.Push(element);
			}
			string ReadName(ElementName name)
			{
				if (String.IsNullOrEmpty(name.Uri))
				{
					return name.LocalName;
				}

				var ns = parser.namespaces.LookupUri(name.Uri);
				if (ns == null) return "{" + name.Uri + "}" + name.LocalName;
				else if (ns.Prefix == null) return name.LocalName;
				else return ns.Prefix + ":" + name.LocalName;
			}
			public void EndElement()
			{
				if (elementStack.Count > 1)
				{
					var element = elementStack.Pop();
					elementStack.Peek().AddChild(element.Build());
				}
			}
			public void Characters(string @string)
			{
				elementStack.Peek().AddChild(new XmlTextNode(@string));
			}
		}

		readonly NamespacePrefixes namespaces;
		internal XmlParser(NamespacePrefixes namespaces)
		{
			this.namespaces = namespaces;
		}
		public XmlElement ParseStream(Stream inputStream)
		{
			var nodeGenerator = new NodeGenerator(this);
			SimpleSax.ParseStream(inputStream, nodeGenerator);
			return nodeGenerator.Root;
		}
		public XmlElement ParseString(string value)
		{
			var nodeGenerator = new NodeGenerator(this);
			SimpleSax.ParseString(value, nodeGenerator);
			return nodeGenerator.Root;
		}
	}
}

