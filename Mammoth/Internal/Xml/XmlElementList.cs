using System.Collections.Generic;
using System.Linq;

namespace Mammoth.Internal.Xml
{
	internal class XmlElementList : IEnumerable<XmlElement>
	{
		readonly List<XmlElement> elements;
		internal XmlElementList(IEnumerable<XmlElement> elements)
		{
			this.elements = elements.ToList();
		}
		public IEnumerator<XmlElement> GetEnumerator()
		{
			return elements.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return elements.GetEnumerator();
		}
		public XmlElementList FindChildren(string name)
		{
			return new XmlElementList(elements.SelectMany(
				element => element.FindChildren(name)));
		}
	}
}

