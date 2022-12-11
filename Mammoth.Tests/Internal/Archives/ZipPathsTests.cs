using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mammoth.Internal.Archives.Tests
{
	[TestClass()]
	public class ZipPathsTests
	{
		[TestMethod()]
		[DataRow("/a", "", "a")]
		[DataRow("a/b", "a", "b")]
		[DataRow("a/b/c", "a/b", "c")]
		[DataRow("/a/b/c", "/a/b", "c")]
		public void SplitPathSplitZipPathsOnLastForwardSlash(string path, string dirname, string basename)
		{
			var result = ZipPaths.SplitPath(path);
			Assert.IsNotNull(result);
			Assert.AreEqual(dirname, result.Dirname);
			Assert.AreEqual(basename, result.Basename);
		}

		[TestMethod()]
		[DataRow("name")]
		public void WhenPathHasNoForwardSlashesThenSplitPathReturnsEmptyDirname(string path)
		{
			var result = ZipPaths.SplitPath(path);
			Assert.IsNotNull(result);
			Assert.AreEqual("", result.Dirname);
			Assert.AreEqual(path, result.Basename);
		}

		[TestMethod()]
		[DataRow("a/b", "a", "b")]
		[DataRow("a/b/c", "a/b", "c")]
		[DataRow("/a/b/c", "/a/b", "c")]
		[DataRow("a/b/c/d", "a", "b", "c", "d")]
		[DataRow("/a/b/c/d", "/a", "b", "c", "d")]
		public void JoinPathJoinsArgumentsWithForwardSlashes(string expected, params string[] paths)
		{
			string path = ZipPaths.JoinPath(paths);
			Assert.AreEqual(expected, path);
		}

		[TestMethod()]
		[DataRow("a", "a", "")]
		[DataRow("b", "", "b")]
		[DataRow("a/b", "a", "", "b")]
		public void EmptyPartsAreIgnoredWhenJoiningPaths(string expected, params string[] paths)
		{
			string path = ZipPaths.JoinPath(paths);
			Assert.AreEqual(expected, path);
		}

		[TestMethod()]
		[DataRow("/b", "a", "/b")]
		[DataRow("/b/c", "a", "/b", "c")]
		[DataRow("/b", "/a", "/b")]
		[DataRow("/a", "/a")]
		public void WhenJoiningPathsThenAbsolutePathsIgnoreEarlierPaths(string expected, params string[] paths)
		{
			string path = ZipPaths.JoinPath(paths);
			Assert.AreEqual(expected, path);
		}
	}
}