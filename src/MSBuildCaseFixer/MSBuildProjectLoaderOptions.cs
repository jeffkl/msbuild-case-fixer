using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    public record struct MSBuildProjectLoaderOptions
    {
        /// <summary>
        /// Gets or sets an <see cref="IReadOnlyCollection{T}" /> containing paths to the projects to load.
        /// </summary>
        public IReadOnlyCollection<string> ProjectPaths { get; set; }

        /// <summary>
        /// Gets or sets an <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing global properties to use when loading the projects.
        /// </summary>
        public IReadOnlyDictionary<string, string> GlobalProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the user can be prompted interactively.
        /// </summary>
        public bool Interactive { get; set; }
    }
}