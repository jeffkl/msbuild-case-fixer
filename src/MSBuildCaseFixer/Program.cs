using Microsoft.Build.Locator;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using DirectoryInfoWrapper = Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents the main logic for the application.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// Gets or sets an <see cref="IEnvironmentVariableProvider" /> to use when manipulating environment variables.
        /// </summary>
        public static IEnvironmentVariableProvider EnvironmentVariableProvider { get; set; } = SystemEnvironmentVariableProvider.Instance;

        /// <summary>
        /// Gets or sets an <see cref="IFileSystem" /> to use when interacting with the file system.
        /// </summary>
        public static IFileSystem FileSystem { get; set; } = new FileSystem();

        /// <summary>
        /// Gets or sets an <see cref="IMSBuildProjectLoader" /> to use when loading projects.
        /// </summary>
        public static IMSBuildProjectLoader ProjectLoader { get; set; } = new MSBuildProjectLoader();

        /// <summary>
        /// Executes the program with the specified arguments.
        /// </summary>
        /// <param name="projectGlobs">The project or solution to load to discover projects that need to be searched.</param>
        /// <param name="root">The root directory for your repository.</param>
        /// <param name="commit"><c>true</c> to commit changes to disk, otherwise <c>false</c>.</param>
        /// <param name="debug">Launch the program under the debugger.</param>
        /// <returns></returns>
        public static int Execute(string projectGlobs, string root, bool commit, bool debug)
        {
            if (debug)
            {
                Debugger.Launch();
            }

            IFileInfo? msBuildExePath;

            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                VisualStudioInstance? visualStudioInstance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault();

                Assembly thisAssembly = Assembly.GetExecutingAssembly();

                if (visualStudioInstance?.VisualStudioRootPath == null)
                {
                    if (!Utility.TryFindOnPath("MSBuild.exe", validator: null, EnvironmentVariableProvider, FileSystem, out msBuildExePath))
                    {
                        // TODO: Log an error that MSBuild.exe can't be found
                        return 0;
                    }
                }
                else
                {
                    msBuildExePath = FileSystem.FileInfo.FromFileName(Path.Combine(visualStudioInstance.MSBuildPath, "MSBuild.exe"));
                }
                using (MSBuildFeatureFlags.Enable(EnvironmentVariableProvider, msBuildExePath!.FullName))
                {
                    AppDomain appDomain = AppDomain.CreateDomain(
                    thisAssembly.FullName,
                    securityInfo: null,
                    info: new AppDomainSetup
                    {
                        ApplicationBase = msBuildExePath!.DirectoryName,
                        ConfigurationFile = Path.Combine(msBuildExePath.DirectoryName!, Path.ChangeExtension(msBuildExePath.Name, ".exe.config")),
                    });

                    appDomain.SetData(nameof(msBuildExePath), msBuildExePath.FullName);

                    return appDomain
                        .ExecuteAssembly(
                            thisAssembly.Location,
                            Environment.GetCommandLineArgs().Skip(1).ToArray());
                }
            }

            Console.WriteLine("{0} / {1}", projectGlobs, debug);

            List<string> projectPaths = GetProjectPaths(projectGlobs).ToList();

            MSBuildProjectCaseFixer msbuildCaseFixer = new(ProjectLoader, root, FileSystem);

            msbuildCaseFixer.Run(projectPaths, commit);

            return 0;
        }

        private static IEnumerable<string> GetProjectPaths(string glob)
        {
            Matcher matcher = GetMatcher(glob);

            PatternMatchingResult result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(Environment.CurrentDirectory)));

            if (result.HasMatches)
            {
                foreach (FilePatternMatch file in result.Files)
                {
                    yield return file.Path;
                }
            }

            Matcher GetMatcher(string path)
            {
                string currentDirectory = Environment.CurrentDirectory;

                Matcher matcher = new Matcher();

                char[] splitChars = new char[] { ';', ',' };

                foreach (string? item in path.Split(splitChars).Select(i => i.Trim()))
                {
                    if (string.IsNullOrWhiteSpace(item))
                    {
                        continue;
                    }

                    if (item.StartsWith("!", StringComparison.Ordinal))
                    {
                        matcher.AddExclude(item.TrimStart('!'));
                    }
                    else
                    {
                        matcher.AddInclude(item.Replace(currentDirectory, string.Empty));
                    }
                }

                return matcher;
            }
        }
    }
}