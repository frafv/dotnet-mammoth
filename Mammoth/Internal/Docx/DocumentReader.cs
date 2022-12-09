using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Mammoth.Internal.Archives;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;
using Mammoth.Internal.Xml;

namespace Mammoth.Internal.Docx
{
	internal static class DocumentReader
	{
		class PartWithBody
		{
			public XmlElement Part { get; }
			public BodyXmlReader BodyReader { get; }

			public PartWithBody(XmlElement part, BodyXmlReader bodyReader)
			{
				Part = part;
				BodyReader = bodyReader;
			}
		}
		class PartWithBodyReader
		{
			readonly ZipArchive zipFile;
			readonly ContentTypes contentTypes;
			readonly IFileReader fileReader;
			readonly Numbering numbering;
			readonly Styles styles;

			public PartWithBodyReader(
				ZipArchive zipFile,
				ContentTypes contentTypes,
				IFileReader fileReader,
				Numbering numbering,
				Styles styles
			)
			{
				this.zipFile = zipFile;
				this.contentTypes = contentTypes;
				this.fileReader = fileReader;
				this.numbering = numbering;
				this.styles = styles;
			}

			public PartWithBody ReadPart(string name)
			{
				var relationships = ReadRelationships(zipFile, FindRelationshipsPathFor(name));
				var bodyReader = new BodyXmlReader(styles, numbering, relationships, contentTypes, zipFile, fileReader);
				var part = TryParseOfficeXml(zipFile, name);
				return part == null ? null : new PartWithBody(part, bodyReader);
			}
		}

		internal class PartPaths
		{
			public PartPaths(string mainDocument, string comments, string endnotes, string footnotes, string numbering, string styles)
			{
				MainDocument = mainDocument;
				Comments = comments;
				Endnotes = endnotes;
				Footnotes = footnotes;
				Numbering = numbering;
				Styles = styles;
			}

			public string MainDocument { get; }

			public string Comments { get; }

			public string Endnotes { get; }

			public string Footnotes { get; }

			public string Numbering { get; }

			public string Styles { get; }
		}

		public static InternalResult<Document> ReadDocument(Uri path, ZipArchive zipFile)
		{
			var partPaths = FindPartPaths(zipFile);

			var styles = ReadStyles(zipFile, partPaths);
			var numbering = ReadNumbering(zipFile, partPaths, styles);
			var contentTypes = ReadContentTypes(zipFile);
			var fileReader = new PathRelativeFileReader(path);
			var partReader = new PartWithBodyReader(zipFile, contentTypes, fileReader, numbering, styles);

			var notesResult = ReadNotes(partReader, partPaths);
			var commentsResult = ReadComments(partReader, partPaths);
			var result = partReader.ReadPart(partPaths.MainDocument);
			return InternalResult.Join<Document>(notesResult, commentsResult,
				new DocumentXmlReader(result.BodyReader, notesResult.Value, commentsResult.Value)
					.ReadElement(result.Part));
		}
		public static PartPaths FindPartPaths(ZipArchive archive)
		{
			var packageRelationships = ReadPackageRelationships(archive);
			string documentFilename = FindDocumentFilename(archive, packageRelationships);

			var documentRelationships = ReadRelationships(archive, FindRelationshipsPathFor(documentFilename));

			string Find(string name)
			{
				return FindPartPath(archive, documentRelationships,
					"http://schemas.openxmlformats.org/officeDocument/2006/relationships/" + name,
					ZipPaths.SplitPath(documentFilename).Dirname,
					$"word/{name}.xml");
			}

			return new PartPaths(
				mainDocument: documentFilename,
				comments: Find("comments"),
				endnotes: Find("endnotes"),
				footnotes: Find("footnotes"),
				numbering: Find("numbering"),
				styles: Find("styles"));
		}
		static Relationships ReadPackageRelationships(ZipArchive archive)
		{
			return ReadRelationships(archive, "_rels/.rels");
		}
		static string FindDocumentFilename(ZipArchive archive, Relationships packageRelationships)
		{
			string officeDocumentType = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
			string mainDocumentPath = FindPartPath(archive, packageRelationships, officeDocumentType, "", "word/document.xml");
			if (archive.GetEntry(mainDocumentPath) != null)
				return mainDocumentPath;
			else
				throw new IOException("Could not find main document part. Are you sure this is a valid .docx file?");
		}
		static string FindPartPath(ZipArchive archive, Relationships relationships, string relationshipType, string basePath, string fallbackPath)
		{
			var targets = relationships.FindTargetsByType(relationshipType)
				.Select(target => ZipPaths.JoinPath(basePath, target).TrimStart('/'));
			var validTargets = targets.Where(target => archive.GetEntry(target) != null);
			return validTargets.DefaultIfEmpty(fallbackPath).First();
		}
		static InternalResult<Comment[]> ReadComments(PartWithBodyReader partReader, PartPaths partPaths)
		{
			return partReader.ReadPart(partPaths.Comments) is PartWithBody result ?
				new CommentXmlReader(result.BodyReader).ReadElement(result.Part) :
				InternalResult.Success(new Comment[0]);
		}
		static InternalResult<Notes> ReadNotes(PartWithBodyReader partReader, PartPaths partPaths)
		{
			return InternalResult.Map(
				partReader.ReadPart(partPaths.Footnotes) is PartWithBody footnote ?
					NotesXmlReader.Footnote(footnote.BodyReader).ReadElement(footnote.Part) :
					InternalResult.Success(new Note[0]),
				partReader.ReadPart(partPaths.Endnotes) is PartWithBody endnote ?
					NotesXmlReader.Endnote(endnote.BodyReader).ReadElement(endnote.Part) :
					InternalResult.Success(new Note[0]),
				(footnotes, endnotes) => new Notes(footnotes.Concat(endnotes)));
		}
		static Styles ReadStyles(ZipArchive file, PartPaths partPaths)
		{
			return TryParseOfficeXml(file, partPaths.Styles) is XmlElement element ?
				StylesXml.ReadStylesXmlElement(element) : Styles.EMPTY;
		}
		static Numbering ReadNumbering(ZipArchive file, PartPaths partPaths, Styles styles)
		{
			return TryParseOfficeXml(file, partPaths.Numbering) is XmlElement element ?
				NumberingXml.ReadNumberingXmlElement(element, styles) : Numbering.EMPTY;
		}
		static ContentTypes ReadContentTypes(ZipArchive file)
		{
			return TryParseOfficeXml(file, "[Content_Types].xml") is XmlElement element ?
				ContentTypesXml.ReadContentTypesXmlElement(element) : ContentTypes.DEFAULT;
		}
		static Relationships ReadRelationships(ZipArchive zipFile, string name)
		{
			return TryParseOfficeXml(zipFile, name) is XmlElement element ?
				RelationshipsXml.ReadRelationshipsXmlElement(element) : Relationships.EMPTY;
		}
		static string FindRelationshipsPathFor(string name)
		{
			var parts = ZipPaths.SplitPath(name);
			return ZipPaths.JoinPath(parts.Dirname, "_rels", parts.Basename + ".rels");
		}
		static XmlElement TryParseOfficeXml(ZipArchive zipFile, string name)
		{
			var entry = zipFile.GetEntry(name);
			if (entry == null) return null;
			using (var stream = entry.Open())
			{
				return OfficeXml.ParseXml(stream);
			}
		}
		static XmlElement ParseOfficeXml(ZipArchive zipFile, string name)
		{
			return TryParseOfficeXml(zipFile, name) ??
				throw new IOException($"Missing entry in file: {name}");
		}
	}
}

