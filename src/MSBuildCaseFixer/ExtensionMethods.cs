using Microsoft.Build.Evaluation;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents extension methods used by this assembly.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Gets the current item's full path in the correct case according to the file system.
        /// </summary>
        /// <param name="projectItem">The current <see cref="ProjectItem" />.</param>
        /// <param name="fileSystem">An <see cref="IFileSystem" /> to use when getting information from the file system.</param>
        /// <returns></returns>
        public static string GetFullPathInCorrectCase(this ProjectItem projectItem, IFileSystem fileSystem)
        {
            try
            {
                IFileInfo fileInfo = fileSystem.FileInfo.FromFileName(Path.Combine(projectItem.Project.DirectoryPath, projectItem.EvaluatedInclude));

                if (!fileInfo.Exists)
                {
                    return projectItem.EvaluatedInclude;
                }

                using (FileStream stream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    StringBuilder stringBuilder = new StringBuilder(NativeMethods.GetFinalPathNameByHandle(stream.SafeFileHandle, null, 0, 0));

                    NativeMethods.GetFinalPathNameByHandle(stream.SafeFileHandle, stringBuilder, stringBuilder.Capacity, 0);

                    return stringBuilder.ToString(4, stringBuilder.Capacity - 5);
                }
            }
            catch (Exception)
            {
                return projectItem.EvaluatedInclude;
            }
        }
    }
}