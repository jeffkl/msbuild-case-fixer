namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a resolved import in an MSBuild project.
    /// </summary>
    public interface IMSBuildResolvedImport
    {
        /// <summary>
        /// Gets the full path to the project that contains the import.
        /// </summary>
        string ContainingProject { get; }

        /// <summary>
        /// Gets the evaluated full path to the project.
        /// </summary>
        string EvaluatedProjectPath { get; }

        /// <summary>
        /// Gets the unevaluated path to the project.
        /// </summary>
        string UnevaluatedProjectPath { get; }
    }
}