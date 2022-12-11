using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Results.Tests;
using Mammoth.Internal.Xml;
using Mammoth.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.NoteReference;
using static Mammoth.Internal.Documents.Tests.DocumentElementAssert;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;
using static Mammoth.Internal.Docx.Tests.BodyXmlReaderMakers;
using static Mammoth.Internal.Docx.Tests.OfficeXmlBuilders;
using static Mammoth.Internal.Xml.XmlNodes;
using VerticalAlignment = Mammoth.Internal.Documents.Run.RunVerticalAlignment;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class BodyXmlReaderTests
	{
		[TestMethod()]
		public void TextFromTextElementIsRead()
		{
			var element = TextXml("Hello!");
			IsTextElement(ReadSuccess(BodyReader(), element), "Hello!");
		}

		[TestMethod()]
		public void ReadTextWithinRun()
		{
			var element = RunXml(TextXml("Hello!"));
			IsRun(
				ReadSuccess(BodyReader(), element),
				Run(WithChildren(Text("Hello!"))));
		}

		[TestMethod()]
		public void ReadTextWithinParagraph()
		{
			var element = ParagraphXml(RunXml(TextXml("Hello!")));
			IsParagraph(
				ReadSuccess(BodyReader(), element),
				Paragraph(WithChildren(Run(WithChildren(Text("Hello!"))))));
		}

		[TestMethod()]
		public void ParagraphHasNoStyleIfItHasNoProperties()
		{
			var element = ParagraphXml();
			HasStyle(
				ReadSuccess(BodyReader(), element),
				null);
		}

		[TestMethod()]
		public void WhenParagraphHasStyleIdInStylesThenStyleNameIsReadFromStyles()
		{
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:pStyle", new XmlAttributes { ["w:val"] = "Heading1" })));

			var style = new Style("Heading1", "Heading 1");
			var styles = new Styles(
				new Dictionary<string, Style> { ["Heading1"] = style },
				new Dictionary<string, Style>(),
				new Dictionary<string, Style>(),
				new Dictionary<string, NumberingStyle>());
			HasStyle(
				ReadSuccess(BodyReader(styles), element),
				style);
		}

		[TestMethod()]
		public void WarningIsEmittedWhenParagraphStyleCannotBeFound()
		{
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:pStyle", new XmlAttributes { ["w:val"] = "Heading1" })));
			IsInternalResult(
				Read(BodyReader(), element),
				value => HasStyle(value, new Style("Heading1")),
				"Paragraph style with ID Heading1 was referenced but not defined in the document");
		}

		[TestClass()]
		public class ParagraphIndentTests
		{
			[TestMethod()]
			public void WhenWStartIsSetThenStartIndentIsReadFromWStart()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:start"] = "720", ["w:left"] = "40" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedStart: "720");
			}

			[TestMethod()]
			public void WhenWStartIsNotSetThenStartIndentIsReadFromWLeft()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:left"] = "720" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedStart: "720");
			}

			[TestMethod()]
			public void WhenWEndIsSetThenEndIndentIsReadFromWEnd()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:end"] = "720", ["w:right"] = "40" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedEnd: "720");
			}

			[TestMethod()]
			public void WhenWEndIsNotSetThenEndIndentIsReadFromWRight()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:right"] = "720" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedEnd: "720");
			}

			[TestMethod()]
			public void ParagraphHasIndentFirstLineReadFromParagraphPropertiesIfPresent()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:firstLine"] = "720" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedFirstLine: "720");
			}

			[TestMethod()]
			public void ParagraphHasIndentHangingReadFromParagraphPropertiesIfPresent()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes { ["w:hanging"] = "720" });
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml),
					expectedHanging: "720");
			}

			[TestMethod()]
			public void WhenIndentAttributesArentSetThenIndentsAreNotSet()
			{
				var paragraphXml = ParagraphWithIndent(new XmlAttributes());
				HasIndent(
					ReadSuccess(BodyReader(), paragraphXml));
			}

			XmlElement ParagraphWithIndent(IDictionary<string, string> attributes)
			{
				return ParagraphXml(
					Element("w:pPr",
						Element("w:ind", attributes)));
			}
		}

		[TestMethod()]
		public void ParagraphHasNoNumberingIfItHasNoNumberingProperties()
		{
			var element = ParagraphXml();
			HasNumbering(
				ReadSuccess(BodyReader(), element),
				null);
		}

		[TestMethod()]
		public void ParagraphHasNumberingPropertiesFromParagraphPropertiesIfPresent()
		{
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:numPr", new XmlAttributes(),
						Element("w:ilvl", new XmlAttributes { ["w:val"] = "1" }),
						Element("w:numId", new XmlAttributes { ["w:val"] = "42" }))));

			var numbering = NumberingMap(new Dictionary<string, IDictionary<string, Numbering.AbstractNumLevel>>
			{
				["42"] = new Dictionary<string, Numbering.AbstractNumLevel> { ["1"] = Numbering.AbstractNumLevel.Ordered("1") }
			});

			HasNumbering(
				ReadSuccess(BodyReader(numbering), element),
				NumberingLevel.Ordered("1"));
		}

		[TestMethod()]
		public void NumberingOnParagraphStyleTakesPrecedenceOverNumPr()
		{
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:pStyle", new XmlAttributes { ["w:val"] = "List" }),
					Element("w:numPr", new XmlAttributes(),
						Element("w:ilvl", new XmlAttributes { ["w:val"] = "1" }),
						Element("w:numId", new XmlAttributes { ["w:val"] = "42" }))));

			var numbering = NumberingMap(new Dictionary<string, IDictionary<string, Numbering.AbstractNumLevel>>
			{
				["42"] = new Dictionary<string, Numbering.AbstractNumLevel> { ["1"] = new Numbering.AbstractNumLevel("1", false) },
				["43"] = new Dictionary<string, Numbering.AbstractNumLevel> { ["1"] = new Numbering.AbstractNumLevel("1", true, "List") }
			});
			var styles = new Styles(
				new Dictionary<string, Style> { ["List"] = new Style("List") },
				new Dictionary<string, Style>(),
				new Dictionary<string, Style>(),
				new Dictionary<string, NumberingStyle>());

			HasNumbering(
				ReadSuccess(BodyReader(numbering, styles), element),
				NumberingLevel.Ordered("1"));
		}

		[TestMethod()]
		public void NumberingPropertiesAreIgnoredIfLevelIsMissing()
		{
			// TODO: emit warning
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:numPr",
						Element("w:numId", new XmlAttributes { ["w:val"] = "42" }))));

			var numbering = NumberingMap(new Dictionary<string, IDictionary<string, Numbering.AbstractNumLevel>>
			{
				["42"] = new Dictionary<string, Numbering.AbstractNumLevel> { ["1"] = Numbering.AbstractNumLevel.Ordered("1") }
			});

			HasNumbering(
				ReadSuccess(BodyReader(numbering), element),
				null);
		}

		[TestMethod()]
		public void NumberingPropertiesAreIgnoredIfNumIdIsMissing()
		{
			// TODO: emit warning
			var element = ParagraphXml(
				Element("w:pPr",
					Element("w:numPr", new XmlAttributes(),
						Element("w:ilvl", new XmlAttributes { ["w:val"] = "1" }))));

			var numbering = NumberingMap(new Dictionary<string, IDictionary<string, Numbering.AbstractNumLevel>>
			{
				["42"] = new Dictionary<string, Numbering.AbstractNumLevel> { ["1"] = Numbering.AbstractNumLevel.Ordered("1") }
			});

			HasNumbering(
				ReadSuccess(BodyReader(numbering), element),
				null);
		}

		[TestClass()]
		public class ComplexFieldsTests
		{
			const string URI = "http://example.com";
			readonly XmlElement BEGIN_COMPLEX_FIELD = Element("w:r",
				Element("w:fldChar", new XmlAttributes { ["w:fldCharType"] = "begin" }));
			readonly XmlElement SEPARATE_COMPLEX_FIELD = Element("w:r",
				Element("w:fldChar", new XmlAttributes { ["w:fldCharType"] = "separate" }));
			readonly XmlElement END_COMPLEX_FIELD = Element("w:r",
				Element("w:fldChar", new XmlAttributes { ["w:fldCharType"] = "end" }));
			readonly XmlElement HYPERLINK_INSTRTEXT = Element("w:instrText",
				XmlNodes.Text($" HYPERLINK \"{URI}\""));
			bool IsEmptyHyperlinkedRun(DocumentElement element)
			{
				return IsHyperlinkedRun(element, child => HasChildren(child));
			}
			bool IsHyperlinkedRun(DocumentElement element, Func<DocumentElement, bool> matcher)
			{
				switch (element)
				{
					case Run run:
						return HasChildren(element, child => IsHyperlink(child) && matcher(child));
					default:
						return IsRun(element);
				}
			}

			[TestMethod()]
			public void RunsInAComplexFieldForHyperlinksWithoutSwitchAreReadAsExternalHyperlinks()
			{
				var hyperlinkRunXml = RunXml("this is a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					HYPERLINK_INSTRTEXT,
					SEPARATE_COMPLEX_FIELD,
					hyperlinkRunXml,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasHref(c2, URI) && HasChildren(c2,
							c3 => IsTextElement(c3, "this is a hyperlink"))),
					IsEmptyRun);
			}

			[TestMethod()]
			public void RunsInAComplexFieldForHyperlinksWithLSwitchAreReadAsInternalHyperlinks()
			{
				var hyperlinkRunXml = RunXml("this is a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					Element("w:instrText",
						XmlNodes.Text(" HYPERLINK \\l \"InternalLink\"")),
					SEPARATE_COMPLEX_FIELD,
					hyperlinkRunXml,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasAnchor(c2, "InternalLink") && HasChildren(c2,
							c3 => IsTextElement(c3, "this is a hyperlink"))),
					IsEmptyRun);
			}

			[TestMethod()]
			public void RunsAfterAComplexFieldForHyperlinksAreNotReadAsHyperlinks()
			{
				var afterEndXml = RunXml("this will not be a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					HYPERLINK_INSTRTEXT,
					SEPARATE_COMPLEX_FIELD,
					END_COMPLEX_FIELD,
					afterEndXml);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					IsEmptyRun,
					c1 => IsRun(c1) && HasChildren(c1,
							c2 => IsTextElement(c2, "this will not be a hyperlink")));
			}

			[TestMethod()]
			public void HandleSplitInstrTextElements()
			{
				var hyperlinkRunXml = RunXml("this is a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					Element("w:instrText",
						XmlNodes.Text(" HYPE")),
					Element("w:instrText",
						XmlNodes.Text($"RLINK \"{URI}\"")),
					SEPARATE_COMPLEX_FIELD,
					hyperlinkRunXml,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasHref(c2, URI) && HasChildren(c2,
							c3 => IsTextElement(c3, "this is a hyperlink"))),
					IsEmptyRun);
			}

			[TestMethod()]
			public void HyperlinkIsNotEndedByEndOfNestedComplexField()
			{
				var authorInstrText = Element("w:instrText",
					XmlNodes.Text(" AUTHOR \"John Doe\""));
				var hyperlinkRunXml = RunXml("this is a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					HYPERLINK_INSTRTEXT,
					SEPARATE_COMPLEX_FIELD,
					BEGIN_COMPLEX_FIELD,
					authorInstrText,
					SEPARATE_COMPLEX_FIELD,
					END_COMPLEX_FIELD,
					hyperlinkRunXml,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasHref(c2, URI) && HasChildren(c2,
							c3 => IsTextElement(c3, "this is a hyperlink"))),
					IsEmptyRun);
			}

			[TestMethod()]
			public void ComplexFieldNestedWithinAHyperlinkComplexFieldIsWrappedWithTheHyperlink()
			{
				var authorInstrText = Element("w:instrText",
					XmlNodes.Text(" AUTHOR \"John Doe\""));
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					HYPERLINK_INSTRTEXT,
					SEPARATE_COMPLEX_FIELD,
					BEGIN_COMPLEX_FIELD,
					authorInstrText,
					SEPARATE_COMPLEX_FIELD,
					RunXml("John Doe"),
					END_COMPLEX_FIELD,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasHref(c2, URI) && HasChildren(c2,
							c3 => IsTextElement(c3, "John Doe"))),
					IsEmptyHyperlinkedRun,
					IsEmptyRun);
			}

			[TestMethod()]
			public void FieldWithoutSeparateFldCharIsIgnored()
			{
				var hyperlinkRunXml = RunXml("this is a hyperlink");
				var element = ParagraphXml(
					BEGIN_COMPLEX_FIELD,
					HYPERLINK_INSTRTEXT,
					SEPARATE_COMPLEX_FIELD,
					BEGIN_COMPLEX_FIELD,
					END_COMPLEX_FIELD,
					hyperlinkRunXml,
					END_COMPLEX_FIELD);
				var paragraph = ReadSuccess(BodyReader(), element);

				IsParagraph(paragraph);
				HasChildren(paragraph,
					IsEmptyRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					IsEmptyHyperlinkedRun,
					c1 => IsHyperlinkedRun(c1,
						c2 => HasHref(c2, URI) && HasChildren(c2,
							c3 => IsTextElement(c3, "this is a hyperlink"))),
					IsEmptyRun);
			}
		}

		[TestMethod()]
		public void RunHasNoStyleIfItHasNoProperties()
		{
			var element = RunXml();
			HasStyle(
				ReadSuccess(BodyReader(), element),
				null);
		}

		[TestMethod()]
		public void WhenRunHasStyleIdInStylesThenStyleNameIsReadFromStyles()
		{
			var element = RunXml(
				Element("w:rPr",
					Element("w:rStyle", new XmlAttributes { ["w:val"] = "Heading1Char" })));

			var style = new Style("Heading1Char", "Heading 1 Char");
			var styles = new Styles(
				new Dictionary<string, Style>(),
				new Dictionary<string, Style> { ["Heading1Char"] = style },
				new Dictionary<string, Style>(),
				new Dictionary<string, NumberingStyle>()
			);
			HasStyle(
				ReadSuccess(BodyReader(styles), element),
				style);
		}

		[TestMethod()]
		public void WarningIsEmittedWhenRunStyleCannotBeFound()
		{
			var element = RunXml(
				Element("w:rPr",
					Element("w:rStyle", new XmlAttributes { ["w:val"] = "Heading1Char" })));

			IsInternalResult(
				Read(BodyReader(), element),
				value => HasStyle(value, new Style("Heading1Char")),
				"Run style with ID Heading1Char was referenced but not defined in the document");
		}

		[TestMethod()]
		public void RunIsNotBoldIfBoldElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsBold));
		}

		[TestMethod()]
		public void RunIsBoldIfBoldElementIsPresent()
		{
			var element = RunXmlWithProperties(Element("w:b"));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(true, run.IsBold));
		}

		[TestMethod()]
		public void RunIsNotItalicIfItalicElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsItalic));
		}

		[TestMethod()]
		public void RunIsItalicIfItalicElementIsPresent()
		{
			var element = RunXmlWithProperties(Element("w:i"));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(true, run.IsItalic));
		}

		[TestMethod()]
		public void RunIsNotUnderlinedIfUnderlineElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsNotUnderlinedIfUnderlineElementIsPresentWithoutValAttribute()
		{
			var element = RunXmlWithProperties(Element("w:u"));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsNotUnderlinedIfUnderlineElementIsPresentAndValIsFalse()
		{
			var element = RunXmlWithProperties(Element("w:u", new XmlAttributes { ["w:val"] = "false" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsNotUnderlinedIfUnderlineElementIsPresentAndValIs0()
		{
			var element = RunXmlWithProperties(Element("w:u", new XmlAttributes { ["w:val"] = "0" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsNotUnderlinedIfUnderlineElementIsPresentAndValIsNone()
		{
			var element = RunXmlWithProperties(Element("w:u", new XmlAttributes { ["w:val"] = "none" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsUnderlinedIfUnderlineElementIsPresentAndValIsNotNoneNorFalsy()
		{
			var element = RunXmlWithProperties(Element("w:u", new XmlAttributes { ["w:val"] = "single" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(true, run.IsUnderline));
		}

		[TestMethod()]
		public void RunIsNotStruckthroughIfStrikethroughElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsStrikethrough));
		}

		[TestMethod()]
		public void RunIsStruckthroughIfStrikethroughElementIsPresent()
		{
			var element = RunXmlWithProperties(Element("w:strike"));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(true, run.IsStrikethrough));
		}

		[TestMethod()]
		public void RunIsNotSmallCapsIfSmallCapsElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(false, run.IsSmallCaps));
		}

		[TestMethod()]
		public void RunIsSmallCapsIfSmallCapsElementIsPresent()
		{
			var element = RunXmlWithProperties(Element("w:smallCaps"));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(true, run.IsSmallCaps));
		}

		[TestClass()]
		public class RunBooleanPropertyTests
		{
			void AssertRunProperty(bool expected, Documents.Run run, string propertyName, string message = null)
			{
				switch (propertyName)
				{
					case nameof(Documents.Run.IsBold):
						Assert.AreEqual(expected, run.IsBold, message);
						break;
					case nameof(Documents.Run.IsUnderline):
						Assert.AreEqual(expected, run.IsUnderline, message);
						break;
					case nameof(Documents.Run.IsItalic):
						Assert.AreEqual(expected, run.IsItalic, message);
						break;
					case nameof(Documents.Run.IsStrikethrough):
						Assert.AreEqual(expected, run.IsStrikethrough, message);
						break;
					case nameof(Documents.Run.IsAllCaps):
						Assert.AreEqual(expected, run.IsAllCaps, message);
						break;
					case nameof(Documents.Run.IsSmallCaps):
						Assert.AreEqual(expected, run.IsSmallCaps, message);
						break;
				}
			}

			[TestMethod()]
			[DataRow(nameof(Documents.Run.IsBold), "w:b")]
			[DataRow(nameof(Documents.Run.IsUnderline), "w:u")]
			[DataRow(nameof(Documents.Run.IsItalic), "w:i")]
			[DataRow(nameof(Documents.Run.IsStrikethrough), "w:strike")]
			[DataRow(nameof(Documents.Run.IsAllCaps), "w:caps")]
			[DataRow(nameof(Documents.Run.IsSmallCaps), "w:smallCaps")]
			public void RunBooleanPropertyIsFalseIfElementIsPresentAndValIsFalse(string propertyName, string tagName)
			{
				string message = $"{propertyName} property is false if {tagName} element is present and val is false";
				var element = RunXmlWithProperties(
					Element(tagName, new XmlAttributes { ["w:val"] = "false" }));

				Is<Run>(
					ReadSuccess(BodyReader(), element),
					run => AssertRunProperty(false, run, propertyName, message));
			}

			[TestMethod()]
			[DataRow("bold", "w:b")]
			[DataRow("underline", "w:u")]
			[DataRow("italic", "w:i")]
			[DataRow("strikethrough", "w:strike")]
			[DataRow("allCaps", "w:caps")]
			[DataRow("smallCaps", "w:smallCaps")]
			public void RunBooleanPropertyIsFalseIfElementIsPresentAndValIs0(string propertyName, string tagName)
			{
				string message = $"{propertyName} property is false if {tagName} element is present and val is 0";
				var element = RunXmlWithProperties(
					Element(tagName, new XmlAttributes { ["w:val"] = "0" }));

				Is<Run>(
					ReadSuccess(BodyReader(), element),
					run => AssertRunProperty(false, run, propertyName, message));
			}

			[TestMethod()]
			[DataRow("bold", "w:b")]
			[DataRow("underline", "w:u")]
			[DataRow("italic", "w:i")]
			[DataRow("strikethrough", "w:strike")]
			[DataRow("allCaps", "w:caps")]
			[DataRow("smallCaps", "w:smallCaps")]
			public void RunBooleanPropertyIsTrueIfElementIsPresentAndValIsTrue(string propertyName, string tagName)
			{
				string message = $"{propertyName} property is true if {tagName} element is present and val is true";
				var element = RunXmlWithProperties(
					Element(tagName, new XmlAttributes { ["w:val"] = "true" }));

				Is<Run>(
					ReadSuccess(BodyReader(), element),
					run => AssertRunProperty(true, run, propertyName, message));
			}

			[TestMethod()]
			[DataRow("bold", "w:b")]
			[DataRow("underline", "w:u")]
			[DataRow("italic", "w:i")]
			[DataRow("strikethrough", "w:strike")]
			[DataRow("allCaps", "w:caps")]
			[DataRow("smallCaps", "w:smallCaps")]
			public void RunBooleanPropertyIsTrueIfElementIsPresentAndValIs1(string propertyName, string tagName)
			{
				string message = $"{propertyName} property is true if {tagName} element is present and val is 1";
				var element = RunXmlWithProperties(
					Element(tagName, new XmlAttributes { ["w:val"] = "1" }));

				Is<Run>(
					ReadSuccess(BodyReader(), element),
					run => AssertRunProperty(true, run, propertyName, message));
			}
		}

		[TestMethod()]
		public void RunHasBaselineVerticalAlignmentIfVerticalAlignmentElementIsNotPresent()
		{
			var element = RunXmlWithProperties();

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(VerticalAlignment.BASELINE, run.VerticalAlignment));
		}

		[TestMethod()]
		public void RunIsSuperscriptIfVerticalAlignmentPropertyIsSetToSuperscript()
		{
			var element = RunXmlWithProperties(
				Element("w:vertAlign", new XmlAttributes { ["w:val"] = "superscript" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(VerticalAlignment.SUPERSCRIPT, run.VerticalAlignment));
		}

		[TestMethod()]
		public void RunIsSubscriptIfVerticalAlignmentPropertyIsSetToSubscript()
		{
			var element = RunXmlWithProperties(
				Element("w:vertAlign", new XmlAttributes { ["w:val"] = "subscript" }));

			Is<Run>(
				ReadSuccess(BodyReader(), element),
				run => Assert.AreEqual(VerticalAlignment.SUBSCRIPT, run.VerticalAlignment));
		}

		[TestMethod()]
		public void ReadTabElement()
		{
			var element = Element("w:tab");

			Is<Tab>(
				ReadSuccess(BodyReader(), element),
				tab => Assert.AreSame(tab, Tab.TAB));
		}

		[TestMethod()]
		public void NoBreakHyphenElementIsReadAsNonBreakingHyphenCharacter()
		{
			var element = Element("w:noBreakHyphen");

			IsTextElement(
				ReadSuccess(BodyReader(), element),
				"\u2011");
		}

		[TestMethod()]
		public void SoftHyphenElementIsReadAsSoftHyphenCharacter()
		{
			var element = Element("w:softHyphen");

			IsTextElement(
				ReadSuccess(BodyReader(), element),
				"\u00ad");
		}

		[TestMethod()]
		public void SymWithSupportedFontAndSupportedCodePointInAsciiRangeIsConvertedToText()
		{
			var element = Element("w:sym", new XmlAttributes { ["w:font"] = "Wingdings", ["w:char"] = "28" });

			var result = ReadSuccess(BodyReader(), element);

			IsTextElement(
				result,
				"\uD83D\uDD7F");
		}

		[TestMethod()]
		public void SymWithSupportedFontAndSupportedCodePointInPrivateUseAreaIsConvertedToText()
		{
			var element = Element("w:sym", new XmlAttributes { ["w:font"] = "Wingdings", ["w:char"] = "F028" });

			var result = ReadSuccess(BodyReader(), element);

			IsTextElement(
				result,
				"\uD83D\uDD7F");
		}

		[TestMethod()]
		public void SymWithUnsupportedFontAndCodePointProducesEmptyResultWithWarning()
		{
			var element = Element("w:sym", new XmlAttributes { ["w:font"] = "Dingwings", ["w:char"] = "28" });

			var result = ReadAll(BodyReader(), element);

			IsInternalResult(result,
				value => Assert.AreEqual(0, value.Length),
				"A w:sym element with an unsupported character was ignored: char 28 in font Dingwings");
		}

		[TestMethod()]
		public void BrWithoutExplicitTypeIsReadAsLineBreak()
		{
			var element = Element("w:br");

			Assert.AreSame(Break.LINE_BREAK,
				ReadSuccess(BodyReader(), element));
		}

		[TestMethod()]
		public void BrWithTextWrappingTypeIsReadAsLineBreak()
		{
			var element = Element("w:br", new XmlAttributes { ["w:type"] = "textWrapping" });

			Assert.AreSame(Break.LINE_BREAK,
				ReadSuccess(BodyReader(), element));
		}

		[TestMethod()]
		public void BrWithPageTypeIsReadAsPageBreak()
		{
			var element = Element("w:br", new XmlAttributes { ["w:type"] = "page" });

			Assert.AreSame(Break.PAGE_BREAK,
				ReadSuccess(BodyReader(), element));
		}

		[TestMethod()]
		public void BrWithColumnTypeIsReadAsColumnBreak()
		{
			var element = Element("w:br", new XmlAttributes { ["w:type"] = "column" });

			Assert.AreSame(Break.COLUMN_BREAK,
				ReadSuccess(BodyReader(), element));
		}

		[TestMethod()]
		public void WarningOnBreaksThatArentRecognised()
		{
			var element = Element("w:br", new XmlAttributes { ["w:type"] = "unknownBreakType" });

			IsInternalResult(
				ReadAll(BodyReader(), element),
				value => Assert.AreEqual(0, value.Length),
				"Unsupported break type: unknownBreakType");
		}

		[TestMethod()]
		public void ReadTableElements()
		{
			var element = Element("w:tbl",
				Element("w:tr",
					Element("w:tc",
						Element("w:p"))));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => HasChildren(table,
					row => HasChildren(row,
						cell => IsTableCell(cell,
							new TableCell(1, 1, new[] { Paragraph() })))));
		}

		[TestMethod()]
		public void TableHasNoStyleIfItHasNoProperties()
		{
			var element = Element("w:tbl");
			HasStyle(
				ReadSuccess(BodyReader(), element),
				null);
		}

		[TestMethod()]
		public void WhenTableHasStyleIdInStylesThenStyleNameIsReadFromStyles()
		{
			var element = Element("w:tbl",
				Element("w:tblPr",
					Element("w:tblStyle", new XmlAttributes { ["w:val"] = "TableNormal" })));

			var style = new Style("TableNormal", "Normal Table");
			var styles = new Styles(
				new Dictionary<string, Style>(),
				new Dictionary<string, Style>(),
				new Dictionary<string, Style> { ["TableNormal"] = style },
				new Dictionary<string, NumberingStyle>()
			);
			HasStyle(
				ReadSuccess(BodyReader(styles), element),
				style);
		}

		[TestMethod()]
		public void WarningIsEmittedWhenTableStyleCannotBeFound()
		{
			var element = Element("w:tbl",
				Element("w:tblPr",
					Element("w:tblStyle", new XmlAttributes { ["w:val"] = "TableNormal" })));

			IsInternalResult(
				Read(BodyReader(), element),
				value => HasStyle(value, new Style("TableNormal")),
				"Table style with ID TableNormal was referenced but not defined in the document");
		}

		[TestMethod()]
		public void TblHeaderMarksTableRowAsHeader()
		{
			var element = Element("w:tbl",
				Element("w:tr",
					Element("w:trPr",
						Element("w:tblHeader"))),
				Element("w:tr"));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => HasChildren(table,
					row => IsTableRow(row, expectedHeader: true),
					row => IsTableRow(row, expectedHeader: false)));
		}

		[TestMethod()]
		public void GridspanIsReadAsColspanForTableCell()
		{
			var element = Element("w:tbl",
				Element("w:tr",
					Element("w:tc",
						Element("w:tcPr",
							Element("w:gridSpan", new XmlAttributes { ["w:val"] = "2" })),
						Element("w:p"))));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => IsTable(table, Table(
					TableRow(
						TableCell(WithColspan(2),
							WithChildren(Paragraph()))))));
		}

		[TestMethod()]
		public void VmergeIsReadAsRowspanForTableCell()
		{
			var element = Element("w:tbl",
				WTr(WTc()),
				WTr(WTc(WTcPr(WVmerge("restart")))),
				WTr(WTc(WTcPr(WVmerge("continue")))),
				WTr(WTc(WTcPr(WVmerge("continue")))),
				WTr(WTc()));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => IsTable(table, Table(
					TableRow(TableCell()),
					TableRow(TableCell(WithRowspan(3))),
					TableRow(),
					TableRow(),
					TableRow(TableCell()))));
		}

		[TestMethod()]
		public void VmergeWithoutValIsTreatedAsContinue()
		{
			var element = Element("w:tbl",
				WTr(WTc(WTcPr(WVmerge("restart")))),
				WTr(WTc(WTcPr(Element("w:vMerge")))));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => IsTable(table, Table(
					TableRow(TableCell(WithRowspan(2))),
					TableRow())));
		}

		[TestMethod()]
		public void VmergeAccountsForCellsSpanningColumns()
		{
			var element = Element("w:tbl",
				WTr(WTc(), WTc(), WTc(WTcPr(WVmerge("restart")))),
				WTr(WTc(WTcPr(WGridspan("2"))), WTc(WTcPr(WVmerge("continue")))),
				WTr(WTc(), WTc(), WTc(WTcPr(WVmerge("continue")))),
				WTr(WTc(), WTc(), WTc()));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => IsTable(table, Table(
					TableRow(TableCell(), TableCell(), TableCell(WithRowspan(3))),
					TableRow(TableCell(WithColspan(2))),
					TableRow(TableCell(), TableCell()),
					TableRow(TableCell(), TableCell(), TableCell()))));
		}

		[TestMethod()]
		public void NoVerticalCellMergingIfMergedCellsDoNotLineUp()
		{
			var element = Element("w:tbl",
				WTr(WTc(WTcPr(WGridspan("2"))), WTc(WTcPr(WVmerge("restart")))),
				WTr(WTc(), WTc(WTcPr(WVmerge("continue")))));

			Is<Table>(
				ReadSuccess(BodyReader(), element),
				table => IsTable(table, Table(
					TableRow(TableCell(WithColspan(2)), TableCell()),
					TableRow(TableCell(), TableCell()))));
		}

		[TestMethod()]
		public void WarningIfNonRowInTable()
		{
			var element = Element("w:tbl",
				Element("w:p"));

			IsInternalResult(
				Read(BodyReader(), element),
				table => IsTable(table, Table(new[] { Paragraph() })),
				"unexpected non-row element in table, cell merging may be incorrect");
		}

		[TestMethod()]
		public void WarningIfNonCellInTableRow()
		{
			var element = Element("w:tbl",
				WTr(Element("w:p")));

			IsInternalResult(
				Read(BodyReader(), element),
				table => IsTable(table, Table(TableRow(new[] { Paragraph() }))),
				"unexpected non-cell element in table row, cell merging may be incorrect");
		}

		[TestMethod()]
		public void HyperlinkIsReadAsExternalHyperlinkIfItHasARelationshipId()
		{
			var relationships = new Relationships(new[]
			{
				HyperlinkRelationship("r42", "http://example.com")
			});
			var element = Element("w:hyperlink", new XmlAttributes { ["r:id"] = "r42" }, RunXml());
			var link = ReadSuccess(BodyReader(relationships), element);
			IsHyperlink(link);
			HasHref(link, "http://example.com");
			HasAnchor(link, null);
			HasChildren(link, child => IsRun(child) && HasChildren(child));
		}

		[TestMethod()]
		public void HyperlinkIsReadAsExternalHyperlinkIfItHasARelationshipIdAndAnAnchor()
		{
			var relationships = new Relationships(new[]
			{
				HyperlinkRelationship("r42", "http://example.com/")
			});
			var element = Element("w:hyperlink", new XmlAttributes { ["r:id"] = "r42", ["w:anchor"] = "fragment" }, RunXml());
			var link = ReadSuccess(BodyReader(relationships), element);
			IsHyperlink(link);
			HasHref(link, "http://example.com/#fragment");
		}

		[TestMethod()]
		public void HyperlinkExistingFragmentIsReplacedWhenAnchorIsSetOnExternalLink()
		{
			var relationships = new Relationships(new[]
			{
				HyperlinkRelationship("r42", "http://example.com/#previous")
			});
			var element = Element("w:hyperlink", new XmlAttributes { ["r:id"] = "r42", ["w:anchor"] = "fragment" }, RunXml());
			var link = ReadSuccess(BodyReader(relationships), element);
			IsHyperlink(link);
			HasHref(link, "http://example.com/#fragment");
		}

		[TestMethod()]
		public void HyperlinkIsReadAsInternalHyperlinkIfItHasAnAnchorAttribute()
		{
			var element = Element("w:hyperlink", new XmlAttributes { ["w:anchor"] = "start" }, RunXml());
			var link = ReadSuccess(BodyReader(), element);
			IsHyperlink(link);
			HasAnchor(link, "start");
			HasHref(link, null);
		}

		[TestMethod()]
		public void HyperlinkIsIgnoredIfItDoesNotHaveARelationshipIdNorAnchor()
		{
			var element = Element("w:hyperlink", RunXml());
			IsRun(
				ReadSuccess(BodyReader(), element),
				Run(WithChildren()));
		}

		[TestMethod()]
		public void HyperlinkTargetFrameIsRead()
		{
			var element = Element("w:hyperlink", new XmlAttributes
			{
				["w:anchor"] = "start",
				["w:tgtFrame"] = "_blank"
			});
			var link = ReadSuccess(BodyReader(), element);
			IsHyperlink(link);
			HasTargetFrame(link, "_blank");
		}

		[TestMethod()]
		public void HyperlinkEmptyTargetFrameIsIgnored()
		{
			var element = Element("w:hyperlink", new XmlAttributes
			{
				["w:anchor"] = "start",
				["w:tgtFrame"] = ""
			});
			var link = ReadSuccess(BodyReader(), element);
			IsHyperlink(link);
			HasTargetFrame(link, null);
		}

		[TestMethod()]
		public void GoBackBookmarkIsIgnored()
		{
			var element = Element("w:bookmarkStart", new XmlAttributes { ["w:name"] = "_GoBack" });
			IsInternalResult(
				ReadAll(BodyReader(), element),
				value => Assert.AreEqual(0, value.Length));
		}

		[TestMethod()]
		public void BookmarkStartIsReadIfNameIsNotGoBack()
		{
			var element = Element("w:bookmarkStart", new XmlAttributes { ["w:name"] = "start" });
			Is<Bookmark>(
				ReadSuccess(BodyReader(), element),
				bookmark => AreEqual(new Bookmark("start"), bookmark));
		}

		[TestMethod()]
		public void FootnoteReferenceHasIdRead()
		{
			var element = Element("w:footnoteReference", new XmlAttributes { ["w:id"] = "4" });
			Is<NoteReference>(
				ReadSuccess(BodyReader(), element),
				note => AreEqual(FootnoteReference("4"), note));
		}

		[TestMethod()]
		public void EndnoteReferenceHasIdRead()
		{
			var element = Element("w:endnoteReference", new XmlAttributes { ["w:id"] = "4" });
			Is<NoteReference>(
				ReadSuccess(BodyReader(), element),
				note => AreEqual(EndnoteReference("4"), note));
		}

		[TestMethod()]
		public void CommentReferenceHasIdRead()
		{
			var element = Element("w:commentReference", new XmlAttributes { ["w:id"] = "4" });
			Is<CommentReference>(
				ReadSuccess(BodyReader(), element),
				comment => AreEqual(new CommentReference("4"), comment));
		}

		[TestMethod()]
		public void TextBoxesHaveContentAppendedAfterContainingParagraph()
		{
			var textBox = Element("w:pict",
				Element("v:shape",
					Element("v:textbox",
						Element("w:txbxContent",
							ParagraphXml(
								RunXml(TextXml("[textbox-content]")))))));
			var paragraph = ParagraphXml(
				RunXml(TextXml("[paragragh start]")),
				RunXml(textBox, TextXml("[paragragh end]")));

			var expected = new[]
			{
				Paragraph(WithChildren(
					Run(WithChildren(
						new Text("[paragragh start]"))),
					Run(WithChildren(
						new Text("[paragragh end]"))))),
				Paragraph(WithChildren(
					Run(WithChildren(
						new Text("[textbox-content]")))))
			};

			IsInternalResult(
				ReadAll(BodyReader(), paragraph),
				value => SequenceEqual(expected, value));
		}


		const string IMAGE_BYTES = "Not an image at all!";
		const string IMAGE_RELATIONSHIP_ID = "rId5";

		[TestMethod()]
		public void ReadImagedataElementsWithIdAttribute()
		{
			AssertCanReadEmbeddedImage(image =>
				Element("v:imagedata", new XmlAttributes { ["r:id"] = image.RelationshipId, ["o:title"] = image.AltText }));
		}

		[TestMethod()]
		public void WhenImagedataElementHasNoRelationshipIdThenItIsIgnoredWithWarning()
		{
			var element = Element("v:imagedata");

			IsInternalResult(
				ReadAll(BodyReader(), element),
				value => Assert.AreEqual(0, value.Length),
				"A v:imagedata element without a relationship ID was ignored");
		}

		[TestMethod()]
		public void ReadInlinePictures()
		{
			AssertCanReadEmbeddedImage(image =>
				InlineImageXml(EmbeddedBlipXml(image.RelationshipId), image.AltText));
		}

		[TestMethod()]
		public void AltTextTitleIsUsedIfAltTextDescriptionIsMissing()
		{
			var element = InlineImageXml(
				EmbeddedBlipXml(IMAGE_RELATIONSHIP_ID),
				title: "It's a hat");

			var image = ReadEmbeddedImage(element);

			Assert.AreEqual("It's a hat", image.AltText);
		}

		[TestMethod()]
		public void AltTextTitleIsUsedIfAltTextDescriptionIsBlank()
		{
			var element = InlineImageXml(
				EmbeddedBlipXml(IMAGE_RELATIONSHIP_ID),
				" ",
				"It's a hat");

			var image = ReadEmbeddedImage(element);

			Assert.AreEqual("It's a hat", image.AltText);
		}

		[TestMethod()]
		public void AltTextDescriptionIsPreferredToAltTextTitle()
		{
			var element = InlineImageXml(
				EmbeddedBlipXml(IMAGE_RELATIONSHIP_ID),
				"It's a hat",
				"hat");

			var image = ReadEmbeddedImage(element);

			Assert.AreEqual("It's a hat", image.AltText);
		}

		[TestMethod()]
		public void ReadAnchoredPictures()
		{
			AssertCanReadEmbeddedImage(image =>
				AnchoredImageXml(EmbeddedBlipXml(image.RelationshipId), image.AltText));
		}

		void AssertCanReadEmbeddedImage(Func<EmbeddedImage, XmlElement> generateXml)
		{
			var element = generateXml(new EmbeddedImage(IMAGE_RELATIONSHIP_ID, "It's a hat"));
			var image = ReadEmbeddedImage(element);
			Assert.AreEqual(image.AltText, "It's a hat");
			Assert.AreEqual(image.ContentType, "image/png");
			Assert.AreEqual(
				IMAGE_BYTES,
				ToString(image.Stream));
		}

		Image ReadEmbeddedImage(XmlElement element)
		{
			var relationships = new Relationships(new[]
			{
				ImageRelationship(IMAGE_RELATIONSHIP_ID, "media/hat.png")
			});
			var file = InMemoryArchive.FromStrings(new Dictionary<string, string> { ["word/media/hat.png"] = IMAGE_BYTES });
			return (Image)ReadSuccess(
				BodyReader(relationships, file),
				element);
		}

		static string ToString(Stream stream)
		{
			var mem = new MemoryStream();
			stream.CopyTo(mem);
			mem.Position = 0;
			using (var reader = new StreamReader(mem, Encoding.UTF8))
				return reader.ReadToEnd();
		}

		class EmbeddedImage
		{
			public readonly string RelationshipId;
			public readonly string AltText;

			public EmbeddedImage(string relationshipId, string altText)
			{
				this.RelationshipId = relationshipId;
				this.AltText = altText;
			}
		}

		class InMemoryFileReader : IFileReader
		{
			readonly IDictionary<string, string> entries;

			public InMemoryFileReader(IDictionary<string, string> entries)
			{
				this.entries = entries;
			}

			public Stream Open(string name)
			{
				string value = entries[name];
				return new MemoryStream(Encoding.UTF8.GetBytes(value));
			}
		}

		[TestMethod()]
		public void ReadLinkedPictures()
		{
			var element = InlineImageXml(LinkedBlipXml(IMAGE_RELATIONSHIP_ID), "");
			var relationships = new Relationships(new[]
			{
				ImageRelationship(IMAGE_RELATIONSHIP_ID, "file:///media/hat.png")
			});

			var image = (Image)ReadSuccess(
				BodyReader(relationships, new InMemoryFileReader(new Dictionary<string, string> { ["file:///media/hat.png"] = IMAGE_BYTES })),
				element);

			Assert.AreEqual(IMAGE_BYTES,
				ToString(image.Stream));
		}

		[TestMethod()]
		public void WarningIfBlipHasNoImageFile()
		{
			var element = InlineImageXml(Element("a:blip"), "");

			var result = ReadAll(BodyReader(), element);

			IsInternalResult(result,
				value => Assert.AreEqual(0, value.Length),
				"Could not find image file for a:blip element");
		}

		[TestMethod()]
		public void WarningIfImageTypeIsUnsupportedByWebBrowsers()
		{
			var element = InlineImageXml(EmbeddedBlipXml(IMAGE_RELATIONSHIP_ID), "");
			var relationships = new Relationships(new[]
			{
				ImageRelationship(IMAGE_RELATIONSHIP_ID, "media/hat.emf")
			});
			var file = InMemoryArchive.FromStrings(new Dictionary<string, string> { ["word/media/hat.emf"] = IMAGE_BYTES });
			var contentTypes = new ContentTypes(new Dictionary<string, string> { ["emf"] = "image/x-emf" }, new Dictionary<string, string>());

			var result = Read(
				BodyReader(relationships, file, contentTypes),
				element);

			InternalResultAssert.IsResult(
				result,
				"Image of type image/x-emf is unlikely to display in web browsers");
		}

		[TestMethod()]
		public void NoElementsCreatedIfImageCannotBeFoundInWDrawing()
		{
			var element = Element("w:drawing");

			var result = ReadAll(BodyReader(), element);

			IsInternalResult(result, value => Assert.AreEqual(0, value.Length));
		}

		[TestMethod()]
		public void NoElementsCreatedIfImageCannotBeFoundInWpInline()
		{
			var element = Element("wp:inline");

			var result = ReadAll(BodyReader(), element);

			IsInternalResult(result, value => Assert.AreEqual(0, value.Length));
		}

		XmlElement InlineImageXml(XmlElement blip, string description = null, string title = null)
		{
			return Element("w:drawing",
				Element("wp:inline", ImageXml(blip, description, title).ToArray()));
		}

		XmlElement AnchoredImageXml(XmlElement blip, String description)
		{
			return Element("w:drawing",
				Element("wp:anchor", ImageXml(blip, description).ToArray()));
		}

		IEnumerable<XmlNode> ImageXml(XmlElement blip, string description, string title = null)
		{
			var properties = new XmlAttributes();
			if (description != null) properties.Add("descr", description);
			if (title != null) properties.Add("title", title);

			yield return
				Element("wp:docPr", properties);
			yield return
				Element("a:graphic",
					Element("a:graphicData",
						Element("pic:pic",
							Element("pic:blipFill", blip))));
		}

		XmlElement EmbeddedBlipXml(string relationshipId)
		{
			return BlipXml(new XmlAttributes { ["r:embed"] = relationshipId });
		}

		XmlElement LinkedBlipXml(string relationshipId)
		{
			return BlipXml(new XmlAttributes { ["r:link"] = relationshipId });
		}

		XmlElement BlipXml(IDictionary<string, string> attributes)
		{
			return Element("a:blip", attributes);
		}

		[TestMethod()]
		public void SdtIsReadUsingSdtContent()
		{
			var element = Element("w:sdt", Element("w:sdtContent", TextXml("Blackdown")));

			IsInternalResult(
					ReadAll(BodyReader(), element),
					value => SequenceEqual(new[] { Text("Blackdown") }, value));
		}

		[TestMethod()]
		public void AppropriateElementsHaveTheirChildrenReadNormally()
		{
			AssertChildrenAreReadNormally("w:ins");
			AssertChildrenAreReadNormally("w:object");
			AssertChildrenAreReadNormally("w:smartTag");
			AssertChildrenAreReadNormally("w:drawing");
			AssertChildrenAreReadNormally("v:group");
			AssertChildrenAreReadNormally("v:rect");
			AssertChildrenAreReadNormally("v:roundrect");
			AssertChildrenAreReadNormally("v:shape");
			AssertChildrenAreReadNormally("v:textbox");
			AssertChildrenAreReadNormally("w:txbxContent");
		}

		private void AssertChildrenAreReadNormally(string name)
		{
			var element = Element(name, ParagraphXml());

			IsParagraph(
				ReadSuccess(BodyReader(), element),
				Paragraph());
		}

		[TestMethod()]
		public void IgnoredElementsAreIgnoredWithoutWarning()
		{
			AssertIsIgnored("office-word:wrap");
			AssertIsIgnored("v:shadow");
			AssertIsIgnored("v:shapetype");
			AssertIsIgnored("w:bookmarkEnd");
			AssertIsIgnored("w:sectPr");
			AssertIsIgnored("w:proofErr");
			AssertIsIgnored("w:lastRenderedPageBreak");
			AssertIsIgnored("w:commentRangeStart");
			AssertIsIgnored("w:commentRangeEnd");
			AssertIsIgnored("w:del");
			AssertIsIgnored("w:footnoteRef");
			AssertIsIgnored("w:endnoteRef");
			AssertIsIgnored("w:annotationRef");
			AssertIsIgnored("w:pPr");
			AssertIsIgnored("w:rPr");
			AssertIsIgnored("w:tblPr");
			AssertIsIgnored("w:tblGrid");
			AssertIsIgnored("w:tcPr");
		}

		private void AssertIsIgnored(String name)
		{
			var element = Element(name, ParagraphXml());

			IsInternalResult(
				ReadAll(BodyReader(), element),
				value => Assert.AreEqual(0, value.Length));
		}

		[TestMethod()]
		public void UnrecognisedElementsAreIgnoredWithWarning()
		{
			var element = Element("w:huh");
			IsInternalResult(
				ReadAll(BodyReader(), element),
				value => Assert.AreEqual(0, value.Length),
				"An unrecognised element was ignored: w:huh");
		}

		[TestMethod()]
		public void TextNodesAreIgnoredWhenReadingChildren()
		{
			var element = RunXml(XmlNodes.Text("[text]"));
			IsRun(
				ReadSuccess(BodyReader(), element),
				Run(WithChildren()));
		}

		static DocumentElement ReadSuccess(BodyXmlReader reader, XmlElement element)
		{
			var result = Read(reader, element);
			Assert.AreEqual(0, result.Warnings.Length);
			return result.Value;
		}

		static InternalResult<DocumentElement> Read(BodyXmlReader reader, XmlElement element)
		{
			var result = ReadAll(reader, element);
			Assert.AreEqual(1, result.Value.Length);
			return result.Map(elements => elements[0]);
		}

		static InternalResult<DocumentElement[]> ReadAll(BodyXmlReader reader, XmlElement element)
		{
			return reader.ReadElement(element).ToResult();
		}

		static XmlElement ParagraphXml(params XmlNode[] children)
		{
			return Element("w:p", children);
		}

		static XmlElement RunXml(string text)
		{
			return RunXml(TextXml(text));
		}

		static XmlElement RunXml(params XmlNode[] children)
		{
			return Element("w:r", children);
		}

		static XmlElement RunXmlWithProperties(params XmlNode[] children)
		{
			return Element("w:r", Element("w:rPr", children));
		}

		static XmlElement TextXml(string value)
		{
			return Element("w:t", XmlNodes.Text(value));
		}

		void IsInternalResult<T>(InternalResult<T> result, Action<T> valueMatcher, params string[] warnings)
			where T : class
		{
			valueMatcher(result.Value);
			InternalResultAssert.IsResult(result, warnings);
		}

		static bool HasChildren(DocumentElement element, params Func<DocumentElement, bool>[] matchers)
		{
			Assert.IsInstanceOfType(element, typeof(IHasChildren));
			var parent = (IHasChildren)element;
			var children = parent.Children.ToList();
			Assert.AreEqual(matchers.Length, children.Count, "Should equal children count");
			if (matchers.Length == 0) return false;
			for (int k = 0; k < matchers.Length; k++)
				matchers[k](children[k]);
			return true;
		}

		static void Is<T>(DocumentElement element, Action<T> matcher)
			where T : DocumentElement
		{
			Assert.IsNotNull(element);
			Assert.IsInstanceOfType(element, typeof(T));
			matcher((T)element);
		}

		static Text Text(string value)
		{
			return new Text(value);
		}

		static Relationship HyperlinkRelationship(string relationshipId, string target)
		{
			return new Relationship(relationshipId, target, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/hyperlink");
		}

		static Relationship ImageRelationship(string relationshipId, string target)
		{
			return new Relationship(relationshipId, target, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/image");
		}

		static Numbering NumberingMap(IDictionary<string, IDictionary<string, Numbering.AbstractNumLevel>> numbering)
		{
			return new Numbering(
				numbering.ToDictionary(
					entry => entry.Key,
					entry => new Numbering.AbstractNum(entry.Value)),
				numbering.Keys.ToDictionary(
					numId => numId,
					numId => new Numbering.Num(numId)),
				Styles.EMPTY);
		}
	}
}