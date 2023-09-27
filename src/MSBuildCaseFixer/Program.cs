using Microsoft.Build.Locator;
using Microsoft.Extensions.FileSystemGlobbing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents the main logic for the application.
    /// </summary>
    public static partial class Program
    {
        /// <summary>
        /// Gets or sets an <see cref="IEnvironmentProvider" /> to use when manipulating environment variables.
        /// </summary>
        public static IEnvironmentProvider EnvironmentProvider { get; set; } = SystemEnvironmentProvider.Instance;

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
        /// <param name="commit"><see langword="true" /> to commit changes to disk, otherwise <see langword="false" />.</param>
        /// <param name="debug">Launch the program under the debugger.</param>
        /// <returns></returns>
        public static int Execute(string projectGlobs, bool commit, bool debug)
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
                    if (!Utility.TryFindOnPath("MSBuild.exe", validator: null, EnvironmentProvider, FileSystem, out msBuildExePath))
                    {
                        // TODO: Log an error that MSBuild.exe can't be found
                        return 0;
                    }
                }
                else
                {
                    msBuildExePath = FileSystem.FileInfo.New(FileSystem.Path.Combine(visualStudioInstance.MSBuildPath, "MSBuild.exe"));
                }

                using (MSBuildFeatureFlags.Enable(EnvironmentProvider, msBuildExePath!.FullName))
                {
                    return
                        AppDomain.CreateDomain(
                            thisAssembly.FullName,
                            securityInfo: null,
                            info: new AppDomainSetup
                            {
                                ApplicationBase = msBuildExePath!.DirectoryName,
                                ConfigurationFile = FileSystem.Path.Combine(msBuildExePath.DirectoryName!, FileSystem.Path.ChangeExtension(msBuildExePath.Name, ".exe.config")),
                            })
                        .ExecuteAssembly(
                            thisAssembly.Location,
                            EnvironmentProvider.GetCommandLineArgs().Skip(1).ToArray());
                }
            }

            IDirectoryInfo? root = GetRepositoryRoot(FileSystem);

            if (root == null)
            {
                // TODO: Log error
                return 0;
            }

            Console.WriteLine("{0} / {1}", projectGlobs, debug);

            List<string> projectPaths = GetProjectPaths(FileSystem, projectGlobs).ToList();

            MSBuildProjectCaseFixer msbuildCaseFixer = new(ProjectLoader, root, FileSystem);

            msbuildCaseFixer.Run(projectPaths, commit);

            return 0;
        }

        internal static IEnumerable<string> GetProjectPaths(IFileSystem fileSystem, string glob)
        {
            Matcher matcher = GetMatcher(glob);

            PatternMatchingResult result = matcher.Execute(new MicrosoftExtensionsFileSystemGlobbingAbstractionsDirectoryInfoWrapper(fileSystem.DirectoryInfo.New(fileSystem.Directory.GetCurrentDirectory())));

            if (result.HasMatches)
            {
                foreach (FilePatternMatch file in result.Files)
                {
                    yield return file.Path;
                }
            }

            Matcher GetMatcher(string path)
            {
                string currentDirectory = FileSystem.Directory.GetCurrentDirectory();

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

        internal static IDirectoryInfo? GetRepositoryRoot(IFileSystem fileSystem)
        {
            IDirectoryInfo? currentDirectory = fileSystem.DirectoryInfo.New(fileSystem.Directory.GetCurrentDirectory());

            do
            {
                if (fileSystem.Directory.Exists(FileSystem.Path.Combine(currentDirectory.FullName, ".git")))
                {
                    return currentDirectory;
                }

                currentDirectory = currentDirectory.Parent;
            }
            while (currentDirectory != null);

            return null;
        }
    }
}