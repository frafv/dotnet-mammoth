using System.Linq;
using System.Text;

namespace Mammoth.Internal.Html
{
	internal static class HtmlWriter
	{
		class Visitor : HtmlNode.IVisitor
		{
			readonly StringBuilder builder;
			internal Visitor(StringBuilder builder)
			{
				this.builder = builder;
			}
			void HtmlNode.IVisitor.Visit(HtmlElement element)
			{
				builder.Append('<').Append(element.TagName);

				GenerateAttributes(element.Attributes, builder);

				if (element.IsVoid)
				{
					builder.Append(" />");
				}
				else
				{
					builder.Append('>');

					foreach (var child in element.Children)
						Write(child, builder);

					builder
						.Append("</")
						.Append(element.TagName)
						.Append('>');
				}
			}
			void HtmlNode.IVisitor.Visit(HtmlTextNode node)
			{
				builder.Append(EscapeText(node.Value));
			}
			void HtmlNode.IVisitor.Visit(HtmlForceWrite forceWrite)
			{
			}
		}
		public static void Write(HtmlNode node, StringBuilder builder)
		{
			node.Accept(new Visitor(builder));
		}
		static void GenerateAttributes(HtmlAttributes attributes, StringBuilder builder)
		{
			foreach (string attribute in attributes.Keys.OrderBy(k => k))
			{
				builder
					.Append(' ')
					.Append(attribute)
					.Append("=\"")
					.Append(EscapeAttributeValue(attributes[attribute]))
					.Append('"');
			}
		}
		static string EscapeText(string text)
		{
			return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
		}
		static string EscapeAttributeValue(string value)
		{
			return EscapeText(value).Replace("\"", "&quot;");
		}
	}
}

