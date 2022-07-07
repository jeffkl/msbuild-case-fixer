using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSBuildCaseFixer
{
    internal sealed class MSBuildProjectItem : IMSBuildProjectItem
    {
        private static readonly FieldInfo ProjectItemXmlFieldInfo = typeof(ProjectItem).GetField("_xml", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly ProjectItem _projectItem;

        public MSBuildProjectItem(ProjectItem projectItem)
        {
            _projectItem = projectItem ?? throw new ArgumentNullException(nameof(projectItem));

            ProjectItemElement? itemElement = ProjectItemXmlFieldInfo.GetValue(_projectItem) as ProjectItemElement;

            ProjectFullPath = itemElement?.ContainingProject.FullPath ?? string.Empty;
        }

        public IReadOnlyCollection<IMSBuildProjectItemMetadata> DirectMetadata => new CollectionWrapper<MSBuildProjectItemMetadata, ProjectMetadata>(_projectItem.DirectMetadata, _projectItem.DirectMetadataCount, (itemMetadata) => new MSBuildProjectItemMetadata(itemMetadata));

        public string EvaluatedInclude => _projectItem.EvaluatedInclude;

        public bool IsImported => _projectItem.IsImported;

        public string ItemType => _projectItem.ItemType;

        public string ProjectFullPath { get; private set; }

        public string UnevaluatedInclude => _projectItem.UnevaluatedInclude;

        public string GetMetadataValue(string name) => _projectItem.GetMetadataValue(name);
    }
}