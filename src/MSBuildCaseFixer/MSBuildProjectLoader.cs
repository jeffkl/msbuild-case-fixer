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
        private MSBuildProjectLoaderOptions _options;

        public event EventHandler<IMSBuildProject>? ProjectLoaded;

        public IReadOnlyCollection<IMSBuildProject> Load(MSBuildProjectLoaderOptions options)
        {
            _options = options;

            ILogger[] loggers =
            [
                //new BinaryLogger
                //{
                //    Parameters = "LogFile=msbuild.binlog",
                //    CollectProjectImports = BinaryLogger.ProjectImportsCollectionMode.Embed,
                //    Verbosity = LoggerVerbosity.Diagnostic,
                //},
                new ConsoleLogger
                {
                    Parameters = "Verbosity=Minimal",
                },
            ];

            EvaluationContext evaluationContext = EvaluationContext.Create(EvaluationContext.SharingPolicy.Shared);

            using ProjectCollection projectCollection = new ProjectCollection(
                globalProperties: null,
                loggers: loggers,
                remoteLoggers: null,
                ToolsetDefinitionLocations.Default,
                maxNodeCount: 1,
                onlyLogCriticalEvents: false,
                loadProjectsReadOnly: true);

            Dictionary<string, string> projectGraphEntryPointGlobalProperties = options.GlobalProperties as Dictionary<string, string>
                ?? options.GlobalProperties.ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);

            ProjectGraphEntryPoint[] projectGraphEntryPoints = options.ProjectPaths.Select(i => new ProjectGraphEntryPoint(i, projectGraphEntryPointGlobalProperties)).ToArray();

            ProjectGraph projectGraph = new ProjectGraph(projectGraphEntryPoints, projectCollection, CreateProjectInstance);

            return new CollectionWrapper<IMSBuildProject, Project>(projectCollection.LoadedProjects, (project) => new MSBuildProject(project));

            /// <summary>
            /// Creates a <see cref="ProjectInstance" /> for the specified project.
            /// </summary>
            /// <param name="projectPath">The full path to the project file to create a <see cref="ProjectInstance" /> for.</param>
            /// <param name="globalProperties">A <see cref="Dictionary{TKey, TValue}" /> containing global properties to use when creating the project instance.</param>
            /// <param name="projectCollection">The <see cref="ProjectCollection" /> to load the project under.</param>
            /// <returns>A <see cref="ProjectInstance" /> for the specified project.</returns>
            ProjectInstance CreateProjectInstance(string projectPath, Dictionary<string, string> globalProperties, ProjectCollection projectCollection)
            {
                ProjectOptions projectOptions = new ProjectOptions
                {
                    EvaluationContext = evaluationContext,
                    GlobalProperties = globalProperties,
                    Interactive = _options.Interactive,
                    LoadSettings = ProjectLoadSettings.Default,
                    ProjectCollection = projectCollection,
                };

                Project project = Project.FromFile(projectPath, projectOptions);

                ProjectLoaded?.Invoke(this, new MSBuildProject(project));

                return project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup, evaluationContext);
            }
        }


    }
}