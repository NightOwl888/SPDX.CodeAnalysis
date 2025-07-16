using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SPDX.CodeAnalysis
{
    internal static class PathInternal
    {
        private interface IPath
        {
            //bool IsValidDriveChar(char value);
            //bool IsDevice(ReadOnlySpan<char> path);
            //bool IsDeviceUNC(ReadOnlySpan<char> path);
            //bool IsExtended(ReadOnlySpan<char> path);
            int GetRootLength(ReadOnlySpan<char> path);
            bool IsDirectorySeparator(char c);
            bool IsEffectivelyEmpty(ReadOnlySpan<char> path);

            bool IsPathRooted(ReadOnlySpan<char> path);
            ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path);

            char DirectorySeparatorChar { get; }
            char AltDirectorySeparatorChar { get; }
        }

        private sealed class Windows : IPath
        {
            //internal const char DirectorySeparatorChar = '\\';
            //internal const char AltDirectorySeparatorChar = '/';

            public char DirectorySeparatorChar => '\\';
            public char AltDirectorySeparatorChar => '/';

            internal const char VolumeSeparatorChar = ':';

            //internal const string UncExtendedPathPrefix = @"\\?\UNC\";

            // \\?\, \\.\, \??\
            internal const int DevicePrefixLength = 4;
            // \\
            internal const int UncPrefixLength = 2;
            // \\?\UNC\, \\.\UNC\
            internal const int UncExtendedPrefixLength = 8;

            /// <summary>
            /// Returns true if the given character is a valid drive letter
            /// </summary>
            public bool IsValidDriveChar(char value)
            {
                return (uint)((value | 0x20) - 'a') <= (uint)('z' - 'a');
            }

            /// <summary>
            /// Returns true if the path uses any of the DOS device path syntaxes. ("\\.\", "\\?\", or "\??\")
            /// </summary>
            public bool IsDevice(ReadOnlySpan<char> path)
            {
                // If the path begins with any two separators is will be recognized and normalized and prepped with
                // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
                return IsExtended(path)
                    ||
                    (
                        path.Length >= DevicePrefixLength
                        && IsDirectorySeparator(path[0])
                        && IsDirectorySeparator(path[1])
                        && (path[2] == '.' || path[2] == '?')
                        && IsDirectorySeparator(path[3])
                    );
            }

            /// <summary>
            /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
            /// </summary>
            public bool IsDeviceUNC(ReadOnlySpan<char> path)
            {
                return path.Length >= UncExtendedPrefixLength
                    && IsDevice(path)
                    && IsDirectorySeparator(path[7])
                    && path[4] == 'U'
                    && path[5] == 'N'
                    && path[6] == 'C';
            }

            /// <summary>
            /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
            /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
            /// and path length checks.
            /// </summary>
            public bool IsExtended(ReadOnlySpan<char> path)
            {
                // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
                // Skipping of normalization will *only* occur if back slashes ('\') are used.
                return path.Length >= DevicePrefixLength
                    && path[0] == '\\'
                    && (path[1] == '\\' || path[1] == '?')
                    && path[2] == '?'
                    && path[3] == '\\';
            }

            /// <summary>
            /// Gets the length of the root of the path (drive, share, etc.).
            /// </summary>
            public int GetRootLength(ReadOnlySpan<char> path)
            {
                int pathLength = path.Length;
                int i = 0;

                bool deviceSyntax = IsDevice(path);
                bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

                if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
                {
                    // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                    if (deviceUnc || (pathLength > 1 && IsDirectorySeparator(path[1])))
                    {
                        // UNC (\\?\UNC\ or \\), scan past server\share

                        // Start past the prefix ("\\" or "\\?\UNC\")
                        i = deviceUnc ? UncExtendedPrefixLength : UncPrefixLength;

                        // Skip two separators at most
                        int n = 2;
                        while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                            i++;
                    }
                    else
                    {
                        // Current drive rooted (e.g. "\foo")
                        i = 1;
                    }
                }
                else if (deviceSyntax)
                {
                    // Device path (e.g. "\\?\.", "\\.\")
                    // Skip any characters following the prefix that aren't a separator
                    i = DevicePrefixLength;
                    while (i < pathLength && !IsDirectorySeparator(path[i]))
                        i++;

                    // If there is another separator take it, as long as we have had at least one
                    // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                    if (i < pathLength && i > DevicePrefixLength && IsDirectorySeparator(path[i]))
                        i++;
                }
                else if (pathLength >= 2
                    && path[1] == VolumeSeparatorChar
                    && IsValidDriveChar(path[0]))
                {
                    // Valid drive specified path ("C:", "D:", etc.)
                    i = 2;

                    // If the colon is followed by a directory separator, move past it (e.g "C:\")
                    if (pathLength > 2 && IsDirectorySeparator(path[2]))
                        i++;
                }

                return i;
            }

            /// <summary>
            /// True if the given character is a directory separator.
            /// </summary>
            public bool IsDirectorySeparator(char c)
            {
                return c == DirectorySeparatorChar || c == AltDirectorySeparatorChar;
            }

            /// <summary>
            /// Returns true if the path is effectively empty for the current OS.
            /// For unix, this is empty or null. For Windows, this is empty, null, or
            /// just spaces ((char)32).
            /// </summary>
            public bool IsEffectivelyEmpty(ReadOnlySpan<char> path)
            {
                if (path.IsEmpty)
                    return true;

                foreach (char c in path)
                {
                    if (c != ' ')
                        return false;
                }
                return true;
            }

            // From Path.Windows
            public bool IsPathRooted(ReadOnlySpan<char> path)
            {
                int length = path.Length;
                return (length >= 1 && IsDirectorySeparator(path[0]))
                    || (length >= 2 && IsValidDriveChar(path[0]) && path[1] == VolumeSeparatorChar);
            }

            /// <remarks>
            /// Unlike the string overload, this method will not normalize directory separators.
            /// </remarks>
            public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
            {
                if (PathInternal.IsEffectivelyEmpty(path))
                    return ReadOnlySpan<char>.Empty;

                int pathRoot = PathInternal.GetRootLength(path);
                return pathRoot <= 0 ? ReadOnlySpan<char>.Empty : path.Slice(0, pathRoot);
            }
        }

        private sealed class Unix : IPath
        {
            //internal const char DirectorySeparatorChar = '/';
            //internal const char AltDirectorySeparatorChar = '/';

            public char DirectorySeparatorChar => '/';
            public char AltDirectorySeparatorChar => '/';

            internal const string DirectorySeparatorCharAsString = "/";

            public int GetRootLength(ReadOnlySpan<char> path)
            {
                return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
            }

            public bool IsDirectorySeparator(char c)
            {
                // The alternate directory separator char is the same as the directory separator,
                // so we only need to check one.
                Debug.Assert(DirectorySeparatorChar == AltDirectorySeparatorChar);
                return c == DirectorySeparatorChar;
            }

            public bool IsEffectivelyEmpty(ReadOnlySpan<char> path)
            {
                return path.IsEmpty;
            }

            // From Path.Unix.cs
            public bool IsPathRooted(ReadOnlySpan<char> path)
            {
                return path.StartsWith(DirectorySeparatorChar);
            }

            public ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path)
            {
                return IsPathRooted(path) ? DirectorySeparatorCharAsString.AsSpan() : ReadOnlySpan<char>.Empty;
            }
        }


        private static readonly IPath osPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? new Windows() : new Unix();

        public static bool IsDirectorySeparator(char c) => osPath.IsDirectorySeparator(c);


        internal static int GetRootLength(ReadOnlySpan<char> path) => osPath.GetRootLength(path);

        /// <summary>
        /// Returns true if the path is effectively empty for the current OS.
        /// For unix, this is empty or null. For Windows, this is empty, null, or
        /// just spaces ((char)32).
        /// </summary>
        internal static bool IsEffectivelyEmpty(ReadOnlySpan<char> path) => osPath.IsEffectivelyEmpty(path);

        internal static bool IsPathRooted(ReadOnlySpan<char> path) => osPath.IsPathRooted(path);

        internal static ReadOnlySpan<char> GetPathRoot(ReadOnlySpan<char> path) => osPath.GetPathRoot(path);

        internal static char DirectorySeparatorChar => osPath.DirectorySeparatorChar;
        internal static char AltDirectorySeparatorChar => osPath.AltDirectorySeparatorChar;
    }
}
