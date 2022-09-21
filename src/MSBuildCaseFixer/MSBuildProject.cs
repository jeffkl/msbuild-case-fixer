using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an evaluated MSBuild project.
    /// </summary>
    internal sealed class MSBuildProject : IMSBuildProject
    {
        /// <summary>
        /// Stores a <see cref="Lazy{T}" /> containing the global properties which avoids MSBuild making a copy every time they are accessed.
        /// </summary>
        private readonly Lazy<IReadOnlyDictionary<string, string>> _globalPropertiesLazy;

        /// <summary>
        /// Stores the evaluated <see cref="Project" />.
        /// </summary>
        private readonly Project _project;

        public MSBuildProject(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            _globalPropertiesLazy = new Lazy<IReadOnlyDictionary<string, string>>(() => new ReadOnlyDictionaryWrapper(_project.GlobalProperties));
        }

        /// <inheritdoc cref="IMSBuildProject.DirectoryPath" />
        public string DirectoryPath => _project.DirectoryPath;

        /// <inheritdoc cref="IMSBuildProject.FullPath" />
        public string FullPath => _project.FullPath;

        /// <inheritdoc cref="IMSBuildProject.GlobalProperties" />
        public IReadOnlyDictionary<string, string> GlobalProperties => _globalPropertiesLazy.Value;

        /// <inheritdoc cref="IMSBuildProject.Imports" />
        public IReadOnlyCollection<IMSBuildResolvedImport> Imports => new CollectionWrapper<IMSBuildResolvedImport, ResolvedImport>(_project.Imports, (resolvedImport) => new MSBuildResolvedImport(resolvedImport));

        /// <inheritdoc cref="IMSBuildProject.Items" />
        public IReadOnlyCollection<IMSBuildProjectItem> Items => new CollectionWrapper<IMSBuildProjectItem, ProjectItem>(_project.Items, (projectItem) => new MSBuildProjectItem(projectItem));

        /// <inheritdoc cref="IMSBuildProject.GetProperty(string)" />
        public string GetProperty(string name) => _project.GetPropertyValue(name);
    }
}