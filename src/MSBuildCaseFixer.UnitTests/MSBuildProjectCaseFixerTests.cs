using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MSBuildCaseFixer.UnitTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="MSBuildProjectCaseFixer" /> class.
    /// </summary>
    public class MSBuildProjectCaseFixerTests : TestBase
    {
        /// <inheritdoc cref="TestBase(ITestOutputHelper)" />
        public MSBuildProjectCaseFixerTests(ITestOutputHelper outputHelper)
            : base(outputHelper)
        {
        }

        /// <summary>
        /// Verifies that the <see cref="MSBuildProjectCaseFixer.TryGetReplacement(string, string, out string)" /> method correctly determines a string replacement.
        /// </summary>
        /// <param name="unevaluated">The unevaluated value contained in an MSBuild project.</param>
        /// <param name="corrected">The actual value that has the correct file system casing.</param>
        /// <param name="expected">The expected replcement value returned by <see cref="MSBuildProjectCaseFixer.TryGetReplacement(string, string, out string)" />.</param>
        [Theory]
        [InlineData(@"$(Foo)\bar\baz.cs", @"D:\repo\tools\Bar\Baz.cs", @"$(Foo)\Bar\Baz.cs")]
        [InlineData(@"$(PkgMicrosoft_VfsForGitEnvironment)\Content\Installers\Windows\VFSForGit\*.exe", @"C:\Users\jeffkl\.nuget\packages\microsoft.vfsforgitenvironment\1.0.21085.9\content\Installers\Windows\VFSForGit\SetupGVFS.1.0.21085.9.exe", @"$(PkgMicrosoft_VfsForGitEnvironment)\content\Installers\Windows\VFSForGit\*.exe")]
        public void TryGetReplacementTest(string unevaluated, string corrected, string expected)
        {
            MSBuildProjectCaseFixer.TryGetReplacement(unevaluated, corrected, out string actual).ShouldBeTrue();

            actual.ShouldBe(expected);
        }
    }
}