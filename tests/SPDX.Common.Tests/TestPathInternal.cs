using System;
using NUnit.Framework;
using System.Collections.Generic;
using SPDX.CodeAnalysis;

namespace SPDX.Common.Tests
{
    public class TestPathInternal
    {
        private static readonly bool IsWindows = OperatingSystem.IsWindows();

        public static IEnumerable<TestCaseData> RootSegment_CommonCases_TestData()
        {
            if (IsWindows)
            {

                // Drive-rooted & drive-relative
                yield return new TestCaseData(@"C:\foo\bar", @"C:").SetName("DriveRooted_Backslash");
                yield return new TestCaseData(@"C:\\foo\bar", @"C:").SetName("DriveRooted_Backslash_DoubleSlash");
                yield return new TestCaseData(@"C:\\\foo\bar", @"C:").SetName("DriveRooted_Backslash_TripleSlash");
                yield return new TestCaseData(@"C:/foo/bar", @"C:").SetName("DriveRooted_ForwardSlash");
                yield return new TestCaseData(@"C://foo/bar", @"C:").SetName("DriveRooted_ForwardSlash_DoubleSlash");
                yield return new TestCaseData(@"C:///foo/bar", @"C:").SetName("DriveRooted_ForwardSlash_TripleSlash");
                yield return new TestCaseData(@"C:foo\bar", @"C:").SetName("DriveRelative_Backslash");
                yield return new TestCaseData(@"C:foo/bar", @"C:").SetName("DriveRelative_ForwardSlash");

                // Current drive rooted
                yield return new TestCaseData(@"\foo\bar", @"\").SetName("CurrentDriveRooted_Backslash");
                yield return new TestCaseData(@"/foo/bar", @"/").SetName("CurrentDriveRooted_ForwardSlash");

                // UNC (include server, exclude share)
                yield return new TestCaseData(@"\\server\share\folder", @"\\server").SetName("UNC_Backslash");
                yield return new TestCaseData(@"\\server\\share\folder", @"\\server").SetName("UNC_Backslash_DoubleSlash");
                yield return new TestCaseData(@"\\server\\\share\folder", @"\\server").SetName("UNC_Backslash_TripleSlash");
                yield return new TestCaseData(@"//server/share/folder", @"//server").SetName("UNC_ForwardSlash");
                yield return new TestCaseData(@"//server//share/folder", @"//server").SetName("UNC_ForwardSlash_DoubleSlash");
                yield return new TestCaseData(@"//server///share/folder", @"//server").SetName("UNC_ForwardSlash_TripleSlash");
                yield return new TestCaseData(@"\\server", @"\\server").SetName("UNC_ServerOnly");
                yield return new TestCaseData(@"//server", @"//server").SetName("UNC_ServerOnly_Forward");

                // Extended UNC (include \\?\UNC\server, exclude share)
                yield return new TestCaseData(@"\\?\UNC\server\share\foo", @"\\?\UNC\server").SetName("ExtUNC_Backslash");
                yield return new TestCaseData(@"\\?\UNC\\server\share\foo", @"\\?\UNC").SetName("ExtUNC_Backslash_DoubleSlash");
                yield return new TestCaseData(@"\\?\UNC\\\server\share\foo", @"\\?\UNC").SetName("ExtUNC_Backslash_TripleSlash");
                yield return new TestCaseData(@"//?/UNC/server/share/foo", @"//?/UNC/server").SetName("ExtUNC_ForwardSlash");
                yield return new TestCaseData(@"//?/UNC//server/share/foo", @"//?/UNC").SetName("ExtUNC_ForwardSlash_DoubleSlash");
                yield return new TestCaseData(@"//?/UNC///server/share/foo", @"//?/UNC").SetName("ExtUNC_ForwardSlash_TripleSlash");

                // Extended DOS device / device paths
                yield return new TestCaseData(@"\\?\C:\foo", @"\\?\C:").SetName("ExtDrive_Backslash");
                yield return new TestCaseData(@"\\?\C:\\foo", @"\\?\C:").SetName("ExtDrive_Backslash_DoubleSlash");
                yield return new TestCaseData(@"\\?\C:\\\foo", @"\\?\C:").SetName("ExtDrive_Backslash_TripleSlash");
                yield return new TestCaseData(@"//?/C:/foo", @"//?/C:").SetName("ExtDrive_ForwardSlashAsDevice");
                yield return new TestCaseData(@"//?/C://foo", @"//?/C:").SetName("ExtDrive_ForwardSlashAsDevice_DoubleSlash");
                yield return new TestCaseData(@"//?/C:///foo", @"//?/C:").SetName("ExtDrive_ForwardSlashAsDevice_TripleSlash");
                yield return new TestCaseData(@"\\.\C:\foo", @"\\.\C:").SetName("DeviceDrive");
                yield return new TestCaseData(@"\\.\C:\\foo", @"\\.\C:").SetName("DeviceDrive_DoubleSlash");
                yield return new TestCaseData(@"\\.\C:\\\foo", @"\\.\C:").SetName("DeviceDrive_TripleSlash");
                yield return new TestCaseData(@"\\.\pipe\name", @"\\.\pipe").SetName("NamedPipe");
                yield return new TestCaseData(@"\\.\pipe\\name", @"\\.\pipe").SetName("NamedPipe_DoubleSlash");
                yield return new TestCaseData(@"\\.\pipe\\\name", @"\\.\pipe").SetName("NamedPipe_TripleSlash");
                yield return new TestCaseData(@"\\.\PhysicalDrive0", @"\\.\PhysicalDrive0").SetName("PhysicalDrive");

                // Volume GUID
                yield return new TestCaseData(@"\\?\Volume{12345678-1234-1234-1234-1234567890ab}\foo",
                                              @"\\?\Volume{12345678-1234-1234-1234-1234567890ab}")
                             .SetName("VolumeGuid");
                yield return new TestCaseData(@"\\?\Volume{12345678-1234-1234-1234-1234567890ab}\\foo",
                                              @"\\?\Volume{12345678-1234-1234-1234-1234567890ab}")
                             .SetName("VolumeGuid_DoubleSlash");
                yield return new TestCaseData(@"\\?\Volume{12345678-1234-1234-1234-1234567890ab}\\\foo",
                                              @"\\?\Volume{12345678-1234-1234-1234-1234567890ab}")
                             .SetName("VolumeGuid_DoubleSlash");
            }
            else
            {
                yield return new TestCaseData(@"/root/folder", "/").SetName("RootedFolder");
            }
        }

        [TestCaseSource(nameof(RootSegment_CommonCases_TestData))]
        public void TestGetRootSegment(string input, string expected)
        {
            string actual = PathInternal.GetRootSegment(input.AsSpan()).ToString();

            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
