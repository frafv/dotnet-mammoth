using System;
using System.Collections.Generic;
using System.Linq;
using Mammoth.Internal.Documents;
using Mammoth.Internal.Results;

namespace Mammoth.Internal.Docx
{
	internal class ReadResult
	{
		internal static readonly ReadResult EMPTY_SUCCESS = Success();
		readonly DocumentElement[] elements;
		readonly DocumentElement[] extra;
		readonly string[] warnings;

		internal ReadResult(IEnumerable<DocumentElement> elements, IEnumerable<DocumentElement> extra, IEnumerable<string> warnings)
			: this(elements?.ToArray(), extra?.ToArray(), warnings?.ToArray())
		{ }
		private ReadResult(DocumentElement[] elements, DocumentElement[] extra, string[] warnings)
		{
			this.elements = elements;
			this.extra = extra;
			this.warnings = warnings;
		}

		public static ReadResult Join(params ReadResult[] results) => Join((IEnumerable<ReadResult>)results);
		public static ReadResult Join(IEnumerable<ReadResult> results)
		{
			results = results.ToList();
			return new ReadResult(
				elements: results.SelectMany(result => result.elements),
				extra: results.SelectMany(result => result.extra),
				warnings: results.SelectMany(result => result.warnings));
		}

		public static ReadResult Map<T>(InternalResult<T> first, ReadResult second,
			Func<T, IEnumerable<DocumentElement>, DocumentElement> function)
			where T : class
		{
			return new ReadResult(
				new[] { function(first.Value, second.elements) },
				second.extra,
				first.Warnings.Concat(second.warnings).ToArray());
		}

		public static ReadResult Success(params DocumentElement[] elements)
		{
			return new ReadResult(elements, Enumerable.Empty<DocumentElement>(), Enumerable.Empty<string>());
		}

		public static ReadResult EmptyWithWarning(params string[] warnings)
		{
			return WithWarning(Enumerable.Empty<DocumentElement>(), warnings);
		}

		public static ReadResult WithWarning(DocumentElement element, params string[] warnings)
		{
			return ReadResult.WithWarning(Enumerable.Repeat(element, 1), warnings);
		}

		public static ReadResult WithWarning(IEnumerable<DocumentElement> elements, params string[] warnings)
		{
			return new ReadResult(elements, Enumerable.Empty<DocumentElement>(), warnings);
		}

		public ReadResult Map(Func<IEnumerable<DocumentElement>, DocumentElement> function)
		{
			return new ReadResult(
				new[] { function(elements) },
				extra,
				warnings);
		}

		public ReadResult FlatMap(Func<IEnumerable<DocumentElement>, ReadResult> function)
		{
			var result = function(elements);
			return new ReadResult(result.elements, extra.Concat(result.extra), warnings.Concat(result.warnings));
		}

		public ReadResult ToExtra()
		{
			return new ReadResult(Enumerable.Empty<DocumentElement>(), elements.Concat(extra), warnings);
		}
		public ReadResult AppendExtra()
		{
			return new ReadResult(elements.Concat(extra), Enumerable.Empty<DocumentElement>(), warnings);
		}
		public InternalResult<DocumentElement[]> ToResult()
		{
			return new InternalResult<DocumentElement[]>(this.elements, this.warnings);
		}
	}
}

