using System;
using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Xml
{
	internal class XmlElement : XmlNode, IXmlElementLike
	{
		internal XmlElement(string name, IDictionary<string, string> attributes, IEnumerable<XmlNode> children)
		{
			Name = name;
			Attributes = XmlAttributes.Create(attributes);
			Children = children.ToList();
		}
		public string Name { get; }

		public XmlAttributes Attributes { get; }

		public string GetAttribute(string name)
		{
			return GetAttributeOrNone(name) ??
				throw new Exception($"Element has no '{name}' attribute");
		}
		public string GetAttributeOrNone(string name)
		{
			return Attributes.TryGetValue(name, out string value) ? value : null;
		}
		public override string InnerText => String.Join("", Children.Select(c => c.InnerText));

		public override void Accept(IVisitor visitor)
		{
			visitor.Visit(this);
		}
		public IList<XmlNode> Children { get; }

		public override string ToString()
		{
			return $"XmlElement(name={Name}, attributes count={Attributes.Count}, children count={Children.Count})";
		}
		public XmlElementList FindChildren(string name)
		{
			return new XmlElementList(FindChildrenIterable(name));
		}
		public XmlElement FindChild(string name)
		{
			return FindChildrenIterable(name).FirstOrDefault();
		}
		public bool HasChild(string name)
		{
			return FindChildrenIterable(name).Any();
		}
		public IXmlElementLike FindChildOrEmpty(string name)
		{
			return FindChildrenIterable(name).DefaultIfEmpty(NullXmlElement.INSTANCE).First();
		}
		IEnumerable<XmlElement> FindChildrenIterable(string name)
		{
			return Children
				.OfType<XmlElement>()
				.Where(child => child.Name.Equals(name));
		}
	}
}

