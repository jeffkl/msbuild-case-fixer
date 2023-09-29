using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an evaluated MSBuild project.
    /// </summary>
    internal record struct MSBuildProject : IMSBuildProject
    {
        /// <summary>
        /// Stores a <see cref="Lazy{T}" /> containing the global properties which avoids MSBuild making a copy every time they are accessed.
        /// </summary>
        private readonly Lazy<IReadOnlyDictionary<string, string>> _globalPropertiesLazy;

        /// <summary>
        /// Stores the evaluated <see cref="Project" />.
        /// </summary>
        private readonly Project _project;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildProject " /> class.
        /// </summary>
        /// <param name="project">A <see cref="Project" /> object representing the MSBuild project.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="project" /> is <see langword="null" />.</exception>
        public MSBuildProject(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            _globalPropertiesLazy = new Lazy<IReadOnlyDictionary<string, string>>(() => new ReadOnlyDictionaryWrapper<string, string>(project.GlobalProperties));
        }

        public string DirectoryPath => _project.DirectoryPath;

        public string FullPath => _project.FullPath;

        public IReadOnlyDictionary<string, string> GlobalProperties => _globalPropertiesLazy.Value;

        public IReadOnlyCollection<IMSBuildResolvedImport> Imports => new CollectionWrapper<IMSBuildResolvedImport, ResolvedImport>(_project.Imports, (resolvedImport) => new MSBuildResolvedImport(resolvedImport));

        public IReadOnlyCollection<IMSBuildProjectItem> Items => new CollectionWrapper<IMSBuildProjectItem, ProjectItem>(_project.Items, (projectItem) => new MSBuildProjectItem(projectItem));

        public string GetProperty(string name) => _project.GetPropertyValue(name);
    }
}