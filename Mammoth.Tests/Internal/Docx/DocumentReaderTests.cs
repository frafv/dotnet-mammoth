using System.Collections.Generic;
using System.IO;
using Mammoth.Internal.Documents.Tests;
using Mammoth.Internal.Results.Tests;
using Mammoth.Internal.Xml;
using Mammoth.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static Mammoth.Internal.Documents.Tests.DocumentElementMakers;
using static Mammoth.Internal.Xml.XmlNodes;

namespace Mammoth.Internal.Docx.Tests
{
	[TestClass()]
	public class DocumentReaderTests
	{
		static readonly NamespacePrefixes mainDocumentNamespaces = NamespacePrefixes.Builder()
			.Put("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main")
			.Build();
		static readonly NamespacePrefixes relationshipsNamespaces = NamespacePrefixes.Builder()
			.DefaultPrefix("http://schemas.openxmlformats.org/package/2006/relationships")
			.Build();

		[TestMethod()]
		public void MainDocumentIsFoundUsingPackageRelationships()
		{
			var result = DocumentReader.ReadDocument(null, InMemoryArchive.FromStrings(new Dictionary<string, string>
			{
				["word/document2.xml"] = XmlWriter.ToString(
					Element("w:document",
						Element("w:body",
							Element("w:p",
								Element("w:r",
									Element("w:t", Text("Hello.")))))),
					mainDocumentNamespaces),
				["_rels/.rels"] = XmlWriter.ToString(
					Element("Relationships",
						Element("Relationship", new XmlAttributes
						{
							["Id"] = "rId1",
							["Target"] = "/word/document2.xml",
							["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
						})),
					relationshipsNamespaces)
			}));

			InternalResultAssert.IsResult(result);
			DocumentElementAssert.AreEqual(
				Document(WithChildren(ParagraphWithText("Hello."))),
				result.Value);
		}

		[TestMethod()]
		[ExpectedException(typeof(IOException))]
		public void ErrorIsThrownWhenMainDocumentPartDoesNotExist()
		{
			var archive = InMemoryArchive.FromStrings(new Dictionary<string, string>
			{
				["_rels/.rels"] = XmlWriter.ToString(
					Element("Relationships",
						Element("Relationship", new XmlAttributes
						{
							["Id"] = "rId1",
							["Target"] = "/word/document2.xml",
							["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
						})),
					relationshipsNamespaces)
			});
			ExceptionAssert.ThrowMessage("Could not find main document part. Are you sure this is a valid .docx file?",
				() => DocumentReader.ReadDocument(null, archive));
		}

		[TestClass()]
		public class PartPathTests
		{
			[TestMethod()]
			public void MainDocumentPartIsFoundUsingPackageRelationships()
			{
				var archive = InMemoryArchive.FromStrings(new Dictionary<string, string>
				{
					["word/document2.xml"] = " ",
					["_rels/.rels"] = XmlWriter.ToString(
						Element("Relationships",
							Element("Relationship", new XmlAttributes
							{
								["Id"] = "rId1",
								["Target"] = "/word/document2.xml",
								["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
							})),
						relationshipsNamespaces)
				});

				var partPaths = DocumentReader.FindPartPaths(archive);
				Assert.AreEqual("word/document2.xml", partPaths.MainDocument);
			}

			[TestMethod()]
			public void WhenRelationshipForMainDocumentCannotBeFoundThenFallbackIsUsed()
			{
				var archive = InMemoryArchive.FromStrings(new Dictionary<string, string>
				{
					["word/document.xml"] = " "
				});

				var partPaths = DocumentReader.FindPartPaths(archive);
				Assert.AreEqual("word/document.xml", partPaths.MainDocument);
			}

			void AssertPartPathsProperty(string expected, DocumentReader.PartPaths paths, string name, string message = null)
			{
				switch (name)
				{
					case nameof(DocumentReader.PartPaths.MainDocument):
						Assert.AreEqual(expected, paths.MainDocument, message);
						break;
					case nameof(DocumentReader.PartPaths.Comments):
						Assert.AreEqual(expected, paths.Comments, message);
						break;
					case nameof(DocumentReader.PartPaths.Endnotes):
						Assert.AreEqual(expected, paths.Endnotes, message);
						break;
					case nameof(DocumentReader.PartPaths.Footnotes):
						Assert.AreEqual(expected, paths.Footnotes, message);
						break;
					case nameof(DocumentReader.PartPaths.Numbering):
						Assert.AreEqual(expected, paths.Numbering, message);
						break;
					case nameof(DocumentReader.PartPaths.Styles):
						Assert.AreEqual(expected, paths.Styles, message);
						break;
				}
			}

			[TestMethod()]
			[DataRow(nameof(DocumentReader.PartPaths.Comments))]
			[DataRow(nameof(DocumentReader.PartPaths.Endnotes))]
			[DataRow(nameof(DocumentReader.PartPaths.Footnotes))]
			[DataRow(nameof(DocumentReader.PartPaths.Numbering))]
			[DataRow(nameof(DocumentReader.PartPaths.Styles))]
			public void PartsRelatedToMainDocument(string name)
			{
				string message = $"{name} part is found using main document relationships";
				var archive = InMemoryArchive.FromStrings(new Dictionary<string, string>
				{
					["_rels/.rels"] = XmlWriter.ToString(
						Element("Relationships",
							Element("Relationship", new XmlAttributes
							{
								["Id"] = "rId1",
								["Target"] = "/word/document.xml",
								["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
							})),
						relationshipsNamespaces),
					["word/document.xml"] = " ",
					["word/_rels/document.xml.rels"] = XmlWriter.ToString(
						Element("Relationships",
							Element("Relationship", new XmlAttributes
							{
								["Id"] = "rId2",
								["Target"] = "target-path.xml",
								["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/" + name.ToLowerInvariant()
							})),
						relationshipsNamespaces),
					["word/target-path.xml"] = " "
				});

				var partPaths = DocumentReader.FindPartPaths(archive);
				AssertPartPathsProperty("word/target-path.xml", partPaths, name, message);
			}

			[TestMethod()]
			[DataRow(nameof(DocumentReader.PartPaths.Comments))]
			[DataRow(nameof(DocumentReader.PartPaths.Endnotes))]
			[DataRow(nameof(DocumentReader.PartPaths.Footnotes))]
			[DataRow(nameof(DocumentReader.PartPaths.Numbering))]
			[DataRow(nameof(DocumentReader.PartPaths.Styles))]
			public void WhenRelationshipForPartCannotBeFoundThenFallbackIsUsed(string name)
			{
				string message = $"{name} part is found using main document relationships";
				var archive = InMemoryArchive.FromStrings(new Dictionary<string, string>
				{
					["word/document.xml"] = " ",
					["_rels/.rels"] = XmlWriter.ToString(
						Element("Relationships",
							Element("Relationship", new XmlAttributes
							{
								["Id"] = "rId1",
								["Target"] = "/word/document.xml",
								["Type"] = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"
							})
						),
						relationshipsNamespaces)
				});

				var partPaths = DocumentReader.FindPartPaths(archive);
				AssertPartPathsProperty($"word/{name.ToLowerInvariant()}.xml", partPaths, name, message);
			}
		}
	}
}