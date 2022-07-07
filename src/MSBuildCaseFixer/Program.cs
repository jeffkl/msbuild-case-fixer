using Microsoft.Build.Locator;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents the main logic for the application.
    /// </summary>
    public static class Program
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
        /// <param name="project">The project or solution to load to discover projects that need to be searched.</param>
        /// <param name="root">The root directory for your repository.</param>
        /// <param name="debug">Launch the program under the debugger.</param>
        /// <returns></returns>
        public static int Main(string project, string root, bool debug)
        {
            if (debug)
            {
                Debugger.Launch();
            }

            IFileInfo? msBuildExePath;

            VisualStudioInstance? visualStudioInstance = MSBuildLocator.QueryVisualStudioInstances().FirstOrDefault();

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

            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            {
                AppDomain appDomain = AppDomain.CreateDomain(
                    thisAssembly.FullName,
                    securityInfo: null,
                    info: new AppDomainSetup
                    {
                        ApplicationBase = msBuildExePath!.DirectoryName,
                        ConfigurationFile = Path.Combine(msBuildExePath.DirectoryName!, Path.ChangeExtension(msBuildExePath.Name, ".exe.config")),
                    });

                return appDomain
                    .ExecuteAssembly(
                        thisAssembly.Location,
                        Environment.GetCommandLineArgs().Skip(1).ToArray());
            }

            MSBuildProjectCaseFixer msbuildCaseFixer = new(ProjectLoader, EnvironmentVariableProvider, FileSystem);

            msbuildCaseFixer.FixProjectCase(project, root, msBuildExePath!);

            return 1;
        }
    }
}