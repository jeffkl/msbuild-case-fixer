using Microsoft.Build.Evaluation;
using System;

namespace MSBuildCaseFixer
{
    internal record struct MSBuildProjectItemMetadata : IMSBuildProjectItemMetadata
    {
        /// <summary>
        /// Stores an <see cref="ProjectMetadata" /> object
        /// </summary>
        private readonly ProjectMetadata _projectMetadata;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildProjectItemMetadata " /> class.
        /// </summary>
        /// <param name="projectMetadata">A <see cref="ProjectMetadata" /> object from an MSBuild project.</param>
        /// <exception cref="ArgumentNullException">When <paramref name="projectMetadata" /> is <see langword="null" />.</exception>
        public MSBuildProjectItemMetadata(ProjectMetadata projectMetadata)
        {
            _projectMetadata = projectMetadata ?? throw new ArgumentNullException(nameof(projectMetadata));
        }

        public string EvaluatedValue => _projectMetadata.EvaluatedValue;

        public string UnevaluatedValue => _projectMetadata.UnevaluatedValue;

        public string Name => _projectMetadata.Name;
    }
}