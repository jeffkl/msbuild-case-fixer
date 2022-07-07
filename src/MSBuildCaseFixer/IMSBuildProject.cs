using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an interface for a class that reads information from an MSBuild project.
    /// </summary>
    public interface IMSBuildProject
    {
        /// <summary>
        /// Gets the directory of the MSBuild project.
        /// </summary>
        string DirectoryPath { get; }

        /// <summary>
        /// Gets the full path to the MSBuild project.
        /// </summary>
        string FullPath { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing the global properties of the project.
        /// </summary>
        IReadOnlyDictionary<string, string> GlobalProperties { get; }

        /// <summary>
        /// Gets a collection of <see cref="IMSBuildResolvedImport" /> objects containing imports of the project.
        /// </summary>
        IReadOnlyCollection<IMSBuildResolvedImport> Imports { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyCollection{T}" /> containing <see cref="IMSBuildProjectItem" /> objects representing the items of the project.
        /// </summary>
        IReadOnlyCollection<IMSBuildProjectItem> Items { get; }

        /// <summary>
        /// Gets the value of an MSBuild property.
        /// </summary>
        /// <param name="name">The name of the property to retrieve the value of.</param>
        /// <returns>The value of the specified property if found, otherwise <see cref="string.Empty" />.</returns>
        string GetProperty(string name);
    }
}