using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an interface for an MSBuild project item.
    /// </summary>
    public interface IMSBuildProjectItem
    {
        /// <summary>
        /// Gets metadata specified on the item.
        /// </summary>
        IReadOnlyCollection<IMSBuildProjectItemMetadata> DirectMetadata { get; }

        /// <summary>
        /// Gets the evaluted include value for the current item.
        /// </summary>
        string EvaluatedInclude { get; }

        /// <summary>
        /// Gets a value indicating whether or not the item was from an imported file.
        /// </summary>
        bool IsImported { get; }

        /// <summary>
        /// Gets the item type of the current item.
        /// </summary>
        string ItemType { get; }

        /// <summary>
        /// Gets the full path to the project that contains the item.
        /// </summary>
        string ProjectFullPath { get; }

        /// <summary>
        /// Gets the unevaluated include value for the current item.
        /// </summary>
        string UnevaluatedInclude { get; }

        /// <summary>
        /// Gets the value of specified metadata.
        /// </summary>
        /// <param name="name">The name of the metadata.</param>
        /// <returns>The evaluated value of the metadata if one exists, otherwise <see cref="string.Empty" />.</returns>
        string GetMetadataValue(string name);

        string GetElementLocation();
    }
}