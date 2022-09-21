using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading;

namespace MSBuildCaseFixer
{
    internal static class Utility
    {
        private static readonly ConcurrentDictionary<FileInfo, Lazy<string?>> _filePathCache = new ConcurrentDictionary<FileInfo, Lazy<string?>>(FileInfoFullNameCaseInsensitiveComparer.Instance);

        private static int _uniqueFilePathCount = 0;

        public static int CacheCount => _filePathCache.Count;

        public static int FileCount => _uniqueFilePathCount;

        public static bool TryGetFullPathInCorrectCase(string rootPath, string path, out string? actual)
        {
            actual = default;

            try
            {
                FileInfo fileInfo = new FileInfo(Path.Combine(rootPath, path));

                Lazy<string?> lazy = _filePathCache.GetOrAdd(
                    fileInfo,
                    (fileInfo) => new Lazy<string?>(() =>
                    {
                        if (!fileInfo.Exists)
                        {
                            return null;
                        }
                        
                        Interlocked.Increment(ref _uniqueFilePathCount);

                        using (FileStream stream = fileInfo.OpenRead())
                        {
                            StringBuilder stringBuilder = new StringBuilder(NativeMethods.GetFinalPathNameByHandle(stream.SafeFileHandle, lpszFilePath: null, cchFilePath: 0, dwFlags: 0));

                            NativeMethods.GetFinalPathNameByHandle(stream.SafeFileHandle, stringBuilder, stringBuilder.Capacity, dwFlags: 0);

                            return stringBuilder.ToString(4, stringBuilder.Capacity - 5);
                        }
                    }));

                actual = lazy.Value;

                return actual is not null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to find the specified executable on the PATH.
        /// </summary>
        /// <param name="exe">The name of the executable to find.</param>
        /// <param name="validator">A <see cref="Func{T, TResult}" /> that validates if a found item on the PATH is what the caller is looking for.</param>
        /// <param name="environmentVariableProvider">An <see cref="IEnvironmentVariableProvider" /> to use when reading environment variables.</param>
        /// <param name="fileSystem">An <see cref="IFileSystem" /> to use when accessing the file system.</param>
        /// <param name="fileInfo">Receives a <see cref="IFileInfo" /> object with details about the executable if found.</param>
        /// <returns><code>true</code> if an executable could be found, otherwise <code>false</code>.</returns>
        internal static bool TryFindOnPath(string exe, Func<IFileInfo, bool>? validator, IEnvironmentVariableProvider environmentVariableProvider, IFileSystem fileSystem, out IFileInfo? fileInfo)
        {
            string? pathEnvironmentVariable = environmentVariableProvider.GetEnvironmentVariable("PATH");

            fileInfo = default;

            if (string.IsNullOrWhiteSpace(pathEnvironmentVariable))
            {
                return false;
            }

            IFileInfo candidateFileInfo;

            foreach (string? entry in pathEnvironmentVariable!.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                try
                {
                    candidateFileInfo = fileSystem.FileInfo.FromFileName(Path.Combine(entry.Trim(), exe));
                }
                catch (Exception)
                {
                    continue;
                }

                if (!candidateFileInfo.Exists)
                {
                    continue;
                }

                if (validator != null)
                {
                    try
                    {
                        if (!validator(candidateFileInfo))
                        {
                            continue;
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                fileInfo = candidateFileInfo;

                return true;
            }

            return false;
        }

        private class FileInfoFullNameCaseInsensitiveComparer : IEqualityComparer<FileInfo>
        {
            private FileInfoFullNameCaseInsensitiveComparer()
            {
            }

            public static FileInfoFullNameCaseInsensitiveComparer Instance { get; } = new FileInfoFullNameCaseInsensitiveComparer();

            public bool Equals(FileInfo x, FileInfo y)
            {
                return string.Equals(x.FullName, y.FullName, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode(FileInfo obj)
            {
                return obj.FullName.GetHashCode();
            }
        }
    }
}