using System;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an interface for a class that loads MSBuild projects.
    /// </summary>
    public interface IMSBuildProjectLoader
    {
        /// <summary>
        /// Loads the specified MSBuild project and all of its dependencies.
        /// </summary>
        /// <param name="projectPaths">An <see cref="IReadOnlyCollection{T}" /> containing paths to the projects to load.</param>
        /// <param name="globalProperties">A <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing global properties to use when loading the projects.</param>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing <see cref="IMSBuildProject" /> objects.</returns>
        IReadOnlyCollection<IMSBuildProject> Load(IReadOnlyCollection<string> projectPaths, IReadOnlyDictionary<string, string> globalProperties);

        event EventHandler<IMSBuildProject> ProjectLoaded;
    }
}