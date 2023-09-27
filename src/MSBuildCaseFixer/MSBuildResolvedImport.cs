using Microsoft.Build.Evaluation;

namespace MSBuildCaseFixer
{
    internal record struct MSBuildResolvedImport : IMSBuildResolvedImport
    {
        /// <summary>
        /// Stores the <see cref="ResolvedImport" /> object from the MSBuild project.
        /// </summary>
        private readonly ResolvedImport _resolvedImport;

        /// <summary>
        /// Stores the element location as a string.
        /// </summary>
        private string? _elementLocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="MSBuildResolvedImport" /> class.
        /// </summary>
        /// <param name="resolvedImport">The <see cref="ResolvedImport" /> from the MSBuild project.</param>
        internal MSBuildResolvedImport(ResolvedImport resolvedImport)
        {
            _resolvedImport = resolvedImport;
        }

        public string ContainingProject => _resolvedImport.ImportingElement.ContainingProject.FullPath;

        public string EvaluatedProjectPath => _resolvedImport.ImportedProject.FullPath;

        public string UnevaluatedProjectPath => _resolvedImport.ImportingElement.Project;

        public string ElementLocation => _elementLocation ??= _resolvedImport.ImportingElement.Location.ToString();
    }
}