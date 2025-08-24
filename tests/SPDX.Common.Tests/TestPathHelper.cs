using System;
using NUnit.Framework;
using System.Collections.Generic;
using SPDX.CodeAnalysis;

namespace SPDX.Common.Tests
{
    public class TestPathHelper
    {
        private static readonly bool IsWindows = OperatingSystem.IsWindows();
        public static IEnumerable<object[]> Combine_CommonCases_TestData()
        {
            if (IsWindows)
            {
                // Joining relative paths
                yield return new object[] { new string[] { "foo", "bar" }, false, @"foo\bar" };
                yield return new object[] { new string[] { "foo/bar/baz", "stuff" }, false, @"foo\bar\baz\stuff" };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff" }, false, @"foo\bar\baz\stuff" };
                yield return new object[] { new string[] { @"foo\bar\baz", "" }, true, @"foo\bar\baz\" };
                yield return new object[] { new string[] { @"foo\bar\baz", "" }, false, @"foo\bar\baz" };

                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized" }, false, @"foo\bar\baz\stuff\specialized" };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized" }, false, @"foo\bar\baz\stuff\specialized" };

                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized" + '\\' }, false, @"foo\bar\baz\stuff\specialized" + '\\' };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized/" }, false, @"foo\bar\baz\stuff\specialized" + '\\' };

                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized" }, true, @"foo\bar\baz\stuff\specialized" + '\\' };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized" }, true, @"foo\bar\baz\stuff\specialized" + '\\' };

                // Joining rooted paths
                yield return new object[] { new string[] { @"C:\foo", "bar" }, false, @"C:\foo\bar" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "stuff" }, false, @"C:\foo\bar\baz\stuff" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "stuff" }, true, @"C:\foo\bar\baz\stuff\" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "" }, true, @"C:\foo\bar\baz\" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "" }, false, @"C:\foo\bar\baz" };

                yield return new object[] { new string[] { "C:/foo/bar/baz", @"stuff\specialized" }, false, @"C:\foo\bar\baz\stuff\specialized" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "stuff/specialized" }, false, @"C:\foo\bar\baz\stuff\specialized" };

                yield return new object[] { new string[] { "C:/foo/bar/baz", @"stuff\specialized\" }, false, @"C:\foo\bar\baz\stuff\specialized\" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "stuff/specialized/" }, false, @"C:\foo\bar\baz\stuff\specialized\" };

                yield return new object[] { new string[] { "C:/foo/bar/baz", @"stuff\specialized" }, true, @"C:\foo\bar\baz\stuff\specialized\" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", "stuff/specialized" }, true, @"C:\foo\bar\baz\stuff\specialized\" };
                yield return new object[] { new string[] { @"C:\foo\bar\baz", @"stuff/specialized\" }, true, @"C:\foo\bar\baz\stuff\specialized\" };
                
                // Check removal of multiple consecutive delimiters
                yield return new object[] { new string[] { @"C:\foo\\bar\baz", @"stuff\\specialized\" }, false, @"C:\foo\bar\baz\stuff\specialized\" };
                yield return new object[] { new string[] { @"C:\foo\\/\\bar\baz", @"stuff\\/\\specialized\" }, false, @"C:\foo\bar\baz\stuff\specialized\" };


                // UNC paths with variations
                yield return new object[] { new string[] { @"\\server\share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"//server/share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"\\server/share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"//server\share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"\\server\share\", "folder" }, true, @"\\server\share\folder\" };

                // UNC paths with trailing slashes
                yield return new object[] { new string[] { @"\\server\share", "" }, true, @"\\server\share\" };
                yield return new object[] { new string[] { @"//server/share", "" }, true, @"\\server\share\" };

                // UNC root only
                yield return new object[] { new string[] { @"\\server\share", "" }, false, @"\\server\share" };
                yield return new object[] { new string[] { @"\\server\share\", "" }, false, @"\\server\share\" };
                yield return new object[] { new string[] { @"//server/share/", "" }, false, @"\\server\share\" };

                // UNC with multiple slashes
                yield return new object[] { new string[] { @"\\server\\share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"//server//share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"\\server\\/\\share", "folder" }, false, @"\\server\share\folder" };
                yield return new object[] { new string[] { @"//server//\//share", "folder" }, false, @"\\server\share\folder" };

                // Long path prefix (\\?\)
                yield return new object[] { new string[] { @"\\?\C:\folder", "sub" }, false, @"\\?\C:\folder\sub" };
                yield return new object[] { new string[] { @"\\?\C:/folder/", "sub" }, true, @"\\?\C:\folder\sub\" };

                // Device path prefix (\\.\)
                yield return new object[] { new string[] { @"\\.\C:\folder", "sub" }, false, @"\\.\C:\folder\sub" };
                yield return new object[] { new string[] { @"\\.\C:/folder/", "sub" }, true, @"\\.\C:\folder\sub\" };

                // Drive-relative paths (C:foo\bar, C:.foo\bar, C:..foo\bar)
                yield return new object[] { new string[] { @"C:foo", "sub" }, false, @"C:foo\sub" };
                yield return new object[] { new string[] { @"C:.foo", "sub" }, false, @"C:.foo\sub" };
                yield return new object[] { new string[] { @"C:..foo", "sub" }, false, @"C:..foo\sub" };
                yield return new object[] { new string[] { @"C:..foo", "sub" }, true, @"C:..foo\sub\" };
                yield return new object[] { new string[] { @"C:foo\bar", "sub" }, false, @"C:foo\bar\sub" };
                yield return new object[] { new string[] { @"C:.foo\bar", "sub" }, false, @"C:.foo\bar\sub" };
                yield return new object[] { new string[] { @"C:..foo\bar", "sub" }, false, @"C:..foo\bar\sub" };
                yield return new object[] { new string[] { @"C:foo/bar", "sub" }, false, @"C:foo\bar\sub" };
                yield return new object[] { new string[] { @"C:.foo/bar", "sub" }, false, @"C:.foo\bar\sub" };
                yield return new object[] { new string[] { @"C:..foo/bar", "sub" }, false, @"C:..foo\bar\sub" };
            }
            else // Unix-style paths
            {
                // Joining relative paths
                yield return new object[] { new string[] { "foo", "bar" }, false, "foo/bar" };
                yield return new object[] { new string[] { "foo/bar/baz", "stuff" }, false, "foo/bar/baz/stuff" };
                
                // Note that in Unix-style paths, \ is a valid character within a folder or file name.
                // So, we expect it not to be normalized.
                
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff" }, false, @"foo\bar\baz/stuff" };
                yield return new object[] { new string[] { "foo/bar/baz", "" }, true, "foo/bar/baz/" };
                yield return new object[] { new string[] { "foo/bar/baz", "" }, false, "foo/bar/baz" };
                
                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized" }, false, @"foo/bar/baz/stuff\specialized" };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized" }, false, @"foo\bar\baz/stuff/specialized" };

                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized\" }, false, @"foo/bar/baz/stuff\specialized\" };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized/" }, false, @"foo\bar\baz/stuff/specialized/" };
                
                yield return new object[] { new string[] { "foo/bar/baz", @"stuff\specialized" }, true, @"foo/bar/baz/stuff\specialized/" };
                yield return new object[] { new string[] { @"foo\bar\baz", "stuff/specialized" }, true, @"foo\bar\baz/stuff/specialized/" };
                
                // Joining rooted paths
                yield return new object[] { new string[] { "/foo", "bar" }, false, "/foo/bar" };
                yield return new object[] { new string[] { "/foo/bar/baz", "stuff" }, false, "/foo/bar/baz/stuff" };
                
                // Note that in Unix-style paths, \ is a valid character within a folder or file name.
                // So, we expect it not to be normalized.
                
                yield return new object[] { new string[] { @"/foo\bar\baz", "stuff" }, false, @"/foo\bar\baz/stuff" };
                yield return new object[] { new string[] { "/foo/bar/baz", "" }, true, "/foo/bar/baz/" };
                yield return new object[] { new string[] { "/foo/bar/baz", "" }, false, "/foo/bar/baz" };
                
                yield return new object[] { new string[] { @"/foo/bar/baz", @"stuff\specialized" }, false, @"/foo/bar/baz/stuff\specialized" };
                yield return new object[] { new string[] { @"\foo\bar\baz", "stuff/specialized" }, false, @"\foo\bar\baz/stuff/specialized" };

                yield return new object[] { new string[] { "/foo/bar/baz", @"stuff\specialized\" }, false, @"/foo/bar/baz/stuff\specialized\" };
                yield return new object[] { new string[] { @"\foo\bar\baz", "stuff/specialized/" }, false, @"\foo\bar\baz/stuff/specialized/" };
                
                yield return new object[] { new string[] { "/foo/bar/baz", @"stuff\specialized" }, true, @"/foo/bar/baz/stuff\specialized/" };
                yield return new object[] { new string[] { @"\foo\bar\baz", "stuff/specialized" }, true, @"\foo\bar\baz/stuff/specialized/" };
                yield return new object[] { new string[] { "/foo/bar/baz/", "stuff/specialized/" }, true, @"/foo/bar/baz/stuff/specialized/" };
                
                // Check removal of multiple consecutive delimiters
                yield return new object[] { new string[] { "/foo//bar/baz", "stuff//specialized/" }, false, @"/foo/bar/baz/stuff/specialized/" };
                yield return new object[] { new string[] { "/foo/////bar/baz", "stuff/////specialized/" }, false, @"/foo/bar/baz/stuff/specialized/" }; 
            }

        }
        
        [TestCaseSource(nameof(Combine_CommonCases_TestData))]
        public void TestNormalizeAndCombine(string[] paths, bool ensureTrailingSlash, string expected)
        {
            Assert.That(paths, Is.Not.Null);
            Assert.That(paths.Length, Is.EqualTo(2));

            string actual = PathHelper.NormalizeAndCombine(paths[0].AsSpan(), paths[1].AsSpan(), ensureTrailingSlash);

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}