using Microsoft.Build.Evaluation;
using System;

namespace MSBuildCaseFixer
{
    internal sealed class MSBuildProjectItemMetadata : IMSBuildProjectItemMetadata
    {
        private readonly ProjectMetadata _projectMetadata;

        public MSBuildProjectItemMetadata(ProjectMetadata? projectMetadata)
        {
            _projectMetadata = projectMetadata ?? throw new ArgumentNullException(nameof(projectMetadata));
        }

        public string EvaluatedValue => _projectMetadata.EvaluatedValue;
        
        public string UnevaluatedValue => _projectMetadata.UnevaluatedValue;

        public string Name => _projectMetadata.Name;
    }
}