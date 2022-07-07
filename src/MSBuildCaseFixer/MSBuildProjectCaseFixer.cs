using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace MSBuildCaseFixer
{
    internal class MSBuildProjectCaseFixer
    {
        private readonly IEnvironmentVariableProvider _environmentVariableProvider;
        private readonly IFileSystem _fileSystem;
        private readonly IMSBuildProjectLoader _projectLoader;

        public MSBuildProjectCaseFixer(IMSBuildProjectLoader projectLoader, IEnvironmentVariableProvider environmentVariableProvider, IFileSystem fileSystem)
        {
            _projectLoader = projectLoader ?? throw new ArgumentNullException(nameof(projectLoader));
            _environmentVariableProvider = environmentVariableProvider ?? throw new ArgumentNullException(nameof(environmentVariableProvider));
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        }

        public void FixProjectCase(string project, string root, IFileInfo msBuildExePath)
        {
            Stopwatch sw = Stopwatch.StartNew();

            IReadOnlyCollection<IMSBuildProject> msbuildProjects;

            using (MSBuildFeatureFlags.Enable(_environmentVariableProvider, msBuildExePath!.FullName))
            {
                Dictionary<string, string> globalProperties = new Dictionary<string, string>
                {
                    ["ExcludeRestorePackageImports"] = "true",
                    ["TraversalTranslateProjectFileItems"] = "false",
                };

                Stopwatch sw2 = Stopwatch.StartNew();

                Console.WriteLine("Loading projects...");

                msbuildProjects = _projectLoader.Load(project, new Dictionary<string, string>());

                sw2.Stop();

                Console.WriteLine("Loaded {0} project(s) in {1:N1}s", msbuildProjects.Count, sw2.Elapsed.TotalSeconds);
            }

            ConcurrentDictionary<string, ConcurrentDictionary<string, string>> replacementsByFile = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

            foreach (IMSBuildProject msbuildProject in msbuildProjects)
            {
                string targetFrameworks = msbuildProject.GetProperty("TargetFrameworks");
                string targetFramework;

                IReadOnlyDictionary<string, string> globalProperties = msbuildProject.GlobalProperties;

                if (!globalProperties.TryGetValue("TargetFramework", out targetFramework))
                {
                    targetFramework = msbuildProject.GetProperty("TargetFramework");

                    // Console.WriteLine("{0} = {1}", msbuildProject.FullPath, msbuildProject.Items.Count);

                    Parallel.ForEach(msbuildProject.Imports, (import) =>
                    {
                        if (import.EvaluatedProjectPath.StartsWith(root))
                        {
                            bool result = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, import.EvaluatedProjectPath, out string? actual);

                            if (result && !string.Equals(import.EvaluatedProjectPath, actual) && actual != null)
                            {
                                if (TryGetReplacement(import.UnevaluatedProjectPath, actual, out string replacement))
                                {
                                    ConcurrentDictionary<string, string> replacementsForThisItem = replacementsByFile.GetOrAdd(import.ContainingProject, (fullPath) => new ConcurrentDictionary<string, string>());

                                    replacementsForThisItem.TryAdd(import.UnevaluatedProjectPath, replacement);

                                    // Console.WriteLine("  {0} / {1} / {2} / {3}", item.UnevaluatedProjectPath, item.EvaluatedProjectPath, actual, replacement);
                                }
                            }
                        }
                    });

                    Parallel.ForEach(msbuildProject.Items, (item) =>
                    {
                        string projectFullPath = msbuildProject.FullPath;

                        if (item.IsImported)
                        {
                            if (!item.ProjectFullPath.StartsWith(root))
                            {
                                return;
                            }

                            projectFullPath = item.ProjectFullPath;
                        }

                        ConcurrentDictionary<string, string> replacementsForThisItem = replacementsByFile.GetOrAdd(projectFullPath, (fullPath) => new ConcurrentDictionary<string, string>());

                        string fullPath = item.GetMetadataValue("FullPath");

                        bool result = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, fullPath, out string? actual);

                        if (result && !string.Equals(fullPath, actual) && actual != null)
                        {
                            if (TryGetReplacement(item.UnevaluatedInclude, actual, out string replacement))
                            {
                                replacementsForThisItem.TryAdd(item.UnevaluatedInclude, replacement);
                            }

                            //Console.WriteLine("  {0} / {1} / {2} / {3}", msbuildProjectItem.ItemType, fullPath, actual, replacement);
                        }

                        foreach (IMSBuildProjectItemMetadata metadatum in item.DirectMetadata)
                        {
                            if (File.Exists(metadatum.EvaluatedValue))
                            {
                                fullPath = metadatum.EvaluatedValue;

                                result = Utility.TryGetFullPathInCorrectCase(msbuildProject.DirectoryPath, metadatum.EvaluatedValue, out actual);

                                if (result && actual != null && !string.Equals(fullPath, actual))
                                {
                                    if (TryGetReplacement(metadatum.UnevaluatedValue, actual, out string replacement))
                                    {
                                        replacementsForThisItem.TryAdd(metadatum.UnevaluatedValue, replacement);
                                    }

                                    //Console.WriteLine("  {0} / {1}={2} / {3} / {4}", msbuildProjectItem.ItemType, metadatum.Name, metadatum.EvaluatedValue, actual, replacement);
                                }
                            }
                        }
                    });
                }
            }

            foreach (KeyValuePair<string, ConcurrentDictionary<string, string>> item in replacementsByFile)
            {
                if (!item.Value.Any())
                {
                    continue;
                }

                Console.WriteLine("{0}", item.Key);

                foreach (KeyValuePair<string, string> value in item.Value)
                {
                    Console.WriteLine("  {0} => {1}", value.Key, value.Value);
                }
                Console.WriteLine();
            }

            // TODO: Save 
            // TODO: DryRun
            // TODO: Better output

            Console.WriteLine("Completed in {0:N1}s and looked up {1:N0} unique paths of which {2:N0} where files", sw.Elapsed.TotalSeconds, Utility.CacheCount, Utility.FileCount);

            Console.ReadLine();
        }

        internal static bool TryGetReplacement(string unevaluated, string corrected, out string replacement)
        {
            //const string s =                 @"$(Foo)\bar\baz.cs";
            //const string y = @"D:\cloud\private\tools\Bar\Baz.cs";

            replacement = string.Empty;

            int x = unevaluated.Length - 1, y = corrected.Length - 1;

            for (; x >= 0 && y >= 0; x--, y--)
            {
                char one = unevaluated[x];
                char two = corrected[y];

                if (!char.ToLowerInvariant(one).Equals(char.ToLowerInvariant(two)))
                {
                    replacement = $"{unevaluated.Substring(0, x + 1)}{corrected.Substring(y + 1)}";

                    return !string.Equals(unevaluated, replacement);
                }
            }

            if (x < 0)
            {
                replacement = corrected.Substring(y + 1);

                return !string.Equals(unevaluated, replacement);
            }

            return false;
        }
    }
}