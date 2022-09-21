using System;

namespace MSBuildCaseFixer
{
    /// <summary>
    /// Represents a class used to set MSBuild feature flags.
    /// </summary>
    internal sealed class MSBuildFeatureFlags : IDisposable
    {
        private const string MSBuild_Exe_Path = nameof(MSBuild_Exe_Path);
        private const string MSBuildCacheFileEnumerations = nameof(MSBuildCacheFileEnumerations);
        private const string MSBuildLoadAllFilesAsReadOnly = nameof(MSBuildLoadAllFilesAsReadOnly);
        private const string MSBuildSkipEagerWildcardEvaluationRegexes = nameof(MSBuildSkipEagerWildcardEvaluationRegexes);
        private const string MSBuildUseSimpleProjectRootElementCacheConcurrency = nameof(MSBuildUseSimpleProjectRootElementCacheConcurrency);

        private readonly IEnvironmentVariableProvider _environmentVariableProvider;

        private MSBuildFeatureFlags(IEnvironmentVariableProvider environmentVariableProvider, string msBuildExePath)
        {
            _environmentVariableProvider = environmentVariableProvider ?? throw new ArgumentNullException(nameof(environmentVariableProvider));

            // Enables wildcard enumeration cache
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildCacheFileEnumerations, "1");

            // Enables a mode where all files are loaded as read-only
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildLoadAllFilesAsReadOnly, "1");

            // Skips enumeration of any item that contains wildcards (* or ?) unless it ends in *proj
            //_environmentVariableProvider.SetEnvironmentVariable(MSBuildSkipEagerWildcardEvaluationRegexes, @"[*?]+.*(?<!proj)$");

            // Enables the simple project root element cache
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildUseSimpleProjectRootElementCacheConcurrency, "1");

            // Sets where the main MSBuild toolset is located
            _environmentVariableProvider.SetEnvironmentVariable(MSBuild_Exe_Path, msBuildExePath ?? throw new ArgumentNullException(nameof(msBuildExePath)));
        }

        public static IDisposable Enable(IEnvironmentVariableProvider environmentVariableProvider, string msBuildExePath)
        {
            return new MSBuildFeatureFlags(environmentVariableProvider, msBuildExePath);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _environmentVariableProvider.SetEnvironmentVariable(MSBuild_Exe_Path, null);
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildCacheFileEnumerations, null);
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildLoadAllFilesAsReadOnly, null);
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildSkipEagerWildcardEvaluationRegexes, null);
            _environmentVariableProvider.SetEnvironmentVariable(MSBuildUseSimpleProjectRootElementCacheConcurrency, null);
        }
    }
}