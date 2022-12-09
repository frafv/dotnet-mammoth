using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mammoth.Internal.Html
{
	internal static class Html
	{
		internal static readonly HtmlNode FORCE_WRITE = HtmlForceWrite.FORCE_WRITE;
		public static string Write(IEnumerable<HtmlNode> nodes)
		{
			var builder = new StringBuilder();
			foreach (var node in nodes)
				HtmlWriter.Write(node, builder);
			return builder.ToString();
		}

		public static HtmlNode Text(string value)
		{
			return new HtmlTextNode(value);
		}

		public static HtmlNode Element(string tagName, params HtmlNode[] children) =>
			Element(tagName, (IEnumerable<HtmlNode>)children);
		public static HtmlNode Element(string tagName, IEnumerable<HtmlNode> children)
		{
			return Element(tagName, null, children);
		}
		public static HtmlNode Element(string tagName, IDictionary<string, string> attributes, params HtmlNode[] children) =>
			Element(tagName, attributes, (IEnumerable<HtmlNode>)children);
		public static HtmlNode Element(string tagName, IDictionary<string, string> attributes, IEnumerable<HtmlNode> children)
		{
			return new HtmlElement(new HtmlTag(tagName, attributes), children);
		}
		public static HtmlNode CollapsibleElement(params string[] tagNames)
		{
			return CollapsibleElement(tagNames, null);
		}
		public static HtmlNode CollapsibleElement(string tagName, params HtmlNode[] children)
		{
			return CollapsibleElement(tagName, null, children);
		}
		public static HtmlNode CollapsibleElement(string tagName, IDictionary<string, string> attributes, params HtmlNode[] children) =>
			CollapsibleElement(tagName, attributes, (IEnumerable<HtmlNode>)children);
		public static HtmlNode CollapsibleElement(string tagName, IDictionary<string, string> attributes, IEnumerable<HtmlNode> children)
		{
			return CollapsibleElement(new[] { tagName }, attributes, children);
		}
		public static HtmlNode CollapsibleElement(IEnumerable<string> tagNames, IDictionary<string, string> attributes, params HtmlNode[] children) =>
			CollapsibleElement(tagNames, attributes, (IEnumerable<HtmlNode>)children);
		public static HtmlNode CollapsibleElement(IEnumerable<string> tagNames, IDictionary<string, string> attributes, IEnumerable<HtmlNode> children)
		{
			return new HtmlElement(new HtmlTag(tagNames, attributes, isCollapsible: true), children);
		}
		public static IEnumerable<HtmlNode> StripEmpty(IEnumerable<HtmlNode> nodes)
		{
			return nodes.SelectMany(node => StripEmptyVisitor.Visit(node));
		}
		class StripEmptyVisitor : HtmlNode.IVisitor
		{
			IEnumerable<HtmlNode> result;
			StripEmptyVisitor() { }
			public static IEnumerable<HtmlNode> Visit(HtmlNode node)
			{
				var visitor = new StripEmptyVisitor();
				node.Accept(visitor);
				return visitor.result;
			}
			void HtmlNode.IVisitor.Visit(HtmlElement element) => result = Map(element);
			IEnumerable<HtmlNode> Map(HtmlElement element)
			{
				var children = StripEmpty(element.Children);
				if (children.Any() || element.IsVoid)
					yield return new HtmlElement(element.Tag, children);
			}
			void HtmlNode.IVisitor.Visit(HtmlTextNode node) => result = Map(node);
			IEnumerable<HtmlNode> Map(HtmlTextNode node)
			{
				if (!String.IsNullOrEmpty(node.Value))
					yield return node;
			}
			void HtmlNode.IVisitor.Visit(HtmlForceWrite forceWrite) => result = Map(forceWrite);
			IEnumerable<HtmlNode> Map(HtmlForceWrite forceWrite)
			{
				yield return forceWrite;
			}
		}
		public static IList<HtmlNode> Collapse(IEnumerable<HtmlNode> nodes)
		{
			var collapsed = new List<HtmlNode>();
			foreach (var node in nodes)
				CollapsingAdd(collapsed, node);
			return collapsed;
		}
		static void CollapsingAdd(IList<HtmlNode> collapsed, HtmlNode node)
		{
			var collapsedNode = CollapseVisitor.Visit(node);
			if (!TryCollapse(collapsed, collapsedNode))
				collapsed.Add(collapsedNode);
		}
		class CollapseVisitor : HtmlNode.IVisitor
		{
			HtmlNode result;
			CollapseVisitor() { }
			public static HtmlNode Visit(HtmlNode node)
			{
				var visitor = new CollapseVisitor();
				node.Accept(visitor);
				return visitor.result;
			}
			void HtmlNode.IVisitor.Visit(HtmlElement element)
			{
				result = new HtmlElement(element.Tag, Collapse(element.Children));
			}
			void HtmlNode.IVisitor.Visit(HtmlTextNode node)
			{
				result = node;
			}
			void HtmlNode.IVisitor.Visit(HtmlForceWrite forceWrite)
			{
				result = forceWrite;
			}
		}
		static bool TryCollapse(IList<HtmlNode> collapsed, HtmlNode node)
		{
			if (collapsed.LastOrDefault() is HtmlElement last &&
				node is HtmlElement next)
				if (next.IsCollapsible && IsMatch(last, next))
				{
					string separator = next.Separator;
					if (!String.IsNullOrEmpty(separator))
						last.Children.Add(Text(separator));
					foreach (var child in next.Children)
						CollapsingAdd(last.Children, child);
					return true;
				}
			return false;
		}
		static bool IsMatch(HtmlElement first, HtmlElement second)
		{
			return second.TagNames.Contains(first.TagName) &&
				first.Attributes.Equals(second.Attributes);
		}
	}
}

