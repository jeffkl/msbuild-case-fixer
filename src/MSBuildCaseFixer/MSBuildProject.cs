using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;

namespace MSBuildCaseFixer
{
    internal sealed class MSBuildProject : IMSBuildProject
    {
        private readonly Lazy<IReadOnlyDictionary<string, string>> _globalPropertiesLazy;

        private readonly Project _project;

        public MSBuildProject(Project project)
        {
            _project = project ?? throw new ArgumentNullException(nameof(project));

            _globalPropertiesLazy = new Lazy<IReadOnlyDictionary<string, string>>(() => new ReadOnlyDictionaryWrapper(_project.GlobalProperties));
        }

        public string DirectoryPath => _project.DirectoryPath;

        public string FullPath => _project.FullPath;

        public IReadOnlyDictionary<string, string> GlobalProperties => _globalPropertiesLazy.Value;

        public IReadOnlyCollection<IMSBuildResolvedImport> Imports => new CollectionWrapper<IMSBuildResolvedImport, ResolvedImport>(_project.Imports, (resolvedImport) => new MSBuildResolvedImport(resolvedImport));

        public IReadOnlyCollection<IMSBuildProjectItem> Items => new CollectionWrapper<IMSBuildProjectItem, ProjectItem>(_project.Items, (projectItem) => new MSBuildProjectItem(projectItem));

        public string GetProperty(string name) => _project.GetPropertyValue(name);
    }
}