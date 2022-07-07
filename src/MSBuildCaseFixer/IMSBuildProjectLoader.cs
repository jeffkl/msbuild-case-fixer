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
        /// <param name="path">The path to the project to load.</param>
        /// <param name="globalProperties">A <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing global properties to use when loading the projects.</param>
        /// <returns>A <see cref="IReadOnlyDictionary{TKey, TValue}" /> containing <see cref="IMSBuildProject" /> objects.</returns>
        IReadOnlyCollection<IMSBuildProject> Load(string path, IReadOnlyDictionary<string, string> globalProperties);
    }
}