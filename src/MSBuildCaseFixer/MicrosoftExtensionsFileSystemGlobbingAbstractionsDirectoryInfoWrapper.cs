using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

using DirectoryInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase;
using FileInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileInfoBase;
using FileSystemInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileSystemInfoBase;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a wrapper of a <see cref="IDirectoryInfo " /> object as a <see cref="DirectoryInfoBase" /> object.
    /// </summary>
    internal class MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper : DirectoryInfoBase
    {
        private readonly IDirectoryInfo _directoryInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper" /> class for the specified <see cref="IDirectoryInfo" />.
        /// </summary>
        /// <param name="directoryInfo">The <see cref="IDirectoryInfo" /> object to wrap.</param>
        public MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper(IDirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
        }

        /// <inheritdoc cref="FileSystemInfoBase.FullName" />
        public override string FullName => _directoryInfo.FullName;

        /// <inheritdoc cref="FileSystemInfoBase.Name" />
        public override string Name => _directoryInfo.Name;

        /// <inheritdoc cref="FileSystemInfoBase.ParentDirectory" />
        public override DirectoryInfoBase ParentDirectory => new MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper(_directoryInfo.Parent!);

        /// <inheritdoc cref="DirectoryInfoBase.EnumerateFileSystemInfos" />
        public override IEnumerable<FileSystemInfoBase> EnumerateFileSystemInfos() => _directoryInfo.EnumerateFiles().Select(i => new MicrosoftExtensionsFileSystemGlobbingAbstractionsFileInfoWrapper(i));

        /// <inheritdoc cref="DirectoryInfoBase.GetDirectory(string)" />
        public override DirectoryInfoBase GetDirectory(string path) => new MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper(_directoryInfo.FileSystem.DirectoryInfo.New(path));

        /// <inheritdoc cref="DirectoryInfoBase.GetFile(string)" />
        public override FileInfoBase GetFile(string path) => new MicrosoftExtensionsFileSystemGlobbingAbstractionsFileInfoWrapper(_directoryInfo.FileSystem.FileInfo.New(path));
    }
}