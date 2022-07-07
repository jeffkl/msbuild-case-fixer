using Microsoft.Build.Evaluation;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace MSBuildCaseFixer
{
    internal static class ExtensionMethods
    {
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