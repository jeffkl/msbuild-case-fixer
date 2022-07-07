namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an interface for a class that stores metadata for an MSBuild item.
    /// </summary>
    public interface IMSBuildProjectItemMetadata
    {
        /// <summary>
        /// Gets the evaluated value for the metadata.
        /// </summary>
        string EvaluatedValue { get; }

        /// <summary>
        /// Gets the name of the metadata.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the unevaluated value for the metadata.
        /// </summary>
        string UnevaluatedValue { get; }
    }
}