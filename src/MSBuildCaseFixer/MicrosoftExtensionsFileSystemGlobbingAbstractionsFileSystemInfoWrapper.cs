using System;
using System.IO.Abstractions;

using DirectoryInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoBase;
using FileInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileInfoBase;
using FileSystemInfoBase = Microsoft.Extensions.FileSystemGlobbing.Abstractions.FileSystemInfoBase;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a <see cref="FileInfoBase" /> wrapper for a <see cref="IFileInfo" /> object.
    /// </summary>
    internal class MicrosoftExtensionsFileSystemGlobbingAbstractionsFileInfoWrapper : FileInfoBase
    {
        private readonly IFileInfo _fileInfo;

        /// <summary>
        /// Initializes a new instance for the <see cref="MicrosoftExtensionsFileSystemGlobbingAbstractionsFileInfoWrapper" /> class for the specified <see cref="IFileInfo " /> object.
        /// </summary>
        /// <param name="fileSystemInfo">The <see cref="IFileInfo " /> object to wrap.</param>
        public MicrosoftExtensionsFileSystemGlobbingAbstractionsFileInfoWrapper(IFileInfo fileSystemInfo)
        {
            _fileInfo = fileSystemInfo ?? throw new ArgumentNullException(nameof(fileSystemInfo));
        }

        /// <inheritdoc cref="FileSystemInfoBase.FullName" />
        public override string FullName => _fileInfo.FullName;

        /// <inheritdoc cref="FileSystemInfoBase.Name" />
        public override string Name => _fileInfo.Name;

        /// <inheritdoc cref="FileSystemInfoBase.ParentDirectory" />
        public override DirectoryInfoBase ParentDirectory => new MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper(_fileInfo.Directory!);
    }
}