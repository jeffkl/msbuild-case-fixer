using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents an MSBuild project item.
    /// </summary>
    internal sealed class MSBuildProjectItem : IMSBuildProjectItem
    {
        /// <summary>
        /// Stores the <see cref="FieldInfo "/> of the private _xml field of a <see cref="ProjectItem" /> object.
        /// </summary>
        private static readonly FieldInfo ProjectItemXmlFieldInfo = typeof(ProjectItem).GetField("_xml", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Stores the <see cref="ProjectItemElement" /> representing the project item.
        /// </summary>
        private readonly ProjectItemElement? _itemElement;

        /// <summary>
        /// Stores the current <see cref="ProjectItem" /> object.
        /// </summary>
        private readonly ProjectItem _projectItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildProjectItem" /> class.
        /// </summary>
        /// <param name="projectItem">The <see cref="ProjectItem" /> to wrap.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public MSBuildProjectItem(ProjectItem projectItem)
        {
            _projectItem = projectItem ?? throw new ArgumentNullException(nameof(projectItem));

            _itemElement = ProjectItemXmlFieldInfo.GetValue(_projectItem) as ProjectItemElement;

            ProjectFullPath = _itemElement?.ContainingProject.FullPath ?? string.Empty;
        }

        /// <inheritdoc cref="IMSBuildProjectItem.DirectMetadata" />
        public IReadOnlyCollection<IMSBuildProjectItemMetadata> DirectMetadata => new CollectionWrapper<MSBuildProjectItemMetadata, ProjectMetadata>(_projectItem.DirectMetadata, _projectItem.DirectMetadataCount, (itemMetadata) => new MSBuildProjectItemMetadata(itemMetadata));

        /// <inheritdoc cref="IMSBuildProjectItem.EvaluatedInclude" />
        public string EvaluatedInclude => _projectItem.EvaluatedInclude;

        /// <inheritdoc cref="IMSBuildProjectItem.IsImported" />
        public bool IsImported => _projectItem.IsImported;

        /// <inheritdoc cref="IMSBuildProjectItem.ItemType" />
        public string ItemType => _projectItem.ItemType;

        /// <inheritdoc cref="IMSBuildProjectItem.ProjectFullPath" />
        public string ProjectFullPath { get; private set; }

        /// <inheritdoc cref="IMSBuildProjectItem.UnevaluatedInclude" />
        public string UnevaluatedInclude => _projectItem.UnevaluatedInclude;

        public string GetElementLocation() => _itemElement?.Location.ToString() ?? string.Empty;

        /// <inheritdoc cref="IMSBuildProjectItem.GetMetadataValue(string)" />
        public string GetMetadataValue(string name) => _projectItem.GetMetadataValue(name);
    }
}