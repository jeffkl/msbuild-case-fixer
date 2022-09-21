using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Graph;
using Microsoft.Build.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildCaseFixer
{
    internal class MSBuildProjectLoader : IMSBuildProjectLoader
    {
        public event EventHandler<IMSBuildProject>? ProjectLoaded;

        public IReadOnlyCollection<IMSBuildProject> Load(IReadOnlyCollection<string> projectPaths, IReadOnlyDictionary<string, string> globalProperties)
        {
            ILogger[] loggers = new ILogger[]
            {
                //new BinaryLogger
                //{
                //    Parameters = "LogFile=msbuild.binlog",
                //    CollectProjectImports = BinaryLogger.ProjectImportsCollectionMode.Embed,
                //    Verbosity = LoggerVerbosity.Diagnostic,
                //},
            };

            using ProjectCollection projectCollection = new ProjectCollection(
                globalProperties: null,
                loggers: loggers,
                remoteLoggers: null,
                ToolsetDefinitionLocations.Default,
                maxNodeCount: 1,
                onlyLogCriticalEvents: false,
                loadProjectsReadOnly: true);

            EvaluationContext evaluationContext = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);

            Dictionary<string, string> projectGraphEntryPointGlobalProperties = globalProperties as Dictionary<string, string>
                ?? globalProperties.ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);

            ProjectGraphEntryPoint[] projectGraphEntryPoints = projectPaths.Select(i => new ProjectGraphEntryPoint(i, projectGraphEntryPointGlobalProperties)).ToArray();

            ProjectGraph projectGraph = new ProjectGraph(
                projectGraphEntryPoints,
                projectCollection,
                (projectFullPath, projectGlobalProperties, projectCollectionForProject) =>
                {
                    (Project project, ProjectInstance projectInstance) = CreateProjectInstance(projectFullPath, projectGlobalProperties, projectCollectionForProject, evaluationContext);

                    OnProjectLoaded(project);

                    return projectInstance;
                });

            return new CollectionWrapper<IMSBuildProject, Project>(projectCollection.LoadedProjects, (project) => new MSBuildProject(project));
        }

        private static (Project Project, ProjectInstance ProjectInstance) CreateProjectInstance(
            string projectPath,
            Dictionary<string, string> globalProperties,
            ProjectCollection projectCollection,
            EvaluationContext evaluationContext)
        {
            ProjectOptions projectOptions = new ProjectOptions
            {
                EvaluationContext = evaluationContext,
                GlobalProperties = globalProperties,
                LoadSettings = ProjectLoadSettings.Default,
                ProjectCollection = projectCollection
            };

            Project project = Project.FromFile(projectPath, projectOptions);

            return (project, project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup, evaluationContext));
        }

        private void OnProjectLoaded(Project project)
        {
            ProjectLoaded?.Invoke(this, new MSBuildProject(project));
        }
    }
}