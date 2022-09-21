using Microsoft.Build.Evaluation;

namespace MSBuildCaseFixer
{
    internal sealed class MSBuildResolvedImport : IMSBuildResolvedImport
    {
        private readonly ResolvedImport _resolvedImport;

        internal MSBuildResolvedImport(ResolvedImport resolvedImport)
        {
            _resolvedImport = resolvedImport;
        }

        public string ContainingProject => _resolvedImport.ImportingElement.ContainingProject.FullPath;

        public string EvaluatedProjectPath => _resolvedImport.ImportedProject.FullPath;

        public string UnevaluatedProjectPath => _resolvedImport.ImportingElement.Project;

        public string GetElementLocation() => _resolvedImport.ImportingElement.Location.ToString();
    }
}