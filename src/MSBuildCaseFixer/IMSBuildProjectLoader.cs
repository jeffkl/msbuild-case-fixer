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
        /// Occurs when a project is loaded.
        /// </summary>
        event EventHandler<IMSBuildProject> ProjectLoaded;

        /// <summary>
        /// Loads the specified MSBuild project and all of its dependencies.
        /// </summary>
        /// <param name="options">A <see cref="MSBuildProjectLoaderOptions" /> object with options for how to projects should be loaded.</param>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing <see cref="IMSBuildProject" /> objects.</returns>
        IReadOnlyCollection<IMSBuildProject> Load(MSBuildProjectLoaderOptions options);
    }
}