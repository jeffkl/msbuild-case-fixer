using Microsoft.Build.Evaluation;
using System;

namespace MSBuildCaseFixer
{
    internal sealed class MSBuildResolvedImport : IMSBuildResolvedImport
    {
        private readonly object _obj;

        internal MSBuildResolvedImport(ResolvedImport? resolvedImport)
        {
            if (resolvedImport is null)
            {
                throw new ArgumentException(nameof(resolvedImport));
            }

            _obj = resolvedImport;

            EvaluatedProjectPath = resolvedImport.Value.ImportedProject.FullPath;

            UnevaluatedProjectPath = resolvedImport.Value.ImportingElement.Project;

            ContainingProject = resolvedImport.Value.ImportingElement.ContainingProject.FullPath;
        }

        public string EvaluatedProjectPath { get; set; }

        public string UnevaluatedProjectPath { get; set; }
        public string ContainingProject { get; }
    }
}